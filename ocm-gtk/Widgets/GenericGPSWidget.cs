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
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class GenericGPSWidget : Gtk.Bin, IGPSConfig
	{

		public GenericGPSWidget ()
		{
			this.Build ();
		}

		public string GetBabelFormat ()
		{
			return formatEntry.Text;
		}
		
		public void SetBabelFormat(String val)
		{
			formatEntry.Text = val;
		}

		public int GetCacheLimit ()
		{
			if (limitCheck.Active)
				return int.Parse (limitEntry.Text);
			return -1;
		}

		public void SetCacheLimit (int val)
		{
			if (val == -1) {
				limitCheck.Active = false;
				limitEntry.Text = "1000";
			} else {
				limitCheck.Active = true;
				limitEntry.Text = val.ToString ();
			}
		}

		public string GetOutputFile ()
		{
			return fileEntry.Text;
		}
		
		public void SetOutputFile(String file)
		{
			fileEntry.Text = file;
		}
		
		public int GetLogLimit()
		{
			return -1;
		}
		
		public ocmengine.WaypointNameMode GetNameMode()
		{
			switch (nameCombo.Active)
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
					nameCombo.Active = 0;
					break;
				case ocmengine.WaypointNameMode.NAME:
					nameCombo.Active = 1;
					break;
				default:
					nameCombo.Active = 2;
					break;
			}
		}
		
		public ocmengine.WaypointDescMode GetDescMode()
		{
			switch (descCombo.Active)
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
					descCombo.Active = 0;
					break;
				case ocmengine.WaypointDescMode.CODESIZEANDHINT:
					descCombo.Active = 1;
					break;
				default:
					descCombo.Active = 2;
					break;
					
			}
		}
		
		protected virtual void OnInfoClick (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.gpsbabel.org/readme.html");
		}
		
		protected virtual void onLimitToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCheck.Active;
		}
		
		public bool IncludeAttributes ()
		{
			return true;
		}
		
		protected virtual void OnFileClick (object sender, System.EventArgs e)
		{
			
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Select File location"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			dlg.CurrentName = "caches.loc";
			FileFilter filter = new FileFilter ();
			filter.Name = "All Files";
			filter.AddMimeType ("*/*");
			dlg.AddFilter (filter);
			
			if (dlg.Run () == (int)ResponseType.Accept) {
				fileEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		protected virtual void OnLimitCheckToggled (object sender, System.EventArgs e)
		{
			this.limitEntry.Sensitive = limitCheck.Active;;
		}
		
		public string FieldNotesFile
		{
			get { return null;}
			set {}
		}
		
		
		
		
		
		
		
	}
}
