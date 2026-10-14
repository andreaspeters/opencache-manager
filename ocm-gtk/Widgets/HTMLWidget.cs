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
using WebKit;
using Gtk;

namespace ocmgtk
{

	[System.ComponentModel.ToolboxItem(true)]
	public partial class HTMLWidget : Gtk.Bin
	{
		protected WebView m_view;
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
			m_view.LoadChanged += HandleM_viewLoadChanged;
			m_view.DecidePolicy += HandleM_viewDecidePolicy;
		}

		void HandleM_viewLoadChanged (object o, LoadChangedArgs args)
		{
			if (args.LoadEvent == LoadEvent.Finished)
				contentLoaded = true;
		}

		void HandleM_viewDecidePolicy (object o, DecidePolicyArgs args)
		{
			if (!contentLoaded || args.DecisionType != PolicyDecisionType.NavigationAction ||
			    !(args.Decision is NavigationPolicyDecision decision))
			{
				args.RetVal = false;
				return;
			}

			string uri = decision.Request.Uri;
			if (uri == null)
			{
				args.RetVal = false;
				return;
			}

			decision.Ignore();
			if (uri.StartsWith("ocm://", StringComparison.Ordinal))
			{
				string[] request = uri.Substring(6).Split('/');
				if (request[0].Equals("deleteLog"))
				{
					MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, Mono.Unix.Catalog.GetString("Are you sure you wish to delete this log?"));
					if (((int)ResponseType.Yes) == dlg.Run())
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
					if ((int)ResponseType.Ok == dlg.Run())
					{
						log = dlg.Log;
						m_App.CacheStore.AddLog(log.CacheCode, log);
						dlg.Hide();
						m_App.RefreshAll();
					}
					dlg.Hide();
				}
			}
			else
				OCMApp.TryStartExternalProcess(uri);

			args.RetVal = true;
		}
		
		public void SetHTML(string html, string  baseURL)
		{
			contentLoaded = false;
			m_view.LoadHtml(html, baseURL);
		}
		
		public string HTML
		{
			//set { int i=1;}
			set {
				contentLoaded = false;
				m_view.LoadHtml(value, "http://www.geocaching.com");
			}
		}
		
		public void ExecuteFunction(string func)
		{
			m_view.RunJavascript(func, null, null);
		}
	}
}
