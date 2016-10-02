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
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class CacheInfoWidget : Gtk.Bin
	{
		OCMApp m_App = null;
		private OCMApp App
		{
			set
			{
				quickInfoPane.App = value;
				waypointWidget.App = value;
				descriptionPane.App = value;
				logViewer.App = value;
				m_App = value;
			}
		}
		
		public OCMMainWindow MainWin
		{
			set { 
				cacheNotes.MainWin = value;
				imageWidget.MainWin = value;
				quickInfoPane.MainWin = value;
				logViewer.Win = value;
				App = value.App;
			}
		}

		public CacheInfoWidget ()
		{
			this.Build ();
			quickInfoPane.Logs = logViewer;
		}
		
		public void SetCache(Geocache cache)
		{
			descriptionPane.SetCache(cache);
			cacheNotes.SetCache(cache);
			imageWidget.SetCache(cache);
			if (cache == null)
				logViewer.SetCacheLogs(null);
			else
				logViewer.SetCacheLogs(m_App.CacheStore.GetCacheLogs(cache.Name));
			if (cache == null)
				waypointWidget.SetCacheAndPoints(null, null);
			else
				waypointWidget.SetCacheAndPoints(cache, m_App.CacheStore.GetChildWaypoints(new string[]{cache.Name}));
			quickInfoPane.SetCache(cache);
			if (imageWidget.HasImages)
				imageLabel.Markup = "<b>" + Catalog.GetString("Images") + "</b>";
			else
				imageLabel.Text = Catalog.GetString("Images");
		}
		
		public void SelectChildByName(String name)
		{
			waypointWidget.SelecteWaypointByName(name);
		}
		
		public void GrabImages()
		{
			imageWidget.GrabImages();
		}
	}
}
