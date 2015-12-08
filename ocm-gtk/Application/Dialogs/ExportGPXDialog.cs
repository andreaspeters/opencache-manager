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

namespace ocmgtk
{


	public partial class ExportGPXDialog : Gtk.Dialog
	{
		public ExportGPXDialog ()
		{
			this.Build ();
		}
		
		public void SetCurrentFolder(String folder)
		{
			fileChooser.SetCurrentFolder(folder);
		}
		
		public Dictionary<string,string> WaypointMappings
		{
			get{return waypointSymbolWidget.GetMappings();}
			set{waypointSymbolWidget.PopulateMappings(value);}
		}
		
		public String CurrentName
		{
			set{ fileChooser.CurrentName = value;}
		}
		
		public String Filename
		{
			get { return fileChooser.Filename;}
		}
		
		public void AddFilter(Gtk.FileFilter filter)
		{
			fileChooser.AddFilter(filter);
		}
		
		public bool IsPaperless
		{
			get { return exportPaperlessDetails.Active;}
			set { exportPaperlessDetails.Active = value;}
		}
		
		public int CacheLimit
		{
			get {
				if (limitCheck.Active)
					return int.Parse(limitEntry.Text);
				else
					return -1;
			}
			set
			{
				if (value != -1)
				{
					limitCheck.Active = true;
					limitEntry.Text = value.ToString();
				}
			}
		}
		
		public bool UseMappings
		{
			get { return customSymbolsCheck.Active;}
			set { customSymbolsCheck.Active = value;}
		}
		
		
		public bool IncludeChildren
		{
			get { return includeChildrenCheck.Active; }
			set { includeChildrenCheck.Active = value;}
		}
		
		public ocmengine.WaypointNameMode NameMode
		{
			get{
				switch (nameMode.Active)
				{
					case 0:
						return ocmengine.WaypointNameMode.CODE;
					default:
						return ocmengine.WaypointNameMode.NAME;
				}
			}
			set
			{
				if (value == ocmengine.WaypointNameMode.CODE)
					nameMode.Active = 0;
				else
					nameMode.Active = 1;
			}
		}
		
		public ocmengine.WaypointDescMode DescMode
		{
			get
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
			set
			{
				if (value == ocmengine.WaypointDescMode.DESC)
					descMode.Active = 0;
				else if (value == ocmengine.WaypointDescMode.CODESIZETYPE)
					descMode.Active = 1;
				else
					descMode.Active = 2;
			}
		}
		
		public int LogLimit
		{
			get { 
				if (logLimitCheck.Active)
				    return int.Parse(logLimitEntry.Text);
				else
					return -1;
			}
			set
			{
				if (value != -1)
				{
					logLimitCheck.Active = true;
					logLimitEntry.Text = value.ToString();
				}
			}
		}
		
		public bool IncludeAttributes
		{
			get {return attrCheck.Active;}
			set {attrCheck.Active = value;}
		}
		
		public bool UsePlainText
		{
			get {return usePlainTextCheck.Active;}
			set { usePlainTextCheck.Active = value;}
		}
		
		protected virtual void OnPaperlessToggle (object sender, System.EventArgs e)
		{
			paperlessFrame.Sensitive = exportPaperlessDetails.Active;
		}
		
		protected virtual void OnLimitToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCheck.Active;
		}
		protected virtual void OnLogLimitToggle (object sender, System.EventArgs e)
		{
			logLimitEntry.Sensitive = logLimitCheck.Active;
		}
		
		
	}
}
