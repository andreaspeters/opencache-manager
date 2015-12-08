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
	public partial class GarminUSBWidget : Gtk.Bin, IGPSConfig
	{
		protected virtual void OnHotplugClick (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.gpsbabel.org/os/Linux_Hotplug.html");
		}			

		public GarminUSBWidget ()
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
			
		public ocmengine.WaypointNameMode GetNameMode()
		{
			switch (nameMode.Active)
			{
				case 0:
					return ocmengine.WaypointNameMode.CODE;
				default:
					return ocmengine.WaypointNameMode.NAME;
			}
		}
		
		public void SetNameMode(ocmengine.WaypointNameMode mode)
		{
			switch (mode)
			{
				case ocmengine.WaypointNameMode.CODE:
					nameMode.Active = 0;
					break;
				default:
					nameMode.Active = 1;
					break;
			}
		}
		
		public ocmengine.WaypointDescMode GetDescMode()
		{
			switch (descMode.Active)
			{
				case 0:
					return ocmengine.WaypointDescMode.DESC;
				case 1:
					return ocmengine.WaypointDescMode.CODESIZEANDHINT;
				default:
					return ocmengine.WaypointDescMode.CODESIZETYPE;
			}
		}
		
		public void SetDescMode(ocmengine.WaypointDescMode mode)
		{
			switch (mode)
			{
				case ocmengine.WaypointDescMode.DESC:
					descMode.Active = 0;
					break; 
				case ocmengine.WaypointDescMode.CODESIZEANDHINT:
					descMode.Active = 1;
					break;
				default:
					descMode.Active = 2;
					break;
				
			}
		}
		
		public string GetOutputFile ()
		{
			return "usb:";
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
			set {}
		}
		
	}
}
