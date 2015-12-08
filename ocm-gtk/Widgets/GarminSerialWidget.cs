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

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class GarminSerialWidget : Gtk.Bin, IGPSConfig
	{

		public GarminSerialWidget ()
		{
			this.Build ();
		}
		
			public string GetBabelFormat ()
		{
			return "garmin";
		}
		
		public int GetCacheLimit ()
		{
			if (limitCheck.Active)
				return int.Parse(limitEntry.Text);
			else
				return -1;
		}
		
		public void SetCacheLimit(int val)
		{
			if (val == -1)
			{
				limitCheck.Active = false;
				limitEntry.Text = "500";
			}
			else
			{
				limitCheck.Active = true;
				limitEntry.Text = val.ToString();
			}
		}
		
		public int GetLogLimit()
		{
			return 0;
		}
		
		public bool IgnoreGeocacheOverrides()
		{
			return false;
		}
		
		public void SetGeocacheOverride(bool val)
		{
			//geocacheCheck.Active = val;
		}
		
		public bool IgnoreWaypointOverrides()
		{
			return false;
		}
		
		public void SetIgnoreWaypoint(bool val)
		{
		//	overrideCheck.Active = val;
		}
		
		public ocmengine.WaypointNameMode GetNameMode()
		{
			switch (nameMode.Active)
			{
				case 0:
					return ocmengine.WaypointNameMode.CODE;
				case 1:
					return ocmengine.WaypointNameMode.NAME;
				default:
					return ocmengine.WaypointNameMode.SHORTCODE;
			}
		}
		
		public void SetNameMode(ocmengine.WaypointNameMode mode)
		{
			switch (mode)
			{
				case ocmengine.WaypointNameMode.CODE:
					nameMode.Active = 0;
					break;
				case ocmengine.WaypointNameMode.NAME:
					nameMode.Active = 1;
					break;
				default:
					nameMode.Active = 2;
					break;
			}
		}
		
		public ocmengine.WaypointDescMode GetDescMode()
		{
			return ocmengine.WaypointDescMode.DESC;
		}
		
		public string GetOutputFile ()
		{
			return fileEntry.Text;
		}
		
		public void SetOutputFile(String val)
		{
			fileEntry.Text = val;
		}

		protected virtual void OnLimitToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCheck.Active;
		}
		
		public bool IncludeAttributes ()
		{
			return false;
		}
		
		public string FieldNotesFile
		{
			get { return null;}
			set { }
		}
	}
}
