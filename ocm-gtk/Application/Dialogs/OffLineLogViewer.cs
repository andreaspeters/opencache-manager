// 
//  Copyright 2011  Kyle Campbell
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
using Gtk;
using ocmengine;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Mono.Unix;

namespace ocmgtk
{

	/// <summary>
	/// Field Notes Viewer Dialog
	/// </summary>
	public partial class OffLineLogViewer : Gtk.Dialog
	{
		// Private Members
		// ----------------
		private ListStore m_logList;
		private List<CacheLog> m_Logs;
		private TreeModelSort listSort;
		private OCMMainWindow m_Win;
		private Dictionary<string, Geocache> m_caches;
		private Dictionary<string, Waypoint> m_waypoints = new Dictionary<string, Waypoint>();
		private CacheLog m_currLog;
		bool hasUnsaved = false;
		bool needsRefresh = false;
		
		/// <summary>
		/// If true, after this dialog closes refresh the main cache list, as changes to the 
		/// cache have been done in this dialog
		/// </summary>
		public bool NeedsRefresh
		{
			get{return needsRefresh;}
			set{needsRefresh = value;}
		}
		
		
		/// <summary>
		/// Main Constructor
		/// </summary>
		/// <param name="win">
		/// Reference to OCMMainWindow that hosts this dialog <see cref="OCMMainWindow"/>
		/// </param>
		public OffLineLogViewer (OCMMainWindow win)
		{
			this.Build ();
			this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			BuildLogTreeWidget();
			m_Win = win;
			mapView.App = win.App;
			mapView.HideFindSymbol = true;
			mapView.Reload();
			m_caches = new Dictionary<string, Geocache>();
			fnFieldNotesLabel.Text = String.Format(Catalog.GetString("File Location: {0}"), m_Win.App.AppConfig.FieldNotesFile);
			waypointCombo.Changed += HandleWaypointComboChanged;
			cacheNotes.MainWin = win;
			this.ShowAll();
		}
		
		
		/// <summary>
		/// Populates the list of logs contained in the field notes file
		/// </summary>
		/// <param name="logs">
		/// A list of cache logs<see cref="List<CacheLog>"/>
		/// </param>	
		public void PopulateLogs (List<CacheLog> logs)
		{
			this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			this.Sensitive = false;
			OCMApp.UpdateGUIThread();
			m_logList.Clear ();
			m_caches.Clear ();
			fieldNotesDescPane.Sensitive = false;
			viewCacheButton.Sensitive = false;
			deleteButton.Sensitive = false;
			m_Logs = logs;
			listSort.SetSortColumnId (0, SortType.Descending);
			List<String> caches = new List<String> ();
			foreach (CacheLog log in logs)
			{
				m_logList.AppendValues (log);
				caches.Add (log.CacheCode);
			}
			List<Geocache> cachesInDb = m_Win.App.CacheStore.GetCachesByName (caches.ToArray ());		
			foreach(Geocache cache in cachesInDb)
			{
				m_caches[cache.Name] = cache;
			}
			this.Sensitive = true;
			this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Arrow);
		}
		
		/// <summary>
		/// Creates the log list widget
		/// </summary>
		private void BuildLogTreeWidget()
		{	
			
			m_logList = new ListStore (typeof(CacheLog));
			
			CellRendererText code_cell = new CellRendererText ();
			CellRendererText name_cell = new CellRendererText ();
			CellRendererText date_cell = new CellRendererText ();
			CellRendererPixbuf type_cell = new CellRendererPixbuf ();
			TreeViewColumn log_icon = new TreeViewColumn (Catalog.GetString ("Type"), type_cell);
			log_icon.SortColumnId = 3;
			TreeViewColumn cache_code = new TreeViewColumn (Catalog.GetString ("Code"), code_cell);
			cache_code.SortColumnId = 1;
			TreeViewColumn cache_name = new TreeViewColumn (Catalog.GetString ("Name"), name_cell);
			cache_name.SortColumnId = 2;
			TreeViewColumn log_date = new TreeViewColumn (Catalog.GetString ("Date"), date_cell);
			log_date.SortColumnId = 0;
			
			cache_code.SetCellDataFunc (code_cell, new TreeCellDataFunc(RenderCode));
			log_date.SetCellDataFunc (date_cell, new TreeCellDataFunc(RenderDate));
			log_icon.SetCellDataFunc(type_cell, new TreeCellDataFunc(RenderType));
			cache_name.SetCellDataFunc (name_cell, new TreeCellDataFunc(RenderName));	
			
			logView.AppendColumn (log_icon);
			logView.AppendColumn (cache_code);
			logView.AppendColumn (log_date);
			logView.AppendColumn (cache_name);
			
			listSort = new TreeModelSort(m_logList);
			
			listSort.SetSortFunc (1, TypeCompare);
			listSort.SetSortFunc (0, DateCompare);
			logView.Model = listSort;
			logView.Selection.Changed += HandleLogViewSelectionChanged;
			
		}

		/// <summary>
		/// Sets the log type combo box value based on the current log
		/// </summary>		
		void SetLogChoice()
		{
			if (m_currLog.LogStatus == "Found it")
				logChoice.Active = 0;
			else if (m_currLog.LogStatus == "Didn't find it")
				logChoice.Active = 1;
			else if (m_currLog.LogStatus == "Write note")
				logChoice.Active = 2;
			else 
				logChoice.Active = 3;
		}
		
		/// <summary>
		/// Updates the current cache and field notes file
		/// </summary>
		void SaveLogChanges ()
		{
			try
			{
				m_currLog.LogMessage = logEntry.Buffer.Text;
				m_Win.App.CacheStore.AddLog(m_currLog.CacheCode, m_currLog);
				if (m_caches.ContainsKey(m_currLog.CacheCode))
				{
					m_Win.App.CacheStore.AddWaypointOrCache(m_caches[m_currLog.CacheCode], false, false);
				}
		 		UpdateFNFile();
			}
			catch (Exception e)
			{
				OCMApp.ShowException(e);
			}
		}

		/// <summary>
		/// Writes out an updated field notes file
		/// </summary>
		private void UpdateFNFile()
		{
			string fnFile = m_Win.App.AppConfig.FieldNotesFile;
			FieldNotesHandler.ClearFieldNotes(fnFile);
			FieldNotesHandler.WriteToFile(m_Logs, fnFile);
		}
		
		/// <summary>
		/// Updates all info panes with the current cache information
		/// </summary>
		/// <param name="val">
		/// A geocache object <see cref="CacheLog"/>
		/// </param>
		private void UpdateCacheInfo (CacheLog val)
		{
			Geocache cache = m_caches[val.CacheCode];
			StringBuilder builder = new StringBuilder();
			builder.Append("<b>");
			builder.Append(cache.Name);
			builder.Append(":");
			builder.Append(cache.CacheName);
			if (!String.IsNullOrEmpty(cache.PlacedBy))
			{
				builder.Append(Catalog.GetString(" by "));
				builder.Append(cache.PlacedBy);
			}
			builder.Append("</b><br/>");
			builder.Append(Geocache.GetCTypeString(cache.TypeOfCache));
			builder.Append("<br/><br/>");
			builder.Append(cache.ShortDesc);
			builder.Append("<br/><br/>");
			builder.Append(cache.LongDesc);
			
			cacheDesc.HTML = builder.ToString();
			List<CacheLog> logs =  m_Win.App.CacheStore.GetCacheLogs(cache.Name);
			builder = new StringBuilder();
			foreach(CacheLog log in logs)
			{
				builder.Append(log.toHTML());
			}
			cacheLog.HTML = builder.ToString();
			mapView.ClearCaches();
			mapView.AddMarker(cache, false);
			List<Waypoint> children = m_Win.App.CacheStore.GetChildWaypoints(new string[]{cache.Name});
			ListStore cmodel = waypointCombo.Model as ListStore;
			m_waypoints.Clear();
			cmodel.Clear();
			waypointCombo.AppendText(cache.Name);
			cacheNotes.SetCache(cache);
			firstToFindCheck.Toggled -= OnFTFCheck;
			firstToFindCheck.Active = cache.FTF;
			firstToFindCheck.Toggled += OnFTFCheck;
			if (val.LogStatus == "Found it")
				firstToFindCheck.Sensitive = true;
			else
				firstToFindCheck.Sensitive = false;
			logChoice.Changed -= OnLogTypeChange;
			SetLogChoice();
			logChoice.Changed += OnLogTypeChange;
			m_waypoints.Add(cache.Name, cache);
			foreach(Waypoint pt in children)
			{
				waypointCombo.AppendText(pt.Name);
				m_waypoints.Add(pt.Name, pt);
			}
			waypointCombo.Active = 0;
		}
		
		/// <summary>
		/// Warns the user about unsaved changes when switching selected field note
		/// </summary>
		private void HandleUnsavedChanges ()
		{
			MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, 
					                                      Catalog.GetString("You have unsaved changes that will be lost. Do you wish to save?\n"));
			if ((int) (ResponseType.Yes) == dlg.Run())
			{
				SaveLogChanges ();
			}
			else
			{
				hasUnsaved = false;
				PopulateLogs(FieldNotesHandler.GetLogs(m_Win.App.AppConfig.FieldNotesFile, m_Win.App.OwnerIDs[0]));
			
			}
			dlg.Hide();
			dlg.Dispose();
			hasUnsaved = false;
		}
		
#region Log List Rendering Code
		private void RenderType (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CacheLog log = (CacheLog)model.GetValue (iter, 0);
			CellRendererPixbuf icon = cell as CellRendererPixbuf;
			if (log.LogStatus == "Found it")
				icon.Pixbuf = IconManager.FOUNDICON_S;
			else if (log.LogStatus == "Didn't find it")
				icon.Pixbuf = IconManager.DNF_S;
			else if (log.LogStatus == "Needs Maintenance")
				icon.Pixbuf = IconManager.NEEDS_MAINT_S;
			else
				icon.Pixbuf = IconManager.WRITENOTE_S;
		}
		
		private void RenderCode (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CacheLog log = (CacheLog)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = log.CacheCode;
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CacheLog log = (CacheLog)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			
			if (m_caches.ContainsKey(log.CacheCode))
				text.Text = m_caches[log.CacheCode].CacheName;
			else
				text.Text = Catalog.GetString("<Name Unavailable>");
		}
		
		
		private void RenderDate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CacheLog log = (CacheLog)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = log.LogDate.ToShortDateString() + " " + log.LogDate.ToShortTimeString();
		}
		
		private int DateCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			CacheLog logA = (CacheLog)model.GetValue (tia, 0);
			CacheLog logB = (CacheLog)model.GetValue (tib, 0);
			if (logA == null || logB == null)
				return 0;
			if (logA.LogDate > logB.LogDate)
				return 1;
			else if (logA.LogDate == logB.LogDate)
				return 0;
			else
				return -1;
		}
		
		private int TypeCompare (TreeModel model, TreeIter tia, TreeIter tib)
		{
			CacheLog logA = (CacheLog)model.GetValue (tia, 0);
			CacheLog logB = (CacheLog)model.GetValue (tib, 0);
			if (logA == null || logB == null)
				return 0;
			return logA.LogStatus.CompareTo(logB.LogStatus);
		}
#endregion
		
#region Event Handlers
		void HandleLogEntryBufferChanged (object sender, EventArgs e)
		{
			saveButton.Sensitive = true;
			hasUnsaved = true;
		}
		
		void HandleLogViewSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;			
			if (((TreeSelection)sender).GetSelected (out model, out iter)) {
				CacheLog val = (CacheLog)model.GetValue (iter, 0);
				if (hasUnsaved)
					HandleUnsavedChanges ();
				if (val == null)
				{
					fieldNotesDescPane.Sensitive = false;
					viewCacheButton.Sensitive = false;
					deleteButton.Sensitive = false;
					return;
				}
				
				fieldNotesDescPane.Sensitive = true;
				viewCacheButton.Sensitive = true;
				deleteButton.Sensitive = true;
				m_currLog = val;
				if (m_caches.ContainsKey(val.CacheCode))
				{
					UpdateCacheInfo (val);
				}
				else
				{
					cacheDesc.HTML = Catalog.GetString("<b>Information Unavailable. This cache is not in your OCM database.</b>");
					mapView.ClearCaches();
					firstToFindCheck.Sensitive = false;
				}
				logEntry.Buffer.Changed -= HandleLogEntryBufferChanged;
				logEntry.Buffer.Text = val.LogMessage;
				logEntry.Buffer.Changed += HandleLogEntryBufferChanged;
				deleteButton.Sensitive = true;
				viewCacheButton.Sensitive = true;
				logPane.Sensitive = true;
				saveButton.Sensitive = false;
			} else {
				deleteButton.Sensitive = false;
				viewCacheButton.Sensitive = false;
				fieldNotesDescPane.Sensitive = false;
			}
		}

		protected virtual void OnCloseClick (object sender, System.EventArgs e)
		{
			if (hasUnsaved)
			{
				MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, 
					                                      Catalog.GetString("You have unsaved changes that will be lost. Do you wish to save?\n"));
				if ((int) (ResponseType.Yes) == dlg.Run())
				{
					SaveLogChanges();
				}
				dlg.Hide();
			}
			m_currLog = null;
			m_caches = null;
			this.Hide();
			if (needsRefresh)
				m_Win.RefreshCacheList();
		}	
		
		protected virtual void OnDeleteClick (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (logView.Selection.GetSelected (out model, out itr)) 
			{
				CacheLog log = (CacheLog)model.GetValue (itr, 0);
				MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo,
				                                      Catalog.GetString("Are you sure you want to remove the field note for '{0}'?"),
				                                      log.CacheCode);
				if ((int) ResponseType.Yes == dlg.Run())
				{
					m_Logs.Remove(log);
					PopulateLogs(m_Logs);
					UpdateFNFile();
					logEntry.Buffer.Text = String.Empty;
				}
				dlg.Hide();
				dlg.Dispose();
			}
		}
		protected virtual void OnDeleteAllClick (object sender, System.EventArgs e)
		{
			MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo,
					                                      Catalog.GetString("Are you sure you want to remove all field notes?"));
			try
			{
				if ((int) ResponseType.Yes == dlg.Run())
				{
					FieldNotesHandler.ClearFieldNotes(m_Win.App.AppConfig.FieldNotesFile);
					m_Logs.Clear();
					PopulateLogs(m_Logs);
				}
				dlg.Hide();
				dlg.Dispose();
			}
			catch (Exception e1)
			{
				dlg.Hide();
				OCMApp.ShowException(e1);
			}
		}
		
		protected virtual void OnViewCache (object sender, System.EventArgs e)
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (logView.Selection.GetSelected (out model, out itr)) 
			{
				CacheLog log = (CacheLog)model.GetValue (itr, 0);
				if (!m_Win.CacheList.ContainsCode(log.CacheCode))
				{
					MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, 
					                                      Catalog.GetString("'{0}' is not within the list of unfiltered caches. Your filter settings may have filtered it out or it may not be in your database."),
					                                      log.CacheCode);
					dlg.Run();
					dlg.Hide();
					dlg.Dispose();
					return;
				}
				m_Win.CacheList.SelectCacheByName(log.CacheCode);
				this.Hide();
				this.Dispose();
			}
		}
		
		void HandleWaypointComboChanged (object sender, EventArgs e)
		{
			if (waypointCombo.Active >= 0)
			{
				Waypoint pt = m_waypoints[waypointCombo.ActiveText];	
				mapView.PanTo(pt.Lat, pt.Lon);
			}
		}
		
		protected virtual void OnSaveClick (object sender, System.EventArgs e)
		{
			SaveLogChanges();
			hasUnsaved = false;
			saveButton.Sensitive =false;
			this.QueueDraw();
		}
		
		protected virtual void OnFTFCheck (object sender, System.EventArgs e)
		{
			if (m_caches.ContainsKey(m_currLog.CacheCode))
			{
				Geocache cache = m_caches[m_currLog.CacheCode];
				cache.FTF = firstToFindCheck.Active;
				hasUnsaved = true;
				saveButton.Sensitive = true;
				needsRefresh = true;
			}	
		}
		
		protected virtual void OnLogTypeChange (object sender, System.EventArgs e)
		{
			switch (logChoice.Active)
			{
				case 0:
					m_currLog.LogStatus = "Found it";
					if (m_caches.ContainsKey(m_currLog.CacheCode))
					{
						m_caches[m_currLog.CacheCode].Symbol = "Geocache Found";
						m_caches[m_currLog.CacheCode].FTF = false;
						m_caches[m_currLog.CacheCode].DNF = false;
						firstToFindCheck.Sensitive = true;
					}
					break;
				case 1:
					m_currLog.LogStatus = "Didn't find it";
					if (m_caches.ContainsKey(m_currLog.CacheCode))
					{
						m_caches[m_currLog.CacheCode].Symbol = "Geocache";
						m_caches[m_currLog.CacheCode].FTF = false;
						m_caches[m_currLog.CacheCode].DNF = true;
						firstToFindCheck.Sensitive = false;
					}
					break;
				case 2:
					m_currLog.LogStatus = "Write note";
					firstToFindCheck.Sensitive = false;
					break;
				default:
					m_currLog.LogStatus = "Needs Maintenance";
					firstToFindCheck.Sensitive = false;
					break;
			}
			hasUnsaved = true;
			needsRefresh = true;	
			saveButton.Sensitive = true;
		}
#endregion
	}
}
