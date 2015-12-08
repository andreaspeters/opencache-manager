// 
//  Copyright 2010  campbelk
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

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class HTMLWidget : Gtk.Bin
	{
		protected WebKit.WebView m_view;
		bool contentLoaded = false;
		
		OCMApp m_App;
		public OCMApp App
		{
			set { m_App = value;}
		}
		
		OCMMainWindow m_Win;
		public OCMMainWindow Win
		{
			set { m_Win = value;}
		}
		
		public HTMLWidget ()
		{
			this.Build ();
			m_view = new WebKit.WebView();
			htmlScroll.Add(m_view);
			m_view.LoadFinished += HandleM_viewLoadFinished;
			m_view.NavigationRequested += HandleM_viewNavigationRequested;
		}

		void HandleM_viewLoadFinished (object o, WebKit.LoadFinishedArgs args)
		{
			contentLoaded = true;
		}

		void HandleM_viewNavigationRequested (object o, WebKit.NavigationRequestedArgs args)
		{
			if (contentLoaded)
			{
				m_view.StopLoading();
				if (args.Request.Uri.StartsWith("ocm://"))
				{
					string[]  request = args.Request.Uri.Substring(6).Split('/');
					if (request[0].Equals("deleteLog"))
					{
						MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, Mono.Unix.Catalog.GetString("Are you sure you wish to delete this log?"));
						if (((int) ResponseType.Yes) == dlg.Run())
						{
							dlg.Hide();
							m_App.CacheStore.PurgeLogsByKey(new String[]{request[1]});
							m_App.RefreshAll();
						}
						dlg.Hide();
					}
					else if (request[0].Equals("editLog"))
					{
						OfflineLogDialog dlg = new OfflineLogDialog();
						dlg.MainWin = m_Win;
						dlg.useFullLog = true;
						ocmengine.CacheLog log = m_App.CacheStore.GetCacheLogByKey(request[1]);
						dlg.Log = log;
						if ((int)ResponseType.Ok == dlg.Run ()) 
						{
							log = dlg.Log;
							m_App.CacheStore.AddLog(log.CacheCode, log);
							dlg.Hide ();
							m_App.RefreshAll();
						}
						dlg.Hide ();
					}
				}
				else
					System.Diagnostics.Process.Start(args.Request.Uri);
			}
		}
		
		public void SetHTML(string html, string  baseURL)
		{
			contentLoaded = false;
			m_view.LoadHtmlString(html, baseURL);
		}
		
		public string HTML
		{
			//set { int i=1;}
			set {
				contentLoaded = false;
				m_view.LoadHtmlString(value, "http://www.geocaching.com");
			}
		}
		
		public void ExecuteFunction(string func)
		{
			m_view.ExecuteScript(func);
		}
	}
}
