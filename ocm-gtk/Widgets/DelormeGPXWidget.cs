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
	public partial class DelormeGPXWidget : Gtk.Bin, IGPSConfig
	{

		public DelormeGPXWidget ()
		{
			this.Build ();
		}
		
		protected virtual void OnLimitToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCheck.Active;
		}
		
		public int GetCacheLimit ()
		{
			if (limitCheck.Active)
				return int.Parse(limitEntry.Text);
			return -1;
		}
		
		public void SetCacheLimit(int val)
		{
			if (val == -1)
			{
				limitCheck.Active = false;
				limitEntry.Text = "1000";
			}
			else
			{
				limitCheck.Active = true;
				limitEntry.Text = val.ToString();
			}
		}


		public string GetOutputFile ()
		{
			if (autoNameRado.Active == true)
				return "%auto%";
			return fileEntry.Text;
		}
		
		public void SetOutputFile(String file)
		{
			if (file == "%auto%")
				autoNameRado.Active = true;
			else
			{
				fileEntry.Text = file;
				useFileRadio.Active = true;
			}
		}

		public string GetBabelFormat ()
		{
			return "delgpx";
		}
		
		public int GetLogLimit()
		{
			if (logLimitCheck.Active)
				return int.Parse(logLimitEntry.Text);
			return -1;
		}
		
		public void SetLogLimit(int val)
		{
			if (val == -1)
				logLimitCheck.Active = false;
			else
			{
				logLimitEntry.Text = val.ToString();
				logLimitCheck.Active = true;
			}
		}
		
		public bool IgnoreGeocacheOverrides()
		{
			return false;
		}
		
		public bool IgnoreWaypointOverrides()
		{
			return false;
		}
		
		public ocmengine.WaypointNameMode GetNameMode()
		{
			return ocmengine.WaypointNameMode.CODE;
		}
		
		public ocmengine.WaypointDescMode GetDescMode()
		{
			return ocmengine.WaypointDescMode.DESC;
		}
		
		public string FieldNotesFile
		{
			get { return fieldNotesEntry.Text;}
			set { fieldNotesEntry.Text = value;}
		}
		
		protected virtual void OnFileClick (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Select GPX location"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			dlg.CurrentName = "geocaches.gpx";
			FileFilter filter = new FileFilter ();
			filter.Name = "GPS Exchange Files";
			filter.AddMimeType ("text/xml");
			filter.AddMimeType ("application/xml");
			filter.AddMimeType ("application/x-gpx");
			filter.AddPattern ("*.gpx");
			
			dlg.AddFilter (filter);
			
			if (dlg.Run () == (int)ResponseType.Accept) {
				fileEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		protected virtual void OnLogLimitToggle (object sender, System.EventArgs e)
		{
			logLimitEntry.Sensitive = logLimitCheck.Active;
		}
		protected virtual void OnFileToggle (object sender, System.EventArgs e)
		{
			fileEntry.Sensitive = useFileRadio.Active;
		}
		
		public bool IncludeAttributes ()
		{
			return attrCheck.Active;
		}
		
		public void SetIncludeAttributes(bool val)
		{
			attrCheck.Active = val;
		}
		
		protected virtual void OnFieldBrowse (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Select Field Notes location"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			dlg.CurrentName = "FieldNotes.gpx";
			FileFilter filter = new FileFilter ();
			filter.Name = Catalog.GetString("Text Files");
			filter.AddMimeType ("text/plain");
			filter.AddPattern ("*.txt");			
			dlg.AddFilter (filter);			
			if (dlg.Run () == (int)ResponseType.Accept) {
				fieldNotesEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		
	}
}
