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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ocmengine
{

	[Serializable]
	public class FilterList
	{
		Hashtable m_criteria;

		public const String KEY_CACHETYPE = "cachetype";
		public const String KEY_CONTAINER = "container";
		public const String KEY_DESCRIPTION = "description";
		public const String KEY_TERRAIN_VAL = "terrain_val";
		public const String KEY_TERRAIN_OP = "terrain_op";
		public const String KEY_DIFF_VAL = "diff_val";
		public const String KEY_DIFF_OP = "diff_op";
		public const String KEY_PLACEDBY = "placedby";
		public const String KEY_MINE = "mine";
		public const String KEY_STATUS = "status";
		public const String KEY_PLACEAFTER = "placeafter";
		public const String KEY_PLACEBEFORE = "placebefore";
		public const String KEY_INFOAFTER = "infoafter";
		public const String KEY_INFOBEFORE = "infobefore";
		public const String KEY_INFO_DAYS = "infodays";
		public const String KEY_INFO_NDAYS = "infondays";
		public const String KEY_COUNTRY = "country";
		public const String KEY_STATE = "state";
		public const String KEY_FOUNDON = "foundon";
		public const String KEY_FOUNDBEFORE = "foundbefore";
		public const String KEY_FOUNDAFTER = "foundafter";
		public const String KEY_OWNERID = "ownerID";
		public const String KEY_CHILDREN = "children";
		public const String KEY_NOCHILDREN = "nochildren";
		public const String KEY_NOTES = "notes";
		public const String KEY_CORRECTED = "corrected";
		public const String KEY_NOCORRECTED = "nocorreced";
		public const String KEY_INCATTRS = "incattrs";
		public const String KEY_EXCATTRS = "excattrs";
		public const String KEY_INCNOATTRS = "incnoattrs";
		public const String KEY_EXCNOATTRS = "excnoattrs";
		public const String KEY_DIST_OP = "distop";
		public const String KEY_DIST = "dist";
		public const String KEY_DIST_LAT = "distlat";
		public const String KEY_DIST_LON = "distlon";
		public const String KEY_FTF = "ftf";
		public const String KEY_DNF = "dnf";
		public const String KEY_U1 = "u1";
		public const String KEY_U2 = "u2";
		public const String KEY_U3 = "u3";
		public const String KEY_U4 = "u4";
		public const String KEY_LFOUND_BEFORE = "lastFoundBefore";
		public const String KEY_LFOUND_AFTER = "lastFoundAfter";
		public const String KEY_CACHE_SOURCE = "cacheSource";
		public const String KEY_CACHE_NAME = "cacheName";
		public FilterList ()
		{
			m_criteria = new Hashtable ();
		}

		public void AddFilterCriteria (String key, Object val)
		{
			if (m_criteria.Contains(key))
				m_criteria[key] = val;
			else
				m_criteria.Add (key, val);
		}
		
		public void RemoveCriteria(String key)
		{
			m_criteria.Remove(key);
		}
		
		public object GetCriteria(String key)
		{
			return m_criteria[key];
		}
		
		public Hashtable GetFilterTable()
		{
			return m_criteria;
		}
		
		public bool Contains(String key)
		{
			return m_criteria.Contains(key);
		}
		
		public void Clear()
		{
			m_criteria.Clear();
		}
		
		public int GetCount()
		{
			return m_criteria.Count;
		}
	}
}
