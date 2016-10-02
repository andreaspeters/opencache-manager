/*
 Copyright 2009 Kyle Campbell
 Licensed under the Apache License, Version 2.0 (the "License"); 
 you may not use this file except in compliance with the License. 
 You may obtain a copy of the License at 
 
 		http://www.apache.org/licenses/LICENSE-2.0 
 
 Unless required by applicable law or agreed to in writing, software distributed under the License 
 is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
 implied. See the License for the specific language governing permissions and limitations under the License. 
*/
using System;
using System.Collections.Generic;
using Gtk;
using System.Timers;
using DBus;
using ocmengine;
using ocmengine.SQLLite;
using org.freedesktop.DBus;
using Mono.Unix;

//TEMP
using System.IO;

namespace ocmgtk
{
	public class OCMApp
	{
		static OCMSplash m_splash = null;
		
		private ACacheStore m_Store;
		private OCMMainWindow m_Window;
		private IConfig m_Config;
		private List<String> m_OwnerIDs;
		private GPS m_GPSD;
		private Timer m_GPSDTimer;
		
		public ACacheStore CacheStore
		{
			get { return m_Store;}
		}
		
		public List<string> OwnerIDs
		{
			get
			{
				return m_OwnerIDs;
			}

		}
		
		DateTime m_LogDate = DateTime.Today;
		public DateTime LoggingDate
		{
			get { return m_LogDate;}
			set { m_LogDate = value;}
		}
		
		public IConfig AppConfig
		{
			get { return m_Config;}
		}
		
		public OCMApp()
		{
			m_Config = new Config();
			LoadOwnerIDs ();
		}
		
		
		private double m_CentreLat;
		public double CentreLat
		{
			get { return m_CentreLat;}
			set { 
				m_CentreLat = value;
				AppConfig.LastLat = value;
			}
		}
		
		private double m_CentreLon;
		public double CentreLon
		{
			get { return m_CentreLon;}
			set { 
				m_CentreLon = value;
				AppConfig.LastLon = value;
			}
		}
		
		private string m_CentreName;
		public string CenterName
		{
			get { return m_CentreName;}
			set { 
				m_CentreName = value;
				AppConfig.LastName = value;
			}
		}
		
		private LocationList m_Locations;
		public LocationList Locations
		{
			get { return m_Locations;}
		}
		
		private BookmarkManager m_Bookmarks;
		public BookmarkManager Bookmarks
		{
			get { return m_Bookmarks;}
		}
		
		private QuickFilters m_QuickFilters;
		public QuickFilters QuickFilterList
		{
			get { return m_QuickFilters;}
		}
		
		private GPSProfileList m_Profiles;
		public GPSProfileList Profiles
		{
			get { return m_Profiles;}
		}
		
		private EToolList m_Tools;
		public EToolList Tools
		{
			get { return m_Tools;}
		}
		
		private void LoadOwnerIDs ()
		{
			if (m_OwnerIDs == null)
				m_OwnerIDs = new List<string>();
			else
				m_OwnerIDs.Clear();
			m_OwnerIDs.Add(m_Config.OwnerID);
			if (!String.IsNullOrEmpty(m_Config.OwnerID2))
				m_OwnerIDs.Add(m_Config.OwnerID2);
			if (!String.IsNullOrEmpty(m_Config.OwnerID3))
				m_OwnerIDs.Add(m_Config.OwnerID3);
			if (!String.IsNullOrEmpty(m_Config.OwnerID4))
				m_OwnerIDs.Add(m_Config.OwnerID4);
		}
		
		public void Start()
		{
			Start(null);
		}
		
		public void Start(string filename)
		{
			if (!m_Config.WizardDone)
			{
				SetupAssistant dlg = new SetupAssistant(this);
				dlg.SetPosition (Gtk.WindowPosition.Center);
				dlg.ShowAll ();
				Application.Run();
				return;
			}
			else
			{
				CheckDBFile(m_Config.DBFile);
			}
			InitializeApp (filename, false);		
			Application.Run();
		}
		
		public void InitializeApp (string filename, bool quitAfterImport)
		{
			m_Store = new FileStore(m_Config.DBFile);
			InitalizeTracing ();
			m_Locations = LocationList.LoadLocationList();
			m_QuickFilters = QuickFilters.LoadQuickFilters();
			m_Profiles = GPSProfileList.LoadProfileList();
			m_Tools = EToolList.LoadEToolList();
			m_Bookmarks = new BookmarkManager(this);
			m_CentreLat = AppConfig.LastLat;
			m_CentreLon = AppConfig.LastLon;
			m_CentreName = AppConfig.LastName;
			m_Window = new OCMMainWindow(this);
			QuickFilter startup = m_QuickFilters.GetFilter(AppConfig.StartupFilter);
			if (startup != null)
				m_Window.CacheList.ApplyInitalQuickFilter(startup);
			
			if (filename != null)
			{
				if (filename.EndsWith(".zip"))
					ImportZip(filename);
				else
					ImportGPXFile(filename);
				if (quitAfterImport)
					this.End();
				else
					ShowMainWindow();
			}	
			else
			{
				m_splash = new OCMSplash();
				m_splash.Show();
				UpdateGUIThread();
				m_splash.Preload(this, startup);
			}
	
		}
		
		private void InitalizeTracing ()
		{
			if (AppConfig.ClearTraceLog)
			{
				if (File.Exists(m_Config.DataDirectory+"/ocm.log"))
					File.Delete(m_Config.DataDirectory + "/ocm.log");
			}
			if (AppConfig.EnableTraceLog)
					m_Store.TracingOn(m_Config.DataDirectory + "/ocm.log");
		}
		
		public void ShowMainWindow ()
		{
			m_Window.Title = Utilities.GetShortFileName(m_Config.DBFile) + " - OCM";
			m_Window.Show();
			m_Window.UpdateMapButtons();
			m_Window.CacheMap.Reload();
		}
		
		public void SelectCacheByName(string code)
		{
			m_Window.CacheList.SelectCacheByName(code);
		}
		
		public static void UpdateGUIThread()
		{
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (true);
		}
		
		public void HighlightPointOnMap(double lat, double lon)
		{
			m_Window.CacheMap.HighlightPoint(lat, lon);
		}
		
		public void UpdateStatus(int visible, int found, int disabled, int mine)
		{
			m_Window.UpdateCounts(visible, found, disabled, mine, CacheStore.CacheCount);
		}
		
		public void AddChildWaypoint()
		{
			Geocache parent = m_Window.CacheList.SelectedCache;
			AddChildWaypoint(parent.Lat, parent.Lon);
		}
		
		public void AddChildWaypoint(double lat, double lon)
		{
			Waypoint newPoint = new Waypoint ();
			Geocache parent = m_Window.CacheList.SelectedCache;
			newPoint.Symbol = "Final Location";
			newPoint.Parent = parent.Name;
			newPoint.Lat = lat;
			newPoint.Lon = lon;
			String name = "FL" + parent.Name.Substring (2);
			WaypointDialog dlg = new WaypointDialog ();
			dlg.IgnorePrefix = m_Config.IgnoreWaypointPrefixes;
			dlg.App = this;
			if (m_Config.IgnoreWaypointPrefixes)
			{
				name = parent.Name;
				dlg.IgnorePrefix = true;
			}
			name = m_Store.GetUniqueName(name);
			newPoint.Name = name;
			dlg.SetPoint (newPoint);
			if ((int)ResponseType.Ok == dlg.Run ()) {
				newPoint = dlg.GetPoint ();
				if (newPoint.Symbol == "Final Location")
					parent.HasFinal = true;
				if (!parent.Children)
					parent.Children = true;
				m_Store.AddWaypointOrCache (newPoint, false, false);
				m_Window.Refresh();
				m_Window.CacheInfo.SelectChildByName(newPoint.Name);
			}
			dlg.Hide();
		}
		
		public void EditChildWaypoint(Waypoint pt)
		{
			Geocache parent = m_Window.CacheList.SelectedCache;
			WaypointDialog dlg = new WaypointDialog ();
			string origname = pt.Name;
			dlg.App = this;
			dlg.IgnorePrefix = m_Config.IgnoreWaypointPrefixes;
			dlg.SetPoint (pt);
			if ((int)ResponseType.Ok == dlg.Run ()) {
				pt = dlg.GetPoint ();
				if (pt.Symbol == "Final Location")
					parent.HasFinal = true;
				if (!parent.Children)
					parent.Children = true;
				m_Store.AddWaypointOrCache (pt, false, false);
				if (pt.Name != origname)
					m_Store.DeleteWaypoint(origname);
				m_Window.Refresh();
				m_Window.CacheInfo.SelectChildByName(pt.Name);
			}
			dlg.Hide();
		}
		
		public void RefreshAll()
		{
			m_Window.Refresh();
		}
		
		public void EditCache ()
		{
			ModifyCacheDialog dlg = new ModifyCacheDialog();
			dlg.Cache = m_Window.CacheList.SelectedCache;
			dlg.App = this;
			dlg.IsModifyDialog = true;
			if ((int) ResponseType.Ok == dlg.Run())
			{
				m_Window.ModifyCache(dlg.Cache);
			}
		}
		
		public void NewCache ()
		{
			ModifyCacheDialog dlg = new ModifyCacheDialog();
			Geocache cache = new Geocache();
			cache.Name = CacheStore.GetUniqueName("GCXXXX");
			cache.Lat = m_Window.CacheMap.MapLat;
			cache.Lon = m_Window.CacheMap.MapLon;
			cache.CacheName = Catalog.GetString("Unnamed Cache");
			cache.Archived = false;
			cache.Available = true;
			cache.OwnerID = Catalog.GetString("Unknown");
			cache.Updated = DateTime.Now;
			cache.TypeOfCache = Geocache.CacheType.TRADITIONAL;
			dlg.Cache = cache;
			dlg.App = this;
			dlg.IsModifyDialog = false;
			if ((int) ResponseType.Ok == dlg.Run())
			{
				CacheStore.AddWaypointOrCache(dlg.Cache, false, false);
				m_Window.CacheList.Refresh();
				m_Window.CacheList.SelectCacheByName(dlg.Cache.Name);
			}
		}
		
		public void DeleteChildPoint(String name)
		{
			m_Store.DeleteWaypoint(name);
			m_Window.Refresh();
		}
		
		public static string GetOCMVersion ()
		{
			String version = "Unknown";
			System.IO.StreamReader reader = new System.IO.StreamReader (new System.IO.FileStream ("version/Version.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read));
			version = reader.ReadToEnd ();
			reader.Close ();
			return version;
		}
		
		
		public void SetInitalCacheModel(CacheStoreModel model, int visibleCount, int foundCount, 
		                                int mineCount, int disabledOrArchivedCount)
		{
			m_Window.CacheList.SetInitalModel(model);
			m_Window.CacheMap.SetInitalCaches(model.Caches);
			m_Window.CacheMap.UpdateChildWaypoints();
			m_Window.UpdateCounts(visibleCount, foundCount, mineCount, disabledOrArchivedCount, CacheStore.CacheCount);
		}
		
		public void End()
		{
			m_Store.Dispose();
			m_Window.SaveWindowSettings();
			Application.Quit();
		}
		
		public void SetDBFile(string filename)
		{
			SetDBFile(filename, false);
		}
		
		public void SetDBFile(string filename, bool isNew)
		{
			if (!isNew)
				CheckDBFile(filename);
			FilterList advanced = null;
			List<FilterList> comboFilt = null;
			if (m_Store != null)
			{
				advanced = m_Store.AdvancedFilters;
				comboFilt = m_Store.CombinationFilter;
				m_Store.Dispose();
			}
			m_Store = new FileStore(filename);
			m_Store.AdvancedFilters = advanced;
			m_Store.CombinationFilter = comboFilt;
			m_Config.DBFile = filename;
			m_Window.Title = Utilities.GetShortFileName(filename) + "- OCM";
		}
		
		public void CorrectCoordinates()
		{
			m_Window.CorrectCoordinates();
		}
		
		public void UpdateFilters()
		{
		///	m_Window.UpdateFilters();
		}
		
		public void DoAutoUpdateCheck()
		{
			if (!m_Config.CheckForUpdates)
				return;
			if (DateTime.Now < m_Config.NextUpdateCheck)
				return;
			string ver;
			if (UpdatesAvailable(out ver))
			{
				MessageDialog dlg = new MessageDialog (m_Window, DialogFlags.Modal, MessageType.Info, ButtonsType.YesNo, Catalog.GetString ("A new version \"{0}\" of OCM is available" + "\nWould you like to go to the download page now?"), ver);
				if ((int)ResponseType.Yes == dlg.Run ()) 
				{
					dlg.Hide ();
					System.Diagnostics.Process.Start ("http://sourceforge.net/projects/opencachemanage/files/");
				} 
				else
					dlg.Hide ();
			}
			m_Config.NextUpdateCheck = DateTime.Now.AddDays (m_Config.UpdateInterval);
		}
		
		
		public bool UpdatesAvailable (out string latestVer)
		{
			try 
			{
				string ver = UpdateChecker.GetLatestVer ();
				latestVer = ver;
				if (ver != GetOCMVersion ()) 
				{
					return true;
				}
				return false;
				
			}
			catch (Exception) 
			{
				MessageDialog dlg = new MessageDialog (m_Window, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, Catalog.GetString ("Unable to check for updates, check your " + "network connection."));
				dlg.Run ();
				dlg.Hide ();
				latestVer = "-1";
				return false;
			}
		}
		
		public void EnableGPS()
		{
			EnableGPS(true);
		}
				
		public void EnableGPS (bool doInit)
		{
			m_GPSD = new GPS ();
			m_GPSDTimer = new Timer (m_Config.GPSDPoll * 1000);
			m_GPSDTimer.AutoReset = true;
			m_GPSDTimer.Enabled = true;
			m_GPSDTimer.Elapsed += HandleM_gpsTimerElapsed;
			if (doInit)
			{
				m_Window.SetLocation("GPSD", m_GPSD.Lat, m_GPSD.Lon);
				Timer init = new Timer (1000);
				init.AutoReset = false;
				init.Elapsed += HandleM_gpsTimerElapsed;
				init.Start ();
			}
		}
		
		public void ImportGPXFile(String file)
		{
			m_Window.ImportGPXFile(file, m_Config.AutoCloseWindows);
		}
		
		public void ImportZip(String file)
		{
			m_Window.ImportZip(file);
		}

		void HandleM_gpsTimerElapsed (object sender, ElapsedEventArgs e)
		{
			Application.Invoke (delegate {
				if (m_Config.GPSDAutoMoveMap)
				{
					m_Window.SetLocation("GPSD", m_GPSD.Lat, m_GPSD.Lon);
				}
				else
				{
					m_CentreName = "GPSD";
					m_CentreLat = m_GPSD.Lat;
					m_CentreLon = m_GPSD.Lon;
					m_Window.CacheList.Refresh();
				}
			});
			
			
		}
		
		public void CheckDBFile(String filename)
		{
			if (!File.Exists(filename))
			{
				NoDBDialog dlg = new NoDBDialog();
				int response = dlg.Run();
				if (response == (int) ResponseType.Yes)
				 	ShowOpenDBDialog();
				else if (response == (int) ResponseType.No)
					CreateDB();
				else
					End();
				CheckDBFile(AppConfig.DBFile);
			}
		}
		
		public void CreateDB ()
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Create database"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Save"), ResponseType.Accept);
			dlg.SetCurrentFolder (AppConfig.DataDirectory);
			dlg.CurrentName = "newdb.ocm";
			FileFilter filter = new FileFilter ();
			filter.Name = "OCM Databases";
			filter.AddPattern ("*.ocm");
			dlg.AddFilter (filter);
			if (dlg.Run () == (int)ResponseType.Accept) {
			 if (System.IO.File.Exists (dlg.Filename)) {
					dlg.Hide ();
					MessageDialog mdlg = new MessageDialog (null, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, Catalog.GetString ("Are you sure you want to overwrite '{0}'"), dlg.Filename);
					if ((int)ResponseType.No == mdlg.Run ()) {
						mdlg.Hide ();
						return;
					} else {
						mdlg.Hide ();
						System.IO.File.Delete (dlg.Filename);
					}
				}
				dlg.Hide();
				AppConfig.DBFile = dlg.Filename;
				m_Store = new FileStore(dlg.Filename);
				return;
			}
			dlg.Destroy ();
			return;
		}
		
		private void ShowOpenDBDialog ()
		{
			try {
				FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Open Database"), null, 
				                                               FileChooserAction.Open,
				                                               Catalog.GetString ("Cancel"), 
				                                               ResponseType.Cancel, Catalog.GetString ("Open"), 
				                                               ResponseType.Accept);
				dlg.SetCurrentFolder (AppConfig.DataDirectory);
				FileFilter filter = new FileFilter ();
				filter.Name = "OCM Databases";
				filter.AddPattern ("*.ocm");
				dlg.AddFilter (filter);
				
				if (dlg.Run () == (int)ResponseType.Accept) {
					dlg.Hide ();
					AppConfig.DBFile = dlg.Filename;
					m_Store = new FileStore(dlg.Filename);
				} else {
					dlg.Hide ();
				}
				
			} catch (Exception) {
				return;
			}

		}


		public void DisableGPS ()
		{
			if (null == m_GPSDTimer)
				return;
			m_GPSDTimer.AutoReset = false;
			m_GPSDTimer.Stop ();
			m_GPSDTimer = null;
			m_GPSD = null;
		}
		
		
		public static void ShowException(Exception e)
		{
			ErrorDialog dlg = new ErrorDialog(e);
			dlg.Run();
		}
		
		public static void Main (string[] args)
		{
			
			Application.Init ();
			// Set the localeDirectory right both for developement or for installed versions
			String localeDirectory = Paths.LOCALE_DIR;
			if (localeDirectory.Contains("@" + "expanded_datadir" + "@")) {
				localeDirectory = "./locale";
			}
			
			System.Console.WriteLine(localeDirectory);
		
			Mono.Unix.Catalog.Init ("opencachemanager", localeDirectory);
			OCMApp app = new OCMApp();
			
			
			try
			{
				//##AP BusG.Init ();
				Bus bus = Bus.Session;
				string busName = "org.ocm.dbus";
				if (bus.RequestName (busName) != RequestNameReply.PrimaryOwner) 
				{
					IDBusComm comm = bus.GetObject<IDBusComm> (busName, new ObjectPath ("/org/ocm/dbus"));
					if (args != null)
					{
						if (args.Length > 0) 
							comm.ImportGPX (args[0]);
					}
					comm.ShowOCM();
					return;
				}
				else 
				{
					DBusComm comm = new DBusComm (app);
					bus.Register (new ObjectPath ("/org/ocm/dbus"), comm);
				}
			}
			catch
			{
				System.Console.Error.WriteLine("NO SESSION DBUS RUNNING");
			}
			if (args != null)
				if (args.Length > 0)
					app.Start(args[0]);
			else
				app.Start();
		}
			
	/* OLD APP
			try
			{
				BusG.Init ();
				Bus bus = Bus.Session;
				string busName = "org.ocm.dbus";
				if (bus.RequestName (busName) != RequestNameReply.PrimaryOwner) 
				{
					IDBusComm comm = bus.GetObject<IDBusComm> (busName, new ObjectPath ("/org/ocm/dbus"));
					if (args != null)
					{
						if (args.Length > 0) 
							comm.ImportGPX (args[0]);
					}
					comm.ShowOCM();
					return;
				}
				else 
				{
					DBusComm comm = new DBusComm ();
					bus.Register (new ObjectPath ("/org/ocm/dbus"), comm);
				}
			}
			catch
			{
				System.Console.Error.WriteLine("NO SESSION DBUS RUNNING");
			}
			
			if (args != null)
				if (args.Length > 0)
					m_file = args[0];
			//System.Console.WriteLine("Path is " + "@expanded_datadir@/locale");
			// Set the localeDirectory right both for developement or for installed versions
			String localeDirectory = "@expanded_datadir@/locale";
			if (localeDirectory.Contains("@" + "expanded_datadir" + "@")) {
				localeDirectory = "./locale";
			}
			Mono.Unix.Catalog.Init ("opencachemanager", localeDirectory);
			//Mono.Unix.Catalog.Init ("opencachemanager", "./locale");
			//Mono.Unix.Catalog.Init ("opencachemanager", "@expanded_datadir@/locale");
			Config config = new Config();
			bool runWizard = !config.WizardDone;
			
			if (runWizard) {
				UIMonitor.getInstance ().RunSetupAssistant ();
			} 
			else
			{
				ShowSplash();
			}			
			Application.Run ();
			
		}
		
		public static void ShowSplash()
		{
			m_splash = new OCMSplash();
			m_splash.ShowNow();
			System.Timers.Timer splashtime = new System.Timers.Timer();
			splashtime.AutoReset = false;
			splashtime.Interval = 1000;
			splashtime.Elapsed += HandleSplashtimeElapsed;
			splashtime.Start();
		}

		static void HandleSplashtimeElapsed (object sender, ElapsedEventArgs e)
		{
			Application.Invoke(delegate{
				ShowMain();
			});
		}
		
		public static void ShowMain()
		{
			if (m_splash != null)
			{
				m_splash.Hide();
				m_splash.Dispose();
			}
			
			MainWindow win = new MainWindow();
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
			
			if (m_file != null)
			{
				UIMonitor.getInstance().LoadConfig(false);
				if (m_file.EndsWith(".ocm"))
				{
					UIMonitor.getInstance().SetCurrentDB(m_file, true);
				}
				else
				{
					
					if (m_file.EndsWith(".zip"))
					{
						UIMonitor.getInstance().ImportZip(m_file);
					}
					else
					{
						UIMonitor.getInstance().ImportGPXFile(m_file);
					}
				}
			}
			else
			{
				UIMonitor.getInstance().LoadConfig(true);
			}
		}	*/	
	}
}
