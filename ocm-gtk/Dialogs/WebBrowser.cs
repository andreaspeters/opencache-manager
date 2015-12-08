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
			m_view.LoadStarted += HandleViewLoadStarted;
			m_view.LoadProgressChanged += HandleViewLoadProgressChanged;
			m_view.LoadFinished += HandleM_viewLoadFinished;
			browserScroll.ShowAll();
			this.Maximize();
		}

		void HandleM_viewLoadFinished (object o, LoadFinishedArgs args)
		{
			browserStatus.Push(browserStatus.GetContextId("load"), String.Format("Complete"));
			browserProgress.Hide();
		}
		
		public void LoadURL(String url)
		{
			m_view.LoadUri(url);
		}

		void HandleViewLoadProgressChanged (object o, LoadProgressChangedArgs args)
		{
			browserStatus.Push(browserStatus.GetContextId("load"), String.Format(Catalog.GetString("Loading ({0}%)"), args.Progress.ToString()));
			browserProgress.Fraction = (double) args.Progress / 100d;
		}

		void HandleViewLoadStarted (object o, LoadStartedArgs args)
		{
			browserStatus.Push(browserStatus.GetContextId("load"),"Initializing...");
			browserProgress.Fraction = 0;
		}
	}
}
