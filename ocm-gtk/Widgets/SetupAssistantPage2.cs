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
using System.IO;
using Gtk;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class SetupAssistantPage2 : Gtk.Bin
	{
		
		public string DBFile
		{
			get { return dbEntry.Text;}
		}
		
		public string DefaultMap
		{
			get {
				switch (mapsCombo.Active)
				{
					case 1:
						return "ghyb";
					case 2:
						return "gmap";
					case 3:
						return "gphy";
					default:
						return "osm";
				}
			}
		}
		
		public bool ImperialUnits
		{
			get { 
				if (unitsCombo.Active == 1)
					return true;
				else
					return false;
			}		
		}
		
		public String DataDirectory
		{
			get
			{
				return dataEntry.Text;
			}
		}
		
		
		protected virtual void OnOpenClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Choose Database Location"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Open"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			dlg.CurrentName =  "default.ocm";
			FileFilter filter = new FileFilter ();
			filter.Name = "OCM Databases";
			filter.AddMimeType ("application/x-sqlite3");
			filter.AddPattern ("*.ocm");
			dlg.AddFilter (filter);
			
			if (dlg.Run () == (int)ResponseType.Accept) {
				dbEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}



		internal static string GetDefaultDataDirectory ()
		{
			string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (!string.IsNullOrWhiteSpace(documentsDirectory))
				return System.IO.Path.Combine(documentsDirectory, "ocm");

			string localDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrWhiteSpace(localDataDirectory))
			{
				string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				if (string.IsNullOrWhiteSpace(userProfile))
					userProfile = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrWhiteSpace(userProfile))
					throw new InvalidOperationException("Cannot determine a per-user directory for OpenCacheManager data.");
				localDataDirectory = System.IO.Path.Combine(userProfile, ".local", "share");
			}

			return System.IO.Path.Combine(localDataDirectory, "opencachemanager", "ocm");
		}

		public SetupAssistantPage2 ()
		{
			this.Build ();
			dataEntry.Text = GetDefaultDataDirectory();
			dbEntry.Text = System.IO.Path.Combine(dataEntry.Text, "default.ocm");
		}
		protected virtual void OnDirectoryClick (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Choose Data Directory"), null, FileChooserAction.SelectFolder, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (dataEntry.Text);
						
			if (dlg.Run () == (int)ResponseType.Accept) {
				dataEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		protected virtual void OnDefaultDirChanged (object sender, System.EventArgs e)
		{
			dbEntry.Text = dataEntry.Text + "/default.ocm";
		}
		
		
		
		
	}
}
