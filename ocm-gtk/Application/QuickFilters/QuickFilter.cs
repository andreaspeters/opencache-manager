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
using System.Collections.Generic;
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{

	[Serializable]
	public class QuickFilter
	{
		private string m_name;
		public String Name
		{
			get { return m_name;}
			set { m_name = value;}
		}
		
		
		FilterList m_advancedFilters = null;
		public FilterList AdvancedFilters
		{
			get { return m_advancedFilters;}
			set { m_advancedFilters = value; }
		}
		
		bool m_found;
		public bool Found
		{
			get { return m_found;}
			set { m_found = value;}
		}
		
		bool m_notFound;
		public bool NotFound
		{
			get { return m_notFound;}
			set { m_notFound = value;}
		}
		
		bool m_mine;
		public bool Mine
		{
			get { return m_mine;}
			set { m_mine = value;}
		}
		
		bool m_available;
		public bool Available
		{
			get { return m_available;}
			set { m_available = value;}
		}
		
		bool m_unavailable;
		public bool Unavailable
		{
			get { return m_unavailable;}
			set { m_unavailable = value;}
		}
		
		bool m_archived;
		public bool Archived
		{
			get { return m_archived;}
			set { m_archived = value;}
		}
		
		string m_nameFilter;
		public string NameFilter
		{
			get { return m_nameFilter;}
			set { m_nameFilter = value;}
		}
		
		int m_distFilter;
		public int Distance
		{
			get { return m_distFilter;}
			set { m_distFilter = value;}
		}
		
		List<FilterList> m_ComboFilter = null;
		public List<FilterList> ComboFilter
		{
			get { return m_ComboFilter;}
			set { m_ComboFilter = value;}
		}
		
		
		public enum PREDEF_FILTER {DONE, TODO, MINE, ALL, DNF, CUSTOM, STALE, STALEUNSOLVED, NEW};
		
		public static QuickFilter TODO_FILTER = new QuickFilter(PREDEF_FILTER.TODO);
		public static QuickFilter DONE_FILTER = new QuickFilter(PREDEF_FILTER.DONE);
		public static QuickFilter MINE_FILTER = new QuickFilter(PREDEF_FILTER.MINE);
		public static QuickFilter DNF_FILTER = new QuickFilter(PREDEF_FILTER.DNF);
		public static QuickFilter ALL_FILTER = new QuickFilter(PREDEF_FILTER.ALL);
		public static QuickFilter STALE_FILTER = new QuickFilter(PREDEF_FILTER.STALE);
		public static QuickFilter STALEUNSOLVED_FILTER = new QuickFilter(PREDEF_FILTER.STALEUNSOLVED);
		public static QuickFilter NEW_FILTER = new QuickFilter(PREDEF_FILTER.NEW);
		
		public QuickFilter(): this(PREDEF_FILTER.ALL)
		{
			
		}
		
		public QuickFilter (PREDEF_FILTER mode)
		{
			switch (mode)
			{
				case PREDEF_FILTER.DONE:
					m_advancedFilters = null;
					m_available = true;
					m_unavailable = true;
					m_found = true;
					m_notFound = false;
					m_archived = true;
					m_mine = false;
					m_distFilter = -1;
					m_ComboFilter = null;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("Done");
					break;
				case PREDEF_FILTER.TODO:
					m_advancedFilters = null;
					m_available = true;
					m_unavailable = false;
					m_found = false;
					m_notFound = true;
					m_archived = false;
					m_mine = false;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("To Do");
					break;
				case PREDEF_FILTER.MINE:
					m_advancedFilters = null;
					m_available = false;
					m_unavailable = false;
					m_found = false;
					m_notFound = false;
					m_archived = false;
					m_mine = true;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("Mine");
					break;
				case PREDEF_FILTER.DNF:
					m_advancedFilters = new FilterList();
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_DNF, true);
					m_available = true;
					m_unavailable = false;
					m_found = false;
					m_notFound = true;
					m_archived = false;
					m_mine = false;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("DNF");
					break;
				case PREDEF_FILTER.ALL:
					m_advancedFilters = null;
					m_available = true;
					m_unavailable = true;
					m_found = true;
					m_notFound = true;
					m_archived = true;
					m_mine = true;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("All");
					break;
				case PREDEF_FILTER.STALE:
					m_advancedFilters = new FilterList();
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_INFO_NDAYS, new Config().StaleCacheInterval);
					m_available = true;
					m_unavailable = true;
					m_found = true;
					m_notFound = true;
					m_archived = true;
					m_mine = true;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("Stale");
					break;
				case PREDEF_FILTER.STALEUNSOLVED:
					m_advancedFilters = new FilterList();
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_INFO_NDAYS, new Config().StaleCacheInterval);
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_NOCORRECTED, "true");
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_NOCHILDREN, "Final Location");
					m_available = true;
					m_unavailable = true;
					m_found = true;
					m_notFound = true;
					m_archived = true;
					m_mine = true;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("Stale - Unsolved");
					break;
				case PREDEF_FILTER.NEW:
					m_advancedFilters = new FilterList();
					m_advancedFilters.AddFilterCriteria(FilterList.KEY_PLACEAFTER, DateTime.Today.Subtract(TimeSpan.FromDays(new Config().NewCacheInterval)));
					m_available = true;
					m_unavailable = true;
					m_found = true;
					m_notFound = true;
					m_archived = true;
					m_mine = true;
					m_ComboFilter = null;
					m_distFilter = -1;
					m_nameFilter = String.Empty;
					m_name = Catalog.GetString("Recently Published");
					break;
				default:
					break;
			}
		}
				
		
	}
}
