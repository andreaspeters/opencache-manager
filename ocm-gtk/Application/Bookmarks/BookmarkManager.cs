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
using System.Collections.Generic;
using Gtk;


namespace ocmgtk
{


	public class BookmarkManager
	{
		private OCMApp m_App;
		private OCMMainWindow m_Win;
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}
		
		public BookmarkManager (OCMApp app)
		{
			m_App = app;
		}
		
		public string[] Bookmarks
		{
			get {return m_App.CacheStore.GetBookmarkLists();}
		}
		
		public Menu BuildBookmarkMenu()
		{
			Menu bookmarkMenu = new Menu();
			string[] bookmarks = m_App.CacheStore.GetBookmarkLists();
			int count = 0;
			RadioAction noList = new RadioAction("None", Catalog.GetString("None"), null, null, count);
			noList.Active = true;
			noList.Toggled += HandleNoListToggled;
			bookmarkMenu.Append(noList.CreateMenuItem());
			foreach(string bookmark in bookmarks)
			{
				RadioAction action = new RadioAction(bookmark, bookmark, null, null, count);
				action.Group = noList.Group;
				if (bookmark == m_App.CacheStore.ActiveBookmarkList)
					action.Active = true;
				action.Toggled += HandleActionToggled;	
				bookmarkMenu.Append(action.CreateMenuItem());
			}
			return bookmarkMenu;
		}
		
		public Menu BuildAddToMenu(System.EventHandler handler)
		{
			Menu bookmarkMenu = new Menu();
			string[] bookmarks = m_App.CacheStore.GetBookmarkLists();
			foreach(string bookmark in bookmarks)
			{
				//##AP Action action = new Action(bookmark, bookmark);
				Gtk.Action action = new Gtk.Action(bookmark, bookmark);
				action.Activated += handler;
				bookmarkMenu.Append(action.CreateMenuItem());
			}
			return bookmarkMenu;
		}

		void HandleActionToggled (object sender, EventArgs e)
		{
			RadioAction action = sender as RadioAction;
			if (action.Active)
			{
				m_App.CacheStore.ActiveBookmarkList = action.Label;
				m_Win.CacheList.Refresh();
			}
		}

		void HandleNoListToggled (object sender, EventArgs e)
		{
			if (((sender) as RadioAction).Active)
			{
				m_App.CacheStore.ActiveBookmarkList = null;
				m_Win.CacheList.Refresh();
			}
		}
	}
}
