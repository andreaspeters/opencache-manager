// 
//  Copyright 2011  Kyle Campbell
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
using ocmengine;
using WebKit;
using Mono.Unix;

namespace ocmgtk
{


	public partial class LoggingDialog : Gtk.Dialog
	{
		WebView cacheLog = new WebView();
		protected virtual void OnCloseClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Dispose();
		}
		
		public LoggingDialog ()
		{
			this.Build ();
			scrolledWindow.AddWithViewport(cacheLog);
			cacheLog.LoadStarted += HandleCacheLogLoadStarted;
			cacheLog.LoadProgressChanged += HandleCacheLogLoadProgressChanged;
			cacheLog.LoadFinished += HandleCacheLogLoadFinished;
			this.ShowAll();
		}

		void HandleCacheLogLoadFinished (object o, LoadFinishedArgs args)
		{
			loadProgressBar.Visible = false;
		}

		void HandleCacheLogLoadProgressChanged (object o, LoadProgressChangedArgs args)
		{
			loadProgressBar.Fraction = (double) args.Progress / (double) 100;
			loadProgressBar.Text = String.Format(Catalog.GetString("{0}% complete"), args.Progress);
		}

		void HandleCacheLogLoadStarted (object o, LoadStartedArgs args)
		{
			loadProgressBar.Text = Catalog.GetString("Loading...");
			loadProgressBar.Fraction = 0;
		}
		
		public void LogCache(Geocache cache)
		{
			if (cache.URL.ToString().Contains("opencaching"))
				cacheLog.LoadUri(cache.URL.ToString().Replace("viewcache.php", "log.php"));
			else if (cache.URL.ToString().Contains("geocaching"))
				cacheLog.LoadUri("http://www.geocaching.com/seek/log.aspx?ID=" + cache.CacheID);
			else
				cacheLog.LoadUri(cache.URL.ToString());
			this.Maximize();
		}
	}
}
