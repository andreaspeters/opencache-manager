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
using System.Text;
using System.Text.RegularExpressions;
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReviewWaypointWidget : Gtk.Bin
	{
		
		Geocache m_parent = null;
		public Geocache ParentCache
		{
			set {m_parent = value;}
		}
		
		OCMApp m_App = null;
		public OCMApp App
		{
			set { m_App = value;}
		}
		
		DegreeMinutes m_lat = null;
		DegreeMinutes m_lon = null;
		
		public Match WaypointMatch
		{
			set
			{
				DegreeMinutes[] coord =  Utilities.ParseCoordString(value.Captures[0].Value);	
				m_lat = coord[0];
				m_lon = coord[1];
				StringBuilder builder = new StringBuilder(m_parent.ShortDesc + m_parent.LongDesc);
				builder.Insert(value.Index + value.Length, "</span>");
				builder.Insert(value.Index, "<span style='background-color:green; color=white;'><a name='spot'>");
				builder.Insert(0, "<HTML><HEAD><TITLE>ignore</TITLE>" +
					"<SCRIPT>function gotoSpot(){ window.location.href='#spot';}</SCRIPT>" +
					"</HEAD><BODY onLoad='gotoSpot()'>");
				builder.Append("</BODY></HTML>");
				coordLabel.Text = Utilities.getCoordString(m_lat, m_lon);
				sourceText.HTML = builder.ToString();
				sourceText.ExecuteFunction("gotoSpot()");
				String name = nameEntry.Text;
				if (String.IsNullOrEmpty(name))
				{
					name = "RP" + m_parent.Name.Substring (2);
					if ((bool) m_App.AppConfig.IgnoreWaypointPrefixes)
					{
						name = m_parent.Name;
					}
				}
				name = m_App.CacheStore.GetUniqueName(name);
				nameEntry.Text = name;
				descriptionText.Buffer.Text = Catalog.GetString("Grabbed Waypoint");
			}
		}
		
		private bool IgnorePrefix
		{
			get
			{
				return m_App.AppConfig.IgnoreWaypointPrefixes;
			}
		}
		
		public ocmengine.Waypoint GetPoint()
		{
			Waypoint pt = new Waypoint();
			pt.Name = nameEntry.Text;
			pt.Lat = m_lat.GetDecimalDegrees();
			pt.Lon = m_lon.GetDecimalDegrees();
			pt.Parent = m_parent.Name;
			pt.Symbol = GetPTType(flagEntry.Active);
			pt.Desc = descriptionText.Buffer.Text;
			return pt;
		}
		
		private string GetPTType(int code)
		{
			switch (code)
			{
			case 0:
				return "Final Location";
			case 1:
				return "Parking Area";
			case 2:
				return "Question to Answer";
			case 3:
				return "Reference Point";
			case 4:
				return "Stages of a Multicache";
			case 5:
				return "Trailhead";
			default:
				return "Other";
			}
		}
		
		
		protected virtual void OnComboChanged (object sender, System.EventArgs e)
		{
			String prefix = "OT";
			switch (flagEntry.Active)
			{
				case 0:
					prefix = "FL";
					break;
				case 1:
					prefix = "PK";
					break;
				case 2:
					prefix = "QA";
					break;
				case 3:
					prefix = "RP";
					break;
				case 4:
					prefix = "SM";
					break;
				case 5:
					prefix = "TR";
					break;
				default:
					break;
			}
			String name = nameEntry.Text;
			if (IgnorePrefix)
				 name = m_parent.Name;
			else
				 name = prefix + m_parent.Name.Substring(2);
			name = m_App.CacheStore.GetUniqueName(name);
			nameEntry.Text = name;
		}
		
		
		
		public ReviewWaypointWidget ()
		{
			this.Build ();
		}
	}
}
