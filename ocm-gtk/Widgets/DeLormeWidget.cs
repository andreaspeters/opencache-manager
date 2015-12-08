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
	public partial class DeLormeWidget  : Gtk.Bin, IGPSConfig
	{

		public DeLormeWidget ()
		{
			this.Build ();
		}
		
		public string GetBabelFormat ()
		{
			return "delbin,logs=1,hint_at_end=1,gcsym=1";
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
			if (logCheck.Active)
				return int.Parse(logEntry.Text);
			else
				return 0;
		}
		
		public void SetLogLimit(int val)
		{
			if (val != 0)
			{
				logEntry.Text = val.ToString();
				logCheck.Active = true;
			}
		}
		
		public bool IgnoreGeocacheOverrides()
		{
			return true;
		}
		
		public void SetGeocacheOverride(bool val)
		{
			
		}
		
		public bool IgnoreWaypointOverrides()
		{
			return true;
		}
		
		public void SetIgnoreWaypoint(bool val)
		{
			
		}
		
		public ocmengine.WaypointNameMode GetNameMode()
		{
			return ocmengine.WaypointNameMode.CODE;
		}
		
		
		public ocmengine.WaypointDescMode GetDescMode()
		{
			return ocmengine.WaypointDescMode.DESC;
		}
		
		
		public string GetOutputFile ()
		{
			return "usb:";
		}

		protected virtual void OnLimitToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCheck.Active;
		}
		
		protected virtual void OnHotplugClick (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.gpsbabel.org/news/20100620.html");
		}	
		
		public bool IncludeAttributes ()
		{
			return attrCheck.Active;
		}
		
		public void SetIncludeAttributes(bool val)
		{
			attrCheck.Active = val;
		}
		
		public string FieldNotesFile
		{
			get { return null;}
			set { }
		}
	}
}
