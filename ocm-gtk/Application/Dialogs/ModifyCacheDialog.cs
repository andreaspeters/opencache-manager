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
using ocmengine;

namespace ocmgtk
{

	
	public partial class ModifyCacheDialog : Gtk.Dialog
	{
		
		private Geocache m_cache;
		
		public OCMApp m_App;
		public OCMApp App
		{
			set { m_App = value;}
		}
		
		public Geocache Cache
		{
			set { 
				m_cache = value;
				PopulateDescription();
			}
			get { 
				UpdateCache();
				return m_cache;
			}
		}
		
		public ModifyCacheDialog ()
		{
			this.Build ();
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnCancelClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		public bool IsModifyDialog
		{
			set
			{
				if (value)
				{
					codeEntry.Sensitive = false;
					this.Title = "Modify Cache";
				}
				else
				{
					codeEntry.Sensitive = true;
					this.Title = "Add Cache";
				}
			}
		}
		
		private void PopulateDescription()
		{
			codeEntry.Text = m_cache.Name;
			descriptionEntry.Buffer.Text = m_cache.Desc;
			if (m_cache.URL != null)
				urlEntry.Text = m_cache.URL.ToString();
			else
				urlEntry.Text = String.Empty;
			urlNameEntry.Text = m_cache.URLName;
			coordEntry.Latitude = m_cache.OrigLat;
			coordEntry.Longitude = m_cache.OrigLon;
			nameEntry.Text = m_cache.CacheName;
			shortDescEntry.Buffer.Text = m_cache.ShortDesc;
			longDescEntry.Buffer.Text = m_cache.LongDesc;
			hintEntry.Buffer.Text = m_cache.Hint;
			placedByEntry.Text = m_cache.PlacedBy;
			ownerEntry.Text = m_cache.CacheOwner;
			ownerIDEntry.Text = m_cache.OwnerID;
			cacheIDEntry.Text = m_cache.CacheID;
			udata1Entry.Text = m_cache.User1;
			uData2Entry.Text = m_cache.User2;
			uData3Entry.Text = m_cache.User3;
			uData4Entry.Text = m_cache.User4;
			
			diffEntry.Active = 0;
			if (m_cache.Difficulty > 1 )
				diffEntry.Active = 1;
			if (m_cache.Difficulty > 1.5)
				diffEntry.Active = 2;
			if (m_cache.Difficulty > 2)
				diffEntry.Active = 3;
			if (m_cache.Difficulty > 2.5)
				diffEntry.Active = 4;
			if (m_cache.Difficulty > 3)
				diffEntry.Active = 5;
			if (m_cache.Difficulty > 3.5)
				diffEntry.Active = 6;
			if (m_cache.Difficulty > 4)
				diffEntry.Active = 7;
			if (m_cache.Difficulty > 4.5)
				diffEntry.Active = 8;
			
			terrEntry.Active = 0;
			if (m_cache.Terrain > 1 )
				terrEntry.Active = 1;
			if (m_cache.Terrain > 1.5)
				terrEntry.Active = 2;
			if (m_cache.Terrain > 2)
				terrEntry.Active = 3;
			if (m_cache.Terrain > 2.5)
				terrEntry.Active = 4;
			if (m_cache.Terrain > 3)
				terrEntry.Active = 5;
			if (m_cache.Terrain > 3.5)
				terrEntry.Active = 6;
			if (m_cache.Terrain > 4)
				terrEntry.Active = 7;
			if (m_cache.Terrain > 4.5)
				terrEntry.Active = 8;
			
			
			switch (m_cache.TypeOfCache)
			{
				case Geocache.CacheType.APE:
					typeEntry.Active = 7;
					break;
				case Geocache.CacheType.CITO:
					typeEntry.Active = 13;
					break;
				case Geocache.CacheType.EARTH:
					typeEntry.Active = 4;
					break;
				case Geocache.CacheType.EVENT:
					typeEntry.Active = 5;
					break;
				case Geocache.CacheType.LETTERBOX:
					typeEntry.Active = 6;
					break;
				case Geocache.CacheType.MAZE:
					typeEntry.Active = 8;
					break;
				case Geocache.CacheType.MEGAEVENT:
					typeEntry.Active = 14;
					break;
				case Geocache.CacheType.MULTI:
					typeEntry.Active = 3;
					break;
				case Geocache.CacheType.MYSTERY:
					typeEntry.Active = 2;
					break;
				case Geocache.CacheType.REVERSE:
					typeEntry.Active = 9;
					break;
				case Geocache.CacheType.TRADITIONAL:
					typeEntry.Active = 1;
					break;
				case Geocache.CacheType.VIRTUAL:
					typeEntry.Active = 10;
					break;
				case Geocache.CacheType.WEBCAM:
					typeEntry.Active = 11;
					break;
				case Geocache.CacheType.WHERIGO:
					typeEntry.Active = 12;
					break;
				default:
					typeEntry.Active = 0;
					break;
			}
		}
		
		public void UpdateCache()
		{
			m_cache.Name = codeEntry.Text;
			m_cache.Desc = descriptionEntry.Buffer.Text;
			m_cache.Symbol = "Geocache";
			if (m_cache.TypeOfCache != Geocache.CacheType.GENERIC)
				m_cache.Type = "Geocache|" + Geocache.GetCTypeString(m_cache.TypeOfCache);
			
			try
			{
				m_cache.URL = new Uri(urlEntry.Text);
			}
			catch
			{
				m_cache.URL = null;
			}
			m_cache.URLName = urlNameEntry.Text;
			m_cache.Lat = coordEntry.Latitude;
			m_cache.Lon = coordEntry.Longitude;
			m_cache.CacheName = nameEntry.Text;
			m_cache.ShortDesc = shortDescEntry.Buffer.Text;
			m_cache.LongDesc = longDescEntry.Buffer.Text;
			m_cache.Hint = hintEntry.Buffer.Text;
			m_cache.PlacedBy = placedByEntry.Text;
			m_cache.CacheOwner = ownerEntry.Text;
			m_cache.OwnerID = ownerIDEntry.Text;
			m_cache.CacheID = cacheIDEntry.Text;
			m_cache.User1 = udata1Entry.Text;
			m_cache.User2 = uData2Entry.Text;
			m_cache.User3 = uData3Entry.Text;
			m_cache.User4 = uData4Entry.Text;
			
			m_cache.Difficulty = 1f;
			if (diffEntry.Active == 1 )
				m_cache.Difficulty = 1.5f;
			else if (diffEntry.Active == 2 )
				m_cache.Difficulty = 2f;
			else if (diffEntry.Active == 3 )
				m_cache.Difficulty = 2.5f;
			else if (diffEntry.Active == 4 )
				m_cache.Difficulty = 3f;
			else if (diffEntry.Active == 5 )
				m_cache.Difficulty = 3.5f;
			else if (diffEntry.Active == 6 )
				m_cache.Difficulty = 4f;
			else if (diffEntry.Active == 7 )
				m_cache.Difficulty = 4.5f;
			else if (diffEntry.Active == 8 )
				m_cache.Difficulty = 5f;
			
			m_cache.Terrain = 1f;
			if (terrEntry.Active == 1 )
				m_cache.Terrain = 1.5f;
			else if (terrEntry.Active == 2 )
				m_cache.Terrain = 2f;
			else if (terrEntry.Active == 3 )
				m_cache.Terrain = 2.5f;
			else if (terrEntry.Active == 4 )
				m_cache.Terrain = 3f;
			else if (terrEntry.Active == 5 )
				m_cache.Terrain = 3.5f;
			else if (terrEntry.Active == 6 )
				m_cache.Terrain = 4f;
			else if (terrEntry.Active == 7 )
				m_cache.Terrain = 4.5f;
			else if (terrEntry.Active == 8 )
				m_cache.Terrain = 5f;
			
			
			switch (typeEntry.Active)
			{
				case 7:
					m_cache.TypeOfCache = Geocache.CacheType.APE;
					break;
				case 13:
					m_cache.TypeOfCache = Geocache.CacheType.CITO;
					break;
				case 4:
					m_cache.TypeOfCache = Geocache.CacheType.EARTH;
					break;
				case 5:
					m_cache.TypeOfCache = Geocache.CacheType.EVENT;
					break;
				case 6:
					m_cache.TypeOfCache = Geocache.CacheType.LETTERBOX;
					break;
				case 8:
					m_cache.TypeOfCache = Geocache.CacheType.MAZE;
					break;
				case 14:
					m_cache.TypeOfCache = Geocache.CacheType.MEGAEVENT;
					break;
				case 3:
					m_cache.TypeOfCache = Geocache.CacheType.MULTI;
					break;
				case 2:
					m_cache.TypeOfCache = Geocache.CacheType.MYSTERY;
					break;
				case 9:
					m_cache.TypeOfCache = Geocache.CacheType.REVERSE;
					break;
				case 1:
					m_cache.TypeOfCache = Geocache.CacheType.TRADITIONAL;
					break;
				case 10:
					m_cache.TypeOfCache = Geocache.CacheType.VIRTUAL;
					break;
				case 11:
					m_cache.TypeOfCache = Geocache.CacheType.WEBCAM;
					break;
				case 12:
					m_cache.TypeOfCache = Geocache.CacheType.WHERIGO;
					break;
				default:
					m_cache.TypeOfCache = Geocache.CacheType.GENERIC;
					break;
			}
		}
		protected virtual void OnCorrClick (object sender, System.EventArgs e)
		{
			m_App.CorrectCoordinates();
		}
	}
}
