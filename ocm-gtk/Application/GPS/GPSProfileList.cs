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
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ocmgtk
{

	[Serializable]
	public class GPSProfileList
	{
		
		[NonSerialized]
		OCMMainWindow m_Win;
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}

		public GPSProfileList ()
		{
		}
		
		private Dictionary<string, GPSProfile> m_profiles = new Dictionary<string, GPSProfile>();
		
		public GPSProfile[] Profiles
		{
			get{
				GPSProfile[] profs = new GPSProfile[m_profiles.Count];
				m_profiles.Values.CopyTo(profs, 0);
				return profs;
			}
			set{
				foreach(GPSProfile location in value)
				{
					m_profiles.Add(location.Name, location);
				}				
			}
		}
		
		public GPSProfile GetActiveProfile()
		{
			string active = m_Win.App.AppConfig.GPSProf;
			if (active == null || !m_profiles.ContainsKey(active))
				return null;
			return m_profiles[active];
		}
		
		public Dictionary<string,string> GetActiveMappings()
		{
			string active = m_Win.App.AppConfig.GPSProf;
			if (active == null || !m_profiles.ContainsKey(active))
				return GPSProfileList.GetDefaultMappings ();
			return m_profiles[active].WaypointMappings;
		}
		
		public static Dictionary<string,string> GetDefaultMappings ()
		{
				Dictionary<string,string> mappings = new Dictionary<string, string>();
				// Return default mappings
				mappings.Add("Geocache|Traditional Cache", "Geocache");
				mappings.Add("Geocache|Unknown Cache", "Geocache");
				mappings.Add("Geocache|Virtual Cache",  "Geocache");
				mappings.Add("Geocache|Multi-cache",  "Geocache");
				mappings.Add("Geocache|Project APE Cache",  "Geocache");
				mappings.Add("Geocache|Cache In Trash Out Event", "Geocache");
				mappings.Add("Geocache|Earthcache",  "Geocache");
				mappings.Add("Geocache|Event Cache", "Geocache");
				mappings.Add("Geocache|Letterbox Hybrid",  "Geocache");
				mappings.Add("Geocache|GPS Adventures Exhibit", "Geocache");
				mappings.Add("Geocache|Mega-Event Cache", "Geocache");
				mappings.Add("Geocache|Locationless Cache", "Geocache");
				mappings.Add("Geocache|Webcam cache", "Geocache");
				mappings.Add("Geocache|Wherigo Cache",  "Geocache");
				mappings.Add("Geocache", "Geocache");
				mappings.Add("Geocache Found", "Geocache");
				mappings.Add("Waypoint|Final Location", "Pin, Blue");
				mappings.Add("Waypoint|Parking Area","Parking Area");
				mappings.Add("Waypoint|Reference Point", "Pin, Green");
				mappings.Add("Waypoint|Question to Answer", "Pin, Red");
				mappings.Add("Waypoint|Stages of a Multicache", "Pin, Red");
				mappings.Add("Waypoint|Trailhead", "Trail Head");
				mappings.Add("Waypoint|Other","Pin, Green");
				return mappings;
		}

		
		public void AddProfile(GPSProfile prof)
		{
			if (m_profiles.ContainsKey(prof.Name))
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
				                                      String.Format(Catalog.GetString("Are you sure you wish to " +
				                                      	"overwrite \"{0}\"?"), prof.Name));
				if ((int) ResponseType.Yes != dlg.Run())
				{
					dlg.Hide();
					return;
				}
				else
				{
					dlg.Hide();
					m_profiles.Remove(prof.Name);
				}
			}
			m_profiles.Add(prof.Name, prof);
			UpdateProfFile();
		}
		
		public void UpdateProfile(GPSProfile prof)
		{
			m_profiles.Remove(prof.Name);
			m_profiles.Add(prof.Name, prof);
			UpdateProfFile();
		}
		
		public void DeleteProfile(string name)
		{
			m_profiles.Remove(name);
			UpdateProfFile();
		}
		
		public static GPSProfileList LoadProfileList()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!File.Exists(path + "/ocm/gps.oqf"))
			{
				return new GPSProfileList();
			}
			FileStream fs = new FileStream(path + "/ocm/gps.oqf", FileMode.Open, FileAccess.Read);	
			BinaryFormatter ser = new BinaryFormatter();
			System.Object filters = ser.Deserialize(fs);
			fs.Close();
			return filters as GPSProfileList;
		}
		
		private void UpdateProfFile()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!Directory.Exists("ocm"))
				Directory.CreateDirectory(path + "/ocm");
			path = path + "/ocm";
			BinaryFormatter ser = new BinaryFormatter();
			FileStream fs = new FileStream(path + "/gps.oqf", FileMode.Create, FileAccess.ReadWrite);
			ser.Serialize(fs, this);
			fs.Close();
		}
		
		public Menu BuildProfileTransferMenu()
		{
			Menu etMenu = new Menu();
			foreach (GPSProfile loc in m_profiles.Values)
			{
				Gtk.Action action = new Gtk.Action(loc.Name, loc.Name);
				etMenu.Append(action.CreateMenuItem());
				action.Activated += HandleTransferActionActivated;	
			}
			etMenu.ShowAll();
			return etMenu;
		}
		
		public Menu BuildProfileReceiveMenu()
		{
			Menu etMenu = new Menu();
			foreach (GPSProfile loc in m_profiles.Values)
			{
				if (String.IsNullOrEmpty(loc.FieldNotesFile))
				    continue;
				Gtk.Action action = new Gtk.Action(loc.Name, loc.Name);
				etMenu.Append(action.CreateMenuItem());
				action.Activated += HandleReceiveActionActivated;
			}
			return etMenu;
		}

		void HandleReceiveActionActivated (object sender, EventArgs e)
		{
			m_Win.App.AppConfig.GPSProf =  ((sender) as Gtk.Action).Name;
			m_Win.SetLastGPS(((sender) as Gtk.Action).Name, m_Win.App.Profiles.GetActiveProfile().FieldNotesFile != null);
			m_Win.ReceiveGPSFieldNotes();
		}

		void HandleTransferActionActivated (object sender, EventArgs e)
		{
			m_Win.App.AppConfig.GPSProf =  ((sender) as Gtk.Action).Name;
			m_Win.SetLastGPS(((sender) as Gtk.Action).Name, m_Win.App.Profiles.GetActiveProfile().FieldNotesFile != null);
			m_Win.SendToGPS();
		}
		
		public Menu BuildProfileEditMenu()
		{
			Menu etMenu = new Menu();
			int iCount = 0;
			foreach (GPSProfile loc in m_profiles.Values)
			{
				Gtk.Action action = new Gtk.Action(loc.Name, loc.Name);
				etMenu.Append(action.CreateMenuItem());
				action.Activated += HandleActionActivated;
				iCount ++;
				
			}
			return etMenu;
		}

		void HandleActionActivated (object sender, EventArgs e)
		{
			GPSProfile prof = m_profiles[((sender) as Gtk.Action).Name];
			m_Win.EditProfile(prof);
		}
	}
}
