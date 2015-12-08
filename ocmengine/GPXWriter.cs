// 
//  Copyright 2010  campbelk
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
using System.Xml;
using System.IO;
using ocmengine;

namespace ocmengine
{
	public enum WaypointNameMode{CODE,NAME,SHORTCODE};
	public enum WaypointDescMode{DESC,CODESIZEANDHINT,CODESIZETYPE,FULL};
	public enum HTMLMode{HTML,GARMIN,PLAINTEXT};
	
	public class WriteEventArgs:EventArgs
	{
		private string m_message;
		
		public string Message
		{
			get { return m_message;}
		}
		
		public WriteEventArgs(String message):base()
		{
			m_message = message;
		}
	}
	
	public class GPXWriter
	{
		public const string NS_GPX = "http://www.topografix.com/GPX/1/0";
		public const string NS_CACHE = "http://www.groundspeak.com/cache/1/0";
		public const string NS_OCM = "http://www.groundspeak.com/cache/1/0";
		public const string XSD_DT = "yyyy-MM-ddTHH:mm:ss.fffzzz";
		
		public event WriteEventHandler WriteWaypoint;
		public event WriteEventHandler Complete;
		public delegate void WriteEventHandler(object sender, EventArgs args);
		
		public int guidStart = 9000000;
		
		private Dictionary<string, List<CacheLog>> m_CacheLogs;
		private Dictionary<string, List<TravelBug>> m_TravelBugs;
		private Dictionary<string, List<CacheAttribute>> m_Attrs;
		
		bool m_UseOCMPtTypes = false;
		public Boolean UseOCMPtTypes
		{
		 	set { m_UseOCMPtTypes = value;}
			get { return m_UseOCMPtTypes;}
		}
		
		bool m_includeChildren = true;
		public Boolean IncludeChildWaypoints
		{
			get { return m_includeChildren;}
			set { m_includeChildren = value;}
		}
		
		HTMLMode m_htmlMode = HTMLMode.HTML;
		public HTMLMode HTMLOutput
		{
			get { return m_htmlMode;}
			set { m_htmlMode = value;}
		}
		
		bool m_isMyFinds = false;
		public Boolean IsMyFinds
		{
			get { return m_isMyFinds;}
			set { m_isMyFinds = value;
				if (value)
				{
					m_includeChildren = false;
				}
			}
		}
		
		string m_findsOwner = "ocm";
		public string MyFindsOwner
		{
			get { return m_findsOwner;}
			set { m_findsOwner = value;}
		}
		
		public int GetNextGUID()
		{
			return guidStart++;
		}
		
		bool m_cancel = false;
		public bool Cancel
		{
			get { return m_cancel; }
			set {m_cancel = true;}
		}
		
		private int m_Limit = -1;
		public int Limit
		{
			set { m_Limit = value;}
			get { return m_Limit;}
		}
		
		private int m_LogLimit = -1;
		public int LogLimit
		{
			get { return m_LogLimit;}
			set { m_LogLimit = value;}
		}
		
		private bool m_isFullInfo = true;
		public bool IncludeGroundSpeakExtensions
		{
			set { m_isFullInfo = value;}
			get { return m_isFullInfo;}
		}
		
		WaypointNameMode m_namemode = WaypointNameMode.CODE;
		public WaypointNameMode NameMode
		{
			get { return m_namemode;}
			set { m_namemode = value;}
		}
		
		WaypointDescMode m_descmode = WaypointDescMode.DESC;
		public WaypointDescMode DescriptionMode
		{
			get { return m_descmode;}
			set { m_descmode = value;}
		}
		
		private Dictionary<string,string> m_mappings;
		public Dictionary<string,string> Mappings
		{
			get{return m_mappings;}
		}
		
		bool m_writeAttributes = true;
		public bool WriteAttributes
		{
			get { return m_writeAttributes;}
			set { m_writeAttributes = value;}
		}
		
		private int m_Count = 0;
		private ACacheStore m_Store;
		public ACacheStore CacheStore
		{
			get { return m_Store;}
		}
		
		public List<CacheLog> GetCacheLogs(string code)
		{
			if (m_CacheLogs.ContainsKey(code))
				return m_CacheLogs[code];
			return new List<CacheLog>();
		}
		
		public List<TravelBug> GetTravelBugs(string code)
		{
			if (m_TravelBugs.ContainsKey(code))
				return m_TravelBugs[code];
			return new List<TravelBug>();
		}
		
		public List<CacheAttribute> GetAttributes(string code)
		{
			if (m_Attrs.ContainsKey(code))
				return m_Attrs[code];
			return new List<CacheAttribute>();
		}

			
		public void WriteGPXFile (String name, List<Geocache> caches, Dictionary<string,string> waypointmappings, ACacheStore store)
		{
			m_Store = store;
			FileStream stream = new System.IO.FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Write, 655356);
			m_mappings = waypointmappings;
			XmlTextWriter writer = new XmlTextWriter (stream, System.Text.Encoding.UTF8);
			//Pretty-print the document
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 1;
			writer.IndentChar = '\t';

			List<string> usedCacheNames = new List<string>();
			string[] namesForPreSeach = new string[caches.Count];
			for(int i=0; i < caches.Count; i++)
			{
				namesForPreSeach[i] = caches[i].Name;
			}
			m_CacheLogs = CacheStore.GetCacheLogsMulti(namesForPreSeach);
			m_TravelBugs = CacheStore.GetTravelBugMulti(namesForPreSeach);
			m_Attrs = CacheStore.GetAttributesMulti(namesForPreSeach);			
			
			try {
				// Write out XML processing directive, some applications expect this
				writer.WriteRaw ("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				writer.WriteStartElement ("gpx", NS_GPX);
				writer.WriteAttributeString("creator", "OCM");
				writer.WriteAttributeString("version", "1.0");
				if (IsMyFinds)
					writer.WriteElementString("name", NS_GPX, "My Finds Pocket Query");
				else
					writer.WriteElementString ("name", NS_GPX, "Cache Listing from OCM");
				writer.WriteElementString ("desc", NS_GPX, "Cache Listing from OCM");
				writer.WriteElementString ("author", NS_GPX, "Open Cache Manager");
				writer.WriteElementString ("email", NS_GPX, "kmcamp_ott@yahoo.com");
				writer.WriteElementString ("url", NS_GPX, "http://sourceforge.net/projects/opencachemanage/");
				writer.WriteElementString ("urlname", NS_GPX, "Sourceforge Link");
				writer.WriteElementString ("time", NS_GPX, System.DateTime.Now.ToString (XSD_DT));
				WriteCaches (caches, writer, usedCacheNames, name);
				if (m_includeChildren && !m_cancel)
				{
					List<Waypoint> points = CacheStore.GetChildWaypoints(usedCacheNames.ToArray());
					foreach(Waypoint pt in points)
					{
						pt.WriteToGPX(writer, this);
					}  
				}
				writer.WriteEndElement ();
				this.Complete(this, new WriteEventArgs("Done"));
			} catch (Exception e) {
				throw e;
			} finally {
				writer.Flush ();
				writer.Close ();
			}
		}
		
		private void WriteCaches (List<Geocache> caches, XmlTextWriter writer, List<string> usedCacheNames, String name)
		{
			foreach (Geocache cache in caches)
			{
				if (m_cancel || ((m_Count >= m_Limit) && (m_Limit != -1)))
				{
					return;
				}
				this.WriteWaypoint(this, new WriteEventArgs(String.Format("Writing {0}", cache.Name)));
				usedCacheNames.Add(cache.Name);
				cache.WriteToGPX (writer, this);

// ****************************************************************
// Cyril : export images to GPS
// 

				string debut_dest = Directory.GetParent(name).Parent.FullName;
				string base_dest = debut_dest + "/GeocachePhotos";
				//System.Console.WriteLine(base_dest);
				if (Directory.Exists (base_dest))
				{
					string dir = CacheStore.StoreName;
					string[] fullPath = dir.Split ('/');
					string nom_base = fullPath[fullPath.Length - 1];
					string base_local = dir.Substring(0,(dir.Length - nom_base.Length));
					nom_base = nom_base.Substring (0, nom_base.Length - 4);
					dir = base_local + "ocm_images/" + nom_base + "/" + cache.Name;
					System.Console.WriteLine(dir);
					if (Directory.Exists (dir))
					{
						if(Directory.GetFiles(dir).Length > 0 )
						{
							int longueur = cache.Name.Length;
	                				string dest_path = base_dest+"/"+(cache.Name.Substring(longueur-1,1))+"/"+(cache.Name.Substring(longueur-2,1))	+"/"+(cache.Name);
							
			        			if (! Directory.Exists(dest_path))
							{
								DirectoryInfo dir_GPS = Directory.CreateDirectory(dest_path);
							}
	
							// Create a reference to the local pictures directory.
						        DirectoryInfo dir_local = new DirectoryInfo(dir);
						        // Create an array representing the files in the current directory.
						        FileInfo[] fi = dir_local.GetFiles();
						        foreach (FileInfo fiTemp in fi)
							{
							       // Console.WriteLine("The following files exist in: " + dir);
							        // Print out the names of the files in the current directory.
								//Console.WriteLine(fiTemp.Name);
								if (! File.Exists( dest_path+"/"+fiTemp.Name+".jpg" ))
									File.Copy(dir+"/"+fiTemp.Name, dest_path+"/"+fiTemp.Name+".jpg", true);
								int cnt = 1;
							}
						}
					}		
				}
// ****************************************************************

				m_Count++;
			}
			return;
		}
	}
}
