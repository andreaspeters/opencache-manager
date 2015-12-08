// 
//  Copyright 2011  tweetyhh aka. Florian Plähn
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
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

namespace ocmgtk
{


	/// <summary>
	/// Be aware! This class is my first real GTK code. ;-)
	/// 
	/// This class isn't really good because of the double holding of the
	/// maps. They are stored in the List m_maps and also in the ListStore
	/// m_mapModel. So all changes of the order have to be done in both lists!
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MapSelectionWidget : Gtk.Bin
	{
		private List<MapDescription> m_maps;
		private ListStore m_mapModel;

		public List<MapDescription> Maps {
			get { return m_maps; }
			set {
				m_maps = value;
				foreach (MapDescription md in m_maps) {
					m_mapModel.AppendValues (md);
				}
			}
		}

		public MapSelectionWidget ()
		{
			this.Build ();
			m_mapModel = new ListStore (typeof(MapDescription));
			m_maps = new List<MapDescription> ();
			
			FillList ();
		}

		private void FillList ()
		{
			//mapView = new Gtk.TreeView();
			
//			CellRendererPixbuf activeCell = new CellRendererPixbuf ();
			CellRendererText nameCell = new CellRendererText ();
			CellRendererText baseLayerCell = new CellRendererText ();
			CellRendererText coveredCell = new CellRendererText ();
			
//			TreeViewColumn activeIconColum = new TreeViewColumn (Catalog.GetString ("Active"), activeCell);
			TreeViewColumn nameColumn = new TreeViewColumn (Catalog.GetString ("Name"), nameCell);
			TreeViewColumn baseLayerColumn = new TreeViewColumn (Catalog.GetString ("Baselayer"), baseLayerCell);
			TreeViewColumn coveredColumn = new TreeViewColumn (Catalog.GetString ("Covered"), coveredCell);
			
//			mapView.AppendColumn (activeIconColum);
			mapView.AppendColumn (nameColumn);
			mapView.AppendColumn (baseLayerColumn);
			mapView.AppendColumn (coveredColumn);
			
//			activeIconColum.SetCellDataFunc (activeCell, new TreeCellDataFunc (RenderCacheIcon));
//			activeIconColum.SortColumnId = 0;
			nameColumn.SetCellDataFunc (nameCell, new TreeCellDataFunc (RenderMapName));
			nameColumn.SortColumnId = 0;
			baseLayerColumn.SetCellDataFunc (baseLayerCell, new TreeCellDataFunc (RenderMapBaseLayer));
			baseLayerColumn.SortColumnId = 1;
			coveredColumn.SetCellDataFunc (coveredCell, new TreeCellDataFunc (RenderMapCovered));
			coveredColumn.SortColumnId = 2;
			
			mapView.Model = m_mapModel;
			mapView.Selection.Changed += HandleMapViewSelectionChanged;
		}

		void HandleMapViewSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			
			if (((TreeSelection)sender).GetSelected (out model, out iter)) {
				MapDescription val = (MapDescription) model.GetValue (iter, 0);
				if (val != null)
				{
					upButton.Sensitive = m_maps.IndexOf(val) > 0;
					downButton.Sensitive = m_maps.IndexOf(val) < (m_maps.Count -1);
					deleteButton.Sensitive = true;
					activateButton.Sensitive = !val.Active;
					deactivateButton.Sensitive = val.Active;
					deleteButton.Sensitive = true;
				}
				
			} else {
					upButton.Sensitive = false;
					downButton.Sensitive = false;
					deleteButton.Sensitive = false;
					activateButton.Sensitive = false;
					deactivateButton.Sensitive = false;
					deleteButton.Sensitive = false;
			}
		}

		private void RenderMapName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MapDescription map = (MapDescription)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = map.Name;
			
			if (map.Active) {
				text.Strikethrough = false;
			} else {
				text.Strikethrough = true;
			}
		}

		private void RenderMapBaseLayer (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MapDescription map = (MapDescription)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = map.BaseLayer ? Catalog.GetString("yes") : Catalog.GetString("no");
		}

		private void RenderMapCovered (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MapDescription map = (MapDescription)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = map.Covered;
		}

		private void UpdateMaps (Gtk.TreeModel model, Gtk.TreeIter itr)
		{
			model.EmitRowChanged (model.GetPath (itr), itr);
		}

		private void ReloadMaps() {
			m_mapModel.Clear ();
			foreach (MapDescription md in m_maps) {
				m_mapModel.AppendValues (md);
			}
		}
		
		protected virtual void OnActivateButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (mapView.Selection.GetSelected (out model, out itr)) {
				MapDescription map = (MapDescription)model.GetValue (itr, 0);
				map.Active = true;
				UpdateMaps (model, itr);
			}
		}

		protected virtual void OnDeactivateButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (mapView.Selection.GetSelected (out model, out itr)) {
				MapDescription map = (MapDescription)model.GetValue (itr, 0);
				map.Active = false;
				UpdateMaps (model, itr);
			}
		}

		protected virtual void OnOpenButtonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = null;
			try {
				dlg = new FileChooserDialog (Catalog.GetString ("Open Map Description File"), null, FileChooserAction.Open, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Open"), ResponseType.Accept);
				//dlg.SetCurrentFolder (m_conf.DataDirectory);
				FileFilter filter = new FileFilter ();
				filter.Name = Catalog.GetString("OCM Map Files");
				filter.AddPattern ("*.xml");
				dlg.AddFilter (filter);
				
				if (dlg.Run () == (int)ResponseType.Accept) {
					dlg.Hide ();
					m_maps.AddRange(MapManager.GetMapsFromFile(dlg.Filename));
					ReloadMaps();
					dlg.Destroy ();
				} else {
					dlg.Hide ();
					dlg.Destroy ();
				}
			} catch (Exception exception) {
				OCMApp.ShowException(exception);
				if (dlg != null) {
					dlg.Hide();
					dlg.Destroy();
				}
			}
		}
	
		protected virtual void OnDeleteButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (mapView.Selection.GetSelected (out model, out itr)) {
				MapDescription map = (MapDescription)model.GetValue (itr, 0);
				MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, String.Format (Catalog.GetString ("Are you sure you want to delete map {0}?"), map.Name));
				if ((int)ResponseType.Yes == dlg.Run ()) {
					m_maps.Remove(map);
					ReloadMaps();
				}
				dlg.Hide();
			}
		}
		protected virtual void OnRestoreButtonClicked (object sender, System.EventArgs e)
		{
			MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, String.Format (Catalog.GetString ("Are really want to delete all maps and restore the default maps?")));
			if ((int)ResponseType.Yes == dlg.Run ()) {
				m_maps = MapManager.GetMapsFromFile("maps/defaultmaps.xml");
				ReloadMaps();
			}
			dlg.Hide();
		}
		
		protected virtual void OnUpButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (mapView.Selection.GetSelected (out model, out itr)) {
				MapDescription map = (MapDescription)model.GetValue (itr, 0);
				int index = m_maps.IndexOf(map);
				if (index > 0) { // Can't move the first element up
					int[] order = new int[m_maps.Capacity];
					for (int i = 0; i < m_maps.Capacity; i++) {
						order[i] = i;	
					}
					order[index] = index - 1;
					order[index - 1] = index;
					m_mapModel.Reorder(order);	
					// due double data holding, not fine but it works
					m_maps.RemoveAt(index);
					m_maps.Insert(index - 1, map);
					upButton.Sensitive = m_maps.IndexOf(map) > 0;
					downButton.Sensitive = m_maps.IndexOf(map) < (m_maps.Count -1);
				}
			}
		}
		
		protected virtual void OnDownButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (mapView.Selection.GetSelected (out model, out itr)) {
				MapDescription map = (MapDescription)model.GetValue (itr, 0);
				int index = m_maps.IndexOf(map);
				if (index < m_maps.Count - 1) { // Can't move the last element down
					int[] order = new int[m_maps.Capacity];
					for (int i = 0; i < m_maps.Capacity; i++) {
						order[i] = i;	
					}
					order[index] = index + 1;
					order[index + 1] = index;
					m_mapModel.Reorder(order);	
					// due double data holding, not fine but it works
					m_maps.RemoveAt(index);
					m_maps.Insert(index + 1, map);
					upButton.Sensitive = m_maps.IndexOf(map) > 0;
					downButton.Sensitive = m_maps.IndexOf(map) < (m_maps.Count -1);
				}
			}
		}
	}
	
}