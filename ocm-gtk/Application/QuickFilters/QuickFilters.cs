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
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using Mono.Unix;
using Gtk;

namespace ocmgtk
{

	[Serializable]
	public class QuickFilters
	{
		private Dictionary<string, QuickFilter> m_filters = new Dictionary<string, QuickFilter>();
		
		[NonSerializedAttribute]
		OCMMainWindow m_Win;
		
		public OCMMainWindow Window
		{
			set {m_Win = value;}
		}
		
		public QuickFilter[] FilterArray
		{
			get{
				QuickFilter[] filters = new QuickFilter[m_filters.Count];
				m_filters.Values.CopyTo(filters, 0);
				return filters;
			}
			set{
				foreach(QuickFilter filter in value)
				{
					m_filters.Add(filter.Name, filter);
				}				
			}
		}
			
		public QuickFilters ()
		{
		}
		
		public QuickFilter GetFilter(String name)
		{
			if (m_filters.ContainsKey(name))
				return m_filters[name];
			return null;
		}
		
		public void AddFilter(QuickFilter filter)
		{
			if (m_filters.ContainsKey(filter.Name))
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
				                                      String.Format(Catalog.GetString("Are you sure you wish to " +
				                                      	"overwrite \"{0}\"?"), filter.Name));
				if ((int) ResponseType.Yes != dlg.Run())
				{
					dlg.Hide();
					return;
				}
				else
				{
					dlg.Hide();
					m_filters.Remove(filter.Name);
				}
			}
			m_filters.Add(filter.Name, filter);
			UpdateQFFile();
		}
		
		public void DeleteFilter(string name)
		{
			m_filters.Remove(name);
			UpdateQFFile();
		}
		
		public Menu BuildQuickFilterMenu()
		{
			Menu qfMenu = new Menu();
			int iCount = 0;
			
			foreach (QuickFilter filter in m_filters.Values)
			{
				BuildMenuItem (filter, qfMenu);
				iCount ++;
				
			}
			BuildMenuItem(QuickFilter.STALE_FILTER, qfMenu);
			BuildMenuItem(QuickFilter.STALEUNSOLVED_FILTER, qfMenu);
			BuildMenuItem(QuickFilter.DNF_FILTER, qfMenu);
			BuildMenuItem(QuickFilter.NEW_FILTER, qfMenu);			
			return qfMenu;
		}
		
		void BuildMenuItem (QuickFilter filter, Menu qfMenu)
		{
			Gtk.Action action = new Gtk.Action(filter.Name, filter.Name);
			qfMenu.Append(action.CreateMenuItem());
			action.Activated += HandleActionActivated;
		}

		void HandleActionActivated (object sender, EventArgs e)
		{
			String name = ((sender) as Gtk.Action).Name;
			if (name == QuickFilter.STALE_FILTER.Name)
				m_Win.CacheList.ApplyQuickFilter(QuickFilter.STALE_FILTER);
			else if (name == QuickFilter.DNF_FILTER.Name)
				m_Win.CacheList.ApplyQuickFilter(QuickFilter.DNF_FILTER);
			else if (name == QuickFilter.STALEUNSOLVED_FILTER.Name)
				m_Win.CacheList.ApplyQuickFilter(QuickFilter.STALEUNSOLVED_FILTER);
			else if (name == QuickFilter.NEW_FILTER.Name)
				m_Win.CacheList.ApplyQuickFilter(QuickFilter.NEW_FILTER);
			else
				m_Win.CacheList.ApplyQuickFilter(m_filters[((sender) as Gtk.Action).Name]);
		}
		
		public static QuickFilters LoadQuickFilters()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!File.Exists(path + "/ocm/quickfilters.oqf"))
			{
				QuickFilters nfilters = new QuickFilters();
				nfilters.AddFilter(QuickFilter.ALL_FILTER);
				nfilters.AddFilter(QuickFilter.TODO_FILTER);
				nfilters.AddFilter(QuickFilter.DONE_FILTER);
				nfilters.AddFilter(QuickFilter.MINE_FILTER);
				nfilters.AddFilter(QuickFilter.DNF_FILTER);
				return nfilters;
			}
			FileStream fs = new FileStream(path + "/ocm/quickfilters.oqf", FileMode.Open, FileAccess.Read);	
			BinaryFormatter ser = new BinaryFormatter();
			System.Object filters = ser.Deserialize(fs);
			fs.Close();
			QuickFilters qfs = filters as QuickFilters;
			// Check and upgrade Quick filters if needed.
			return filters as QuickFilters;
		}
		
		private void UpdateQFFile()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!Directory.Exists("ocm"))
				Directory.CreateDirectory(path + "/ocm");
			path = path + "/ocm";
			BinaryFormatter ser = new BinaryFormatter();
			FileStream fs = new FileStream(path + "/quickfilters.oqf", FileMode.Create, FileAccess.ReadWrite);
			ser.Serialize(fs, this);
			fs.Close();
		}
	}
}
