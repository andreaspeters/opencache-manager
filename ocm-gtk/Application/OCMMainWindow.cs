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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Unix;
using ocmgtk.printing;
using ocmengine;
using DBus;
using Gtk;
using Gdk;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ocmgtk
{


	public partial class OCMMainWindow : Gtk.Window
	{
		private OCMApp m_app = null;
		private int m_pulseCount = 0;

		static Cursor WAITCURSOR = new Cursor(CursorType.Watch);
		static Cursor ARROWCURSOR = new Cursor(CursorType.Arrow);

		public CacheListWidget CacheList
		{
			get { return cacheList;}
		}

		public MapWidget CacheMap
		{
			get { return ocmMapWidget;}
		}

		private IConfig Config
		{
			get { return m_app.AppConfig;}
		}

		public CacheInfoWidget CacheInfo
		{
			get { return ocmCacheInfo;}
		}

		public OCMApp App
		{
			get { return m_app;}
		}

		/// <summary>
		/// Used for GUI Builder only.
		/// </summary>
		public OCMMainWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}

		public OCMMainWindow (OCMApp app) : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			m_app = app;
			cacheList.App = app;
			CacheMap.App = app;
			ocmCacheInfo.MainWin = this;
			ocmMapWidget.MainWin = this;
			m_app.Bookmarks.MainWin = this;
			m_app.Profiles.MainWin = this;
			m_app.Tools.MainWin = this;
			CacheList.MainWin = this;
			LogToFieldNotesFileAction.Active = Config.UseOfflineLogging;
			MapPopupsAction.Active = Config.MapPopups;
			CacheList.RefreshStart += HandleCacheListRefreshStart;
			CacheList.RefreshEnd += HandleCacheListRefreshEnd;
			CacheList.RefreshPulse += HandleCacheListRefreshPulse;
			CacheList.SelectionChanged += HandleCacheListSelectionChanged;
			ShowNearbyCachesAction.Active = Config.ShowNearby;
			AllWaypointsAction.Active = Config.ShowAllChildren;
			printAction.Sensitive = false;
			m_app.Locations.Window = this;
			m_app.QuickFilterList.Window = this;
			RebuildLocations();
			RebuildQuickFilters();
			RebuildBookmarks();
			RebuildProfiles();
			RebuildTools();
			UpdateLocationLabel();
			UpdateMapButtons();
			this.Resize(m_app.AppConfig.WindowWidth, m_app.AppConfig.WindowHeight);
			this.mainHPane.Position = m_app.AppConfig.HBarPosition;
			this.mainVPane.Position = m_app.AppConfig.VBarPosition;

			if (m_app.Profiles.GetActiveProfile () != null)
				SetLastGPS (m_app.Profiles.GetActiveProfile ().Name, m_app.Profiles.GetActiveProfile ().FieldNotesFile != null);
			else
				SetLastGPS (null, false);

		}

		public void RebuildProfiles()
		{
			Menu transferSub = m_app.Profiles.BuildProfileTransferMenu();
			Menu receiveSub = m_app.Profiles.BuildProfileReceiveMenu();
			Menu editSub = m_app.Profiles.BuildProfileEditMenu();
			(TransferCachesAction.Proxies[0] as MenuItem).Submenu = transferSub;
			(ReceiveFieldNotesAction.Proxies[0] as MenuItem).Submenu = receiveSub;
			(EditGPSProfileAction.Proxies[0] as MenuItem).Submenu = editSub;
		}


		private void RebuildLocations()
		{
			Menu locationSubMenu = m_app.Locations.BuildLocationlMenu();
			(LocationsAction.Proxies[0] as MenuItem).Submenu = locationSubMenu;
		}

		private void RebuildQuickFilters()
		{
			Menu qfiltersMenu = m_app.QuickFilterList.BuildQuickFilterMenu();
			(QuickFilterAction.Proxies[0] as MenuItem).Submenu = qfiltersMenu;
		}

		private void RebuildBookmarks()
		{
			Menu bookmarksMenu = m_app.Bookmarks.BuildBookmarkMenu();
			(BookmarkListAction.Proxies[0] as MenuItem).Submenu = bookmarksMenu;
			Menu addAllMenu = m_app.Bookmarks.BuildAddToMenu(HandleAllToBookmark);
			(AddAllUnfilteredCachesToAction.Proxies[0] as MenuItem).Submenu = addAllMenu;
			Menu addSelMenu = m_app.Bookmarks.BuildAddToMenu(HandleSelectedToBookmark);
			(AddSelectedCacheToAction.Proxies[0] as MenuItem).Submenu = addSelMenu;
		}

		private void RebuildTools()
		{
			Menu toolsMenu = m_app.Tools.BuildEToolMenu();
			(ExternalToolsAction.Proxies[0] as MenuItem).Submenu = toolsMenu;
		}


		public void SaveWindowSettings()
		{
			Config.HBarPosition = mainHPane.Position;
			Config.VBarPosition = mainVPane.Position;
		}

		public void UpdateCounts(int visible, int found, int disabled, int mine, int total)
		{
			if (!String.IsNullOrEmpty(m_app.CacheStore.ActiveBookmarkList))
				mainStatusBar.Push (mainStatusBar.GetContextId ("count"),
			                  String.Format (Catalog.GetString ("Showing {0} of {1} caches from {2}, {3} found, {4} unavailable/archived, {5} placed by me"),
			                                   visible, total, m_app.CacheStore.ActiveBookmarkList, found, disabled, mine));
			else
				mainStatusBar.Push (mainStatusBar.GetContextId ("count"),
			                  String.Format (Catalog.GetString ("Showing {0} of {1} caches, {2} found, {3} unavailable/archived, {4} placed by me"),
			                                   visible, total, found, disabled, mine));
		}

		public void LogFind ()
		{
			if (Config.UseOfflineLogging) {
				LogFindOffline ();
			} else {
				LogCacheOnline ();
			}
		}

		private void LogFindOffline ()
		{
			OfflineLogDialog dlg = new OfflineLogDialog ();
			dlg.MainWin = this;
			CacheLog log = new CacheLog ();
			log.CacheCode = CacheList.SelectedCache.Name;
			log.LogDate = m_app.LoggingDate;
			log.LogStatus = "Found it";
			log.LoggedBy = "OCM";
			log.LogKey = CacheList.SelectedCache.Name + "-ofl";
			log.LogMessage = String.Empty;
			dlg.Log = log;
			if ((int)ResponseType.Ok == dlg.Run ()) {
				log = dlg.Log;
				ProcessOfflineLog (CacheList.SelectedCache, log, dlg.FTF);
				dlg.Hide ();
			}
			dlg.Hide ();
			HandleCacheListSelectionChanged(this, new CacheEventArgs(CacheList.SelectedCache));
		}

		private void LogCacheOnline ()
		{
			MarkAsFound ();
			if (CacheList.SelectedCache.URL == null)
				return;
			LoggingDialog dlg = new LoggingDialog ();
			dlg.LogCache (CacheList.SelectedCache);
			dlg.Run ();
		}


		public bool CreateDB ()
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Create database"), this, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Save"), ResponseType.Accept);
			dlg.SetCurrentFolder (Config.DataDirectory);
			dlg.CurrentName = "newdb.ocm";
			FileFilter filter = new FileFilter ();
			filter.Name = "OCM Databases";
			filter.AddPattern ("*.ocm");
			dlg.AddFilter (filter);
			if (dlg.Run () == (int)ResponseType.Accept) {
				if (dlg.Filename == m_app.CacheStore.StoreName) {
					dlg.Hide ();
					MessageDialog mdlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, Catalog.GetString ("You cannot overwrite the " + "currently open database."));
					mdlg.Run ();
					mdlg.Hide ();
					return false;
				} else if (System.IO.File.Exists (dlg.Filename)) {
					dlg.Hide ();
					MessageDialog mdlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, Catalog.GetString ("Are you sure you want to overwrite '{0}'"), dlg.Filename);
					if ((int)ResponseType.No == mdlg.Run ()) {
						mdlg.Hide ();
						return false;
					} else {
						mdlg.Hide ();
						System.IO.File.Delete (dlg.Filename);
					}
				}
				dlg.Hide();
				RegisterRecentFile (dlg.Filename);
				m_app.SetDBFile(dlg.Filename, true);
				RebuildBookmarks();
				cacheList.Refresh();
				return true;
			}
			dlg.Destroy ();
			return false;
		}

		private void ShowOpenDBDialog ()
		{
			try {
				FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Open Database"), this,
				                                               FileChooserAction.Open,
				                                               Catalog.GetString ("Cancel"),
				                                               ResponseType.Cancel, Catalog.GetString ("Open"),
				                                               ResponseType.Accept);
				dlg.SetCurrentFolder (Config.DataDirectory);
				FileFilter filter = new FileFilter ();
				filter.Name = "OCM Databases";
				filter.AddPattern ("*.ocm");
				dlg.AddFilter (filter);

				if (dlg.Run () == (int)ResponseType.Accept) {
					dlg.Hide ();
					m_app.SetDBFile(dlg.Filename);
					RebuildBookmarks();
					cacheList.Refresh();
					dlg.Destroy ();
				} else {
					dlg.Hide ();
				}

			} catch (Exception) {
				return;
			}

		}

		public void SetLocation(Location loc)
		{
			SetLocation(loc.Name, loc.Latitude, loc.Longitude);
		}

		public void SetLocation(string name, double lat, double lon)
		{
			m_app.CenterName = name;
			m_app.CentreLat = lat;
			m_app.CentreLon = lon;
			ocmMapWidget.PanTo(m_app.CentreLat, m_app.CentreLon);
			UpdateLocationLabel();
			CacheList.Refresh();
		}

		public void ResetToHome()
		{
			SetLocation(Catalog.GetString("Home"), Config.HomeLat, Config.HomeLon);
		}

		private void UpdateLocationLabel()
		{
			LocationLabel.Text = String.Format (Catalog.GetString ("Centred on {0} ({1})"),
			                                    m_app.CenterName, Utilities.getCoordString (m_app.CentreLat, m_app.CentreLon));
		}
		public void CorrectCoordinates()
		{
			CorrectCoordinates(-1,-1);
		}

		public void CorrectCoordinates (double lat, double lon)
		{
			CorrectedCoordinatesDlg dlg = new CorrectedCoordinatesDlg();
			Geocache cache = CacheList.SelectedCache;
			dlg.SetCache(cache, m_app.AppConfig.UseDirectEntryMode);
			if (lat != -1)
				dlg.CorrectedLat = lat;
			if (lon != -1)
				dlg.CorrectedLon = lon;
			if ((int) ResponseType.Ok == dlg.Run())
			{
				if (dlg.IsCorrected)
				{
					cache.CorrectedLat = dlg.CorrectedLat;
					cache.CorrectedLon = dlg.CorrectedLon;
				}
				else
				{
					cache.ClearCorrectedFlag = true;
				}
				ModifyCache(cache);
				// Reselect the cache as it's position will change in the list
				CacheList.SelectCacheByName(cache.Name);
			}
		}

		public void ImportGPXFile (String filename, bool autoclose)
		{
			System.IO.FileStream fs = System.IO.File.OpenRead (filename);
			GPXParser parser = new GPXParser ();
			parser.IgnoreExtraFields = Config.ImportIgnoreExtraFields;
			parser.PreserveFound = Config.ImportPreventStatusOverwrite;
			parser.PurgeLogs = Config.ImportPurgeOldLogs;
			parser.CacheOwner = m_app.OwnerIDs;
			parser.Bookmark = Config.ImportBookmarkList;
			ProgressDialog pdlg = new ProgressDialog (parser);
			pdlg.AutoClose = autoclose;
			pdlg.Icon = this.Icon;
			pdlg.Modal = true;
			pdlg.Start (filename, m_app.CacheStore);
			CacheList.Refresh ();
			fs.Close ();
		}

		public void ImportZip (string filename)
		{
			String tempPath = System.IO.Path.GetTempPath ();
			ProcessStartInfo start = new ProcessStartInfo ();
			start.FileName = "unzip";
			start.Arguments = "-o \"" + filename + "\" -d " + tempPath + "ocm_unzip";
			Process unzip = Process.Start (start);

			while (!unzip.HasExited) {
				// Do nothing until exit
			}

			ImportDirectory (tempPath + "ocm_unzip", true, Config.AutoCloseWindows);
		}

		private void ImportDirectory (String path, bool delete, bool autoClose)
		{
			GPXParser parser = new GPXParser ();
			parser.Bookmark = Config.ImportBookmarkList;
			parser.IgnoreExtraFields = Config.ImportIgnoreExtraFields;
			parser.PreserveFound = Config.ImportPreventStatusOverwrite;
			parser.PurgeLogs = Config.ImportPurgeOldLogs;
			parser.CacheOwner = m_app.OwnerIDs;
			ProgressDialog pdlg = new ProgressDialog (parser);
			pdlg.Icon = this.Icon;
			pdlg.AutoClose = autoClose;
			pdlg.Modal = true;
			pdlg.StartMulti (path, m_app.CacheStore, delete);
			CacheList.Refresh ();
		}

		public void AddBookmark ()
		{
			AddBookMarkDialog dlg = new AddBookMarkDialog();
			if ((int)ResponseType.Ok == dlg.Run())
			{
				m_app.CacheStore.AddBookmarkList(dlg.BookmarkName);
				RebuildBookmarks();
			}
		}

		private Dictionary<string, string> GetExportMappings ()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!File.Exists (path + "/ocm/exportdef.oqf")) {
				return null;
			}
			FileStream fs = new FileStream (path + "/ocm/exportdef.oqf", FileMode.Open, FileAccess.Read);
			BinaryFormatter ser = new BinaryFormatter ();
			System.Object mappings = ser.Deserialize (fs);
			fs.Close ();
			return (Dictionary<string, string>)mappings;
		}

		private void SaveWaypointMappings (Dictionary<string, string> mapping)
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!Directory.Exists ("ocm"))
				Directory.CreateDirectory (path + "/ocm");
			path = path + "/ocm";
			BinaryFormatter ser = new BinaryFormatter ();
			FileStream fs = new FileStream (path + "/exportdef.oqf", FileMode.Create, FileAccess.ReadWrite);
			ser.Serialize (fs, mapping);
			fs.Close ();
		}

		public void ExportGPX ()
		{
			GPXWriter writer = new GPXWriter ();
			ExportProgressDialog edlg = new ExportProgressDialog (writer);
			edlg.AutoClose = Config.AutoCloseWindows;




			try {
				ExportGPXDialog dlg = new ExportGPXDialog ();
				dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
				dlg.CurrentName = "export.gpx";
				dlg.WaypointMappings = GetExportMappings ();
				FileFilter filter = new FileFilter ();
				filter.Name = "GPX Files";
				filter.AddPattern ("*.gpx");
				dlg.AddFilter (filter);
				dlg.UsePlainText = Config.ExportAsPlainText;
				dlg.UseMappings = Config.ExportCustomSymbols;
				dlg.IsPaperless = Config.ExportPaperlessOptions;
				dlg.IncludeChildren = Config.ExportChildren;
				dlg.NameMode = Config.ExportWaypointNameMode;
				dlg.DescMode = Config.ExportWaypointDescMode;
				dlg.IncludeAttributes = Config.ExportIncludeAttributes;
				dlg.LogLimit = Config.ExportLimitLogs;
				dlg.CacheLimit = Config.ExportLimitCaches;
				if (dlg.Run () == (int)ResponseType.Ok) {
					dlg.Hide ();
					writer.Limit = dlg.CacheLimit;
					writer.IncludeGroundSpeakExtensions = dlg.IsPaperless;
					writer.IncludeChildWaypoints = dlg.IncludeChildren;
					writer.UseOCMPtTypes = dlg.UseMappings;
					writer.NameMode = dlg.NameMode;
					writer.DescriptionMode = dlg.DescMode;
					if (dlg.UsePlainText)
						writer.HTMLOutput = HTMLMode.PLAINTEXT;
					writer.WriteAttributes = dlg.IncludeAttributes;
					writer.LogLimit = dlg.LogLimit;
					edlg.Icon = this.Icon;

					Config.ExportAsPlainText = dlg.UsePlainText;
					Config.ExportChildren = dlg.IncludeChildren;
					Config.ExportCustomSymbols = dlg.UseMappings;
					Config.ExportIncludeAttributes = dlg.IncludeAttributes;
					Config.ExportLimitCaches = dlg.CacheLimit;
					Config.ExportLimitLogs = dlg.LogLimit;
					Config.ExportPaperlessOptions = dlg.IsPaperless;
					Config.ExportWaypointDescMode = dlg.DescMode;
					Config.ExportWaypointNameMode = dlg.NameMode;
					SaveWaypointMappings (dlg.WaypointMappings);
					edlg.Start (dlg.Filename, CacheList.UnfilteredCaches, dlg.WaypointMappings, m_app.CacheStore);
					RecentManager manager = RecentManager.Default;
					manager.AddItem ("file://" + dlg.Filename);
				} else {
					edlg.Destroy ();
				}
				dlg.Destroy ();
			} catch (Exception e) {
				//ShowException (e);
				System.Console.WriteLine(e.Message);
				System.Console.WriteLine(e.StackTrace);
				edlg.Destroy ();
			}
		}

		public void ExportFindsGPX ()
		{
			GPXWriter writer = new GPXWriter ();
			writer.IsMyFinds = true;
			writer.MyFindsOwner = m_app.OwnerIDs[0];
			ExportProgressDialog edlg = new ExportProgressDialog (writer);
			edlg.AutoClose = Config.AutoCloseWindows;

			try {
				FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString (" Export Finds GPX File"), this, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Export"), ResponseType.Accept);
				dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
				dlg.CurrentName = "finds.gpx";
				FileFilter filter = new FileFilter ();
				filter.Name = "GPX Files";
				filter.AddPattern ("*.gpx");

				dlg.AddFilter (filter);

				if (dlg.Run () == (int)ResponseType.Accept) {
					dlg.Hide ();
					edlg.Icon = this.Icon;
					edlg.Start (dlg.Filename, m_app.CacheStore.GetFinds(), GPSProfileList.GetDefaultMappings(), m_app.CacheStore);
					RecentManager manager = RecentManager.Default;
					manager.AddItem ("file://" + dlg.Filename);
				} else {
					edlg.Destroy ();
				}
				dlg.Destroy ();
			} catch (Exception e) {
				OCMApp.ShowException(e);
				edlg.Hide ();
			}
		}


		public void ExportGarminPOI ()
		{
			ExportPOIDialog dlg = new ExportPOIDialog (Config);
			if ((int)ResponseType.Ok == dlg.Run ()) {
				GPSProfile poiProfile = new GPSProfile ();
				poiProfile.BabelFormat = "garmin_gpi";
				poiProfile.NameMode = dlg.NameMode;
				poiProfile.DescMode = dlg.DescMode;
				poiProfile.OutputFile = dlg.FileName;
				poiProfile.CacheLimit = dlg.CacheLimit;
				poiProfile.LogLimit = dlg.LogLimit;
				poiProfile.FieldNotesFile = null;
				poiProfile.IncludeAttributes = false;
				poiProfile.ForcePlainText = dlg.UsePlainText;
				poiProfile.Name = "POI";
				StringBuilder builder = new StringBuilder ();
				// Build other properties
				if (dlg.BMPFile != null) {
					builder.Append ("bitmap=\"");
					builder.Append (dlg.BMPFile);
					builder.Append ("\"");
				} else {
					builder.Append ("hide");
				}
				if (dlg.ProximityDistance > 0) {
					builder.Append (",proximity=");
					builder.Append (dlg.ProximityDistance.ToString (CultureInfo.InvariantCulture));
					builder.Append (",units=");
					builder.Append (dlg.ProximityUnits);
				}
				builder.Append (",");
				builder.Append ("category=\"");
				builder.Append (dlg.Category);
				builder.Append ("\"");
				poiProfile.OtherProperties = builder.ToString ();
				SendWaypointsDialog edlg = new SendWaypointsDialog ();
				edlg.Icon = this.Icon;
				edlg.AutoClose = Config.AutoCloseWindows;
				edlg.Start (CacheList.UnfilteredCaches, poiProfile, m_app.CacheStore);
			}
			dlg.Dispose ();
		}

		public void DeleteCacheImages (String cachename)
		{
			string dir = m_app.CacheStore.StoreName;
			string[] fullPath = dir.Split ('/');
			dir = fullPath[fullPath.Length - 1];
			dir = dir.Substring (0, dir.Length - 4);
			dir = Config.DataDirectory + "/ocm_images/" + dir + "/" + cachename;
			if (Directory.Exists (dir))
				Directory.Delete (dir, true);
		}

		public void GrabImagesMulti()
		{
			string baseURL = String.Empty;
			Dictionary<string,string[]> files = new Dictionary<string,string[]>();
			foreach(Geocache cache in CacheList.UnfilteredCaches)
			{
				ScanForImages (files,cache);
			}
			if (files.Count <=0)
			{
				MessageDialog mdlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, Catalog.GetString("No Images Found."));
				mdlg.Run();
				mdlg.Hide();
				return;
			}

			FileDownloadProgress dlg = new FileDownloadProgress();
			dlg.Icon = this.Icon;
			dlg.Start(files);
			CacheInfo.SetCache(CacheList.SelectedCache);
		}

#region GrabImagesMulti
		private void ScanForImages (Dictionary<string, string[]> files, Geocache cache)
		{
			string baseURL = String.Empty;
			if (cache.URL != null)
				baseURL = cache.URL.Scheme + "://" + cache.URL.Host;
			const string IMG = "(<[Ii][Mm][Gg])([^sS][^rR]*)([Ss][Rr][Cc]\\s?=\\s?)\"([^\"]*)\"([^>]*>)";
			MatchCollection matches = Regex.Matches(cache.LongDesc, IMG);
			if (matches.Count == 0)
				return;

			string imagesFolder = GetImagesFolder(cache);
			if (!Directory.Exists(imagesFolder))
				Directory.CreateDirectory(imagesFolder);

			foreach(Match match in matches)
			{
				string url = match.Groups[4].Value;
				if (!url.Contains("://"))
				{
					if (url.StartsWith("/"))
						url = baseURL + url;
					else
						url = baseURL + "/" + url;
				}
				string key = cache.Name + url;
				if (!files.ContainsKey(key))
					files.Add(key, new string[]{url, imagesFolder});
			}
			return;
		}

		private string GetImagesFolder(Geocache cache)
		{
			string dbName = GetDBName ();
			return m_app.AppConfig.DataDirectory + "/ocm_images/" + dbName + "/" + cache.Name;
		}

		private string GetDBName ()
		{
			string dbFile = m_app.CacheStore.StoreName;
			return Utilities.GetShortFileNameNoExtension(dbFile);
		}
#endregion


		/// <summary>
		/// Opens the Add Combination Filter Dialog
		/// </summary>
		public void AddComboFilter ()
		{
			AddComboFilter dlg = new AddComboFilter();
			dlg.MainWin = this;
			dlg.ComboFilter = m_app.CacheStore.CombinationFilter;
			if ((int)ResponseType.Ok == dlg.Run())
			{
				m_app.CacheStore.CombinationFilter = dlg.ComboFilter;
				CacheList.Refresh();
			}
		}


		/// <summary>
		/// Downloads field notes from a device
		/// </summary>
		public void ReceiveGPSFieldNotes ()
		{
			LoadGPSFieldNotes dlg = new LoadGPSFieldNotes ();
			dlg.LastScan = Config.LastGPSFieldNoteScan;
			dlg.LastScanTD = m_app.Profiles.GetActiveProfile ().LastFieldNoteScan;
			dlg.Icon = this.Icon;
			if ((int)ResponseType.Ok == dlg.Run ())
			{
				dlg.Hide ();
				GPSProfile prof =  m_app.Profiles.GetActiveProfile ();
				FieldNotesProgress fprog = new FieldNotesProgress (this);
				fprog.Icon = this.Icon;
				DateTime latest = fprog.ProcessFieldNotes (prof, dlg.LastScan, m_app.OwnerIDs[0]);
				if (latest == DateTime.MinValue)
				{
					return;
				}
				Config.LastGPSFieldNoteScan = latest;
				prof.LastFieldNoteScan = latest;
				m_app.Profiles.UpdateProfile (prof);
			}
			dlg.Hide ();
			dlg.Dispose ();
		}

		/// <summary>
		/// Shows the Field Notes Viewer
		/// </summary>
		public void ShowFieldNotes()
		{
			ShowFieldNotes(false);
		}

		/// <summary>
		/// Shows the Field Notes Viewer
		/// </summary>
		/// <param name="forceRefresh">
		/// Force a reload of the cache list after closing the viewer <see cref="System.Boolean"/>
		/// </param>
		public void ShowFieldNotes (bool forceRefresh)
		{
			if (!System.IO.File.Exists (Config.FieldNotesFile)) {
				MessageDialog mdlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, Catalog.GetString ("There are no field notes."));
				mdlg.Run ();
				mdlg.Hide ();
				return;
			}

			List<CacheLog> logs = FieldNotesHandler.GetLogs (Config.FieldNotesFile, m_app.OwnerIDs[0]);
			OffLineLogViewer dlg = new OffLineLogViewer (this);
			dlg.NeedsRefresh = forceRefresh;
			dlg.Icon = this.Icon;
			dlg.PopulateLogs (logs);
			dlg.Run ();
			if (dlg.NeedsRefresh)
				CacheList.Refresh();
		}

		public void ProcessOfflineLog (Geocache cache, CacheLog log, bool ftf)
		{
			FieldNotesHandler.WriteToFile (log, Config.FieldNotesFile);

			if (cache == null)
				return;
			m_app.CacheStore.AddLog (log.CacheCode, log);
			if (log.LogStatus == "Found it") {
				cache.DNF = false;
				cache.FTF = ftf;
				cache.Symbol = "Geocache Found";
				m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			} else if (log.LogStatus == "Didn't find it") {
				cache.DNF = true;
				cache.FTF = false;
				cache.Symbol = "Geocache";
				m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			} else if (log.LogStatus == "Needs Maintenance") {
				cache.CheckNotes = true;
			}
		}


		public void ShowPreferences ()
		{
			Preferences dlg = new Preferences (Config, m_app.QuickFilterList);
			int oldInterval = Config.UpdateInterval;
			if ((int)ResponseType.Ok == dlg.Run ()) {
				if (Config.UpdateInterval != oldInterval) {
					Config.NextUpdateCheck = DateTime.Now.AddDays (Config.UpdateInterval);
				}
				if (m_app.AppConfig.EnableTraceLog)
					m_app.CacheStore.TracingOn(m_app.AppConfig.DataDirectory + "/ocm.log");
				else
					m_app.CacheStore.TracingOff();
				MapPopupsAction.Active = Config.MapPopups;
				if (m_app.CenterName == Catalog.GetString("Home"))
				{
					m_app.CentreLat = m_app.AppConfig.HomeLat;
					m_app.CentreLon = m_app.AppConfig.HomeLon;
					UpdateLocationLabel();
				}
				CacheMap.SetPopups(MapPopupsAction.Active);
				CacheMap.Reload();
				CacheList.Refresh();
			}
			dlg.Dispose ();
		}

		public void AddGPSProfile ()
		{
			GPSConfiguration dlg = new GPSConfiguration ();
			dlg.Parent = this;
			dlg.Icon = this.Icon;
			if ((int)ResponseType.Ok == dlg.Run ()) {
				GPSProfile prof = new GPSProfile ();
				prof.Name = dlg.ProfileName;
				prof.BabelFormat = dlg.GPSConfig.GetBabelFormat ();
				prof.CacheLimit = dlg.GPSConfig.GetCacheLimit ();
				prof.NameMode = dlg.GPSConfig.GetNameMode ();
				prof.DescMode = dlg.GPSConfig.GetDescMode ();
				prof.LogLimit = dlg.GPSConfig.GetLogLimit ();
				prof.IncludeAttributes = dlg.GPSConfig.IncludeAttributes ();
				prof.OutputFile = dlg.GPSConfig.GetOutputFile ();
				prof.FieldNotesFile = dlg.GPSConfig.FieldNotesFile;
				prof.WaypointMappings = dlg.GPSMappings;
				if (Config.GPSProf == null)
					Config.GPSProf = prof.Name;
				m_app.Profiles.AddProfile (prof);
				RebuildProfiles();
			}
		}

		public void EditProfile (GPSProfile prof)
		{
			GPSConfiguration dlg = new GPSConfiguration (prof);
			dlg.Parent = this;
			dlg.Icon = this.Icon;
			dlg.Title = Catalog.GetString ("Edit GPS Profile...");
			if ((int)ResponseType.Ok == dlg.Run ()) {
				string origName = prof.Name;
				bool isActive = false;
				if ((m_app.Profiles.GetActiveProfile ()) != null && (m_app.Profiles.GetActiveProfile ().Name == origName))
					isActive = true;
				prof.Name = dlg.ProfileName;
				prof.BabelFormat = dlg.GPSConfig.GetBabelFormat ();
				prof.CacheLimit = dlg.GPSConfig.GetCacheLimit ();
				prof.NameMode = dlg.GPSConfig.GetNameMode ();
				prof.DescMode = dlg.GPSConfig.GetDescMode ();
				prof.LogLimit = dlg.GPSConfig.GetLogLimit ();
				prof.IncludeAttributes = dlg.GPSConfig.IncludeAttributes ();
				prof.OutputFile = dlg.GPSConfig.GetOutputFile ();
				prof.WaypointMappings = dlg.GPSMappings;
				prof.FieldNotesFile = dlg.GPSConfig.FieldNotesFile;
				if (origName == prof.Name) {
					m_app.Profiles.UpdateProfile (prof);
				} else {
					m_app.Profiles.DeleteProfile (origName);
					m_app.Profiles.AddProfile (prof);
					if (isActive)
						Config.GPSProf = prof.Name;
				}
				RebuildProfiles();
			}
		}

		public void DeleteGPSProfile ()
		{
			DeleteItem dlg = new DeleteItem (m_app.Profiles);
			if ((int)ResponseType.Ok == dlg.Run ()) {
				if ((m_app.Profiles.GetActiveProfile () != null) && (dlg.ItemToDelete == m_app.Profiles.GetActiveProfile ().Name)) {
					MessageDialog confirm = new MessageDialog (this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, Catalog.GetString ("\"{0}\" is the active" + " GPS profile. Are you sure you wish to delete it?"), dlg.ItemToDelete);
					if ((int)ResponseType.No == confirm.Run ()) {
						confirm.Hide ();
						confirm.Dispose ();
						return;
					}
					confirm.Hide ();
					confirm.Dispose ();
					Config.GPSProf = null;
				}

				m_app.Profiles.DeleteProfile (dlg.ItemToDelete);
				RebuildProfiles();
			}
		}


		public void SendToGPS ()
		{
			if (m_app.Profiles.GetActiveProfile () == null) {
				MessageDialog err = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
				                                       Catalog.GetString ("There is no active GPS profile. Either select an"
				                                                          + " existing one from the GPS Menu or add a new profile ."));
				err.Run ();
				err.Hide ();
				err.Dispose ();
				return;
			}
			SendWaypointsDialog dlg = new SendWaypointsDialog ();
			dlg.Parent = this;
			dlg.Icon = this.Icon;
			dlg.AutoClose = Config.AutoCloseWindows;
			dlg.Start (CacheList.UnfilteredCaches, m_app.Profiles.GetActiveProfile (), m_app.CacheStore);
		}

		public void DeleteSelectedCache ()
		{
			String name = CacheList.SelectedCache.Name;
			MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
			                                       String.Format (Catalog.GetString ("Are you sure you want to delete {0}?"),
			                                                      name));
			if ((int)ResponseType.Yes == dlg.Run ()) {
				dlg.Hide();
				m_app.CacheStore.DeleteWaypoint (name);
				DeleteCacheImages (name);
				CacheList.Refresh();
			}
			dlg.Hide ();
		}

		public void OpenInQLandKarte ()
		{
			GPXWriter writer = new GPXWriter ();
			String tempPath = System.IO.Path.GetTempPath ();
			String tempFile = tempPath + "ocm_export.gpx";
			ExportProgressDialog dlg = new ExportProgressDialog (writer);

						/*Connection DbusConnection = Bus.Session;
			IQLandkarteGT ql = DbusConnection.GetObject<IQLandkarteGT> ("org.qlandkarte.dbus", new ObjectPath ("QLandkarteGT"));
			System.Console.WriteLine("Connected");
			ql.loadGeoData("/home/campbelk/Desktop/export.gpx");*/

			dlg.AutoClose = true;
			dlg.Title = Catalog.GetString ("Preparing to send to QLandKarte GT");
			dlg.CompleteCommand = "qlandkartegt " + tempFile;
			dlg.Icon = this.Icon;
			dlg.Start (tempFile, CacheList.UnfilteredCaches, GPSProfileList.GetDefaultMappings (), m_app.CacheStore);
		}

		public void OpenSelectedCacheInQLandKarte ()
		{
			GPXWriter writer = new GPXWriter ();
			String tempPath = System.IO.Path.GetTempPath ();
			String tempFile = tempPath + "ocm_export.gpx";
			List<Geocache> cache = new List<Geocache> ();
			cache.Add (CacheList.SelectedCache);
			ExportProgressDialog dlg = new ExportProgressDialog (writer);
			dlg.AutoClose = true;
			dlg.Title = Catalog.GetString ("Preparing to send to QLandKarte GT");
			dlg.CompleteCommand = "qlandkartegt " + tempFile;
			dlg.Icon = this.Icon;
			dlg.Start (tempFile, cache, GPSProfileList.GetDefaultMappings (), m_app.CacheStore);
		}

		public void CopyToDB ()
		{
			CopyMoveDialog dlg = new CopyMoveDialog ();
			dlg.Title = "Copy Caches to Another Database...";
			dlg.Filename = Config.DataDirectory;
			dlg.Icon = this.Icon;
			if ((int)ResponseType.Ok == dlg.Run ()) {
				CopyingProgress cp = new CopyingProgress ();
				cp.Icon = this.Icon;
				cp.Start (dlg.Filename, false, dlg.Mode, this);
			}
		}

		public void MoveToDB ()
		{
			CopyMoveDialog dlg = new CopyMoveDialog ();
			dlg.Title = Catalog.GetString ("Move Geocaches...");
			dlg.Title = "Move Caches to Another Database...";
			dlg.Filename = Config.DataDirectory;
			dlg.Icon = this.Icon;
			if ((int)ResponseType.Ok == dlg.Run ()) {
				CopyingProgress cp = new CopyingProgress ();
				cp.Icon = this.Icon;
				cp.Start (dlg.Filename, true, dlg.Mode, this);
				CacheList.Refresh ();
			}

		}

		public void SetLastGPS(string name, bool canReceive)
		{
			if (name != null)
			{
				TransferCachesToLastUsedAction.Label = String.Format(Catalog.GetString("Transfer _Caches to Last Used ({0})..."), name);
				TransferCachesToLastUsedAction.Sensitive = true;
				if (canReceive)
				{
					ReceiveFieldNotesFromLastUsedAction.Label = String.Format(Catalog.GetString("Receive F_ield Notes from Last Used ({0})..."),name);
					ReceiveFieldNotesFromLastUsedAction.Sensitive = true;
				}
				else
				{
					ReceiveFieldNotesFromLastUsedAction.Sensitive = false;
				}
			}
			else
			{
				TransferCachesToLastUsedAction.Sensitive = false;
				ReceiveFieldNotesFromLastUsedAction.Sensitive = false;
			}
		}

		private static void DeregisterBus ()
        {
          try
          {
             //##AP BusG.Init ();
             Bus bus = Bus.Session;
             bus.Unregister(new ObjectPath ("/org/ocm/dbus"));
               bus.ReleaseName("org.ocm.dbus");
          }
           catch
           {
             System.Console.WriteLine("WARNING: Could not deregister from DBUS");
           }
        }

#region Event Handlers
		protected virtual void OnDeleteWindow (object o, Gtk.DeleteEventArgs args)
		{
			DeregisterBus();
			args.RetVal = true;
			m_app.End();
		}
		protected virtual void OnResizeWindow (object o, Gtk.SizeAllocatedArgs args)
		{
			Config.WindowWidth = args.Allocation.Width;
			Config.WindowHeight = args.Allocation.Height;
		}

		void HandleCacheListRefreshPulse (object sender, CacheEventArgs args)
		{
			m_pulseCount++;
			ocmMapWidget.AddGeocache(args.Cache);
			if (m_pulseCount == 50)
			{
				statusProgressBar.Pulse();
				m_pulseCount = 0;
				OCMApp.UpdateGUIThread();
			}
		}

		void HandleAllToBookmark(object sender, EventArgs args)
		{
			//##AP Action action = sender as Action;
			Gtk.Action action = sender as Gtk.Action;
			string bookmark = action.Label;
			List<Geocache> caches = CacheList.UnfilteredCaches;
			m_app.CacheStore.StartUpdate();
			foreach(Geocache cache in caches)
			{
				m_app.CacheStore.AddBoormarkEntry(bookmark, cache.Name);
			}
			m_app.CacheStore.CompleteUpdate();
		}

		void HandleSelectedToBookmark(object sender, EventArgs args)
		{
			//##AP Action action = sender as Action;
			Gtk.Action action = sender as Gtk.Action;
			string bookmark = action.Label;
			Geocache cache = CacheList.SelectedCache;
			m_app.CacheStore.AddBoormarkEntry(bookmark, cache.Name);
		}

		void HandleCacheListRefreshEnd (object sender, RefreshCompleteArgs args)
		{
			if (this.GdkWindow != null)
				this.GdkWindow.Cursor = new Cursor (CursorType.Arrow);
			this.mainHPane.Sensitive = true;
			this.mainMenuBar.Sensitive = true;
			UpdateCounts(args.VisibleCount,args.FoundCount, args.ArchivedDisabledCount, args.MineCount, args.TotalCount);
			statusProgressBar.Visible = false;
			ocmMapWidget.UpdateChildWaypoints();
			OCMApp.UpdateGUIThread();
		}

		void HandleCacheListRefreshStart (object sender, EventArgs args)
		{
			if (this.GdkWindow != null)
				this.GdkWindow.Cursor = new Cursor(CursorType.Watch);
			this.mainHPane.Sensitive = false;
			this.mainMenuBar.Sensitive = false;
			ocmMapWidget.ClearCaches();
			mainStatusBar.Push (mainStatusBar.GetContextId ("refilter"),
			                    Catalog.GetString("Retrieving caches, please wait.."));
			m_pulseCount = 0;
			statusProgressBar.Visible = true;
			OCMApp.UpdateGUIThread();
		}

		protected virtual void OnOpenDatabase (object sender, System.EventArgs e)
		{
			ShowOpenDBDialog ();
		}

		protected virtual void OnNewDatabase (object sender, System.EventArgs e)
		{
			CreateDB();
		}

		protected virtual void OnCompactDatabase (object sender, System.EventArgs e)
		{
			m_app.CacheStore.CompactStore();

		}

		protected virtual void OnQuit (object sender, System.EventArgs e)
		{
			DeregisterBus();
			m_app.End();
		}


		void HandleCacheListSelectionChanged (object sender, CacheEventArgs args)
		{
			if (this.GdkWindow != null)
				this.GdkWindow.Cursor = Cursor.NewFromName(this.Display, "left_ptr_watch");
			OCMApp.UpdateGUIThread();
			SetFilterFlags ();
			Geocache cache = args.Cache;
			ocmCacheInfo.SetCache(cache);
			if (args.Cache == null)
			{
				CacheAction.Sensitive = false;
				printAction.Sensitive = false;
				AddSelectedCacheToAction.Sensitive = false;
				PanToSelectedCacheAction.Sensitive = false;
				ZoomToSelectedCacheAction.Sensitive = false;
				DeleteAction.Sensitive = false;
				DeselectAction.Sensitive = false;
				GrabImagesAction1.Sensitive = false;
				ocmMapWidget.SetCache(cache);
				if (this.GdkWindow != null)
					this.GdkWindow.Cursor = ARROWCURSOR;
				return;
			}

			CacheAction.Sensitive = true;
			GrabImagesAction1.Sensitive = true;
			printAction.Sensitive = true;
			PanToSelectedCacheAction.Sensitive = true;
			AddSelectedCacheToAction.Sensitive = true;
			ZoomToSelectedCacheAction.Sensitive = true;
			DeleteAction.Sensitive = true;
			DeselectAction.Sensitive = true;
			MarkUnfoundAction.Sensitive = (cache.Found || cache.DNF)? true:false;
			MarkDidNotFindAction.Sensitive = cache.DNF? false:true;
			MarkFoundAction.Sensitive = (!cache.Found || cache.FTF)? true:false;
			MarkFirstToFindAction.Sensitive = (!cache.Found || !cache.FTF)?true:false;
			MarkAvailableAction.Sensitive = !cache.Available;
			MarkArchivedAction.Sensitive = (cache.Available || !cache.Archived)? true:false;
			MarkDisabledAction.Sensitive = (cache.Available || cache.Archived)? true:false;
			if (m_app.CacheStore.ActiveBookmarkList != null)
				RemoveSelectedCacheFromBookmarkListAction.Sensitive = true;
			else
				RemoveSelectedCacheFromBookmarkListAction.Sensitive = false;
			ocmMapWidget.SetCache(cache);
			OCMApp.UpdateGUIThread();
			if (this.GdkWindow != null)
				this.GdkWindow.Cursor = ARROWCURSOR;
		}

		private void SetFilterFlags ()
		{
			if (m_app.CacheStore.CombinationFilter != null)
			{
				ClearComboFilterAction.Sensitive = true;
				AdvancedFiltersAction.Sensitive = false;
			}
			else
			{
				ClearComboFilterAction.Sensitive = false;
				AdvancedFiltersAction.Sensitive = true;
			}
			if (m_app.CacheStore.AdvancedFilters != null)
				ClearAdvancedFiltersAction.Sensitive = true;
			else
				ClearAdvancedFiltersAction.Sensitive = false;
			if (CacheList.HasBasicFilters || m_app.CacheStore.AdvancedFilters != null
			    || m_app.CacheStore.CombinationFilter != null)
				ClearAllFiltersAction.Sensitive = true;
			else
				ClearAllFiltersAction.Sensitive = false;
		}


		protected virtual void OnToggleShowNearby (object sender, System.EventArgs e)
		{
			ChildWaypointsAction.Sensitive = ShowNearbyCachesAction.Active;
			ocmMapWidget.ShowNearby = ChildWaypointsAction.Sensitive;
			ocmMapWidget.Refresh();
		}

		protected virtual void OnToggleSelectedCache (object sender, System.EventArgs e)
		{
			ocmMapWidget.ShowAllChildren = AllWaypointsAction.Active;
			ocmMapWidget.Refresh();
		}

		protected virtual void OnPrintCache (object sender, System.EventArgs e)
		{
			CachePrinter printer = new CachePrinter ();
			printer.StartPrinting (CacheList.SelectedCache, this);
		}

		protected virtual void OnAdvancedFiltersClick (object sender, System.EventArgs e)
		{
			AddAdvancedFilter ();
		}

		public void AddAdvancedFilter ()
		{
			FilterDialog dlg = new FilterDialog();
			dlg.MainWin = this;
			dlg.Filter = m_app.CacheStore.AdvancedFilters;
			if (((int) ResponseType.Ok) == dlg.Run())
			{
				m_app.CacheStore.AdvancedFilters = dlg.Filter;
				CacheList.Refresh();
			}
		}

		protected virtual void OnWikiClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://sourceforge.net/apps/mediawiki/opencachemanage/");
		}

		protected virtual void OnForumsClick (object sender, System.EventArgs e)
		{
			Process.Start("http://ocm.dafb-o.de/index.php");
		}

		protected virtual void OnAboutClick (object sender, System.EventArgs e)
		{
			AboutDialog dialog = new AboutDialog ();
			dialog.ProgramName = "Open Cache Manager";
			dialog.Icon = this.Icon;
			dialog.Version = OCMApp.GetOCMVersion();
			dialog.Logo =  new Gdk.Pixbuf ("./icons/scalable/OCMLogo.svg", 96, 96);
			dialog.Website = "http://www.andreas-peters.net/";
			dialog.Copyright = "Copyright Kyle Campbell (c) 2010-2013\nCopyright Andreas Peters (c) 2015-2016";	
			System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.FileStream("licence/Licence.txt",System.IO.FileMode.Open,System.IO.FileAccess.Read));
			dialog.License = reader.ReadToEnd();
			reader.Close();
			dialog.Authors = new String[] { "Kyle Campbell,Florian Plähn - Programming", "Madelayne DeGrâce - Icons",
				"Harrie Klomp - Dutch Translation" , "Thor Dekov Buur - Danish Translation",
				"Michael Massoth/Florian Plähn/Maik Bischoff - German Translation",
				"Josef Kulhánek - Czech Translation","Vicen - Spanish Translation",
				"Per Holmberg - Swedish Translation"};
			dialog.Run ();
			dialog.Hide();

		}

		protected virtual void OnChangeHistoryClick (object sender, System.EventArgs e)
		{
			ChangeHistory dlg = new ChangeHistory();
			dlg.Run();
		}

		protected virtual void OnCheckUpdatesClick (object sender, System.EventArgs e)
		{
			string ver;
			if (m_app.UpdatesAvailable(out ver))
			{
				MessageDialog dlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.YesNo, Catalog.GetString ("A new version \"{0}\" of OCM is available" + "\nWould you like to go to the download page now?"), ver);
				if ((int)ResponseType.Yes == dlg.Run ())
				{
					dlg.Hide ();
					Process.Start ("http://sourceforge.net/projects/opencachemanage/files/");
				}
				else
					dlg.Hide ();
			}
			else
			{
				MessageDialog dlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, Catalog.GetString ("OCM is at the latest version."));
				dlg.Run ();
				dlg.Hide ();

			}
		}

		public void ModifyCache(Geocache cache)
		{
			cache.Distance = Utilities.calculateDistance(m_app.CentreLat, cache.Lat, m_app.CentreLon, cache.Lon);
			cacheList.Resort();
			m_app.CacheStore.AddWaypointOrCache(cache, false, false);
			HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
		}

		public void Refresh()
		{
			HandleCacheListSelectionChanged(this, new CacheEventArgs(CacheList.SelectedCache));
		}

		public void RefreshCacheList()
		{
			CacheList.Refresh();
		}


		protected virtual void OnOCMHomeClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://opencachemanage.sourceforge.net/");
		}

		protected virtual void OnGCHomeClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.geocaching.com");
		}

		protected virtual void OnGCProfileClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.geocaching.com/my");
		}

		protected virtual void OnGCAccountClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.geocaching.com/account/default.aspx");
		}

		protected virtual void OnGCPocketQueryClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.geocaching.com/pocket/default.aspx");
		}

		protected virtual void OnGCStatsClick (object sender, System.EventArgs e)
		{
			Process.Start("http://www.geocaching.com/my/statistics.aspx");
		}

		protected virtual void OnGCFindClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.geocaching.com/seek/default.aspx");
		}

		protected virtual void OnTCHomePageClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.terracaching.com");
		}

		protected virtual void OnTCTraditionalClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.terracaching.com/tdl.cgi?NF=1");
		}

		protected virtual void OnTCLocationlessClick (object sender, System.EventArgs e)
		{
			Process.Start ("http://www.terracaching.com/tdl.cgi?NF=1&L=1");
		}

		protected virtual void OnOCCountryList (object sender, System.EventArgs e)
		{
			Process.Start("http://opencaching.eu");
		}

		protected virtual void OnGPSBabelClick (object sender, System.EventArgs e)
		{
			Process.Start("http://www.gpsbabel.org");
		}

		protected virtual void OnGPSDClick (object sender, System.EventArgs e)
		{
			Process.Start("http://gpsd.berlios.de");
		}

		protected virtual void OnNaviHome (object sender, System.EventArgs e)
		{
			Process.Start("http://www.navicache.com");
		}

		protected virtual void OnMyNavi (object sender, System.EventArgs e)
		{
			Process.Start("http://www.navicache.com/cgi-bin/db/MyNaviCacheHome.pl");
		}

		protected virtual void OnModifyCache (object sender, System.EventArgs e)
		{
			m_app.EditCache ();
		}

		protected virtual void OnAddCorrectedCoordinate (object sender, System.EventArgs e)
		{
			CorrectCoordinates ();
		}

		protected virtual void OnSetSelectedAsCentreClick (object sender, System.EventArgs e)
		{
			Geocache cache = CacheList.SelectedCache;
			SetLocation(cache.Name, cache.Lat, cache.Lon);
		}

		protected virtual void OnViewOnlineClick (object sender, System.EventArgs e)
		{
			Process.Start(CacheList.SelectedCache.URL.ToString());
		}

		protected virtual void OnMarkFound (object sender, System.EventArgs e)
		{
			MarkAsFound ();
		}

		public void MarkAsFound ()
		{
			Geocache cache = CacheList.SelectedCache;
			bool alreadyFound = cache.Found;
			MarkFoundDialog dlg = new MarkFoundDialog ();
			dlg.DialogLabel = String.Format ("Do you wish to mark {0} as found?", cache.Name);
			dlg.LogDate = m_app.LoggingDate;
			if ((int)ResponseType.Cancel == dlg.Run ()) {
				dlg.Hide ();
				return;
			}
			dlg.Hide ();
			cache.FTF = false;
			cache.DNF = false;
			cache.Symbol = "Geocache Found";
			GenerateFindLog (dlg, cache, false);
			m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			if (!alreadyFound)
				CacheList.FoundCount += 1;
			HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
		}

		private void GenerateFindLog (MarkFoundDialog dlg, Geocache cache, bool isDNF)
		{
			m_app.LoggingDate = dlg.LogDate;
			CacheLog log = new CacheLog ();
			log.FinderID = m_app.OwnerIDs[0];
			log.LogDate = dlg.LogDate;
			log.LoggedBy = "OCM";
			if (isDNF)
				log.LogStatus = "Didn't find it";
			else
				log.LogStatus = "Found it";
			log.LogMessage = "AUTO LOG: OCM";
			log.LogKey = cache.Name + log.LogDate.ToFileTime ().ToString ();
			m_app.CacheStore.AddLog (cache.Name, log);
		}

		protected virtual void OnMarkFTF (object sender, System.EventArgs e)
		{
			MarkAsFTF ();
		}

		public void MarkAsFTF ()
		{
			Geocache cache = CacheList.SelectedCache;
			MarkFoundDialog dlg = new MarkFoundDialog ();
			bool alreadyFound = cache.Found;
			dlg.Title = Catalog.GetString ("Mark First to Find");
			dlg.DialogLabel = String.Format ("Do you wish to mark {0} as first to find?", cache.Name);
			dlg.LogDate = m_app.LoggingDate;
			if ((int)ResponseType.Cancel == dlg.Run ()) {
				dlg.Hide ();
				return;
			}
			dlg.Hide ();
			cache.FTF = true;
			cache.DNF = false;
			cache.Symbol = "Geocache Found";
			GenerateFindLog(dlg, cache, false);
			m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			if (!alreadyFound)
				CacheList.FoundCount += 1;
			HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
		}

		protected virtual void OnMarkDNF (object sender, System.EventArgs e)
		{
			MarkAsDNF ();
		}

		public void MarkAsDNF ()
		{
			Geocache cache = CacheList.SelectedCache;
			bool alreadyFound = cache.Found;
			MarkFoundDialog dlg = new MarkFoundDialog ();
			dlg.Title = Catalog.GetString ("Mark Did Not Find");
			dlg.DialogLabel = String.Format ("Do you wish to mark {0} as did not find?", cache.Name);
			if ((int)ResponseType.Cancel == dlg.Run ()) {
				dlg.Hide ();
				return;
			}
			dlg.Hide ();
			cache.FTF = false;
			cache.DNF = true;
			cache.Symbol = "Geocache";
			GenerateFindLog(dlg, cache, true);
			m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			if (alreadyFound)
				CacheList.FoundCount -= 1;
			HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
		}

		protected virtual void OnMarkUnfound (object sender, System.EventArgs e)
		{
			MarkUnfound ();
		}

		public void MarkUnfound ()
		{
			Geocache cache = CacheList.SelectedCache;
			bool alreadyFound = cache.Found;
			MarkFoundDialog dlg = new MarkFoundDialog ();
			dlg.Title = Catalog.GetString ("Mark Did Not Find");
			dlg.DialogLabel = String.Format ("Do you wish to mark {0} as unfound?", cache.Name);
			if ((int)ResponseType.Cancel == dlg.Run ()) {
				dlg.Hide ();
				return;
			}
			dlg.Hide ();
			cache.FTF = false;
			cache.DNF = false;
			cache.Symbol = "Geocache";
			GenerateFindLog(dlg, cache, true);
			m_app.CacheStore.AddWaypointOrCache (cache, false, false);
			if (alreadyFound)
				CacheList.FoundCount -= 1;
			HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
		}

		protected virtual void OnMarkDisabled (object sender, System.EventArgs e)
		{
			MarkDisabled ();
		}

		public void MarkDisabled ()
		{

			Geocache cache = CacheList.SelectedCache;
			bool alreadyDisabled = (cache.Archived || !cache.Available)? true:false;
			MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "Are you sure you want to mark " + cache.Name + " as disabled?");
			if ((int)ResponseType.Yes == dlg.Run ()) {
				cache.Available = false;
				cache.Archived = false;
				m_app.CacheStore.AddWaypointOrCache (cache,false, false);
				if (!alreadyDisabled)
					CacheList.DisabledOrArchived += 1;
				HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
			}
			dlg.Hide ();
		}

		protected virtual void OnMarkArchived (object sender, System.EventArgs e)
		{
			MarkArchived ();
		}

		public void MarkArchived ()
		{
			Geocache cache = CacheList.SelectedCache;
			bool alreadyDisabled = (cache.Archived || !cache.Available)? true:false;
			MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "Are you sure you want to mark " + cache.Name + " as archived?");
			if ((int)ResponseType.Yes == dlg.Run ()) {
				cache.Available = false;
				cache.Archived = true;
				m_app.CacheStore.AddWaypointOrCache (cache,false, false);
				if (!alreadyDisabled)
					CacheList.DisabledOrArchived += 1;
				HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
			}
			dlg.Hide ();
		}

		protected virtual void OnMarkAvailable (object sender, System.EventArgs e)
		{
			MarkAvailable ();
		}

		public void MarkAvailable ()
		{
			Geocache cache = CacheList.SelectedCache;
			bool alreadyDisabled = (cache.Archived || !cache.Available)? true:false;
			MessageDialog dlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "Are you sure you want to mark " + cache.Name + " as available?");
			if ((int)ResponseType.Yes == dlg.Run ()) {
				cache.Available = true;
				cache.Archived = false;
				m_app.CacheStore.AddWaypointOrCache (cache,false, false);
				if (alreadyDisabled)
					CacheList.DisabledOrArchived -= 1;
				HandleCacheListSelectionChanged(this, new CacheEventArgs(cache));
			}
			dlg.Hide ();
		}

		protected virtual void OnAddChildWaypoint (object sender, System.EventArgs e)
		{
			m_app.AddChildWaypoint();
		}

		protected virtual void OnUpClick (object sender, System.EventArgs e)
		{
			mainVPane.Position = 0;
			UpdateMapButtons();
		}

		protected virtual void OnDownClick (object sender, System.EventArgs e)
		{
			mapUpButton.Sensitive = true;
			if (mainVPane.Position >=385)
			{
				mainVPane.Position = mainVPane.MaxPosition - 20;
			}
			else if (mainVPane.Position < 285)
			{
				mainVPane.Position = 285;
			}
			else
			{
				mainVPane.Position = 385;
			}
			UpdateMapButtons();
		}

		protected virtual void OnRemoveFromList (object sender, System.EventArgs e)
		{
			Geocache cache = CacheList.SelectedCache;
			m_app.CacheStore.RemoveBookmarkEntry(m_app.CacheStore.ActiveBookmarkList, cache.Name);
			CacheList.UnfilteredCaches.Remove(cache);
		}

		protected virtual void OnCreateBookmarkList (object sender, System.EventArgs e)
		{
			AddBookmark ();
		}


		protected virtual void OnDeleteBookmarkList (object sender, System.EventArgs e)
		{
			DeleteItem dlg = new DeleteItem(m_app.Bookmarks);
			if ((int) ResponseType.Ok == dlg.Run())
			{
				m_app.CacheStore.RemoveBookmarkList(dlg.ItemToDelete);
				RebuildBookmarks();
			}
		}


		protected virtual void OnAddLocation (object sender, System.EventArgs e)
		{
			AddLocation (ocmMapWidget.MapLat, ocmMapWidget.MapLon);
		}

		public void AddLocation (double lat, double lon)
		{
			AddLocationDialog dlg = new AddLocationDialog();
			dlg.CoordinateWidget.Latitude = lat;
			dlg.CoordinateWidget.Longitude = lon;
			if ((int) ResponseType.Ok == dlg.Run())
			{
				if (m_app.Locations.GetLocation(dlg.LocationName) != null)
				{
					MessageDialog warnDlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo,
					                                          Catalog.GetString("Are you sure you wish to overwrite \"{0}\"?"),
					                                          dlg.LocationName);
					if ((int)ResponseType.No == warnDlg.Run())
					{
						warnDlg.Hide();
						return;
					}
					warnDlg.Hide();
				}
				m_app.Locations.AddLocation(dlg.NewLocation);
				RebuildLocations();
				SetLocation(dlg.NewLocation);
			}
		}

		protected virtual void OnDeleteLocation (object sender, System.EventArgs e)
		{
			DeleteItem dlg = new DeleteItem(m_app.Locations);
			dlg.Title = Catalog.GetString("Delete Location...");
			dlg.Icon = this.Icon;
			if ((int)ResponseType.Ok == dlg.Run())
			{
				m_app.Locations.DeleteLocation(dlg.ItemToDelete);
			}
			RebuildLocations();
		}

		protected virtual void OnPanToCentre (object sender, System.EventArgs e)
		{
			ocmMapWidget.PanTo(m_app.CentreLat, m_app.CentreLon);
		}

		protected virtual void OnPanToSelected (object sender, System.EventArgs e)
		{
			Geocache cache = CacheList.SelectedCache;
			ocmMapWidget.PanTo(cache.Lat, cache.Lon);
		}

		protected virtual void OnZoomToCentre (object sender, System.EventArgs e)
		{
			ocmMapWidget.ZoomTo(m_app.CentreLat, m_app.CentreLon);
		}

		protected virtual void OnZoomToSelected (object sender, System.EventArgs e)
		{
			Geocache cache = CacheList.SelectedCache;
			ocmMapWidget.ZoomTo(cache.Lat, cache.Lon);
		}

		protected virtual void OnCombinationClick (object sender, System.EventArgs e)
		{
			AddComboFilter ();
		}

		protected virtual void OnClearAdvanced (object sender, System.EventArgs e)
		{
			m_app.CacheStore.AdvancedFilters = null;
			CacheList.Refresh();
		}

		protected virtual void OnClearCombo (object sender, System.EventArgs e)
		{
			m_app.CacheStore.CombinationFilter = null;
			CacheList.Refresh();
		}


		protected virtual void OnClearAll (object sender, System.EventArgs e)
		{
			CacheList.ApplyQuickFilter(QuickFilter.ALL_FILTER);
		}


		protected virtual void OnImportGPX (object sender, System.EventArgs e)
		{
			ImportDialog dlg = new ImportDialog (this);
			dlg.SetCurrentFolder (Config.ImportDirectory);
			dlg.IgnoreExtraFields = Config.ImportIgnoreExtraFields;
			dlg.PreventStatusOverwrite = Config.ImportPreventStatusOverwrite;
			dlg.PurgeOldLogs = Config.ImportPurgeOldLogs;
			dlg.ItemToDelete = Config.ImportBookmarkList;
			if (dlg.Run () == (int)ResponseType.Accept) {
				RegisterRecentFile (dlg.Filename);
				dlg.Hide ();
				Config.ImportIgnoreExtraFields = dlg.IgnoreExtraFields;
				Config.ImportPreventStatusOverwrite = dlg.PreventStatusOverwrite;
				Config.ImportPurgeOldLogs = dlg.PurgeOldLogs;
				Config.ImportBookmarkList = dlg.ItemToDelete;
				if (dlg.Filename.EndsWith (".zip"))
					ImportZip (dlg.Filename);
				else
					ImportGPXFile (dlg.Filename, Config.AutoCloseWindows);
			}
			dlg.Destroy ();
		}

		protected virtual void OnImportDirectory (object sender, System.EventArgs e)
		{
			ImportDirectoryDialog dlg = new ImportDirectoryDialog (this);
			dlg.IgnoreExtraFields = Config.ImportIgnoreExtraFields;
			dlg.PreventStatusOverwrite = Config.ImportPreventStatusOverwrite;
			dlg.PurgeOldLogs = Config.ImportPurgeOldLogs;
			dlg.Directory = Config.ImportDirectory;
			dlg.DeleteOnCompletion = Config.ImportDeleteFiles;
			dlg.ItemToDelete = Config.ImportBookmarkList;
			if (dlg.Run () == (int)ResponseType.Ok) {
				dlg.Hide ();
				Config.ImportIgnoreExtraFields = dlg.IgnoreExtraFields;
				Config.ImportPreventStatusOverwrite = dlg.PreventStatusOverwrite;
				Config.ImportPurgeOldLogs = dlg.PurgeOldLogs;
				Config.ImportDeleteFiles = dlg.DeleteOnCompletion;
				Config.ImportBookmarkList = dlg.ItemToDelete;
				ImportDirectory (dlg.Directory, dlg.DeleteOnCompletion, Config.AutoCloseWindows);
			}
			dlg.Destroy ();
		}

		protected virtual void OnImportOpencaching (object sender, System.EventArgs e)
		{
			ImportOpencachingDialog dlg = new ImportOpencachingDialog (this);
			if (dlg.Run () == (int)ResponseType.Ok) {
				dlg.Hide ();
			}
			dlg.Destroy ();
		}		

		private static void RegisterRecentFile (String filename)
		{
			RecentManager manager = RecentManager.Default;
			manager.AddItem ("file://" + filename);
		}

		protected virtual void OnSaveQuickFilter (object sender, System.EventArgs e)
		{
			AddBookMarkDialog dlg = new AddBookMarkDialog ();
			dlg.Title = Catalog.GetString ("Save QuickFilter");
			if (((int)ResponseType.Ok) == dlg.Run ()) {
				QuickFilter filter = new QuickFilter ();
				filter.AdvancedFilters = m_app.CacheStore.AdvancedFilters;
				if (m_app.CacheStore.CombinationFilter != null)
					filter.ComboFilter = new List<FilterList>(m_app.CacheStore.CombinationFilter);
				filter.Name = dlg.BookmarkName;
				CacheList.PopulateQuickFilter(filter);
				m_app.QuickFilterList.AddFilter (filter);
				RebuildQuickFilters();
			}
		}

		protected virtual void OnDeleteQuickFilter (object sender, System.EventArgs e)
		{
			DeleteItem dlg = new DeleteItem(m_app.QuickFilterList);
			dlg.Title = Catalog.GetString("Delete Quick Filter...");
			dlg.Icon = this.Icon;
			if ((int) ResponseType.Ok == dlg.Run())
			{
				m_app.QuickFilterList.DeleteFilter(dlg.ItemToDelete);
				RebuildQuickFilters();
			}
		}

		protected virtual void OnViewFieldNotes (object sender, System.EventArgs e)
		{
			ShowFieldNotes ();
		}

		protected virtual void OnExportGPX (object sender, System.EventArgs e)
		{
			ExportGPX();
		}

		protected virtual void OnExportPOI (object sender, System.EventArgs e)
		{
			ExportGarminPOI();
		}

		protected virtual void OnDelete (object sender, System.EventArgs e)
		{
			DeleteSelectedCache ();
		}

		protected virtual void OnPreferences (object sender, System.EventArgs e)
		{
			ShowPreferences();
		}

		protected virtual void OnDeselect (object sender, System.EventArgs e)
		{
			CacheList.DeselectAll();
		}

		protected virtual void OnAddProfile (object sender, System.EventArgs e)
		{
			AddGPSProfile();
		}

		protected virtual void OnDeleteProfile (object sender, System.EventArgs e)
		{
			DeleteGPSProfile();
		}

		protected virtual void OnViewInQLGT (object sender, System.EventArgs e)
		{
			OpenInQLandKarte();
		}

		protected virtual void OnViewSelectedInQLGT (object sender, System.EventArgs e)
		{
			OpenSelectedCacheInQLandKarte();
		}

		protected virtual void OnConfigure (object sender, System.EventArgs e)
		{
			ConfigureEToolsDlg dlg = new ConfigureEToolsDlg (m_app.Tools);
			dlg.Run ();
			RebuildTools();
		}

		protected virtual void OnLogToFile (object sender, System.EventArgs e)
		{
			Config.UseOfflineLogging = LogToFieldNotesFileAction.Active;
		}

		protected virtual void OnLogFind (object sender, System.EventArgs e)
		{
			LogFind();
		}


		protected virtual void OnDeleteAll (object sender, System.EventArgs e)
		{
			MessageDialog dlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, String.Format (Catalog.GetString ("Are you sure you want to delete these {0} caches?\nThis operation cannot be undone."), CacheList.UnfilteredCaches.Count));
			if ((int)ResponseType.Yes == dlg.Run ()) {
				dlg.Hide ();
				OCMApp.UpdateGUIThread();
				CopyingProgress ddlg = new CopyingProgress ();
				ddlg.Icon = this.Icon;
				ddlg.StartDelete (CacheList.UnfilteredCaches, this);
				CacheList.Refresh();
			} else {
				dlg.Hide ();
			}
		}


		protected virtual void OnCopy (object sender, System.EventArgs e)
		{
			CopyToDB();
		}

		protected virtual void OnMove (object sender, System.EventArgs e)
		{
			MoveToDB();
		}


		protected virtual void OnTransferLast (object sender, System.EventArgs e)
		{
			SendToGPS();
		}

		protected virtual void OnReceiveLast (object sender, System.EventArgs e)
		{
			ReceiveGPSFieldNotes();
		}

		protected virtual void OnFullToggle (object sender, System.EventArgs e)
		{
			if (FullScreenAction.Active)
				Fullscreen();
			else
				Unfullscreen();
		}


		public void UpdateMapButtons()
		{
			if (mainVPane.Position == 0)
			{
				mapUpButton.Visible = false;
				restoreButton.Visible = true;
				mapDownButton.Visible = true;
				minButton.Visible = true;
			}
			else if (mainVPane.Position >= mainVPane.MaxPosition -20)
			{
				mapUpButton.Visible = true;
				restoreButton.Visible = true;
				mapDownButton.Visible = false;
				minButton.Visible = false;
			}
			else
			{
				mapUpButton.Visible = true;
				mapDownButton.Visible = true;
				minButton.Visible = true;
				restoreButton.Visible = false;
			}
			CacheInfo.QueueDraw();
		}


		protected virtual void OnMinClick (object sender, System.EventArgs e)
		{
			mainVPane.Position = mainVPane.MaxPosition - 20;
			UpdateMapButtons();
		}


		protected virtual void onMaxClick (object sender, System.EventArgs e)
		{
			mainVPane.Position = 0;
			UpdateMapButtons();
		}

		protected virtual void OnRestoreClick (object sender, System.EventArgs e)
		{
			mainVPane.Position = 385;
			UpdateMapButtons();
		}

		protected virtual void OnGrabImages (object sender, System.EventArgs e)
		{
			CacheInfo.GrabImages();
		}

		protected virtual void OnMapPopupToggle (object sender, System.EventArgs e)
		{
			CacheMap.SetPopups(MapPopupsAction.Active);
		}

		protected virtual void OnCleanup (object sender, System.EventArgs e)
		{
			CleanupAssistant wizard = new CleanupAssistant(m_app, this);
			wizard.Show();
		}

		protected virtual void OnReduceLogs (object sender, System.EventArgs e)
		{
			ReduceLogsDialog dlg = new ReduceLogsDialog();
			if ((int) ResponseType.Ok == dlg.Run())
			{
				int limit = dlg.LogLimit;
				List<CacheLog> logs = m_app.CacheStore.GetCacheLogs(CacheList.SelectedCache.Name);
				if (logs.Count <= 0)
					return;
				List<string> keys_to_delete = new List<string>();
				for (int i=0; i < logs.Count; i++)
				{
					if (i >= dlg.LogLimit)
						keys_to_delete.Add(logs[i].LogKey);

				}
				m_app.CacheStore.PurgeLogsByKey(keys_to_delete.ToArray());
			}
			CacheInfo.SetCache(CacheList.SelectedCache);
		}

		protected virtual void OnMyFindsClick (object sender, System.EventArgs e)
		{
			ExportFindsGPX();
		}

		protected virtual void OnGrabAllImages (object sender, System.EventArgs e)
		{
			GrabImagesMulti();
		}

		protected virtual void OnNewCache (object sender, System.EventArgs e)
		{
			m_app.NewCache ();
		}
#endregion







	}
}
