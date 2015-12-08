// 
//  Copyright 2010  Kyle Campbell
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Gtk;
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class WaypointWidget : Gtk.Bin
	{

		private ListStore m_childPoints;
		private TreeModelSort m_ListSort;
		private Geocache m_Cache;
		public Geocache Cache
		{
			get{ return m_Cache;}
		}
		
		private OCMApp m_App;
		public OCMApp App
		{
			get { return m_App;}
			set { m_App = value;}
		}

		public WaypointWidget ()
		{
			this.Build ();
			BuildWPTList ();
		}

		public void BuildWPTList ()
		{
			m_childPoints = new ListStore (typeof(Waypoint));
			
			CellRendererText code_cell = new CellRendererText ();
			CellRendererText title_cell = new CellRendererText ();
			CellRendererText coord_cell = new CellRendererText ();
			CellRendererPixbuf icon_cell = new CellRendererPixbuf ();
			TreeViewColumn wpt_icon = new TreeViewColumn (Catalog.GetString ("Type"), icon_cell);
			wpt_icon.SortColumnId = 3;
			TreeViewColumn wpt_code = new TreeViewColumn (Catalog.GetString ("Code"), code_cell);
			wpt_code.SortColumnId = 0;
			TreeViewColumn wpt_Lat = new TreeViewColumn (Catalog.GetString ("Location"), coord_cell);
			wpt_Lat.SortColumnId = 1;
			TreeViewColumn wpt_title = new TreeViewColumn (Catalog.GetString ("Name"), title_cell);
			wpt_title.SortColumnId = 2;
			
			wptView.AppendColumn (wpt_icon);
			wptView.AppendColumn (wpt_code);
			wptView.AppendColumn (wpt_Lat);
			wptView.AppendColumn (wpt_title);
			
			
			wpt_code.SetCellDataFunc (code_cell, new TreeCellDataFunc (RenderCode));
			wpt_title.SetCellDataFunc (title_cell, new TreeCellDataFunc (RenderTitle));
			wpt_Lat.SetCellDataFunc (coord_cell, new TreeCellDataFunc (RenderCoord));
			wpt_icon.SetCellDataFunc (icon_cell, new TreeCellDataFunc (RenderIcon));
			m_ListSort = new TreeModelSort(m_childPoints);
			m_ListSort.SetSortFunc (3, TypeCompare);
			m_ListSort.SetSortFunc (2, NameCompare);
			m_ListSort.SetSortFunc (1, LocationCompare);
			m_ListSort.SetSortFunc (0, CodeCompare);
			
			wptView.Model = m_ListSort;
			wptView.Selection.Changed += OnSelectionChanged;
		}
		
		private int NameCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Waypoint cacheA = (Waypoint)model.GetValue (tia, 0);
			Waypoint cacheB = (Waypoint)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;				
			return String.Compare (cacheA.Desc, cacheB.Desc);
		}
		
		private int TypeCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Waypoint cacheA = (Waypoint)model.GetValue (tia, 0);
			Waypoint cacheB = (Waypoint)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;
			return String.Compare (cacheA.Symbol, cacheB.Symbol);
		}
		
		private int LocationCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Waypoint cacheA = (Waypoint)model.GetValue (tia, 0);
			Waypoint cacheB = (Waypoint)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;
			double compare = GetDistanceFromParent (cacheA) - GetDistanceFromParent(cacheB);
			if (compare > 0)
				return 1; else if (compare == 0)
				return 0;
			else
				return -1;
		}
		
		public double GetDistanceFromParent (Waypoint pt)
		{
			if (pt == null)
				return 0;
			return Utilities.calculateDistance (m_Cache.Lat, pt.Lat, m_Cache.Lon, pt.Lon);
		}

		
		private int CodeCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Waypoint cacheA = (Waypoint)model.GetValue (tia, 0);
			Waypoint cacheB = (Waypoint)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;
			return String.Compare (cacheA.Name, cacheB.Name);
		}

		private void OnSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			
			if (((TreeSelection)sender).GetSelected (out model, out iter)) {
				Waypoint val = (Waypoint)model.GetValue (iter, 0);
				if (val != null)
					m_App.HighlightPointOnMap (val.Lat, val.Lon);
				if (val is Geocache){
					editButton.Sensitive = true;
					deleteButton.Sensitive = false;
				} 
				else if (val.Type == "Geocache - Original")
				{
					editButton.Sensitive = false;
					deleteButton.Sensitive = false;
				}
				else 
				{
					editButton.Sensitive = true;
					deleteButton.Sensitive = true;
				}
			} else {
				editButton.Sensitive = false;
				deleteButton.Sensitive = false;
			}
		}

		public void SetCacheAndPoints(Geocache cache, List<Waypoint> points)
		{
			m_Cache = cache;
			m_childPoints.Clear ();
			if (cache == null)
			{
				addButton.Sensitive = false;
				grabButton.Sensitive = false;
				return;
			}
			else
			{
				addButton.Sensitive = true;
				grabButton.Sensitive = true;
			}
			m_ListSort.SetSortColumnId (1, SortType.Ascending);
			m_childPoints.AppendValues(cache);
			foreach(Waypoint pt in points)
			{
				m_childPoints.AppendValues(pt);
			}
		}
		
		private void RenderTitle (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Waypoint wpt = (Waypoint)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = wpt.Desc;
		}

		private void RenderCode (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Waypoint wpt = (Waypoint)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = wpt.Name;
		}

		private void RenderCoord (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Waypoint wpt = (Waypoint)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = Utilities.getCoordString (wpt.Lat, wpt.Lon);
		}
		
		private void RenderIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Waypoint pt = (Waypoint)model.GetValue (iter, 0);
			CellRendererPixbuf icon = cell as CellRendererPixbuf;
			if (pt is Geocache)
				icon.Pixbuf = IconManager.GetSmallCacheIcon ((pt as Geocache).TypeOfCache);
			else
				icon.Pixbuf = IconManager.GetSmallWaypointIcon(pt.Symbol);
		}

		protected virtual void DoEdit (object sender, System.EventArgs e)
		{
			try {
				
				Gtk.TreeIter itr;
				Gtk.TreeModel model;
				if (wptView.Selection.GetSelected (out model, out itr)) 
				{
					Waypoint wpt = (Waypoint)model.GetValue (itr, 0);
					if (wpt is Geocache)
					{
						m_App.EditCache();
						return;
					}
					m_App.EditChildWaypoint(wpt);
				}
			} catch (Exception ex) {
				OCMApp.ShowException(ex);
			}
		}

		protected virtual void doAdd (object sender, System.EventArgs e)
		{
			m_App.AddChildWaypoint();
		}

		protected virtual void doRemove (object sender, System.EventArgs e)
		{
			Waypoint toDelete = GetSelectedWaypoint ();
			MessageDialog md = new MessageDialog (null, DialogFlags.DestroyWithParent, 
			                                      MessageType.Info, ButtonsType.YesNo, 
			                                      "Are you sure you wish to delete " + toDelete.Name);
			if ((int)ResponseType.Yes == md.Run ()) 
			{
				if (toDelete.Symbol == "Final Location")
					m_Cache.HasFinal = false;
				m_App.DeleteChildPoint(toDelete.Name);
			}				
			md.Hide ();
			md.Dispose ();
		}

		private Waypoint GetSelectedWaypoint ()
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (wptView.Selection.GetSelected (out model, out itr)) {
				return (Waypoint)model.GetValue (itr, 0);
			}
			return null;
		}
		
		public void SelecteWaypointByName(string code)
		{
			try
			{
				if (code == null)
				{
					wptView.Selection.UnselectAll();
					return;
				}
				
				TreeIter itr;
				TreeModel model = wptView.Model;
				wptView.Model.GetIterFirst(out itr);
				do
				{
					Waypoint pt = (Waypoint)model.GetValue (itr, 0);
					if (pt.Name == code)
					{
						wptView.Selection.SelectIter(itr);
						TreePath path = wptView.Model.GetPath(itr);
						wptView.ScrollToCell(path, wptView.Columns[0], true, 0, 0);
						return;
					}
				}
				while (model.IterNext(ref itr));
			}
			catch (Exception e)
			{
				OCMApp.ShowException(e);
			}
		}
		
		public void GrabWaypoints()
		{
			String expr = @"\b[NnSs] ?[0-9]+.? ?[0-9]*\.[0-9]*\s[WwEe] ?[0-9]+.? ?[0-9]*\.[0-9]*";
			String desc = m_Cache.ShortDesc + m_Cache.LongDesc;
			MatchCollection matches = Regex.Matches(desc, expr);
			if (matches.Count > 0)
			{
				ScanWaypointsDialog dlg = new ScanWaypointsDialog(matches.Count, this, matches);
				dlg.Run();
			}
			else
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
				                                      Catalog.GetString("OCM was unable to find any child waypoints."));
				dlg.Run();
				dlg.Hide();
			}
		}
		
		public void AutoGenerateChildren (MatchCollection matches)
		{
			int count = 0;
			m_App.CacheStore.StartUpdate();
			foreach (Match match in matches)
			{
				DegreeMinutes[] coord = Utilities.ParseCoordString (match.Captures[0].Value);
				System.Console.WriteLine (Utilities.getCoordString (coord[0], coord[1]));
				
				Waypoint newPoint = new Waypoint ();
				Geocache parent = m_Cache;
				newPoint.Symbol = "Reference Point";
				newPoint.Parent = parent.Name;
				newPoint.Lat = coord[0].GetDecimalDegrees ();
				newPoint.Lon = coord[1].GetDecimalDegrees ();
				newPoint.Desc = Catalog.GetString ("Grabbed Waypoint");
				String name = "RP" + parent.Name.Substring (2);
				if (m_App.AppConfig.IgnoreWaypointPrefixes)
				{
					name = parent.Name;
				}
				name = m_App.CacheStore.GetUniqueName(name);
				newPoint.Name = name;
				m_App.CacheStore.AddWaypointOrCache(newPoint, false, false);
				count++;
			}
			m_App.CacheStore.CompleteUpdate();
			m_App.RefreshAll();
		}
		
		protected virtual void OnGrabClick (object sender, System.EventArgs e)
		{
			GrabWaypoints();
		}
		
		
	}
}
