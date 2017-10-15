// 
//  Copyright 2011  campbelk
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
using System.Diagnostics;
using System.Text;
using Gtk;
using Gdk;
using ocmengine;
using Mono.Unix;
using System.Timers;

namespace ocmgtk
{
	public struct RefreshCompleteArgs
	{
		public int VisibleCount;
		public int ArchivedDisabledCount;
		public int TotalCount;
		public int MineCount;
		public int FoundCount;
		
		public RefreshCompleteArgs(int visible, int found, int archive, int mine, int total)
		{
			VisibleCount = visible;
			FoundCount = found;
			ArchivedDisabledCount = archive;
			MineCount = mine;
			TotalCount = total;			
		}
	}
	
	public struct CacheEventArgs
	{
		public Geocache Cache;
		
		public CacheEventArgs(Geocache geo)
		{
			Cache = geo;
		}
	}

	[System.ComponentModel.ToolboxItem(true)]
	public partial class CacheListWidget : Gtk.Bin
	{
		public event CacheSelectionHandler SelectionChanged;
		public event RefreshStartHandler RefreshStart;
		public event RefreshPulseHandler RefreshPulse;
		public event RefreshEndHandler RefreshEnd;
		
		public delegate void RefreshStartHandler(object sender, EventArgs args);
		public delegate void RefreshPulseHandler(object sender, CacheEventArgs args);
		public delegate void RefreshEndHandler(object sender, RefreshCompleteArgs args);
		public delegate void CacheSelectionHandler(object sender, CacheEventArgs args);
		
		private const string START_ARCHIVE = "<span fgcolor='red' strikethrough='true'>";
		private const string START_UNAVAIL = "<span fgcolor='red'>";
		private const string START_RECENT_DNF = "<span fgcolor='darkorange'>";
		private const string START_MY_DNF = "<span fgcolor='blue'>";
		private const string START_ITALICS = "<i>";
		private const string START_BOLD = "<b>";
		private const string END_BOLD = "</b>";
		private const string START_UNDERLINE = "<u>";
		private const string END_UNDERLINE = "</u>";
		private const string END_SPAN = "</span>";
		private const string END_ITALICS = "</i>";
		
		private TreeViewColumn m_distanceColumn = null;
		private CacheStoreModel m_list = new CacheStoreModel();
		private TreeModelSort m_sort = null;
		private OCMApp m_app = null;
		private OCMMainWindow m_Win = null;
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}
		
		private int m_disabledOrArchivedCount =0;
		private Geocache m_Selected = null;
		
		public int DisabledOrArchived
		{
			get { return m_disabledOrArchivedCount;}
			set 
			{ 
				m_disabledOrArchivedCount = value;
				if (m_app != null)
					m_app.UpdateStatus(m_visibleCount, m_foundCount, m_disabledOrArchivedCount, m_mineCount);
			}
		}
		private int m_mineCount = 0;
		public int MineCount
		{
			get { return m_mineCount;}
			set 
			{
				m_mineCount = value;
				if (m_app != null)
					m_app.UpdateStatus(m_visibleCount, m_foundCount, m_disabledOrArchivedCount, m_mineCount);
			}
		}
		
		private int m_visibleCount = 0;
		public int VisibleCount
		{
			get { return m_visibleCount;}
			set { 
				m_visibleCount = value;
				if (m_app != null)
					m_app.UpdateStatus(m_visibleCount, m_foundCount, m_disabledOrArchivedCount, m_mineCount);
			}
		}
		
		private int m_foundCount = 0;
		public int FoundCount
		{
			get { return m_foundCount;}
			set { 
				m_foundCount = value;
				if (m_app != null)
					m_app.UpdateStatus(m_visibleCount, m_foundCount, m_disabledOrArchivedCount, m_mineCount);
			}
		}
		
		public bool HasBasicFilters
		{
			get {
				return (availCheck.Active && notAvailCheck.Active && archiveCheck.Active
				&& foundCheck.Active && notFoundCheck.Active && mineCheck.Active && String.IsNullOrEmpty(distanceEntry.Text)
				&& String.IsNullOrEmpty(nameEntry.Text))?false:true;
			}
		}
		
		private bool m_IgnoreStatusChange = false;
		
		
		public OCMApp App
		{
			set { 
				m_app = value;
				if (m_app.AppConfig.ImperialUnits)
				{
					m_distanceColumn.Title = Catalog.GetString("Mi");
					distanceLabel.Text =  Catalog.GetString("Mi");
				}
				else
				{
					m_distanceColumn.Title = Catalog.GetString("km");
					distanceLabel.Text =  Catalog.GetString("km");
				}
			}
		}
		
		private IConfig Config
		{
			get { return m_app.AppConfig;}
		}
		
		private ACacheStore CacheStore
		{
			get { return m_app.CacheStore;}
		}
		
		public Geocache SelectedCache
		{
			get{return m_Selected;}		
		}
		
		public List<Geocache> UnfilteredCaches
		{
			get { return m_list.Caches;}
		}
		
		public CacheListWidget ()
		{
			this.Build ();
			BuildTreeView();
		}
		
		public void SetInitalModel(CacheStoreModel model)
		{
			m_list = model;
			m_sort = new TreeModelSort(new TreeModelAdapter(m_list));
			cacheListTree.Model = m_sort;
			m_list.Resort(m_app.CentreLat, m_app.CentreLon);
			m_sort.SetSortFunc (3, TitleCompare);
			m_sort.SetSortFunc (2, DistanceCompare);
			m_sort.SetSortFunc (1, CodeCompare);
			m_sort.SetSortFunc (0, SymbolCompare);
			UpdateStatus();
		}
		
		public bool ContainsCode(String code)
		{
			foreach(Geocache cache in m_list.Caches)
			{
				if (cache.Name == code)
					return true;
			}
			return false;
		}
		
		
		private void BuildTreeView()
		{
			CellRendererPixbuf typeColRenderer = new CellRendererPixbuf();
			CellRendererText codeColRendered = new CellRendererText();
			CellRendererText titleColRenderer = new CellRendererText();
			CellRendererText distanceColRenderer = new CellRendererText();
			cacheListTree.FixedHeightMode = true;
			
			TreeViewColumn typeColumn = new TreeViewColumn(Catalog.GetString("Type"), typeColRenderer);
			typeColumn.Sizing = TreeViewColumnSizing.Fixed;
			typeColumn.FixedWidth = 40;
			TreeViewColumn codeColumn = new TreeViewColumn(Catalog.GetString("Code"), codeColRendered);
			codeColumn.Sizing = TreeViewColumnSizing.Fixed;
			codeColumn.FixedWidth = 80;
			m_distanceColumn = new TreeViewColumn(Catalog.GetString("Km"), distanceColRenderer);
			m_distanceColumn.FixedWidth = 60;
			m_distanceColumn.Sizing = TreeViewColumnSizing.Fixed;
			TreeViewColumn titleColumn = new TreeViewColumn(Catalog.GetString("Title"), titleColRenderer);
			titleColumn.Sizing = TreeViewColumnSizing.Fixed;
			titleColumn.FixedWidth = 500;
			
			
			
			typeColumn.SetCellDataFunc(typeColRenderer, new TreeCellDataFunc(RenderCacheIcon));
			typeColumn.SortColumnId = 0;
			codeColumn.SetCellDataFunc(codeColRendered, new TreeCellDataFunc(RenderCacheCode));
			codeColumn.SortColumnId = 1;
			m_distanceColumn.SetCellDataFunc(distanceColRenderer, new TreeCellDataFunc(RenderCacheDistance));
			m_distanceColumn.SortColumnId = 2;
			titleColumn.SetCellDataFunc(titleColRenderer, new TreeCellDataFunc(RenderCacheTitle));
			titleColumn.SortColumnId = 3;
			
			cacheListTree.AppendColumn(typeColumn);
			cacheListTree.AppendColumn(codeColumn);
			cacheListTree.AppendColumn(m_distanceColumn);
			cacheListTree.AppendColumn(titleColumn);
			cacheListTree.Selection.Changed += HandleCacheListTreeSelectionChanged;
		}
		
		public void DeselectAll()
		{
			cacheListTree.Selection.UnselectAll();
		}
		
		public void SelectCacheByName(string code)
		{
			if (code == null)
			{
				cacheListTree.Selection.UnselectAll();
				return;
			}
			
			TreeIter itr;
			TreeModel model = cacheListTree.Model;
			cacheListTree.Model.GetIterFirst(out itr);
			do
			{
				Geocache cache = (Geocache)model.GetValue (itr, 0);
				if (cache.Name == code)
				{
					cacheListTree.Selection.SelectIter(itr);
					TreePath path = cacheListTree.Model.GetPath(itr);
					cacheListTree.ScrollToCell(path, cacheListTree.Columns[0], true, 0, 0);
					return;
				}
			}
			while (model.IterNext(ref itr));
		}
		
		public void PopulateQuickFilter(QuickFilter filter)
		{
			if (!String.IsNullOrEmpty(distanceEntry.Text))
				filter.Distance = int.Parse(distanceEntry.Text);
			else
				filter.Distance = -1;
			filter.NameFilter = nameEntry.Text;
			filter.Archived = archiveCheck.Active;
			filter.Available = availCheck.Active;
			filter.Unavailable = notAvailCheck.Active;
			filter.NotFound = notFoundCheck.Active;
			filter.Found = foundCheck.Active;
			filter.Mine = mineCheck.Active;
		}
		
#region Tree Model Rendering Functions
		private void RenderCacheCode (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Geocache cache = (Geocache)model.GetValue (iter, 0);
			if (cache == null)
				return;
			CellRendererText text = cell as CellRendererText;
			cell.CellBackground = null;
			if (m_app.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_app.AppConfig.StaleCacheInterval,0,0,0,0)))
					cell.CellBackground = "gold";			
			}
			if (m_app.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_app.AppConfig.NewCacheInterval,0,0,0,0)))
					cell.CellBackground = "light green";
			}
			if (!cache.Available && !cache.Archived)
				text.Markup = unavailText (cache.Name); else if (cache.Archived)
				text.Markup = archiveText (cache.Name);
			else
				text.Markup = cache.Name;
		}

		private void RenderCacheTitle (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Geocache cache = (Geocache)model.GetValue (iter, 0);
			if (cache == null)
				return;
			CellRendererText text = cell as CellRendererText;
			cell.CellBackground = null;

			if (m_app.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_app.AppConfig.StaleCacheInterval,0,0,0,0)))
					cell.CellBackground = "gold";				
			}
			if (m_app.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_app.AppConfig.NewCacheInterval,0,0,0,0)))
						cell.CellBackground = "light green";
			}		
			StringBuilder builder = new StringBuilder();
			if (cache.Children)
				builder.Append(START_BOLD);
			if (!String.IsNullOrEmpty(cache.Notes))
				builder.Append(START_UNDERLINE);
			if (cache.HasCorrected)
				builder.Append(START_ITALICS);
			if (!cache.Available && !cache.Archived)
				builder.Append(unavailText (cache.CacheName));
			else if (cache.Archived)
				builder.Append(archiveText (cache.CacheName));
			else if (cache.DNF)
				builder.Append(START_MY_DNF + GLib.Markup.EscapeText (cache.CacheName) + END_SPAN);
			else if (cache.CheckNotes)
				builder.Append(START_RECENT_DNF + GLib.Markup.EscapeText (cache.CacheName) + END_SPAN);
			else
				builder.Append(GLib.Markup.EscapeText (cache.CacheName));
			if (cache.HasCorrected)
				builder.Append(END_ITALICS);
			if (!String.IsNullOrEmpty(cache.Notes))
				builder.Append(END_UNDERLINE);
			if (cache.Children)
				builder.Append(END_BOLD);
			text.Markup = builder.ToString();
		}

		private void RenderCacheIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Geocache cache = (Geocache)model.GetValue (iter, 0);
			if (cache == null)
				return;
			CellRendererPixbuf icon = cell as CellRendererPixbuf;
			cell.CellBackground = null;
			if (m_app.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_app.AppConfig.StaleCacheInterval,0,0,0,0)))
					cell.CellBackground = "gold";
			}
			if (m_app.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_app.AppConfig.NewCacheInterval,0,0,0,0)))
					cell.CellBackground = "light green";
			}
			if (cache.FTF)
				icon.Pixbuf = IconManager.FTF_S;
			else if (cache.DNF && Config.ShowDNFIcon)
				icon.Pixbuf = IconManager.DNF_S;
			else if (cache.Found)
				icon.Pixbuf = IconManager.GetSmallCacheIcon (Geocache.CacheType.FOUND);
			else if (m_app.OwnerIDs.Contains(cache.OwnerID)  || m_app.OwnerIDs.Contains(cache.CacheOwner))
				icon.Pixbuf = IconManager.GetSmallCacheIcon (Geocache.CacheType.MINE);
			else if (cache.HasCorrected || cache.HasFinal)
			{
				if (Config.SolvedModeState == SolvedMode.ALL)
					icon.Pixbuf = IconManager.CORRECTED_S;
				else if ((Config.SolvedModeState == SolvedMode.PUZZLES) && 
				        	(cache.TypeOfCache == Geocache.CacheType.MYSTERY))
					icon.Pixbuf = IconManager.CORRECTED_S;
				else
					icon.Pixbuf = IconManager.GetSmallCacheIcon (cache.TypeOfCache);
			}
			else
			{
				icon.Pixbuf = IconManager.GetSmallCacheIcon (cache.TypeOfCache);
			}
		}

		private void RenderCacheDistance (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Geocache cache = (Geocache)model.GetValue (iter, 0);
			if (cache == null)
				return;
			CellRendererText text = cell as CellRendererText;
			cell.CellBackground = null;
			if (m_app.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_app.AppConfig.StaleCacheInterval,0,0,0,0)))
					cell.CellBackground = "gold";				
			}
			if (m_app.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_app.AppConfig.NewCacheInterval,0,0,0,0)))
					cell.CellBackground = "light green";				
			}
			try {
				double dist = cache.Distance;
				if (Config.ImperialUnits)
					dist = Utilities.KmToMiles(dist);
				text.Text = dist.ToString ("0.00");
			} catch (Exception e) {
				text.Text = "?";
				System.Console.WriteLine (e.Message);
			}
		}
		
		private int TitleCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Geocache cacheA = (Geocache)model.GetValue (tia, 0);
			Geocache cacheB = (Geocache)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0 ;
			return String.Compare (cacheA.CacheName, cacheB.CacheName);
		}
		
		private int CodeCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Geocache cacheA = (Geocache)model.GetValue (tia, 0);
			Geocache cacheB = (Geocache)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0 ;
			return String.Compare (cacheA.Name, cacheB.Name);
		}


		private int DistanceCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Geocache cacheA = (Geocache)model.GetValue (tia, 0);
			Geocache cacheB = (Geocache)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;
			double compare = cacheA.Distance - cacheB.Distance;
			if (compare > 0)
				return 1; else if (compare == 0)
				return 0;
			else
				return -1;
		}

		private int SymbolCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			Geocache cacheA = (Geocache)model.GetValue (tia, 0);
			Geocache cacheB = (Geocache)model.GetValue (tib, 0);
			if (cacheA == null || cacheB == null)
				return 0;
			return String.Compare (cacheA.TypeOfCache.ToString (), cacheB.TypeOfCache.ToString ());
		}
		
		public void Resort()
		{
			m_list.Resort(m_app.CentreLat, m_app.CentreLon);
			this.QueueDraw();
		}

		
		private string unavailText (string text)
		{
			return START_UNAVAIL + GLib.Markup.EscapeText (text) + END_SPAN;
		}

		private string archiveText (string text)
		{
			return START_ARCHIVE + GLib.Markup.EscapeText (text) + END_SPAN;
		}
		
#endregion

		
		public void Refresh()
		{
			try
			{
				if (null != this.RefreshStart)
					RefreshStart(this, new EventArgs());
				Geocache selected = SelectedCache;
				m_disabledOrArchivedCount = 0;
				m_visibleCount = 0;
				m_mineCount = 0;
				m_foundCount = 0;
				
				if (m_app.AppConfig.ImperialUnits)
				{
					m_distanceColumn.Title = Catalog.GetString("Mi");
					distanceLabel.Text = Catalog.GetString("Mi");
				}
				else
				{
					m_distanceColumn.Title = Catalog.GetString("km");
					distanceLabel.Text = Catalog.GetString("km");
				}
				
				UpdateStatus ();
				
				CacheStore.GlobalFilters.AddFilterCriteria(FilterList.KEY_STATUS,
				                                           new bool[]{foundCheck.Active, notFoundCheck.Active, mineCheck.Active,
																		availCheck.Active, notAvailCheck.Active, archiveCheck.Active});
				cacheListTree.Model = null;
				if (m_list == null)
				{
					m_list = new CacheStoreModel();
				}
				else
				{
					m_list.Clear();
				}
				
				
				if (m_sort == null)
				{
					m_sort = new TreeModelSort(new TreeModelAdapter(m_list));
					m_sort.SetSortFunc (3, TitleCompare);
					m_sort.SetSortFunc (2, DistanceCompare);
					m_sort.SetSortFunc (0, SymbolCompare);
					m_sort.DefaultSortFunc = DistanceCompare;
				}
				CacheStore.ReadCache += HandleCacheStoreReadCache;
				CacheStore.Complete += HandleCacheStoreComplete;
				CacheStore.GetUnfilteredCaches(m_app.CentreLat, m_app.CentreLon, m_app.OwnerIDs.ToArray());
				CacheStore.ReadCache -= HandleCacheStoreReadCache;
				CacheStore.Complete -= HandleCacheStoreComplete;
			}
			catch (Exception e)
			{
				OCMApp.ShowException(e);
			}
		}
		
		private void UpdateStatus ()
		{
			if (m_app.CacheStore.CombinationFilter != null)
			{
				infoStatus.Markup = "<span fgcolor=\"blue\">A combination filter</span> is applied.";
				statusInfo.Sensitive = true;
				statusInfo.ShowAll();
			}
			else if (m_app.CacheStore.AdvancedFilters != null)
			{
				infoStatus.Markup = "<span fgcolor=\"blue\">Advanced filters</span> have been applied.";
				statusInfo.Sensitive = true;
				statusInfo.ShowAll();
			}
			else
			{
				statusInfo.Sensitive = false;
				statusInfo.HideAll();
			}
		}
		
		public void ApplyInitalQuickFilter(QuickFilter filter)
		{
			SetFilterFields (filter);
		}
		
		public void ApplyQuickFilter(QuickFilter filter)
		{
			SetFilterFields (filter);
			Refresh();
		}
		
		private void SetFilterFields (QuickFilter filter)
		{
			m_IgnoreStatusChange = true;
			foundCheck.Active = filter.Found;
			notFoundCheck.Active = filter.NotFound;
			mineCheck.Active = filter.Mine;
			availCheck.Active = filter.Available;
			notAvailCheck.Active = filter.Unavailable;
			archiveCheck.Active = filter.Archived;
			CheckAvailFlags();
			CheckFoundFlags();
			if (filter.Distance > 0)
				distanceEntry.Text = filter.Distance.ToString();
			else
				distanceEntry.Text = String.Empty;
			if (!String.IsNullOrEmpty(filter.NameFilter))
				nameEntry.Text = filter.NameFilter;
			else
				nameEntry.Text = String.Empty;
			UpdateNameFilter();
			UpdateDistanceFilter();
			m_app.CacheStore.AdvancedFilters = filter.AdvancedFilters;
			m_app.CacheStore.CombinationFilter = filter.ComboFilter;
			m_IgnoreStatusChange = false;
		}
		
		private static Geocache GetSelectedCache (TreeSelection selection)
		{
			TreeIter iter;
			TreeModel model;
			
			if (selection.GetSelected (out model, out iter)) 
				return (Geocache)model.GetValue (iter, 0);
			else
				return null;
		}

#region Popup Menu
		
		private void CreatePopup (Gtk.ButtonPressEventArgs args)
		{
			Menu popup = new Menu ();
			MenuItem setCenterItem = new MenuItem (Catalog.GetString("_Set As Map Centre"));
			MenuItem showOnline = new MenuItem (Catalog.GetString("_View Cache Online"));
			MenuItem mark = new MenuItem(Catalog.GetString("_Mark"));
			Menu markSub = new Menu();
			MenuItem markFound = new MenuItem(Catalog.GetString("Mark _Found"));
			MenuItem markFTF = new MenuItem(Catalog.GetString("Mark First To Find"));
			MenuItem markDNF = new MenuItem(Catalog.GetString("Mark Did Not Find"));
			MenuItem markUnfound = new MenuItem(Catalog.GetString("Mark _Unfound"));
			MenuItem markDisabled = new MenuItem(Catalog.GetString("Mark _Disabled"));
			MenuItem markArchived = new MenuItem(Catalog.GetString("Mark _Archived"));
			MenuItem markAvailable = new MenuItem(Catalog.GetString("Mark A_vailable"));
			MenuItem correctCoordinates = new MenuItem(Catalog.GetString("_Corrected Coordinates..."));
			MenuItem addWaypoint = new MenuItem(Catalog.GetString("Add Child _Waypoint..."));
			MenuItem deleteItem = new MenuItem (Catalog.GetString("Delete..."));
			MenuItem bookmark = new MenuItem(Catalog.GetString("_Add to Bookmark List"));
			MenuItem rmvCache = new MenuItem(Catalog.GetString("_Remove From Bookmark List"));
			MenuItem qlandkarte = new MenuItem(Catalog.GetString("View in _QLandkarte GT..."));
			MenuItem logCache = new MenuItem(Catalog.GetString("_Log Find"));
			MenuItem externalTools = new MenuItem(Catalog.GetString("_External Tools"));
			MenuItem grabImages = new MenuItem(Catalog.GetString("_Grab Images"));
			
			EToolList eTools = m_app.Tools;
			Menu eToolMenu = new Menu();
			foreach(ExternalTool tool in eTools.ToolArray)
			{
				if (tool.Command.Contains("%selected%") || tool.Command.Contains("%selectedname%"))
				{
					Gtk.Action eCmd = new Gtk.Action(tool.Name, tool.Name);
					eCmd.Activated += HandleECmdActivated;
					eToolMenu.Add(eCmd.CreateMenuItem());
				}
			}
			externalTools.Submenu = eToolMenu;
			
			
			TreePath path;
			TreeIter itr;
			cacheListTree.GetPathAtPos((int) args.Event.X,(int) args.Event.Y, out path);
			cacheListTree.Model.GetIter(out itr, path);
			Geocache cache = (Geocache)cacheListTree.Model.GetValue (itr, 0);
			
			if (cache != null)
			{
				logCache.Sensitive = true;
				if (!cache.Available)
				{
					markAvailable.Sensitive = true;
					markDisabled.Sensitive = false;
				}
				else
				{
					markAvailable.Sensitive = false;
					markDisabled.Sensitive = true;
				}
				
				if (!cache.Archived)
					markArchived.Sensitive = true;
				else
					markArchived.Sensitive = false;
				
				if (cache.Symbol.Contains("Found"))
				{
					if (cache.FTF)
					{
						markFTF.Sensitive = false;
						markFound.Sensitive = true;
					}
					else
					{
						markFTF.Sensitive = true;
						markFound.Sensitive = false;
					}
					markUnfound.Sensitive = true;
					markDNF.Sensitive = true;
					
				}
				else
				{
					markFound.Sensitive = true;
					markFTF.Sensitive = true;
					if (cache.DNF)
					{
						markDNF.Sensitive = false;
						markUnfound.Sensitive = true;
					}
					else
					{
						markDNF.Sensitive = true;
						markUnfound.Sensitive = false;
					}
				}
			}
			else
			{
				logCache.Sensitive = false;
			}
			
			string[] bookmarklists = m_app.CacheStore.GetBookmarkLists();
			if (bookmarklists.Length > 0)
			{
				Menu bookMarksSub = new Menu();
				foreach (String str in bookmarklists)
				{
					MenuItem itm = new MenuItem(str);
					if (str == m_app.CacheStore.ActiveBookmarkList)
						itm.Sensitive = false;
					bookMarksSub.Append(itm);
					itm.Activated += HandleItmActivated;
				}
				bookmark.Submenu = bookMarksSub;
			}		
			else
			{
				bookmark.Sensitive = false;
			}
			
			if (m_app.CacheStore.ActiveBookmarkList == null)
				rmvCache.Sensitive = false;
			rmvCache.Activated += HandleRmvCacheActivated;
			
			setCenterItem.Activated += HandleSetCenterItemActivated;
			showOnline.Activated += HandleShowOnlineActivated;
			deleteItem.Activated += HandleDeleteItemActivated;
			markFound.Activated += HandleMarkFoundActivated;
			markUnfound.Activated += HandleMarkUnfoundActivated;
			markDisabled.Activated += HandleMarkDisabledActivated;
			markArchived.Activated += HandleMarkArchivedActivated;
			markAvailable.Activated += HandleMarkAvailableActivated;
			correctCoordinates.Activated += HandleCorrectCoordinatesActivated;
			addWaypoint.Activated += HandleAddWayPointActivated;
			qlandkarte.Activated += HandleQlandkarteActivated;
			logCache.Activated += HandleLogCacheActivated;
			markDNF.Activated += HandleMarkDNFActivated;
			markFTF.Activated += HandleMarkFTFActivated;
			grabImages.Activated += HandleGrabImagesActivated;
			
			popup.Add (setCenterItem);
			popup.Add (showOnline);
			popup.Add (new MenuItem());
			popup.Add (logCache);
			popup.Add (mark);
			markSub.Add(markFound);
			markSub.Add(markFTF);
			markSub.Add(markDNF);
			markSub.Add(markUnfound);
			markSub.Add (markDisabled);
			markSub.Add (markArchived);
			markSub.Add (markAvailable);
			mark.Submenu = markSub;
			popup.Add(addWaypoint);			          
			popup.Add(correctCoordinates);
			popup.Add(grabImages);
			popup.Add(new MenuItem());
			popup.Add(externalTools);
			popup.Add (new MenuItem());
			popup.Add (bookmark);
			popup.Add (rmvCache);
			popup.Add (new MenuItem());
			popup.Add (qlandkarte);
			popup.Add (deleteItem);
			popup.ShowAll ();
			popup.Popup ();
		}

		void HandleGrabImagesActivated (object sender, EventArgs e)
		{
			m_Win.CacheInfo.GrabImages();
		}

		void HandleECmdActivated (object sender, EventArgs e)
		{
			m_app.Tools.GetTool((sender as Gtk.Action).Name).RunCommand(m_Win);
		}

		void HandleMarkFTFActivated (object sender, EventArgs e)
		{
			m_Win.MarkAsFTF();
		}

		void HandleMarkDNFActivated (object sender, EventArgs e)
		{
			m_Win.MarkAsDNF();
		}

		void HandleLogCacheActivated (object sender, EventArgs e)
		{
			m_Win.LogFind();
		}

		void HandleMarkUnfoundActivated (object sender, EventArgs e)
		{
			m_Win.MarkUnfound();
		}

		void HandleMarkFoundActivated (object sender, EventArgs e)
		{
			m_Win.MarkAsFound();
		}

		void HandleQlandkarteActivated (object sender, EventArgs e)
		{
			m_Win.OpenSelectedCacheInQLandKarte();
		}

		void HandleRmvCacheActivated (object sender, EventArgs e)
		{
			Geocache cache = m_Win.CacheList.SelectedCache;
			m_app.CacheStore.RemoveBookmarkEntry(m_app.CacheStore.ActiveBookmarkList, cache.Name);	
			Refresh();
		}

		void HandleItmActivated (object sender, EventArgs e)
		{
			Geocache cache = m_Win.CacheList.SelectedCache;
			m_app.CacheStore.AddBoormarkEntry(((sender as MenuItem).Child as Label).Text, cache.Name);	
		}

		void HandleMarkAvailableActivated (object sender, EventArgs e)
		{
			m_Win.MarkAvailable();
		}

		void HandleMarkArchivedActivated (object sender, EventArgs e)
		{
			m_Win.MarkArchived();
		}

		void HandleMarkDisabledActivated (object sender, EventArgs e)
		{
			m_Win.MarkDisabled();
		}
		
		void HandleCorrectCoordinatesActivated (object sender, EventArgs e)
		{
			m_Win.CorrectCoordinates();
		}

		void HandleAddWayPointActivated (object sender, EventArgs e)
		{
			m_app.AddChildWaypoint();
		}

		void HandleDeleteItemActivated (object sender, EventArgs e)
		{
			m_Win.DeleteSelectedCache();
		}

		void HandleShowOnlineActivated (object sender, EventArgs e)
		{
			Process.Start (m_Win.CacheList.SelectedCache.URL.ToString ());
		}

		void HandleSetCenterItemActivated (object sender, EventArgs e)
		{
			Geocache cache = m_Win.CacheList.SelectedCache;
			m_Win.SetLocation(cache.Name, cache.Lat, cache.Lon);
		}
		
#endregion
		
#region Events
		void HandleCacheStoreComplete (object sender, EventArgs args)
		{
			int totalCount = CacheStore.CacheCount;
			if (null != this.RefreshEnd)
				RefreshEnd(this, new RefreshCompleteArgs(m_visibleCount, 
				                                         m_foundCount, m_disabledOrArchivedCount, m_mineCount, 
				                                         totalCount));
			if (!m_list.isSorted())
				m_list.Resort(m_app.CentreLat, m_app.CentreLon);
			//TEMP: Probably better way to do this then recreate sort model
			m_sort = new TreeModelSort(new TreeModelAdapter(m_list));
			m_sort.SetSortFunc (3, TitleCompare);
			m_sort.SetSortFunc (2, DistanceCompare);
			m_sort.SetSortFunc (1, CodeCompare);
			m_sort.SetSortFunc (0, SymbolCompare);
			m_sort.DefaultSortFunc = DistanceCompare;
			cacheListTree.Model = m_sort;
		}

		void HandleCacheStoreReadCache (object sender, ReadCacheArgs args)
		{
			m_list.Add(args.Cache);
			if (!args.Cache.Available)
				m_disabledOrArchivedCount++;
			else if (args.Cache.Archived)
				m_disabledOrArchivedCount++;
			if (args.Cache.Found)
				m_foundCount++;
			if (m_app.OwnerIDs.Contains(args.Cache.OwnerID) || m_app.OwnerIDs.Contains(args.Cache.PlacedBy))
				m_mineCount++;
			m_visibleCount++;
			if (null != this.RefreshPulse)
				this.RefreshPulse(this, new CacheEventArgs(args.Cache));
		}
		
		protected virtual void OnAvailStatusToggle (object sender, System.EventArgs e)
		{
			if (m_IgnoreStatusChange)
				return;
			CheckAvailFlags ();		
			Refresh();
		}
		
		private void CheckAvailFlags ()
		{
				if (availCheck.Active == false && notAvailCheck.Active == false)
				archiveCheck.Sensitive = false;
			else
				archiveCheck.Sensitive = true;
			if (availCheck.Active == false && archiveCheck.Active == false)
				notAvailCheck.Sensitive = false;
			else
				notAvailCheck.Sensitive = true;
			if (notAvailCheck.Active == false && archiveCheck.Active == false)
				availCheck.Sensitive = false;
			else
				availCheck.Sensitive = true;	
		}
		
		
		
		protected virtual void OnFoundStatusToggle (object sender, System.EventArgs e)
		{
			if (m_IgnoreStatusChange)
				return;
			CheckFoundFlags ();		
			Refresh();
		}
		
		private void CheckFoundFlags ()
		{
			if (foundCheck.Active == false && notFoundCheck.Active == false)
				mineCheck.Sensitive = false;
			else
				mineCheck.Sensitive = true;
			if (foundCheck.Active == false && mineCheck.Active == false)
				notFoundCheck.Sensitive = false;
			else
				notFoundCheck.Sensitive = true;
			if (notFoundCheck.Active == false && mineCheck.Active == false)
				foundCheck.Sensitive = false;
			else
				foundCheck.Sensitive = true;	
		}
				
		void HandleCacheListTreeSelectionChanged (object sender, EventArgs e)
		{
			m_Selected = GetSelectedCache(sender as TreeSelection);
			if (this.SelectionChanged != null)
			{
				this.SelectionChanged(this, new CacheEventArgs(m_Selected));
			}
		}
		
		void HandleRefreshTimerElapsed (object sender, ElapsedEventArgs e)
		{
			Application.Invoke (delegate { Refresh (); });
		}
		
		protected virtual void OnChanged (object sender, System.EventArgs e)
		{
			if (m_IgnoreStatusChange)
				return;
			UpdateDistanceFilter ();
			UpdateNameFilter ();
			this.QueueDraw();
		}
		
		private void UpdateDistanceFilter ()
		{
			if (!String.IsNullOrEmpty(distanceEntry.Text))
			{
				try
				{
					double dist = double.Parse(distanceEntry.Text);
					if (m_app.AppConfig.ImperialUnits)
						dist = Utilities.MilesToKm(dist);
					CacheStore.GlobalFilters.AddFilterCriteria(FilterList.KEY_DIST, dist);
					clearDistanceButton.Sensitive = true;
				}
				catch
				{
					MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
					                                      Catalog.GetString("Value is not a number"));
					dlg.Run();
					dlg.Hide();
					dlg.Dispose();
					distanceEntry.Changed -= OnChanged;
					distanceEntry.Text = string.Empty;
					distanceEntry.Changed += OnChanged;;
					return;
				}
			}
			else
			{
				CacheStore.GlobalFilters.RemoveCriteria(FilterList.KEY_DIST);
				clearDistanceButton.Sensitive = false;
			}
		}
		
		private void UpdateNameFilter ()
		{
			if (!String.IsNullOrEmpty(nameEntry.Text))
			{
				CacheStore.GlobalFilters.AddFilterCriteria(FilterList.KEY_CACHE_NAME, nameEntry.Text);
				clearNameButton.Sensitive = true;
			}
			else
			{
				CacheStore.GlobalFilters.RemoveCriteria(FilterList.KEY_CACHE_NAME);
				clearNameButton.Sensitive = false;
			}
		}
		
		protected virtual void OnClearNameClick (object sender, System.EventArgs e)
		{
			nameEntry.Text = String.Empty;
			Refresh();
		}
		
		protected virtual void OnClearDistClicked (object sender, System.EventArgs e)
		{
			distanceEntry.Text = String.Empty;
			Refresh();
		}
		
	
		[GLib.ConnectBeforeAttribute]
		protected virtual void OnButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 3) {
				CreatePopup (args);
			}
		}
		

		protected virtual void OnStatusClick (object sender, System.EventArgs e)
		{
			if (m_Win.App.CacheStore.CombinationFilter != null)
				m_Win.AddComboFilter();
			else if (m_Win.App.CacheStore.AdvancedFilters != null)
				m_Win.AddAdvancedFilter();
			
		}
		
		protected virtual void OnNameActivated (object sender, System.EventArgs e)
		{
			Refresh();
		}
	
		protected virtual void OnDistActivated (object sender, System.EventArgs e)
		{
			Refresh();
		}
#endregion				
	}	

}
