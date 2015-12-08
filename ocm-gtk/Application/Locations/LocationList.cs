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
using System.IO;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using System.Runtime.Serialization.Formatters.Binary;


namespace ocmgtk
{

	[Serializable]
	public class LocationList
	{
		[NonSerialized]
		private ToggleAction gpsdAction;
		
		[NonSerialized]
		private	OCMMainWindow m_Win;
		
		public OCMMainWindow Window
		{
			set {m_Win = value;}
		}
		

		public LocationList ()
		{
		}
		
		private Dictionary<string, Location> m_locations = new Dictionary<string, Location>();
		
		public Location[] Locations
		{
			get{
				Location[] locs = new Location[m_locations.Count];
				m_locations.Values.CopyTo(locs, 0);
				return locs;
			}
			set{
				foreach(Location location in value)
				{
					m_locations.Add(location.Name, location);
				}				
			}
		}
		
		public Location GetLocation(String name)
		{
			if (m_locations.ContainsKey(name))
				return m_locations[name];
			return null;
		}
		
		public bool Contains(Location loc)
		{
			return m_locations.ContainsKey(loc.Name);
		}

		
		public void AddLocation(Location loc)
		{
			if (m_locations.ContainsKey(loc.Name))
			{
				m_locations.Remove(loc.Name);
			}
			m_locations.Add(loc.Name, loc);
			UpdateLocFile();
		}
		
		public void DeleteLocation(string name)
		{
			m_locations.Remove(name);
			UpdateLocFile();
		}
		
		public static LocationList LoadLocationList()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!File.Exists(path + "/ocm/locs.oqf"))
			{
				return new LocationList();
			}
			FileStream fs = new FileStream(path + "/ocm/locs.oqf", FileMode.Open, FileAccess.Read);	
			BinaryFormatter ser = new BinaryFormatter();
			System.Object filters = ser.Deserialize(fs);
			fs.Close();
			return filters as LocationList;
			
		}
		
		private void UpdateLocFile()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!Directory.Exists("ocm"))
				Directory.CreateDirectory(path + "/ocm");
			path = path + "/ocm";
			BinaryFormatter ser = new BinaryFormatter();
			FileStream fs = new FileStream(path + "/locs.oqf", FileMode.Create, FileAccess.ReadWrite);
			ser.Serialize(fs, this);
			fs.Close();
		}
		
		public Menu BuildLocationlMenu()
		{
			Menu etMenu = new Menu();
			
			Gtk.Action home_action = new Gtk.Action("Home", Catalog.GetString("Home"));
			etMenu.Append(home_action.CreateMenuItem());
			home_action.Activated += HandleHome_actionActivated;
			int iCount = 0;
			foreach (Location loc in m_locations.Values)
			{
				Gtk.Action action = new Gtk.Action(loc.Name, loc.Name);
				etMenu.Append(action.CreateMenuItem());
				action.Activated += HandleActionActivated;
				iCount ++;
				
			}
			etMenu.Append(new MenuItem());
			gpsdAction = new ToggleAction("UseGPSD", Catalog.GetString("GPSD Position"),null, null);
			//gpsdAction.Active = UIMonitor.getInstance().Configuration.UseGPSD;
			gpsdAction.Toggled += HandleGpsdActionToggled;
			etMenu.Append(gpsdAction.CreateMenuItem());	
			etMenu.ShowAll();
			return etMenu;
		}

		void HandleGpsdActionToggled (object sender, EventArgs e)
		{

			if (((ToggleAction) sender).Active)
			{
				m_Win.App.EnableGPS();
				m_Win.App.AppConfig.UseGPSD = true;
			}
			else
			{
				m_Win.App.DisableGPS();
				m_Win.App.AppConfig.UseGPSD = false;
			}
		}

		void HandleHome_actionActivated (object sender, EventArgs e)
		{
			gpsdAction.Active = false;
			m_Win.ResetToHome();
		}
		
		void HandleActionActivated (object sender, EventArgs e)
		{
			Location loc = m_locations[((sender) as Gtk.Action).Name];
			gpsdAction.Active = false;
			m_Win.SetLocation(loc);
		}
	}
}
