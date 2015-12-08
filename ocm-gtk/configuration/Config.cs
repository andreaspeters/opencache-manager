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
using System.Collections.Generic;
using ocmengine;
using GConf;
using Mono.Unix;

namespace ocmgtk
{


	public class Config:IConfig
	{
		private Client m_client;
		public SolvedMode SolvedModeState
		{
			get 
			{ 
				return (SolvedMode) Enum.Parse(typeof(SolvedMode), this.Get("/apps/ocm/solved_mode", 
				                                                                 SolvedMode.PUZZLES.ToString()) 
				                                     							as string); 
			}
			set 
			{ 
				this.Set("/apps/ocm/solved_mode", value.ToString()); 
			}
		}
		
		public bool ShowDNFIcon
		{
				get { return (bool) this.Get("/apps/ocm/dnficon", true);}
				set { this.Set("/apps/ocm/dnficon", value);}
		}
		
		public double LastLat
		{
			get { return (double) this.Get ("/apps/ocm/lastlat", 0.0);}
			set { this.Set("/apps/ocm/lastlat", value);}
		}
		
		public double LastLon
		{
			get { return (double) this.Get ("/apps/ocm/lastlon", 0.0);}
			set { this.Set("/apps/ocm/lastlon", value);}
		}
		
		public string LastName
		{
			get { return (string) this.Get("/apps/ocm/lastname", Catalog.GetString("Home"));}
			set { this.Set("/apps/ocm/lastname",value);}
		}
		
		public bool UseDirectEntryMode
		{
			get { return (bool) this.Get("/apps/ocm/direct_entry", false);}
			set { this.Set("/apps/ocm/direct_entry", value);}
		}
		
		public double HomeLat
		{
			get { return (double) this.Get ("/apps/ocm/homelat", 0.0);}
			set { this.Set ("/apps/ocm/homelat", value);}
		}
		
		public double HomeLon
		{
			get { return (double) this.Get ("/apps/ocm/homelon", 0.0);}
			set { this.Set ("/apps/ocm/homelon", value);}
		}
		
		public int MapPoints
		{
			get {return (int) this.Get("/apps/ocm/mappoints", 100);}
			set { this.Set("/apps/ocm/mappoints", value);}
		}
		
		public string OwnerID
		{
			get { return (string)this.Get ("/apps/ocm/memberid", String.Empty);}
			set { this.Set ("/apps/ocm/memberid",value);}
		}
		
		public string OwnerID2
		{
			get { return (string)this.Get ("/apps/ocm/memberid2", String.Empty);}
			set { this.Set ("/apps/ocm/memberid2",value);}
		}
		
		public string OwnerID3
		{
			get { return (string)this.Get ("/apps/ocm/memberid3", String.Empty);}
			set { this.Set ("/apps/ocm/memberid3",value);}
		}
		
		public string OwnerID4
		{
			get { return (string)this.Get ("/apps/ocm/memberid4", String.Empty);}
			set { this.Set ("/apps/ocm/memberid4",value);}
		}
		
		public Boolean ImperialUnits
		{
			get { return (Boolean) this.Get("/apps/ocm/imperial", false);}
			set { this.Set("/apps/ocm/imperial", value);}
		}
		
		public int WindowWidth
		{
			get { return  (int) this.Get("/apps/ocm/winwidth", 1024);}
			set { this.Set("/apps/ocm/winwidth",value);}
		}
		
		public int WindowHeight
		{
			get { return (int) this.Get("/apps/ocm/winheight", 768);}
			set { this.Set("/apps/ocm/winheight", value);}
		}
		
		public int VBarPosition
		{
			get { return (int) this.Get("/apps/ocm/vpos", 300);}
			set { this.Set("/apps/ocm/vpos", value);}
		}
		
		public int HBarPosition
		{
			get { return (int) this.Get("/apps/ocm/hpos", 400);}
			set { this.Set("/apps/ocm/hpos", value);}
		}
		
		public string MapType
		{
			get { return (string) this.Get("/apps/ocm/defmap", "osm");}
			set { this.Set("/apps/ocm/defmap", value);}
		}
		
		public string DBFile
		{
			get { return (string)this.Get ("/apps/ocm/currentdb", String.Empty);}
			set { this.Set("/apps/ocm/currentdb", value);}
		}
		
		public string DataDirectory
		{
			get { return this.Get("/apps/ocm/datadir", System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments)) as string;}
			set { this.Set("/apps/ocm/datadir", value);}
		}
		
		public bool UseOfflineLogging
		{
			get { return (Boolean) this.Get("/apps/ocm/offlinelogging", false);}
			set { this.Set("/apps/ocm/offlinelogging", value);}
		}
		
		
		public string FieldNotesFile
		{
			get { return DataDirectory + "/cachelogs.txt";}
		}
		public string ImportDirectory
		{
			get { return (String) this.Get("/apps/ocm/importdir", System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));}
			set { this.Set("/apps/ocm/importdir", value);}
		}
		
		public bool UseGPSD
		{
			get { return (Boolean) this.Get("/apps/ocm/gpsd/onstartup", false);}
			set { this.Set("/apps/ocm/gpsd/onstartup", value);}
		}
		
		public int GPSDPoll
		{
			get { return (int) this.Get("/apps/ocm/gpsd/poll", 30);}
			set { this.Set("/apps/ocm/gpsd/poll", value);}
		}
		
		public bool GPSDAutoMoveMap
		{
			get { return (bool) this.Get("/apps/ocm/gpsd/recenter", true);}
			set { this.Set("/apps/ocm/gpsd/recenter", value); }
		}
		
		public string StartupFilter
		{
			get { return (string)this.Get ("/apps/ocm/startupfilter", String.Empty);}
			set { this.Set ("/apps/ocm/startupfilter", value);}
		}
		
		public bool ShowNearby
		{
			get { return (Boolean) this.Get("/apps/ocm/shownearby", true);}
			set { this.Set("/apps/ocm/shownearby", value);}
		}
		
		public bool ShowAllChildren
		{
			get { return (bool) this.Get("/apps/ocm/showallchildren", false);}
			set { this.Set("/apps/ocm/showallchildren",value);}
		}
		
		public bool WizardDone
		{
			get { return Boolean.Parse( (string) this.Get("/apps/ocm/wizardone", "false"));}
			set { this.Set("/apps/ocm/wizardone",value?"true":"false");}
		}
		
		public string GPSProf
		{
			get { return this.Get("/apps/ocm/gps/currentprof", null) as string;}
			set 
			{ 
				if (value == null)
					UnsetKey("/apps/ocm/gps/currentprof");
				else
					this.Set("/apps/ocm/gps/currentprof", value);
			}
		}
		
		public bool IgnoreWaypointPrefixes
		{
			get { return (bool) this.Get("/apps/ocm/noprefixes", false);}
			set { this.Set("/apps/ocm/noprefixes", value);}
		}
		
		public bool CheckForUpdates
		{
			get { return (bool) this.Get("/apps/ocm/update/checkForUpdates", true);}
			set {this.Set("/apps/ocm/update/checkForUpdates", value);}
		}

		public bool AutoCloseWindows
		{
			get { return (bool) this.Get("/apps/ocm/autoCloseWindows", false);}
			set {this.Set("/apps/ocm/autoCloseWindows", value);}	
		}
		
		public bool AutoSelectCacheFromMap
		{
			get { return (bool) this.Get("/apps/ocm/map/autoSelectCacheFromMap", false);}
			set {this.Set("/apps/ocm/map/autoSelectCacheFromMap", value);}	
		}
		
		public DateTime LastGPSFieldNoteScan
		{
			get {
				string dt = (string) this.Get("/apps/ocm/gps/lastscan", DateTime.MinValue.ToString("o"));
				return DateTime.Parse(dt);
			}
			set
			{
				 this.Set("/apps/ocm/gps/lastscan", value.ToString("o"));
			}
		}
		
		public DateTime NextUpdateCheck
		{
			get {
				string dt = (string) this.Get("/apps/ocm/update/nextcheck", DateTime.Today.ToString("o"));
				return DateTime.Parse(dt);
			}
			set
			{
				 this.Set("/apps/ocm/update/nextcheck", value.ToString("o"));
			}
		}
		
		public int UpdateInterval
		{
			get { return (int) this.Get("/apps/ocm/update/updateInterval",7);}
			set { this.Set("/apps/ocm/update/updateInterval",value);}
		}
		
		public bool ImportPreventStatusOverwrite
		{
			get { return (Boolean) this.Get("/apps/ocm/importnooverwrite", false);}
			set { this.Set("/apps/ocm/importnooverwrite",value);}
		}
		
		public bool ImportPurgeOldLogs
		{
			get { return (Boolean) this.Get("/apps/ocm/importpurgelogs", false);}
			set { this.Set("/apps/ocm/importpurgelogs",value);}
		}
		
		public bool ImportIgnoreExtraFields
		{
			get { return (Boolean) this.Get("/apps/ocm/importignoregsak", false);}
			set { this.Set("/apps/ocm/importignoregsak",value);}
		}
		
		public bool ImportDeleteFiles
		{
			get { return (Boolean) this.Get("/apps/ocm/importdeletefiles", false);}
			set { this.Set("/apps/ocm/importdeletefiles",value);}
		}
		
		public string ImportBookmarkList
		{
			get { return (string) this.Get("/apps/ocm/importbookmark", null);}
			set {
				if (value == null)
					UnsetKey("/apps/ocm/importbookmark");
				else
					this.Set("/apps/ocm/importbookmark",value);
			}
		}
		
		public int ExportLimitCaches
		{
			get { return (int) this.Get("/apps/ocm/exportlimitcaches", -1);}
			set { this.Set("/apps/ocm/exportlimitcaches", value);}
		}
		
		public bool ExportChildren{
			get { return (bool) this.Get("/apps/ocm/exportchildren", true);}
			set { this.Set("/apps/ocm/exportchildren", value);}
		}
			
		public bool ExportPaperlessOptions
		{
			get { return (bool) this.Get("/apps/ocm/exportpaperless", true);}
			set { this.Set("/apps/ocm/exportpaperless", value);}
		}
			
		public bool ExportExtraFields
		{
			get { return (bool) this.Get("/apps/ocm/exportextrafields", false);}
			set { this.Set("/apps/ocm/exportextrafields", value);}
		}
			
		public bool ExportCustomSymbols
		{
			get { return (bool) this.Get("/apps/ocm/exportcustomsym", false);}
			set { this.Set("/apps/ocm/exportcustomsym", value);}
		}
		
		public int ExportLimitLogs
		{
			get { return (int) this.Get("/apps/ocm/exportlimitlogs", -1);}
			set { this.Set("/apps/ocm/exportlimitlogs", value);}
		}
		
		public bool ExportIncludeAttributes
		{
			get { return (bool) this.Get("/apps/ocm/exportattrs", false);}
			set { this.Set("/apps/ocm/exportattrs", value);}
		}
		
		public bool ExportAsPlainText
		{
			get { return (bool) this.Get("/apps/ocm/exportastext", false);}
			set { this.Set("/apps/ocm/exportastext", value);}
		}
		
		public ocmengine.WaypointDescMode ExportWaypointDescMode
		{
			get { string val = this.Get("/apps/ocm/exportdescmode", ocmengine.WaypointDescMode.DESC.ToString()) as string;
				return (ocmengine.WaypointDescMode)Enum.Parse(typeof(ocmengine.WaypointDescMode), val);
			}
			set { this.Set("/apps/ocm/exportdescmode", value.ToString());}
		}
		
		public ocmengine.WaypointNameMode ExportWaypointNameMode
		{
			get { string val = this.Get("/apps/ocm/exportnamemode", ocmengine.WaypointNameMode.CODE.ToString()) as string;
				return (ocmengine.WaypointNameMode)Enum.Parse(typeof(ocmengine.WaypointNameMode), val);
			}
			set { this.Set("/apps/ocm/exportnamemode", value.ToString());}
		}
		
		public string ExportPOIFile
		{
			get { return (string) this.Get("/apps/ocm/exportpoifile", System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments) + "/geocaches.gpi");}
			set { this.Set("/apps/ocm/exportpoifile", value);}
		}
		
		public int ExportPOILogLimit
		{
			get { return (int) this.Get("/apps/ocm/exportpoilogs", -1);}
			set { this.Set("/apps/ocm/exportpoilogs", value);}
		}
		
		public bool ExportPOIForcePlain
		{
			get { return (bool) this.Get("/apps/ocm/exportpoiastext", false);}
			set { this.Set("/apps/ocm/exportpoiastext", value);}
		}
		
		public double ExportPOIProxDist
		{
			get { return (double) this.Get("/apps/ocm/exportpoidist", -1d);}
			set { this.Set("/apps/ocm/exportpoidist", value);}
		}
		
		public string ExportPOIProxUnits
		{
			get { return this.Get("/apps/ocm/exportpoiproxunit", "m") as string;}
			set { this.Set("/apps/ocm/exportpoiproxunit", value);}
		}
		
		public WaypointNameMode ExportPOINameMode
		{
			get 
			{ 
				return (WaypointNameMode) Enum.Parse(typeof(WaypointNameMode), this.Get("/apps/ocm/exportpoinamemode", 
				                                                                 WaypointNameMode.CODE.ToString()) 
				                                     							as string); 
			}
			set 
			{ 
				this.Set("/apps/ocm/exportpoinamemode", value.ToString()); 
			}
		}
		
		public WaypointDescMode ExportPOIDescMode
		{
			get 
			{ 
				return (WaypointDescMode) Enum.Parse(typeof(WaypointDescMode), this.Get("/apps/ocm/exportpoidescmode", 
				                                                                 WaypointDescMode.DESC.ToString()) 
				                                     							as string); 
			}
			set 
			{ 
				this.Set("/apps/ocm/exportpoidescmode", value.ToString()); 
			}
		}

		public string ExportPOICategory
		{
			get { return (string) this.Get("/apps/ocm/exportpoicat", "Geocaches");}
			set { this.Set("/apps/ocm/exportpoicat", value);}
		}
		
		public int ExportPOICacheLimit
		{
			get { return (int) this.Get("/apps/ocm/exportpoilimit", -1);}
			set { this.Set("/apps/ocm/exportpoilimit", value);}
		}
		
		public bool ExportPOIIncludeChildren
		{
			get { return (bool) this.Get("/apps/ocm/exportpoichildren", false);}
			set { this.Set("/apps/ocm/exportpoichildren", value);}
		}
		
		public string ExportPOIBitmap
		{
			get { return (string) this.Get("/apps/ocm/exportpoibmp", null);}
			set { 
				if (value == null)
					this.UnsetKey("/apps/ocm/exportpoibmp");
				else
					this.Set("/apps/ocm/exportpoibmp", value);
			}
		}
		
		public List<MapDescription> OpenLayerMaps
		{
			get { 
				string xmlString = (string) this.Get("/apps/ocm/maps/openlayermaps", null);
				if (string.IsNullOrEmpty(xmlString)) {
					return MapManager.GetMapsFromFile("./maps/defaultmaps.xml");
				}
				else {
					return MapManager.GetMapsFromString(xmlString);
				}
			}
			set { this.Set("/apps/ocm/maps/openlayermaps", MapManager.CreateXML(value)); }
		}
		
		public bool ShowDiffTerrIcon
		{
			get { return (bool) this.Get("/apps/ocm/showdiffterr", true);}
			set { this.Set("/apps/ocm/showdiffterr", value);}
		}
		
		public bool EnableTraceLog
		{
			get { return (bool) this.Get("/apps/ocm/enabletrace", false);}
			set { this.Set("/apps/ocm/enabletrace", value);}
		}
		
		public bool ClearTraceLog
		{
			get { return (bool) this.Get("/apps/ocm/cleartrace", false);}
			set { this.Set("/apps/ocm/cleartrace", value);}
		}
		
		public bool ShowStaleCaches
		{
			get { return (bool) this.Get("/apps/ocm/showstale", true);}
			set { this.Set("/apps/ocm/showstale", value);}
		}
		
		public int StaleCacheInterval
		{
			get { return (int) this.Get("/apps/ocm/staleinterval", 60);}
			set { this.Set("/apps/ocm/staleinterval", value);}
		}
	
		public void CheckForDefaultGPS(GPSProfileList list, OCMMainWindow win)
		{
			string defName = this.GPSProf;
			string defType = this.Get("/apps/ocm/gps/type", null) as string;
			if (defName == null && defType != null)
			{
				// Check for legacy GPS Config (pre 0.23) or use Garmin GPX as
				// default config.
				GPSProfile profile = new GPSProfile();
				profile.Name = "Default";
				profile.CacheLimit = (int) this.Get("/apps/ocm/gps/limit", -1);
			
				profile.BabelFormat = this.Get("/apps/ocm/gps/type", "OCM_GPX") as string;
				string nm = this.Get("/apps/ocm/gps/namemode", WaypointNameMode.CODE.ToString()) as string;
				profile.NameMode = (WaypointNameMode) Enum.Parse(typeof(WaypointNameMode), nm);
				string dm = this.Get("/apps/ocm/gps/descmode", WaypointDescMode.DESC.ToString()) as string;
				profile.DescMode = (WaypointDescMode) Enum.Parse(typeof(WaypointDescMode), dm);
				profile.LogLimit = (int) this.Get("/apps/ocm/gps/loglimit", -1);
				profile.IncludeAttributes = (bool) this.Get("/apps/ocm/gps/incattr", false);
				profile.OutputFile = this.Get("/apps/ocm/gps/file", "/media/GARMIN/Garmin/GPX/geocaches.gpx") as string;
				if (profile.BabelFormat == "delgpx")
					profile.FieldNotesFile = "/media/EM_USERMAPS/FieldNotes.txt";
				else if (profile.BabelFormat == "OCM_GPX")
					profile.FieldNotesFile = "/media/GARMIN/Garmin/geocache_visits.txt";
				UpgradeWaypointMappings(profile);
				list.AddProfile(profile);
				GPSProf = profile.Name;
				win.RebuildProfiles();
				
				//Cleanup legacy keys
				UnsetKey("/apps/ocm/gps/limit");
				UnsetKey("/apps/ocm/gps/type");
				UnsetKey("/apps/ocm/gps/namemode");
				UnsetKey("/apps/ocm/gps/descmode");
				UnsetKey("/apps/ocm/gps/loglimit");
				UnsetKey("/apps/ocm/gps/incattr");	
				UnsetKey("/apps/ocm/gps/file");
			}
		}
		
		public bool MapPopups
		{
			get { return (bool) this.Get("/apps/ocm/mappopups", true);}
			set { this.Set("/apps/ocm/mappopups", value);}
		}
		
		public bool ShowNewCaches
		{
			get { return (bool) this.Get("/apps/ocm/shownewcaches", true);}
			set { this.Set("/apps/ocm/shownewcaches", value);}
		}
		
		public int NewCacheInterval
		{
			get { return (int) this.Get("/apps/ocm/newcacheinterval", 14);}
			set { this.Set("/apps/ocm/newcacheinterval", value);}
		}
		
		public bool ShowRecentDNF
		{
			get { return (bool) this.Get("/apps/ocm/showrecentdnf", true);}
			set { this.Set("/apps/ocm/showrecentdnf", value);}
		}
		
		private void UpgradeWaypointMappings(GPSProfile profile)
		{
			Dictionary<string, string> mappings = new Dictionary<string, string>();
			mappings.Add("Geocache|Traditional Cache", this.Get("/apps/ocm/wmappings/Geocache_Traditional_Cache", "Geocache") as string);
			mappings.Add("Geocache|Unknown Cache",this.Get("/apps/ocm/wmappings/Geocache_Unknown_Cache", "Geocache") as string);
			mappings.Add("Geocache|Virtual Cache", this.Get("/apps/ocm/wmappings/Geocache_Virtual_Cache", "Geocache") as string);
			mappings.Add("Geocache|Multi-cache", this.Get("/apps/ocm/wmappings/Geocache_Multi-cache", "Geocache") as string);
			mappings.Add("Geocache|Project APE Cache", this.Get("/apps/ocm/wmappings/Geocache_Project_APE_Cache", "Geocache") as string);
			mappings.Add("Geocache|Cache In Trash Out Event", this.Get("/apps/ocm/wmappings/Geocache_Cache_In_Trash_Out_Event", "Geocache") as string);
			mappings.Add("Geocache|Earthcache", this.Get("/apps/ocm/wmappings/Geocache_Earthcache", "Geocache") as string);
			mappings.Add("Geocache|Event Cache", this.Get("/apps/ocm/wmappings/Geocache_Event_Cache", "Geocache") as string);
			mappings.Add("Geocache|Letterbox Hybrid", this.Get("/apps/ocm/wmappings/Geocache_Letterbox_Hybrid", "Geocache") as string);
			mappings.Add("Geocache|GPS Adventures Exhibit",this.Get("/apps/ocm/wmappings/Geocache_GPS_Adventures_Exhibit", "Geocache") as string);
			mappings.Add("Geocache|Mega-Event Cache", this.Get("/apps/ocm/wmappings/Geocache_Mega-Event_Cache", "Geocache") as string);
			mappings.Add("Geocache|Locationless Cache",this.Get("/apps/ocm/wmappings/Geocache_Locationless_Cache", "Geocache") as string);
			mappings.Add("Geocache|Webcam Cache", this.Get("/apps/ocm/wmappings/Geocache_Webcam_Cache", "Geocache") as string);
			mappings.Add("Geocache|Wherigo Cache", this.Get("/apps/ocm/wmappings/Geocache_Wherigo_Cache", "Geocache") as string);
			mappings.Add("Geocache", this.Get("/apps/ocm/wmappings/Geocache", "Geocache") as string);
			mappings.Add("Geocache Found", this.Get("/apps/ocm/wmappings/Geocache_Found", "Geocache Found") as string);
			mappings.Add("Waypoint|Final Location", this.Get("/apps/ocm/wmappings/Waypoint_Final_Location", "Pin, Blue") as string);
			mappings.Add("Waypoint|Parking Area", this.Get("/apps/ocm/wmappings/Waypoint_Parking_Area", "Parking Area") as string);
			mappings.Add("Waypoint|Reference Point", this.Get("/apps/ocm/wmappings/Waypoint_Reference_Point", "Pin, Green") as string);
			mappings.Add("Waypoint|Question to Answer", this.Get("/apps/ocm/wmappings/Waypoint_Question_to_Answer", "Pin, Red") as string);
			mappings.Add("Waypoint|Stages of a Multicache", this.Get("/apps/ocm/wmappings/Waypoint_Stages_of_a_Multicache", "Pin, Red") as string);
			mappings.Add("Waypoint|Trailhead", this.Get("/apps/ocm/wmappings/Waypoint_Trailhead", "Trail Head") as string);
			mappings.Add("Waypoint|Other", this.Get("/apps/ocm/wmappings/Waypoint_Other", "Pin, Green") as string);
			profile.WaypointMappings = mappings;
			
			//Cleanup legacy keys
			UnsetKey("/apps/ocm/wmappings/Geocache_Traditional_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Unknown_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Virtual_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Multi-cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Project_APE_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Cache_In_Trash_Out_Event");
			UnsetKey("/apps/ocm/wmappings/Geocache_Earthcache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Event_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Letterbox_Hybrid");
			UnsetKey("/apps/ocm/wmappings/Geocache_GPS_Adventures_Exhibit");
			UnsetKey("/apps/ocm/wmappings/Geocache_Mega-Event_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Locationless_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Webcam_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Webcam_cache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Wherigo_Cache");
			UnsetKey("/apps/ocm/wmappings/Geocache");
			UnsetKey("/apps/ocm/wmappings/Geocache_Found");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Final_Location");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Parking_Area");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Reference_Point");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Stages_of_a_Multicache");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Question_to_Answer");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Trailhead");
			UnsetKey("/apps/ocm/wmappings/Waypoint_Other");
		}
		
		private void UnsetKey(String keyname)
		{
			System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
			info.FileName = "/usr/bin/gconftool-2";
			info.Arguments = "--unset " + keyname;
			System.Diagnostics.Process.Start(info);
		}

		public Config ()
		{
			m_client = new Client();
		}
		
		private object Get(String key, Object def)
		{
			try
			{
				return m_client.Get(key);
			}
			catch
			{
				return def;
			}
		}
		
		private void Set(String key, Object val)
		{
			m_client.Set(key, val);
		}
		
		
	}
}
