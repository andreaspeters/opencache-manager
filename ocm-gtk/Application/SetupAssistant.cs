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
using ocmengine;
using ocmengine.SQLLite;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class SetupAssistant : Gtk.Assistant
	{
		SetupAssistantPage1 page1 = new SetupAssistantPage1 ();
		SetupAssistantPage2 page2 = new SetupAssistantPage2 ();
		SetupAssistantPage3 page3 = new SetupAssistantPage3 ();
		SetupAssistantPage4 summ = new SetupAssistantPage4 ();
		
		OCMApp m_App;

		public SetupAssistant (OCMApp app)
		{
			this.Build ();	
			m_App = app;
			AppendPage (page1);
			AppendPage (page2);
			AppendPage (page3);
			AppendPage (summ);
			Title = Catalog.GetString("Setup Assistant");
			SetPageTitle (page1, Catalog.GetString("Welcome"));
			SetPageComplete (page1, true);
			SetPageType (page1, Gtk.AssistantPageType.Intro);
			SetPageTitle (page2, Catalog.GetString("Setup a Database"));
			SetPageComplete (page2, true);
			SetPageTitle (page3, Catalog.GetString("User Details"));
			SetPageType (page3, Gtk.AssistantPageType.Content);
			SetPageComplete(page3, true);
			SetPageTitle(summ, "Summary");
			SetPageType (summ, Gtk.AssistantPageType.Summary);
			SetPageComplete(summ, true);
			WidthRequest = 600;
			HeightRequest = 500;
			this.Cancel += HandleHandleCancel;
			this.Apply += HandleHandleApply;
			this.Close += HandleHandleClose;
		}

		void HandleHandleClose (object sender, EventArgs e)
		{
			// The setup field may be empty (for example, when the platform has no
			// Documents folder). Use a per-user data directory rather than passing an
			// empty path to Directory.CreateDirectory.
			string dataDirectory = page2.DataDirectory;
			bool useDefaultDataDirectory = string.IsNullOrWhiteSpace(dataDirectory);
			if (useDefaultDataDirectory)
			{
				string localDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (string.IsNullOrWhiteSpace(localDataDirectory))
				{
					string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					if (string.IsNullOrWhiteSpace(userProfile))
						userProfile = Environment.GetEnvironmentVariable("HOME");
					if (string.IsNullOrWhiteSpace(userProfile))
						throw new InvalidOperationException("Cannot determine a per-user directory for OpenCacheManager data.");
					localDataDirectory = Path.Combine(userProfile, ".local", "share");
				}
				dataDirectory = Path.Combine(localDataDirectory, "opencachemanager");
			}
			else if (!Path.IsPathRooted(dataDirectory))
			{
				dataDirectory = Path.GetFullPath(dataDirectory);
			}

			string dbFile = page2.DBFile;
			if (string.IsNullOrWhiteSpace(dbFile) || dbFile == Path.Combine(Path.GetPathRoot(dataDirectory) ?? Path.DirectorySeparatorChar.ToString(), "default.ocm"))
				dbFile = Path.Combine(dataDirectory, "default.ocm");

			Directory.CreateDirectory(dataDirectory);
			string dbDirectory = Path.GetDirectoryName(dbFile);
			if (!string.IsNullOrEmpty(dbDirectory))
				Directory.CreateDirectory(dbDirectory);
			if (!File.Exists(dbFile))
			{
				FileStore store = new FileStore(dbFile);
				store.Dispose();
			}

			Config config = new Config();
			config.DataDirectory = dataDirectory;
			config.DBFile = dbFile;
			config.HomeLat = page3.HomeLat;
			config.HomeLon = page3.HomeLon;
			config.OwnerID = page3.MemberID;
			config.ImperialUnits = page2.ImperialUnits;
			config.MapType = page2.DefaultMap;
			config.WizardDone = true;
			this.Hide();
			this.Dispose();
			m_App.InitializeApp(null, false);
		}

		void HandleHandleApply (object sender, EventArgs e)
		{
			
		}

		void HandleHandleCancel (object sender, EventArgs e)
		{
			this.Hide ();
		}
	}
}
