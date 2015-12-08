// 
//  Copyright 2011  campbelk
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
using Mono.Unix;
using Gtk;
using Gdk;

namespace ocmgtk
{


	public class CleanupAssistant:Gtk.Assistant
	{
		CleanUpPage1 page1 = new CleanUpPage1();
		public CleanupPage2 page2 = new CleanupPage2();
		CleanupConfirmPage confirm = new CleanupConfirmPage();
		CleanupSummary summaryPage = new CleanupSummary();
		CleanupManager mgr = null;
		OCMApp m_App;
		OCMMainWindow m_Win;
		
		public CleanupManager Manager
		{
			get { return mgr;}
		}

		public CleanupAssistant (OCMApp app, OCMMainWindow win)
		{
			win.CacheList.ApplyQuickFilter(QuickFilter.ALL_FILTER);
			mgr = new CleanupManager(app.CacheStore, win.CacheList.UnfilteredCaches);
			m_Win = win;
			m_App = app;
			AppendPage(page1);
			AppendPage(page2);
			AppendPage(confirm);
			AppendPage(summaryPage);
			Title = Catalog.GetString("Cleanup Assistant");
			SetPageTitle(page1, Catalog.GetString("Introduction"));
			SetPageComplete(page1, true);
			SetPageType(page1, AssistantPageType.Intro);
			SetPageTitle(page2, Catalog.GetString("Options"));
			SetPageComplete(page2, true);
			SetPageType(page2, AssistantPageType.Content);
			SetPageTitle(confirm, Catalog.GetString("Confirm Cleanup"));
			SetPageComplete(confirm, true);
			SetPageType(confirm, AssistantPageType.Confirm);
			SetPageTitle(summaryPage, Catalog.GetString("Summary"));
			SetPageComplete(summaryPage, true);
			SetPageType(summaryPage, AssistantPageType.Summary);
			WidthRequest = 800;
			HeightRequest = 500;
			Modal = true;
			SetPosition(WindowPosition.Center);
			this.ShowAll();
			this.Cancel += HandleHandleCancel;
			this.Apply += HandleHandleApply;
			this.Close += HandleHandleClose;
			this.Prepare += HandleHandlePrepare;
		}

		void HandleHandlePrepare (object o, PrepareArgs args)
		{
			if (args.Page == confirm)
			{
				this.GdkWindow.Cursor = new Cursor (CursorType.Watch);
				OCMApp.UpdateGUIThread();
				confirm.Prepare();
				this.GdkWindow.Cursor = new Cursor (CursorType.Arrow);
			}
		}

		void HandleHandleClose (object sender, EventArgs e)
		{
			this.Hide();
			m_Win.CacheList.Refresh();
		}

		void HandleHandleApply (object sender, EventArgs e)
		{
			mgr.logLimit = page2.LogLimit;
			mgr.Cleanup();	
		}

		void HandleHandleCancel (object sender, EventArgs e)
		{
			this.Hide();
		}
	}
}
