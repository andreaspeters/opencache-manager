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
using Gtk;
using WebKit;
using Mono.Unix;

namespace ocmgtk
{


	public partial class WebBrowser : Gtk.Window
	{
		WebView m_view;
		public WebBrowser () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			m_view = new WebView();
			browserScroll.AddWithViewport(m_view);
			m_view.LoadChanged += HandleViewLoadChanged;
			browserScroll.ShowAll();
			this.Maximize();
		}

		void HandleViewLoadChanged (object o, LoadChangedArgs args)
		{
			switch (args.LoadEvent)
			{
				case LoadEvent.Started:
					browserStatus.Push(browserStatus.GetContextId("load"), "Initializing...");
					browserProgress.Fraction = 0;
					browserProgress.Show();
					break;
				case LoadEvent.Committed:
					browserStatus.Push(browserStatus.GetContextId("load"), Catalog.GetString("Loading..."));
					browserProgress.Fraction = 0.5;
					break;
				case LoadEvent.Finished:
					browserStatus.Push(browserStatus.GetContextId("load"), "Complete");
					browserProgress.Hide();
					break;
			}
		}
		
		public void LoadURL(String url)
		{
			m_view.LoadUri(url);
		}
	}
}
