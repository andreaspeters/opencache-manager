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
using System.Data;
using System.IO;
using System.Text;
namespace ocmengine
{


	public abstract class ACacheStore
	{
		public event ReadCacheEventHandler ReadCache;
		public event ReadCompleteEventHandler Complete;
		public delegate void ReadCacheEventHandler(object sender, ReadCacheArgs args);
		public delegate void ReadCompleteEventHandler(object sender, EventArgs args);
		public abstract string StoreName {get;}
		public abstract string ConnectionString{get;}
		public abstract bool NeedsUpgrade{get;}
		public abstract IDbConnection Connection{get;}
		public abstract int CacheCount{get;}
		private StreamWriter m_TraceWriter = null;
		
		protected FilterList m_Global = new FilterList();
		public FilterList GlobalFilters
		{
			get { return m_Global;}
		}
		
		protected FilterList m_Advanced = null;
		public FilterList AdvancedFilters
		{
			get { return m_Advanced;}
			set { 
				m_Advanced = value;
			}
		}
		
		protected List<FilterList> m_ComboFilter = null;
		public List<FilterList> CombinationFilter
		{
			get { return m_ComboFilter;}
			set 
			{ 
				m_ComboFilter = value;
			}
		}
		
		protected string m_bmrkList = null;
		public string ActiveBookmarkList
		{
			get { return m_bmrkList;}
			set { m_bmrkList = value;}
		}
		
		
		public ACacheStore (string connectionString)
		{
		}
		
		public ACacheStore (string connectionString, string tracelog)
		{
			TracingOn (tracelog);
		}
		
		public abstract void Dispose();

		public abstract void UpgradeStore();
		
		public abstract void CompactStore();
		
		/// <summary>
		/// Gets the unfiltered caches. This method is asynchronous, as caches are read you must
		/// handle the ReadCache event when a cache is read, and the Complete event when the run is finished.
		/// </summary>
		/// <param name="lat">
		/// Starting latitude <see cref="System.Double"/>
		/// </param>
		/// <param name="lon">
		/// Starting longitude <see cref="System.Double"/>
		/// </param>
		/// <param name="ownerIDs">
		/// A list of owner IDs so that OCM can filter out caches by owner ID<see cref="List<System.String>"/>
		/// </param>
		public abstract void GetUnfilteredCaches(double lat, double lon, string[] ownerIDs);
		
		/// <summary>
		/// Gets caches that match the cache codes provided. Filters are ignored for this call.
		/// </summary>
		/// <param name="cachenames">
		/// A list of cache codrs<see cref="System.String[]"/>
		/// </param>
		/// <returns>
		/// A list of caches<see cref="List<System.String>"/>
		/// </returns>
		public abstract List<Geocache> GetCachesByName(string[] cachecode);
		
		public abstract List<Geocache> GetFinds();
		
		/// <summary>
		/// Gets a set of waypoints by waypoint name. Filters are ignored for this call.
		/// </summary>
		/// <param name="waypointcodes">
		/// A list of waypoint codes <see cref="System.String[]"/>
		/// </param>
		/// <returns>
		/// An array of waypoints <see cref="Waypoint[]"/>
		/// </returns>
		public abstract List<Waypoint> GetWaypointsByName(string[] waypointcodes);
		
		
		/// <summary>
		/// Gets the child waypoints of a cache
		/// </summary>
		/// <param name="cachecode">
		/// The cache code of the parent cache <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A list of child waypoints<see cref="List<Waypoint>"/>
		/// </returns>
		public abstract List<Waypoint> GetChildWaypoints(string[] parentCaches);
		
		public abstract CacheAttribute[] GetAttributes(string cachecode);
		public abstract Dictionary<string, List<CacheAttribute>> GetAttributesMulti(string[] cachecodes);
		
		
		/// <summary>
		/// Gets the cache logs for a cache
		/// </summary>
		/// <param name="cachecode">
		/// The cache code of the partent cache <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A list of cache logs <see cref="List<CacheLog>"/>
		/// </returns>
		public abstract List<CacheLog> GetCacheLogs(string cachecode);
		public abstract CacheLog GetCacheLogByKey(string key);
		public abstract Dictionary<string, List<CacheLog>> GetCacheLogsMulti(string[] cachecodes);
		
		public abstract TravelBug[] GetTravelBugs(string cachecode);
		public abstract Dictionary<string, List<TravelBug>> GetTravelBugMulti(string[] cachecodes);
		
		public abstract DateTime GetLastFound(string cachecode);
		public abstract DateTime GetLastFoundBy(string cachecode, string finderId);
		public abstract DateTime GetLastDNFBy(string cachecode, string finderId);
		
		public abstract CacheLog GetLastLog(string cachecode);
		public abstract CacheLog GetLastLogBy(string cachecode, string finderId);
		public abstract CacheLog GetLastFindLogBy(string cachecode, string finderId);
		
		/// <summary>
		/// Starts an update transaction with the database. Without calling this method, the adds
		/// will be atomic and will not perform if you are adding multiple objects
		/// </summary>
		public abstract void StartUpdate();
		
		/// <summary>
		/// Cancels an update and rolls back the changes
		/// </summary>
		public abstract void CancelUpdate();
		
		/// <summary>
		/// Completes an update and commits all changes
		/// </summary>
		public abstract void CompleteUpdate();
		
		
		public abstract void AddTravelBug(string parent, TravelBug bug);
		public abstract void AddLog(string parent, CacheLog log);
		public abstract void AddAttribute(string parent, CacheAttribute attribute);
		public abstract void PurgeAllLogs(string[] cachecodes);
		public abstract void PurgeAllAttributes(string[] cachecodes);
		public abstract void PurgeAllTravelBugs(string[] cachecodes);
		public abstract void PurgeLogsByKey(string[] logkeys);
		
		public abstract void AddBookmarkList(string listName);
		public abstract void AddBoormarkEntry(string listName, string cacheCode);
		public abstract void RemoveBookmarkEntry(string listName, string cacheCode);
		public abstract void RemoveBookmarkList(string listName);
		public abstract string[] GetBookmarkLists();
				
		/// <summary>
		/// Adds a waypoint to the store. If a waypoint already exists with this name, the entry is replaced
		/// </summary>
		/// <param name="point">
		/// The waypoint to add <see cref="Waypoint"/>
		/// </param>
		public abstract void AddWaypointOrCache(Waypoint point, bool lockSymbol, bool partialUpdate);
		
		/// <summary>
		/// Deletes a waypoint from the store
		/// </summary>
		/// <param name="name">
		/// The name of the waypoint to delete<see cref="System.String"/>
		/// </param>
		public abstract void DeleteWaypoint(string name);	
		
		public abstract string GetUniqueName(string name);
		
		protected void FireReadEvent(Geocache cache)
		{
			if (this.ReadCache != null)
				this.ReadCache(this, new ReadCacheArgs(cache));
		}
		
		protected void FireCompleteEvent()
		{
			if (this.Complete != null)
				this.Complete(this, new EventArgs());
		}
			
		public void TracingOn (string logFile)
		{
			if (m_TraceWriter != null)
			{
				m_TraceWriter.Flush();
				m_TraceWriter.Close();	
			}
			m_TraceWriter = new StreamWriter(new FileStream(logFile, FileMode.Append));
		}
		
		public void TracingOff ()
		{
			if (m_TraceWriter == null)
				return;
			m_TraceWriter.Flush();
			m_TraceWriter.Close();			
		}
		
		protected void Trace(string msg)
		{
			if (null == m_TraceWriter)
				return;
			m_TraceWriter.WriteLine(System.DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss.fff: ") + msg);
			m_TraceWriter.Flush();
		}

		
		public static string Escape(string val)
		{
			return val.Replace("'", "''");
		}
		
		public static string ArrayToSQL(String[] val_array)
		{
			bool atLeastOne = false;
			StringBuilder builder = new StringBuilder();
			foreach(string val in val_array)
			{
				if (atLeastOne)
					builder.Append(",");
				else
					atLeastOne = true;
				builder.Append("'");
				builder.Append(val);
				builder.Append("'");
			}
			return builder.ToString();
		}
	}
}
