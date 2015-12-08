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
using System.Collections.Generic;
using ocmengine;

namespace ocmgtk
{


	public class CleanupManager
	{

		List<String> logsToDelete;
		List<Geocache> cachesToCleanup;
		public bool compactDB = true;
		public bool backupDB = true;
		public bool purgeLogs = true;
		public int logLimit = 5;
		public string backupName = null;
		ACacheStore store;
		
		
		public int DeleteCount
		{
			get { return logsToDelete.Count;}
		}
		
		public CleanupManager (ACacheStore astore, List<Geocache> caches)
		{
			logsToDelete = new List<string>();
			cachesToCleanup = caches;
			store = astore;
		}
		
		public void Cleanup()
		{
			if (backupDB)
			{
				String storeName = store.StoreName;
				String directory = System.IO.Path.GetDirectoryName(storeName);
				backupName = directory + "/" + DateTime.Now.ToFileTime().ToString() + Utilities.GetShortFileName(storeName) + ".bak";
			}
			System.IO.File.Copy(store.StoreName, backupName);
			if (purgeLogs)
				store.PurgeLogsByKey(logsToDelete.ToArray());
			if (compactDB)
				store.CompactStore();
		}
			
		
		public void BuildLogsToDelete()
		{
			string[] caches = new string[cachesToCleanup.Count];
			int iCount = 0;
			foreach(Geocache cache in cachesToCleanup)
			{
				caches[iCount] = cache.Name;
				iCount++;
			}
			
			Dictionary<string, List<CacheLog>> logs = store.GetCacheLogsMulti(caches);
			logsToDelete.Clear();
			foreach(string key in logs.Keys)
			{
				List<CacheLog> logl = logs[key];
				for (int i=0; i < logl.Count; i++)
				{
					if (i >= logLimit)
						logsToDelete.Add(logl[i].LogKey);
				}
			}
		}
		
	}
}
