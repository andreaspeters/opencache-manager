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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using ocmengine;

namespace ocmengine
{
	public class ParseEventArgs:EventArgs
	{
		private string m_message;
		
		public string Message
		{
			get { return m_message;}
		}
		
		public ParseEventArgs(String message):base()
		{
			m_message = message;
		}
	}
	
	public class GPXParser
	{
		private ACacheStore m_store = null;
		public delegate void ParseEventHandler(object sender, EventArgs args);
		public event ParseEventHandler ParseWaypoint;
		public event ParseEventHandler Complete;

		public List<string> m_ownid = new List<string>();
		public string m_source = "unknown";
		public List<string> CacheOwner
		{
			set {m_ownid = value;}
		}	
		
		bool m_ignoreExtraFields = false;
		public bool IgnoreExtraFields
		{
			set { m_ignoreExtraFields = value;}
		}
		
		bool m_purgeLogs = false;
		public bool PurgeLogs
		{
			set { m_purgeLogs = value;}
		}
		
		bool m_preserveFound = false;
		public bool PreserveFound
		{
			set { m_preserveFound = value;}
		}
		
		string m_bookmark = null;
		public string Bookmark
		{
			set { m_bookmark = value;}
		}
		
		
		private DateTime gpx_date;
		
		private Boolean m_cancel = false;
		private System.Data.IDbTransaction m_trans = null;
		public Boolean Cancel
		{
			set { m_cancel = true;}
			get { return m_cancel;}
		}
		
		public void StartUpdate(ACacheStore store)
		{
			store.StartUpdate();
		}
		
		public void EndUpdate(ACacheStore store)
		{
			store.CompleteUpdate();
		}
		
		public int PreParseForSingle(FileStream fs, ACacheStore store)
		{
			XmlReader rdr = XmlReader.Create(fs);
			rdr.Settings.IgnoreWhitespace = true;
			int count = 0;
			List<String> waypoints = new List<String>();
			while (rdr.Read())
			{
				if (rdr.Name == "wpt" && rdr.IsStartElement())
				{
					count++;
				}
				else if (rdr.Name == "waypoint" && rdr.IsStartElement())
				{
					count++;
				}
				else if (rdr.LocalName == "name" && rdr.IsStartElement())
				{
					waypoints.Add(ACacheStore.Escape(rdr.ReadElementContentAsString()));
				}
			}
			rdr.Close();
			store.PurgeAllTravelBugs(waypoints.ToArray());
			if (m_purgeLogs)
				store.PurgeAllLogs(waypoints.ToArray());
			store.PurgeAllAttributes(waypoints.ToArray());
			return count;
		}
		
		public int parseTotal(FileStream fs)
		{
			XmlReader rdr = XmlReader.Create(fs);
			rdr.Settings.IgnoreWhitespace = true;
			int count = 0;
			while (rdr.Read())
			{
				if (rdr.Name == "wpt" && rdr.IsStartElement())
				{
					count++;
				}
				else if (rdr.Name == "waypoint" && rdr.IsStartElement())
				{
					count++;
				}
			}
			rdr.Close();
			return count;
		}
		
		public void clearForImport(FileStream fs, ACacheStore store)
		{
			XmlReader rdr = XmlReader.Create(fs);
			rdr.Settings.IgnoreWhitespace = true;
			List<String> waypoints = new List<String>();
			while (rdr.Read())
			{
				if (rdr.LocalName == "name" && rdr.IsStartElement())
				{
					waypoints.Add(ACacheStore.Escape(rdr.ReadElementContentAsString()));
				}
			}
			rdr.Close();
			store.PurgeAllTravelBugs(waypoints.ToArray());
			if (m_purgeLogs)
				store.PurgeAllLogs(waypoints.ToArray());
			store.PurgeAllAttributes(waypoints.ToArray());
			return;
		}
				
		public void parseGPXFile(FileStream fs, ACacheStore store)
		{			
			m_store = store;
			XmlReader reader = XmlReader.Create(fs);
			reader.Settings.IgnoreWhitespace = true;
			while (reader.Read())
			{
				if (m_cancel)
				{
					m_store.CancelUpdate();
					return;
				}
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						if (reader.Name == "time")
							gpx_date = reader.ReadElementContentAsDateTime();
						if (reader.Name == "loc")
							m_source = reader.GetAttribute("src");
						if (reader.Name == "url")
						{
							string val = reader.ReadElementContentAsString();
							if (val.Contains("opencaching")||val.Contains("gctour"))
								m_source = "opencaching";
						}
						if (reader.Name == "wpt")
						{
							Waypoint pt = processWaypoint(reader);
							pt.Updated = gpx_date;
							m_store.AddWaypointOrCache(pt, m_preserveFound, true);
							if ((pt is Geocache) && (m_bookmark != null))
								m_store.AddBoormarkEntry(m_bookmark,pt.Name);
						}
						
						if (reader.Name == "waypoint" && reader.IsStartElement())
						{
							Waypoint pt = processLocWaypoint(reader);
							pt.Updated = System.DateTime.Now;
							m_store.AddWaypointOrCache(pt, m_preserveFound, true);
							if ((pt is Geocache) && (m_bookmark != null))
								m_store.AddBoormarkEntry(m_bookmark,pt.Name);
						}
						break;
					case XmlNodeType.EndElement:
						break;
				}
			}
			reader.Close();
			this.Complete(this, EventArgs.Empty);
		}
		
		
		
		private Waypoint processWaypoint(XmlReader reader)
		{
			Waypoint newPoint = new Waypoint();
		
			String lat = reader.GetAttribute("lat");
			String lon = reader.GetAttribute("lon");
			
			newPoint.Lat = float.Parse(lat, CultureInfo.InvariantCulture);
			newPoint.Lon = float.Parse(lon, CultureInfo.InvariantCulture);
			
			bool breakLoop = false;
			
			while (!breakLoop && reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:					
						processWptElement(ref newPoint, reader);
						break;
					case XmlNodeType.EndElement:
						if (reader.Name == "wpt")
							breakLoop = true;
						break;
				}
			}
			return newPoint;
		}
		
		private Waypoint processLocWaypoint(XmlReader reader)
		{
			Waypoint newPoint = new Waypoint();
			bool breakLoop = false;
			while (!breakLoop && reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						processLocWpt(ref newPoint, reader);
						break;					
					case XmlNodeType.EndElement:
						if (reader.Name == "waypoint")
							breakLoop = true;
					break;
				}
			}
			return newPoint;
		}
		
		public void processLocWpt(ref Waypoint pt, XmlReader reader)
		{
			if (reader.Name == "name")
			{
				pt.Name = reader.GetAttribute("id");
				pt.Desc = reader.ReadElementContentAsString().Trim();
				this.ParseWaypoint(this, new ParseEventArgs(String.Format("Processing Waypoint {0}", pt.Name)));
			}
			else if (reader.Name == "coord")
			{
				pt.Lat = double.Parse(reader.GetAttribute("lat"), CultureInfo.InvariantCulture);
				pt.Lon = double.Parse(reader.GetAttribute("lon"), CultureInfo.InvariantCulture);
			}
			else if (reader.Name == "terrain")
			{
				(pt as Geocache).Terrain = float.Parse(reader.ReadString());
			}
			else if (reader.Name == "difficulty")
			{
				(pt as Geocache).Difficulty = float.Parse(reader.ReadString());
			}
			else if (reader.Name == "container")
			{
				string val = reader.ReadString().Trim();
				if (val == "2")
					(pt as Geocache).Container = "Micro";	
				else if (val == "8")
					(pt as Geocache).Container = "Small";	
				else if (val == "3")
					(pt as Geocache).Container = "Regular";	
				else if (val == "4")
					(pt as Geocache).Container = "Large";	
				else if (val == "5")
					(pt as Geocache).Container = "Virtual";
				else
					(pt as Geocache).Container = "Not Chosen";
			}
			else if (reader.Name == "type")
			{
				pt.Symbol = reader.ReadElementContentAsString();
				pt.Type = pt.Symbol;
				if (pt.Type == "Geocache")
				{
					if (pt.URL == null)
						pt.URL = new Uri("http://geocaching.com");
					pt  = Geocache.convertFromWaypoint(pt);
					if (m_source == "NaviCache")
					{
						ParseNaviCache (pt);
					}
				}					
			}
			else if (reader.Name == "link")
			{
				pt.URLName = reader.GetAttribute("text");
				pt.URL = new Uri(reader.ReadElementContentAsString());
				((Geocache) pt).LongDesc = "<a href=\"" + pt.URL.ToString() + "\">View Online</a>";
			}
		}
		
		private void ParseNaviCache (Waypoint pt)
		{
			Geocache cache = pt as Geocache;
			cache.Symbol = "NaviCache";
			String[] lines = pt.Desc.Split('\n');
			for (int i=0; i < lines.Length; i++)
			{
				if (i ==0 )
				{
					cache.CacheName = lines[0];
				}
				else if (i == 1)
					continue;
				else
				{
					String[] details = lines[i].Split(':');
					String detail = details[1].Trim();
					if (i == 2)
					{
						ParseCacheType(detail, ref cache);
					}
					else if (i == 3)
					{
						if (detail == "Normal")
							cache.Container = "Regular";
						else if (detail == "Virtual" || detail == "Unknown")
							cache.Container = "Not chosen";
						else
							cache.Container = detail;
					}
					else if (i == 4)
					{
						cache.Difficulty = float.Parse(detail, CultureInfo.InvariantCulture);
					}
					else if (i == 5)
					{
						cache.Terrain = float.Parse(detail, CultureInfo.InvariantCulture);
					}
					
				}
			}
			cache.Desc = cache.CacheName + " ("  + cache.Difficulty + "/" + cache.Terrain + ")";
		}
		
		private void processWptElement(ref Waypoint pt, XmlReader reader)
		{
			if (pt is Geocache)
			{
				parseGeocacheElement(ref pt, reader);
			}
			else if (reader.LocalName == "name")
			{
				pt.Name = reader.ReadString();
				this.ParseWaypoint(this, new ParseEventArgs(String.Format("Processing Waypoint {0}", pt.Name)));
				pt.Parent = "GC" +  pt.Name.Substring(2);
			}
			else if (reader.LocalName == "url")
			{
				String url = reader.ReadString().Trim();
				if (!String.IsNullOrEmpty(url))
					pt.URL = new Uri(url);
			}
			else if (String.Equals(reader.LocalName, "parent",StringComparison.InvariantCultureIgnoreCase))
			{
				pt.Parent = reader.ReadString();
			}	
			else if (reader.LocalName == "desc")
			{
				pt.Desc = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "time")
			{
				string strVal = reader.ReadString();
				pt.Time = DateTime.Parse(strVal);
			}
			else if (reader.LocalName == "link")
			{
				pt.URL = new Uri(reader.GetAttribute("href"));
				pt.URLName = pt.Name;
			}
			else if (reader.LocalName == "urlname")
			{
				pt.URLName = reader.ReadString();
			}
			else if (reader.LocalName == "sym")
			{
				pt.Symbol = reader.ReadString();
				
			}
			else if (reader.LocalName == "type")
			{
				pt.Type = reader.ReadString();
				if (pt.Type.StartsWith("Geocache") || pt.Type.StartsWith("TerraCache"))
				{
				    pt = Geocache.convertFromWaypoint(pt);
				}
				else if (pt.Symbol == "Waymark")
				{
					// TEMP: For now, convert Waymark into a virtual cache
					pt = Geocache.convertFromWaypoint(pt);
					Geocache cache = pt as Geocache;
					cache.ShortDesc = pt.Type + "\n\n";
					cache.CacheName = pt.Type + ":  " + pt.URLName;
					// Need to give it a unique ID for Oregon/Colorado.
					Random rand = new Random();
					cache.CacheID = rand.Next(50000000).ToString();
					cache.TypeOfCache = Geocache.CacheType.VIRTUAL;
					cache.Type = "Geocache";
					cache.LongDesc = cache.Desc;
					cache.Container = "Virtual";
					cache.Difficulty = 1.0f;
					cache.Terrain = 1.0f;
					pt.Desc = cache.CacheName;
					pt.Symbol = "Geocache";
					pt.Type = "Geocache|Virtual Cache";
					cache.PlacedBy = "Unknown";
				}
			}
		}
		
		private void parseGeocacheElement(ref Waypoint pt, XmlReader reader)
		{
			Geocache cache = pt as Geocache;
			if (m_source == "opencaching")
			{
				cache = ParseOpenCache(reader, ref cache);
			}
			else if (reader.NamespaceURI.StartsWith("http://www.groundspeak.com/cache") 
			         || reader.NamespaceURI.StartsWith("http://www.gsak.net/xmlv1/5"))
			{
				cache = ParseGroundSpeakCache (reader, ref cache);
			}
			else if (reader.NamespaceURI.StartsWith("http://www.TerraCaching.com"))
			{
				cache = ParseTerraCache (reader, ref cache);
			}
		}
		
		private Geocache ParseTerraCache (XmlReader reader, ref Geocache cache)
		{
			if (reader.LocalName == "terracache")
			{
				cache.CacheID = reader.GetAttribute("id");
				cache.Available = true;
				cache.Archived = false;
			}
			else if (reader.LocalName == "name")
			{
				cache.CacheName = reader.ReadElementContentAsString();
				if (cache.CacheName.ToUpperInvariant().Contains("UNAVAILABLE"))
					cache.Available = false;
			}
			else if (reader.LocalName == "description")
			{
				cache.LongDesc = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "style")
			{
				ParseCacheType(reader.ReadElementContentAsString(), ref cache);
				if ((cache.TypeOfCache == Geocache.CacheType.TRADITIONAL) && cache.Name.StartsWith("LC"))
					cache.TypeOfCache = Geocache.CacheType.REVERSE;
			}
			else if (reader.LocalName == "owner")
			{
				cache.OwnerID = reader.GetAttribute("id");
				cache.PlacedBy = reader.ReadElementContentAsString();
				cache.CacheOwner = cache.PlacedBy;
			}
			else if (reader.LocalName == "hint")
			{
				cache.Hint = reader.ReadElementContentAsString();
				cache.Hint = cache.Hint.Replace("&gt;", ">");
				cache.Hint = cache.Hint.Replace("&lt;", "<");
				cache.Hint = cache.Hint.Replace("&amp;", "&");
			}
			else if (reader.LocalName == "tps_points")
			{
				cache.ShortDesc += "<b>TPS Points: </b>";
				cache.ShortDesc += reader.ReadElementContentAsString();
				cache.ShortDesc += "<br>";
			}
			else if (reader.LocalName == "country")
			{
			 	cache.Country = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "state")
			{
				cache.State = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "mce_score")
			{
				cache.ShortDesc += "<b>MCE Score: </b>";
				cache.ShortDesc += reader.ReadElementContentAsString();
				cache.ShortDesc += "<br>";
			}
			else if (reader.LocalName == "physical_challenge")
			{
				cache.ShortDesc += "<b>Physical Challenge: </b>";
				cache.ShortDesc += reader.ReadElementContentAsString();
				cache.ShortDesc += "<br>";
			}
			else if (reader.LocalName == "mental_challenge")
			{
				cache.ShortDesc += "<b>Mental Challenge: </b>";
				cache.ShortDesc += reader.ReadElementContentAsString();
				cache.ShortDesc += "<br>";
			}
			else if (reader.LocalName == "camo_challenge")
			{
				cache.ShortDesc += "<b>Cammo Challenge: </b>";
				cache.ShortDesc += reader.ReadElementContentAsString();
				cache.ShortDesc += "<hr noshade>";
			}
			else if (reader.LocalName == "size")
			{
				string sizeVal = reader.ReadElementContentAsString();
				if (String.IsNullOrEmpty(sizeVal))
				{
					cache.Container = "Not chosen";
				}
				else
				{
					int size = int.Parse(sizeVal);
					switch(size)
					{
						case 1:
							cache.Container = "Large";
							break;
						case 2:
							cache.Container = "Regular";
							break;
						case 3:
							cache.Container = "Small";
							break;
						case 4:
							cache.Container = "Micro";
							break;
						case 5: 
							cache.Container = "Micro";
							break;
					}
				}
			}
			else if (reader.LocalName == "logs" && !reader.IsEmptyElement)
			{
				ParseTerraLogs(ref cache, reader);
			}
			//TEMP FIX THIS
			cache.Difficulty = 1;
			cache.Terrain = 1;
			return cache;
		}
		
		private Geocache ParseOpenCache (XmlReader reader, ref Geocache cache)
		{
			if (reader.LocalName == "cache" || reader.LocalName == "geocache")
			{
				string avail = reader.GetAttribute("available");
				string arch = reader.GetAttribute("archived");
				if (!String.IsNullOrEmpty(avail))
					cache.Available = Boolean.Parse(avail);
				else
					cache.Available = true;
				if (!String.IsNullOrEmpty(arch))
					cache.Archived = Boolean.Parse(arch);
				else
					cache.Archived = false;
				cache.CacheID = reader.GetAttribute("id");
			}
			else if (reader.LocalName == "name")
			{
				cache.CacheName = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "owner")
			{
				cache.OwnerID = reader.GetAttribute("id");
				cache.CacheOwner = reader.ReadElementContentAsString();
				cache.PlacedBy = cache.CacheOwner;
			}
			else if (reader.LocalName == "type")
			{
				ParseCacheType(reader.ReadElementContentAsString(), ref cache);
			}
			else if (reader.LocalName == "difficulty")
			{
				string diff = reader.ReadElementContentAsString();
				cache.Difficulty = float.Parse(diff, CultureInfo.InvariantCulture);
			}
			else if (reader.LocalName == "terrain")
			{
				string terr = reader.ReadElementContentAsString();
				cache.Terrain = float.Parse(terr, CultureInfo.InvariantCulture);
			}
			else if (reader.LocalName == "short_description" || reader.LocalName == "summary")
			{
				string html = reader.GetAttribute("html");
				string val = reader.ReadElementContentAsString();
				if (html.Equals("False", StringComparison.InvariantCultureIgnoreCase))
					val = val.Replace("\n", "<br/>");
				cache.ShortDesc = val;
			}
			else if (reader.LocalName == "long_description" || reader.LocalName == "description")
			{
				string html = reader.GetAttribute("html");	
				string val = reader.ReadElementContentAsString();
				if (html.Equals("False", StringComparison.InvariantCultureIgnoreCase))
					val = val.Replace("\n", "<br/>");
				cache.LongDesc = val;
			}
			else if (reader.LocalName == "encoded_hints" || reader.LocalName == "hints")
			{
				cache.Hint = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "container")
			{
				cache.Container = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "logs" && !reader.IsEmptyElement)
			{
				ParseCacheLogs(ref cache, reader);
			}
			else if (reader.LocalName == "travelbugs" && !reader.IsEmptyElement)
			{
				ParseTravelBugs(ref cache, reader);
			}
			else if (reader.LocalName == "country")
			{
				cache.Country = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "state")
			{
				cache.State = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "attributes")
			{
				parseCacheAttrs(ref cache, reader);
			}
			return cache;
		}
		
		private Geocache ParseGroundSpeakCache (XmlReader reader, ref Geocache cache)
		{
			if (reader.LocalName == "cache")
			{
				string avail = reader.GetAttribute("available");
				string arch = reader.GetAttribute("archived");
				if (!String.IsNullOrEmpty(avail))
					cache.Available = Boolean.Parse(avail);
				else
					cache.Available = true;
				if (!String.IsNullOrEmpty(arch))
					cache.Archived = Boolean.Parse(arch);
				else
					cache.Archived = false;
				cache.CacheID = reader.GetAttribute("id");
			}
			else if (reader.LocalName == "DNF" && !m_ignoreExtraFields)
			{
				cache.DNF = reader.ReadElementContentAsBoolean();
			}
			else if (reader.LocalName == "FirstToFind"  && !m_ignoreExtraFields)
			{
				cache.FTF = reader.ReadElementContentAsBoolean();
			}
			else if (reader.LocalName == "UserData" && !m_ignoreExtraFields)
			{
				cache.User1 = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "User2" && !m_ignoreExtraFields)
			{
				cache.User2 = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "User3" && !m_ignoreExtraFields)
			{
				cache.User3 = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "User4" && !m_ignoreExtraFields)
			{
				cache.User4 = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "LatBeforeCorrect" && !m_ignoreExtraFields)
			{
				double corLat = cache.OrigLat;
				cache.Lat = reader.ReadElementContentAsDouble();
				cache.CorrectedLat = corLat;
			}
			else if (reader.LocalName == "LonBeforeCorrect" && !m_ignoreExtraFields)
			{
				double corLon = cache.OrigLon;
				cache.Lon = reader.ReadElementContentAsDouble();
				cache.CorrectedLon = corLon;
			}
			else if (reader.LocalName == "name")
			{
				cache.CacheName = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "placed_by")
			{
				cache.PlacedBy = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "owner")
			{
				cache.OwnerID = reader.GetAttribute("id");
				cache.CacheOwner = reader.ReadElementContentAsString();					
			}
			else if (reader.LocalName == "type")
			{
				ParseCacheType(reader.ReadElementContentAsString(), ref cache);
			}
			else if (reader.LocalName == "difficulty")
			{
				string diff = reader.ReadElementContentAsString();
				cache.Difficulty = float.Parse(diff, CultureInfo.InvariantCulture);
			}
			else if (reader.LocalName == "terrain")
			{
				string terr = reader.ReadElementContentAsString();
				cache.Terrain = float.Parse(terr, CultureInfo.InvariantCulture);
			}
			else if (reader.LocalName == "short_description")
			{
				string html = reader.GetAttribute("html");
				string val = reader.ReadElementContentAsString();
				if (html.Equals("False", StringComparison.InvariantCultureIgnoreCase))
					val = val.Replace("\n", "<br/>");
				cache.ShortDesc = val;
			}
			else if (reader.LocalName == "long_description")
			{
				string html = reader.GetAttribute("html");	
				string val = reader.ReadElementContentAsString();
				if (html.Equals("False", StringComparison.InvariantCultureIgnoreCase))
					val = val.Replace("\n", "<br/>");
				cache.LongDesc = val;
			}
			else if (reader.LocalName == "encoded_hints")
			{
				cache.Hint = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "container")
			{
				cache.Container = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "logs" && !reader.IsEmptyElement)
			{
				ParseCacheLogs(ref cache, reader);
			}
			else if (reader.LocalName == "travelbugs" && !reader.IsEmptyElement)
			{
				ParseTravelBugs(ref cache, reader);
			}
			else if (reader.LocalName == "country")
			{
				cache.Country = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "state")
			{
				cache.State = reader.ReadElementContentAsString();
			}
			else if (reader.LocalName == "attributes")
			{
				parseCacheAttrs(ref cache, reader);
			}
			return cache;
		}
		
		private void ParseTravelBugs(ref Geocache cache, XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.LocalName == "travelbug")
					parseTravelBug(ref cache, reader);
				if (reader.LocalName == "travelbugs")
					return;
			}
		}
	
		
		private void parseTravelBug(ref Geocache cache, XmlReader reader)
		{
			TravelBug bug = new TravelBug();
			bug.ID = reader.GetAttribute("id");
			bug.Ref = reader.GetAttribute("ref");
			while (reader.Read())
			{
				if (reader.LocalName == "travelbug")
				{
						m_store.AddTravelBug(cache.Name, bug);
						return;
				}
				if (reader.LocalName == "name")
				{
					bug.Name = reader.ReadElementContentAsString();
				}
			}			
		}
		
		private void ParseCacheLogs(ref Geocache cache, XmlReader reader)
		{
			bool logsChecked = false;
			while (reader.Read())
			{
				if (reader.LocalName == "log")
				{
					ParseCacheLog(ref cache, reader, ref logsChecked);
				}
				if (reader.LocalName == "logs")
				{
					return;
				}
			}
		}
		
		private void ParseCacheLog(ref Geocache cache, XmlReader reader, ref bool logsChecked)
		{
			CacheLog log = new CacheLog();
			log.LogID = reader.GetAttribute("id");
			log.LogKey = cache.Name + log.LogID;
			bool breakLoop = false;
			while (!breakLoop && reader.Read())
			{					
				if ((reader.LocalName == "date" || reader.LocalName == "time"))
				{
					string date = reader.ReadString();
					if (date.Contains("/"))
						log.LogDate = DateTime.ParseExact(date, "MM/dd/yyyy'T'HH:mm:ss",CultureInfo.InvariantCulture);
					else
						log.LogDate = DateTime.Parse(date);
				}
				else if (reader.LocalName == "type")
				{
					log.LogStatus = reader.ReadString();
					if (m_ownid.Contains(log.FinderID) && log.LogStatus == "Found it")
					{
						cache.Symbol = "Geocache Found";
						cache.DNF =false;
					}
					else if (m_ownid.Contains(log.LoggedBy) && log.LogStatus == "Found it")
					{
						cache.Symbol = "Geocache Found";
						cache.DNF =false;
					}
					else if (m_ownid.Contains(log.FinderID) && (log.LogStatus == "Didn't find it" || log.LogStatus == "no_find") && !cache.Found)
					{
						cache.DNF = true;
					}
					else if (m_ownid.Contains(log.LoggedBy) && (log.LogStatus == "Didn't find it" || log.LogStatus == "no_find") && !cache.Found)
					{
						cache.DNF = true;
					}
				}
				else if ((reader.LocalName == "finder" || reader.LocalName == "geocacher"))
				{
					log.FinderID = reader.GetAttribute("id");
					log.LoggedBy = reader.ReadString();
					if (m_ownid.Contains(log.LoggedBy) && log.LogStatus == "Found it")
					{
						cache.Symbol = "Geocache Found";
						cache.DNF =false;
					}
					else if (m_ownid.Contains(log.FinderID) && log.LogStatus == "Found it")
					{
						cache.Symbol = "Geocache Found";
						cache.DNF =false;
					}
					else if (m_ownid.Contains(log.LoggedBy) && (log.LogStatus == "Didn't find it") && !cache.Found)
					{
						cache.DNF = true;
					}
					else if (m_ownid.Contains(log.FinderID) && (log.LogStatus == "Didn't find it") && !cache.Found)
					{
						cache.DNF = true;
					}
				}
				else if (reader.LocalName == "text" && reader.IsStartElement())
				{
					if (reader.GetAttribute("encoded") != null)
						log.Encoded = Boolean.Parse(reader.GetAttribute("encoded"));
					else
						log.Encoded = false;
					if( log.LogMessage == "Unknown" ) 
					{
						log.LogMessage = reader.ReadString();
					}
					else
					{
						log.LogMessage += reader.ReadString();
					}
				}
				else if( reader.LocalName == "log_wpt" )
				{
					double lat = double.Parse(reader.GetAttribute("lat"), CultureInfo.InvariantCulture);
					double lon = double.Parse(reader.GetAttribute("lon"), CultureInfo.InvariantCulture);
					if( log.LogMessage == "Unknown" ) 
					{
						log.LogMessage = Utilities.getCoordString(lat,lon) + "\n";
					}
					else
					{
						log.LogMessage = Utilities.getCoordString(lat,lon) + "\n" + log.LogMessage;
					}
				}
				else if (reader.LocalName == "log")
				{
					breakLoop = true;
				}
			}
			if (log.LoggedBy == "GSAK" && log.LogStatus == "Other")
			{
				if (log.LogMessage.Trim() == "Notes:")
				{
					// Empty note, ignore
					return;
				}
				// Convert GSAK notes into OCM notes
				cache.Notes += log.LogMessage;
				cache.Notes += "----------\n";
				return;
			}
			
			m_store.AddLog(cache.Name, log);	
			if (!logsChecked)
			{
				if (log.LogStatus=="Didn't find it" || log.LogStatus == "Needs Maintenance" || log.LogStatus == "no_find")
				{
					cache.CheckNotes = true;
					logsChecked = true;
				}
				else if (log.LogStatus != "Write Note" && log.LogStatus != "Note")
				{
					cache.CheckNotes = false;
					logsChecked = true;
				}
			}
			
		}
		
		private void ParseTerraLogs(ref Geocache cache, XmlReader reader)
		{
			bool logsChecked = false;
			while (reader.Read())
			{
				if (reader.LocalName == "log")
				{
					ParseTerraLog(ref cache, reader, ref logsChecked);
				}
				if (reader.LocalName == "logs")
				{
					return;
				}
			}
		}
		
		
		private void ParseTerraLog(ref Geocache cache, XmlReader reader,ref bool logsChecked)
		{
			CacheLog log = new CacheLog();
			bool breakLoop = false;
			log.LogID = reader.GetAttribute("id");
			log.LogKey = cache.Name + log.LogID;
			while (!breakLoop && reader.Read())
			{
				if (reader.LocalName == "date")
				{
					log.LogDate = reader.ReadElementContentAsDateTime();
				}
				else if (reader.LocalName == "type")
				{
					log.LogStatus = reader.ReadElementContentAsString();
				}
				else if (reader.LocalName == "user")
				{
					log.FinderID = reader.GetAttribute("id");
					log.LoggedBy = reader.ReadElementContentAsString();
					if (m_ownid.Contains(log.FinderID) && log.LogStatus == "find")
					{
						cache.Symbol = "Geocache Found";
					}
					else if (m_ownid.Contains(log.LoggedBy) && log.LogStatus == "find")
					{
						cache.Symbol = "Geocache Found";
					}
				}
				else if (reader.LocalName == "entry")
				{
					log.Encoded = false;
					log.LogMessage = reader.ReadElementContentAsString();
					log.LogMessage = log.LogMessage.Replace("&gt;", ">");
					log.LogMessage = log.LogMessage.Replace("&lt;", "<");
					log.LogMessage = log.LogMessage.Replace("&amp;", "&");
				}
				else if (reader.LocalName == "log")
				{
					breakLoop = true;
				}
			}
			if (!logsChecked)
			{
				if (log.LogStatus=="Didn't find it" || log.LogStatus == "Needs Maintenance" || log.LogStatus == "no_find")
				{
					cache.CheckNotes = true;
					logsChecked = true;
				}
				else if (log.LogStatus != "Write Note" && log.LogStatus != "Note")
				{
					cache.CheckNotes = false;
					logsChecked = true;
				}
			}
			m_store.AddLog(cache.Name, log);			
		}
		
		private void parseCacheAttrs(ref Geocache cache, XmlReader reader)
		{
			if (reader.IsEmptyElement)
				return;
			while (reader.Read())
			{
				if (reader.LocalName == "attribute")
				{
					parseCacheAttribute(ref cache, reader);
				}
				if (reader.LocalName == "attributes")
				{
					return;
				}
			}
		}
		
		private void parseCacheAttribute(ref Geocache cache, XmlReader reader)
		{
			CacheAttribute attr = new CacheAttribute();
			attr.ID = reader.GetAttribute("id");
			string inc = reader.GetAttribute("inc");
			if (inc == "1")
				attr.Include = true;
			else
				attr.Include = false;
			attr.AttrValue = reader.ReadElementContentAsString();
			m_store.AddAttribute(cache.Name, attr);
			return;
		}
		
		
		private void ParseCacheType(String type, ref Geocache cache)
		{
			if ((type == "Unknown Cache") || (type == "Other") || (type == "Puzzle") || (type == "Unknown"))
				cache.TypeOfCache = Geocache.CacheType.MYSTERY;
			else if ((type == "Traditional Cache") || (type == "Classic") || (type == "Normal") || (type == "Traditional"))
				cache.TypeOfCache = Geocache.CacheType.TRADITIONAL;
			else if ((type == "Multi-cache") || (type == "Offset") || (type == "Multi-Part") || (type=="Multi"))
				cache.TypeOfCache = Geocache.CacheType.MULTI;
			else if (type == "Letterbox Hybrid")
				cache.TypeOfCache = Geocache.CacheType.LETTERBOX;
			else if ((type ==  "EarthCache") || (type == "Earthcache"))
				cache.TypeOfCache = Geocache.CacheType.EARTH;
			else if (type =="Wherigo Cache")
				cache.TypeOfCache = Geocache.CacheType.WHERIGO;
			else if ((type == "Webcam Cache") || type == ("Webcam"))
				cache.TypeOfCache = Geocache.CacheType.WEBCAM;
			else if (type == "Cache In Trash Out Event")
				cache.TypeOfCache = Geocache.CacheType.CITO;
			else if (type == "GPS Adventures Exhibit")
				cache.TypeOfCache = Geocache.CacheType.MAZE;
			else if (type == "Mega-Event Cache")
				cache.TypeOfCache = Geocache.CacheType.MEGAEVENT;
			else if ((type == "Event Cache") || (type == "Event"))
				cache.TypeOfCache = Geocache.CacheType.EVENT;
			else if ((type == "Virtual Cache") || (type == "Virtual"))
			    cache.TypeOfCache = Geocache.CacheType.VIRTUAL;
			else if (type == "Project APE Cache")
				cache.TypeOfCache = Geocache.CacheType.APE;
			else if (type == "Lost and Found Event Cache")
				cache.TypeOfCache = Geocache.CacheType.EVENT;
			else if (type == "Moving/Travelling")
				cache.TypeOfCache = Geocache.CacheType.REVERSE;
			else
			{
				System.Console.WriteLine("WARNING: TYPE UNKNOWN:" + type);
				cache.TypeOfCache = Geocache.CacheType.GENERIC;
			}
				
		}
	}
}
