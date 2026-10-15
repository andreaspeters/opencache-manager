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
using Gtk;

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
			// Snapshot widget values before disposing the assistant; disposal can
			// invalidate child widgets and make their Text properties empty.
			string dataDirectory = page2.DataDirectory;
			bool useDefaultDataDirectory = string.IsNullOrWhiteSpace(dataDirectory);
			if (useDefaultDataDirectory)
				dataDirectory = SetupAssistantPage2.GetDefaultDataDirectory();
			else if (!System.IO.Path.IsPathRooted(dataDirectory))
				dataDirectory = System.IO.Path.GetFullPath(dataDirectory);

			string dbFile = page2.DBFile;
			bool generatedRootDbPath = useDefaultDataDirectory &&
				!string.IsNullOrWhiteSpace(dbFile) &&
				System.IO.Path.GetFileName(dbFile) == "default.ocm" &&
				System.IO.Path.GetDirectoryName(dbFile) == System.IO.Path.GetPathRoot(dbFile);
			if (string.IsNullOrWhiteSpace(dbFile) || generatedRootDbPath)
				dbFile = System.IO.Path.Combine(dataDirectory, "default.ocm");
			else if (!System.IO.Path.IsPathRooted(dbFile))
				dbFile = System.IO.Path.GetFullPath(dbFile);

			double homeLat = page3.HomeLat;
			double homeLon = page3.HomeLon;
			string ownerId = page3.MemberID;
			bool imperialUnits = page2.ImperialUnits;
			string mapType = page2.DefaultMap;

			Directory.CreateDirectory(dataDirectory);
			string dbDirectory = System.IO.Path.GetDirectoryName(dbFile);
			if (!string.IsNullOrEmpty(dbDirectory))
				Directory.CreateDirectory(dbDirectory);
			if (!File.Exists(dbFile))
			{
				FileStore store = new FileStore(dbFile);
				store.Dispose();
			}

			// Update the same settings instance that OCMApp.InitializeApp reads.
			IConfig config = m_App.AppConfig;
			config.DataDirectory = dataDirectory;
			config.DBFile = dbFile;
			config.HomeLat = homeLat;
			config.HomeLon = homeLon;
			config.OwnerID = ownerId;
			config.ImperialUnits = imperialUnits;
			config.MapType = mapType;
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
