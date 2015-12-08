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
using Gtk;
using System.Collections.Generic;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class WaypointOverrideWidget : Gtk.Bin
	{
		VBox widgetBox = null;
		public WaypointOverrideWidget ()
		{
			this.Build ();
		}
		
		public void PopulateMappings(Dictionary<string,string> mappings)
		{
			widgetBox = new VBox();
			ScrolledWindow window = new ScrolledWindow();
			window.VscrollbarPolicy = PolicyType.Always;
			if (mappings == null)
			{		
				widgetBox.Add(new SymbolChooser("Geocache|Traditional Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Unknown Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Virtual Cache",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Multi-cache",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Project APE Cache",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Cache In Trash Out Event", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Earthcache",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Event Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Letterbox Hybrid",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|GPS Adventures Exhibit", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Mega-Event Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Locationless Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Webcam Cache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache|Wherigo Cache",  "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache", "Geocache"));
				widgetBox.Add(new SymbolChooser("Geocache Found", "Geocache"));
				widgetBox.Add(new SymbolChooser("Waypoint|Final Location", "Pin, Blue"));
				widgetBox.Add(new SymbolChooser("Waypoint|Parking Area","Parking Area"));
				widgetBox.Add(new SymbolChooser("Waypoint|Reference Point", "Pin, Green"));
				widgetBox.Add(new SymbolChooser("Waypoint|Question to Answer", "Pin, Red"));
				widgetBox.Add(new SymbolChooser("Waypoint|Stages of a Multicache", "Pin, Red"));
				widgetBox.Add(new SymbolChooser("Waypoint|Trailhead", "Trail Head"));
				widgetBox.Add(new SymbolChooser("Waypoint|Other","Pin, Green"));
			}
			else
			{
				foreach (string key in mappings.Keys)
				{
					widgetBox.Add(new SymbolChooser(key,mappings[key]));
				}
			}
			widgetBox.Add(null);
			window.AddWithViewport(widgetBox);
			this.Add(window);
			this.ShowAll();
		}	
		
		public Dictionary<string,string> GetMappings()
		{
			Dictionary<string,string> mappings = new Dictionary<string, string>();
			foreach(SymbolChooser chooser in widgetBox.Children)
			{
				mappings.Add(chooser.Key, chooser.SymbolName);
			}
			return mappings;
		}
	}
}
