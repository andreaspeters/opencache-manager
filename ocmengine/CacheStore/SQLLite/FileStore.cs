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
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ocmengine.SQLLite
{


	public partial class FileStore : ACacheStore
	{
		#region Members and Properties
		private IDbConnection m_Connection;
		private IDbTransaction m_Transaction;

		public override int CacheCount {
			get {
				Trace ("CacheCount: No Connection");
				if (m_Connection == null)
					return -1;
				Trace ("Start CacheCount");
				IDbCommand command = m_Connection.CreateCommand ();
				command.CommandText = COUNT_GC;
				if (m_bmrkList != null)
					command.CommandText += String.Format (BMRK_FILTER_COUNT, m_bmrkList);
				Trace (command.CommandText);
				object val = command.ExecuteScalar ();
				int count = int.Parse (val.ToString ());
				Trace ("End CacheCount");
				return count;
			}
		}

		public override IDbConnection Connection {
			get { return m_Connection; }
		}

		public override string ConnectionString {
			get { return m_Connection.ConnectionString; }
		}

		public override bool NeedsUpgrade {
			get {
				int ver = 0;
				ver = GetDBVersion ();
				if (ver < 5)
					return true;
				return false;
			}
		}

		private string m_StoreName;
		public override string StoreName {
			get {
				return m_StoreName;
			}
		}
#endregion
		
		public FileStore (string connectionString) : base(connectionString)
		{
			m_StoreName = connectionString;
			InitializeStore (connectionString);
		}
		
		public FileStore (string connectionString, string tracelog) : base(connectionString, tracelog)
		{
			m_StoreName = connectionString;
			InitializeStore (connectionString);
		}
		
		void InitializeStore (string connectionString)
		{
			OpenStore (connectionString);
			IDbCommand cmd = m_Connection.CreateCommand ();
			cmd.CommandText = "SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name='GEOCACHE'";
			object val = cmd.ExecuteScalar ();
			int count = int.Parse (val.ToString ());
			if (count != 1)
				CreateStore ();
		}
		

		private void OpenStore (string connectionString)
		{
			Trace("Opening Store...");
			Trace(connectionString);
			if (m_Connection != null)
				m_Connection.Close ();
			m_Connection = (IDbConnection)new SqliteConnection ("Data Source=" +connectionString);
			m_Connection.Open ();
		}

		private void CreateStore ()
		{
			Trace ("Creating Store...");
			ExecuteSQLCommand (CREATE_CACHE_TABLE);
			ExecuteSQLCommand (CREATE_LOGS_TABLE);
			ExecuteSQLCommand (CREATE_TABLE_TBUGS);
			ExecuteSQLCommand (CREATE_ATTRS_TABLE);
			ExecuteSQLCommand (CREATE_TABLE_WPTS);
			ExecuteSQLCommand (CREATE_TABLE_BMRK);
			ExecuteSQLCommand (CREATE_TABLE_BMRK_CACHES);
			ExecuteSQLCommand (CREATE_DB_VER);
			ExecuteSQLCommand (SET_DB_VER);
			Trace ("Store Created");
		}

		public override void Dispose ()
		{
			if (m_Connection == null)
				throw new Exception ("CacheStore Already Closed");
			TracingOff ();
			m_Connection.Close ();
			m_Connection = null;
		}

		private int GetDBVersion ()
		{
			if (m_Connection == null)
				throw new Exception ("CacheStore Not Open");
			int ver = 0;
			IDbCommand command = m_Connection.CreateCommand ();
			command.CommandText = GET_DB_VER;
			IDataReader rdr = command.ExecuteReader ();
			
			while (rdr.Read ()) {
				ver = rdr.GetInt32 (0);
			}
			return ver;
		}

		public override void UpgradeStore ()
		{
			Trace ("Upgrading Store");
			IDbTransaction trans = m_Connection.BeginTransaction ();
			IDbCommand cmd = m_Connection.CreateCommand ();
			int ver = GetDBVersion ();
			if (ver == 0) {
				cmd.CommandText = UPGRADE_GEOCACHE_V0_V1;
				cmd.ExecuteNonQuery ();
				cmd.CommandText = CREATE_TABLE_BMRK;
				cmd.ExecuteNonQuery ();
				cmd.CommandText = CREATE_TABLE_BMRK_CACHES;
				cmd.ExecuteNonQuery ();
				cmd.CommandText = CREATE_DB_VER;
				cmd.ExecuteNonQuery ();
			}
			cmd.CommandText = CLEAR_DB_VER;
			cmd.ExecuteNonQuery ();
			if (ver <= 1) {
				cmd.CommandText = UPGRADE_GEOCACHE_V1_V2;
				cmd.ExecuteNonQuery ();
			}
			if (ver <= 2) {
				cmd.CommandText = UPGRADE_GEOCACHE_V2_V3;
				cmd.ExecuteNonQuery ();
				cmd.CommandText = CREATE_ATTRS_TABLE;
				cmd.ExecuteNonQuery ();
			}
			if (ver <= 3) {
				cmd.CommandText = UPGRADE_GEOCACHE_V3_V4A;
				cmd.ExecuteNonQuery ();
				cmd.CommandText = UPGRADE_GEOCACHE_V3_V4B;
				cmd.ExecuteNonQuery ();
			}
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5A;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5B;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5C;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5D;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5E;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5F;
			cmd.ExecuteNonQuery ();
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5G;
			cmd.ExecuteNonQuery ();
			trans.Commit ();
			trans = m_Connection.BeginTransaction ();
			cmd.CommandText = CREATE_LOGS_TABLE;
			cmd.ExecuteNonQuery ();
			// Drop oldlogs table
			cmd.CommandText = UPGRADE_GEOCACHE_V4_V5I;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = SET_DB_VER;
			cmd.ExecuteNonQuery ();
			cmd.Dispose ();
			trans.Commit ();
			Trace ("End Upgrade");
		}

		public override void CompactStore ()
		{
			Trace ("Start Compacting DB");
			IDbCommand command = m_Connection.CreateCommand ();
			command.CommandText = VACUUM;
			command.ExecuteNonQuery ();
			Trace ("End Compacting DB");
		}

		public override string GetUniqueName (string testname)
		{
			Trace("Generating unique name for " + testname);
			
			object val = ExecuteSQLSingleValue(String.Format(WPT_EXISTS_CHECK, testname));
			int count = int.Parse(val.ToString());
			if (count > 0)
			{
				string oldNumStr = testname.Substring(testname.Length -2);
				try
				{
					int num = int.Parse(oldNumStr);
					num++;
					testname = testname.Substring(0, testname.Length -2) + num.ToString("00");
				}
				catch
				{
					testname += "01";
				}
				return GetUniqueName(testname);
			}
			Trace("Name Generated: " + testname);
			return testname;
		}
		
		private Object ExecuteSQLSingleValue(string sql)
		{
			Trace("Executing SQL:");
			Trace(sql);
			IDbCommand cmd = m_Connection.CreateCommand();
			cmd.CommandText = sql;
			return cmd.ExecuteScalar();
		}

		public override void GetUnfilteredCaches (double lat, double lon, string[] ownerIDs)
		{
			Trace("Getting unfilitered caches");
			if (CombinationFilter != null)
			{
				List<string> dupCheck = new List<string>();
				foreach(FilterList filter in CombinationFilter)
				{
					this.AdvancedFilters = filter;
					if (this.AdvancedFilters.Contains(FilterList.KEY_DIST_LAT))
						lat = (double) this.AdvancedFilters.GetCriteria(FilterList.KEY_DIST_LAT);
					if (this.AdvancedFilters.Contains(FilterList.KEY_DIST_LON))
						lon = (double) this.AdvancedFilters.GetCriteria(FilterList.KEY_DIST_LON);
					GetCaches(ownerIDs, lat, lon, dupCheck);
				}
				this.AdvancedFilters = null;
			}
			else
			{
				if (this.AdvancedFilters != null)
				{
					if (this.AdvancedFilters.Contains(FilterList.KEY_DIST_LAT))
						lat = (double) this.AdvancedFilters.GetCriteria(FilterList.KEY_DIST_LAT);
					if (this.AdvancedFilters.Contains(FilterList.KEY_DIST_LON))
						lon = (double) this.AdvancedFilters.GetCriteria(FilterList.KEY_DIST_LON);
				}
				GetCaches (ownerIDs, lat, lon, null);
			}
			Trace("Completing");
			FireCompleteEvent();
		}
		
		private void GetCaches (string[] ownerIDs, double lat, double lon, List<string> dupCheck)
		{
			List<string> hasChildrenList = BuildHasChildrenList();
			List<string> hasFinalList = BuildHasFinalList();
			StringBuilder sql = new StringBuilder(GET_GC);
			string prefilter = DoPrefilter(ownerIDs);
			if (prefilter != null)
				sql.Append(prefilter);
			if (GlobalFilters.GetCount() > 0)
				sql.Append(BuildWhereClause(GlobalFilters.GetFilterTable(), ownerIDs));
			if (AdvancedFilters != null)
				sql.Append(BuildWhereClause(AdvancedFilters.GetFilterTable(), ownerIDs));
			if (m_bmrkList != null)
				sql.Append(String.Format(BMRK_FILTER, m_bmrkList));
		
			IDataReader rdr = ExecuteSQLQuery(sql.ToString());
			while (rdr.Read())
			{
				Geocache cache = BuildCache(rdr, lat, lon, hasChildrenList, hasFinalList);
				if (PostFilter(cache, lat, lon))
				{
					if (dupCheck != null)
					{
						if (dupCheck.Contains(cache.Name))
							continue;
						else
							dupCheck.Add(cache.Name);
					}
					Trace("Unfiltered cache:" + cache.Name);
					FireReadEvent(cache);
				}
			}
		}
		
		private string DoPrefilter(string[] ownerIDs)
		{
			if (this.AdvancedFilters == null)
				return null;
	
			System.Text.StringBuilder preFilterList = new System.Text.StringBuilder();
			bool atLeastOne = false;
			if (AdvancedFilters.Contains(FilterList.KEY_INCATTRS) || AdvancedFilters.Contains(FilterList.KEY_EXCATTRS)
			    || AdvancedFilters.Contains(FilterList.KEY_CHILDREN))
			{
				StringBuilder refineList = new StringBuilder();
				preFilterList.Append(" AND GEOCACHE.name IN (");
				if (AdvancedFilters.Contains(FilterList.KEY_INCATTRS))
				{
					refineList = BuildInclusionList ("SELECT DISTINCT cachename FROM ATTRIBUTES where inc='True' AND value == ",
					                    FilterList.KEY_INCATTRS,
					                    refineList,
					                    out atLeastOne);
				}
				if (AdvancedFilters.Contains(FilterList.KEY_EXCATTRS))
				{
					refineList = BuildInclusionList ("SELECT DISTINCT cachename FROM ATTRIBUTES where inc='False' AND value ==", 
					                    FilterList.KEY_EXCATTRS,
					                    refineList,
					                    out atLeastOne);
					
				}

				string childTypes = AdvancedFilters.GetCriteria(FilterList.KEY_CHILDREN) as string;
				if (!String.IsNullOrEmpty(childTypes))
				{
					atLeastOne = true;
	
					string cmd = String.Format(HAS_WPT_FILT,childTypes);
					if (refineList.Length > 0)
					{
						cmd += "AND WAYPOINT.parent IN (";
						cmd += refineList.ToString();
						cmd += ")";
					}
					refineList = new StringBuilder();
					
					IDataReader rdr = ExecuteSQLQuery(cmd);
					bool firstDone = false;
					while (rdr.Read())
					{
						if (!firstDone)
							firstDone = true;
						else
							refineList.Append(",");
						refineList.Append("'");
						refineList.Append(rdr.GetString(0));
						refineList.Append("'");
					}
					rdr.Close();
				}
				
				preFilterList.Append(refineList);
				preFilterList.Append(")");
				
			}
			if (AdvancedFilters.Contains(FilterList.KEY_INCNOATTRS) || AdvancedFilters.Contains(FilterList.KEY_EXCNOATTRS) ||
			    AdvancedFilters.Contains(FilterList.KEY_NOCHILDREN)|| AdvancedFilters.Contains(FilterList.KEY_LFOUND_BEFORE)||
			    AdvancedFilters.Contains(FilterList.KEY_LFOUND_AFTER) || (AdvancedFilters.Contains(FilterList.KEY_FOUNDAFTER))||
			    AdvancedFilters.Contains(FilterList.KEY_FOUNDBEFORE) || (AdvancedFilters.Contains(FilterList.KEY_FOUNDON)))
			{
				preFilterList.Append(" AND GEOCACHE.name NOT IN (");
				if (AdvancedFilters.Contains(FilterList.KEY_INCNOATTRS))
				{
					BuildExclusionList ("SELECT DISTINCT cachename FROM ATTRIBUTES where inc='True' AND value IN (", 
					                    FilterList.KEY_INCNOATTRS,
					                    preFilterList,
					                    out atLeastOne);
					
				}
				if (AdvancedFilters.Contains(FilterList.KEY_EXCNOATTRS))
				{
					BuildExclusionList ("SELECT DISTINCT cachename FROM ATTRIBUTES where inc='False' AND value IN (", 
					                    FilterList.KEY_EXCNOATTRS,
					                    preFilterList,
					                    out atLeastOne);
					
				}
				string childTypes = AdvancedFilters.GetCriteria(FilterList.KEY_NOCHILDREN) as string;
				if (!String.IsNullOrEmpty(childTypes))
				{
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(String.Format(HAS_WPT_FILT,childTypes));
					bool firstDone = false;
					while (rdr.Read())
					{
						if (!firstDone)
							firstDone = true;
						else
							preFilterList.Append(",");
						preFilterList.Append("'");
						preFilterList.Append(rdr.GetString(0));
						preFilterList.Append("'");
					}
					rdr.Close();
				}
				if (AdvancedFilters.Contains(FilterList.KEY_LFOUND_BEFORE))
				{
					DateTime date = (DateTime) AdvancedFilters.GetCriteria(FilterList.KEY_LFOUND_BEFORE);
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(LAST_FOUND_FILT);
					bool firstDone = false;
					while (rdr.Read())
					{
						string dtval = rdr.GetString(0);
						string cache = rdr.GetString(1);
						DateTime logDate = DateTime.Parse(dtval);
						if (logDate.Date > date.Date || logDate.Year == 2000)
						{
							if (!firstDone)
								firstDone = true;
							else
								preFilterList.Append(",");
							preFilterList.Append("'");
							preFilterList.Append(cache);
							preFilterList.Append("'");
						}
					}
					rdr.Close();
				}
				if (AdvancedFilters.Contains(FilterList.KEY_LFOUND_AFTER))
				{
					DateTime date = (DateTime) AdvancedFilters.GetCriteria(FilterList.KEY_LFOUND_AFTER);
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(LAST_FOUND_FILT);
					bool firstDone = false;
					while (rdr.Read())
					{
						string dtval = rdr.GetString(0);
						string cache = rdr.GetString(1);
						DateTime logDate = DateTime.Parse(dtval);
						if (logDate.Date < date.Date )
						{
							if (!firstDone)
								firstDone = true;
							else
								preFilterList.Append(",");
							preFilterList.Append("'");
							preFilterList.Append(cache);
							preFilterList.Append("'");
						}
					}
					rdr.Close();
				}
				if (AdvancedFilters.Contains(FilterList.KEY_FOUNDBEFORE))
				{
					DateTime date = (DateTime) AdvancedFilters.GetCriteria(FilterList.KEY_FOUNDBEFORE);
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(String.Format(LAST_FOUND_ME_FILT, ArrayToSQL(ownerIDs)));
					bool firstDone = false;
					while (rdr.Read())
					{
						string dtval = rdr.GetString(0);
						string cache = rdr.GetString(1);
						DateTime logDate = DateTime.Parse(dtval);
						if (logDate.Date > date.Date || logDate.Year == 2000)
						{
							if (!firstDone)
								firstDone = true;
							else
								preFilterList.Append(",");
							preFilterList.Append("'");
							preFilterList.Append(cache);
							preFilterList.Append("'");
						}
					}
					rdr.Close();
				}
				if (AdvancedFilters.Contains(FilterList.KEY_FOUNDON))
				{
					DateTime date = (DateTime) AdvancedFilters.GetCriteria(FilterList.KEY_FOUNDON);
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(String.Format(LAST_FOUND_ME_FILT, ArrayToSQL(ownerIDs)));
					bool firstDone = false;
					while (rdr.Read())
					{
						string dtval = rdr.GetString(0);
						string cache = rdr.GetString(1);
						DateTime logDate = DateTime.Parse(dtval);
						if (logDate.Date != date.Date)
						{
							if (!firstDone)
								firstDone = true;
							else
								preFilterList.Append(",");
							preFilterList.Append("'");
							preFilterList.Append(cache);
							preFilterList.Append("'");
						}
					}
					rdr.Close();
				}
				if (AdvancedFilters.Contains(FilterList.KEY_FOUNDAFTER))
				{
					DateTime date = (DateTime) AdvancedFilters.GetCriteria(FilterList.KEY_FOUNDAFTER);
					atLeastOne = true;
					IDataReader rdr = ExecuteSQLQuery(String.Format(LAST_FOUND_ME_FILT, ArrayToSQL(ownerIDs)));
					bool firstDone = false;
					while (rdr.Read())
					{
						string dtval = rdr.GetString(0);
						string cache = rdr.GetString(1);
						DateTime logDate = DateTime.Parse(dtval);
						if (logDate.Date < date.Date )
						{
							if (!firstDone)
								firstDone = true;
							else
								preFilterList.Append(",");
							preFilterList.Append("'");
							preFilterList.Append(cache);
							preFilterList.Append("'");
						}
					}
					rdr.Close();
				}
				preFilterList.Append(")");
			}
			if (atLeastOne)
				return preFilterList.ToString();
			return null;
		}
		
		private StringBuilder BuildInclusionList (String sql, String key, 
		                                 System.Text.StringBuilder preFilterList, 
		                                 out bool atLeastOne)
		{
				atLeastOne = true;
				StringBuilder refineList = new StringBuilder(preFilterList.ToString());
				List<String> incAttrs = AdvancedFilters.GetCriteria(key) as List<String>;
				IEnumerator<String> ct = incAttrs.GetEnumerator();
				bool firstDone = false;
				while (ct.MoveNext())
				{	
					
					string cmdTxt = sql + "'" + ct.Current + "'";
					if (refineList.Length > 0)
					{
						cmdTxt += " AND cachename IN (";
						cmdTxt += refineList;
						cmdTxt += ")";
						
					}
					if (!firstDone)
						firstDone = true;
				
					IDataReader rdr = ExecuteSQLQuery(cmdTxt);
					refineList.Remove(0, refineList.Length);
					firstDone = false;
					while (rdr.Read())
					{
						if (!firstDone)
							firstDone = true;
						else
							refineList.Append(",");
						refineList.Append("'");
						refineList.Append(rdr.GetString(0));
						refineList.Append("'");
					}
					if (refineList.Length == 0)
						refineList.Append("NULL");
					rdr.Close();
				}
				return refineList;
		}
		
		private void BuildExclusionList (String sql, String key, 
		                                 System.Text.StringBuilder preFilterList, 
		                                 out bool atLeastOne)
		{
			atLeastOne = true;
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			
			List<String> incAttrs = AdvancedFilters.GetCriteria(key) as List<String>;
			IEnumerator<String> ct = incAttrs.GetEnumerator();
			builder.Append(sql);
			bool firstDone = false;
			while (ct.MoveNext())
			{
				if (!firstDone)
					firstDone = true;
				else
					builder.Append(",");
				builder.Append("'");
				builder.Append(ct.Current);
				builder.Append("'");
			}
			builder.Append(")");
			
			IDataReader rdr =  ExecuteSQLQuery(builder.ToString());
			firstDone = false;
			while (rdr.Read())
			{
				if (!firstDone)
					firstDone = true;
				else
					preFilterList.Append(",");
				preFilterList.Append("'");
				preFilterList.Append(rdr.GetString(0));
				preFilterList.Append("'");
			}
			rdr.Close();
		}
		


		public override List<Geocache> GetCachesByName (string[] cachecode)
		{
			Trace("Getting caches by name...");
			List<string> hasChildrenList = BuildHasChildrenList();
			List<string> hasFinalList = BuildHasFinalList();
			StringBuilder sql = new StringBuilder(GET_GC);
			sql.Append(" AND GEOCACHE.name IN (");
			sql.Append(ArrayToSQL(cachecode));
			sql.Append(")");
			List<Geocache> caches = new List<Geocache>();
			IDataReader rdr = ExecuteSQLQuery(sql.ToString());
			while (rdr.Read())
			{
				caches.Add(BuildCache(rdr, 0, 0, hasChildrenList, hasFinalList));
			}
			Trace("Read:" + caches.Count);
			return caches;
		}
		
		public override List<Geocache> GetFinds ()
		{
			Trace("Getting finds...");
			List<string> hasChildrenList = new List<string>();
			List<string> hasFinalList = new List<string>();
			StringBuilder sql = new StringBuilder(GET_GC);
			sql.Append(FOUND_ONLY);
			List<Geocache> caches = new List<Geocache>();
			IDataReader rdr = ExecuteSQLQuery(sql.ToString());
			while (rdr.Read())
			{
				caches.Add(BuildCache(rdr, 0, 0, hasChildrenList, hasFinalList));
			}
			Trace("Read:" + caches.Count);
			return caches;
		}
		
		private List<string> BuildHasChildrenList()
		{
			Trace("Building has children list");
			List<string> hasChildrenList = new List<string>();
			IDataReader rdr = ExecuteSQLQuery(HASCHILDREN_LIST);
			while (rdr.Read())
			{
				hasChildrenList.Add(rdr.GetString(0));
			}
			return hasChildrenList;
		}
		
		private List<string> BuildHasFinalList()
		{
			Trace("Building has final list");
			List<string> hasFinalList = new List<string>();
			IDataReader rdr = ExecuteSQLQuery(HASFINAL_LIST);
			while (rdr.Read())
			{
				hasFinalList.Add(rdr.GetString(0));
			}
			return hasFinalList;
		}
		
		private Geocache BuildCache(IDataReader reader, double lat, double lon, List<string> hasChildren, List<string> hasFinal)
		{
			Trace("Building Cache..");
			Geocache cache = new Geocache();
			// DBVER 0 Fields
			cache.Name = reader.GetString(0);
			Trace(cache.Name);
			cache.Lat = double.Parse(reader.GetString(1), CultureInfo.InvariantCulture);
			cache.Lon = double.Parse(reader.GetString(2), CultureInfo.InvariantCulture);
			String url = reader.GetString(3);
			if (!String.IsNullOrEmpty(url))
				cache.URL = new Uri(url);
			cache.URLName = reader.GetString(4);
			cache.Desc = reader.GetString(5);
			cache.Symbol = reader.GetString(6);
			cache.Type = reader.GetString(7);
			String time = reader.GetString(8);
			cache.Time = DateTime.Parse(time);	
			cache.CacheName = reader.GetString(9);
			cache.CacheID = reader.GetString(10);
			cache.CacheOwner = reader.GetString(11);
			cache.OwnerID = reader.GetString(12);
			cache.PlacedBy = reader.GetString(13);
			cache.Difficulty = float.Parse(reader.GetString(14), CultureInfo.InvariantCulture);
			cache.Terrain = float.Parse(reader.GetString(15), CultureInfo.InvariantCulture);
			cache.Country = reader.GetString(16);
			cache.State = reader.GetString(17);
			cache.TypeOfCache = (Geocache.CacheType) Enum.Parse(typeof (Geocache.CacheType), reader.GetString(18));
			cache.ShortDesc = reader.GetString(19);
			cache.LongDesc = reader.GetString(20);
			cache.Hint = reader.GetString(21);
			cache.Container = reader.GetString(22);
			String archived = reader.GetString(23);
			cache.Archived = Boolean.Parse(archived);
			String available = reader.GetString(24);
			cache.Available = Boolean.Parse(available);
			cache.Updated = DateTime.Parse(reader.GetString(25));
			
			// From this point, the fields have been added in later DB Schema versions
			// Must check to see if they aren't DBNULL before getting the value
			Object val = reader.GetValue(26);
			if (val is string)
				cache.Notes = reader.GetString(26);
			val = reader.GetValue(27);
			if (val is string)
				cache.CheckNotes = Boolean.Parse(val as string);
			else
				cache.CheckNotes = false;
			val = reader.GetValue(28);
			if (val is string)
				cache.CorrectedLat = Double.Parse(val as string, CultureInfo.InvariantCulture);
			val = reader.GetValue(29);
			if (val is string)
				cache.CorrectedLon = Double.Parse(val as string, CultureInfo.InvariantCulture);
			val = reader.GetValue(30);
			if (val is string)
				cache.DNF = Boolean.Parse(val as string);
			val = reader.GetValue(31);
			if (val is string)
				cache.FTF = Boolean.Parse(val as string);
			val = reader.GetValue(32);
			if (val is string)
					cache.User1 = val as string;
			val = reader.GetValue(33);
			if (val is string)
					cache.User2 = val as string;
			val = reader.GetValue(34);
			if (val is string)
					cache.User3 = val as string;
			val = reader.GetValue(35);
			if (val is string)
					cache.User4 = val as string;
		
			// Preprocessed fields from other DB tables... These queries were run first for performance reason
			cache.Children = hasChildren.Contains(cache.Name);
			cache.HasFinal = hasFinal.Contains(cache.Name);
			
			// Calculated properties
			cache.Distance = Utilities.calculateDistance(lat, cache.Lat, lon, cache.Lon);			
			return cache;
		}
		
		public override List<Waypoint> GetWaypointsByName (string[] waypointcodes)
		{
			Trace("Geting Waypoints by Name");
			StringBuilder sql = new StringBuilder(GET_WPTS);
			sql.Append(" WHERE WAYPOINT.name IN(");
			sql.Append(ArrayToSQL(waypointcodes));
			sql.Append(")");
			IDataReader rdr = ExecuteSQLQuery(sql.ToString());
			List<Waypoint> wpts = new List<Waypoint>();
			while (rdr.Read())
			{
				wpts.Add(BuildWaypoint(rdr));
			}
			Trace("Read:" + wpts.Count);
			return wpts;
		}
		
		private Waypoint BuildWaypoint(IDataReader reader)
		{
			Trace("Building Waypoint");
			Waypoint pt = new Waypoint();
			pt.Name = reader.GetString(0);
			Trace(pt.Name);
			pt.Lat = double.Parse(reader.GetString(1), CultureInfo.InvariantCulture);
			pt.Lon = double.Parse(reader.GetString(2), CultureInfo.InvariantCulture);
			string url = reader.GetString(3);
			if (!String.IsNullOrEmpty(url))
				pt.URL = new Uri(url);
			pt.URLName = reader.GetString(4);
			pt.Desc = reader.GetString(5);
			pt.Symbol = reader.GetString(6);
			pt.Type = reader.GetString(7);
			pt.Time = DateTime.Parse(reader.GetString(8));
			pt.Parent = reader.GetString(9);
			pt.Updated = DateTime.Parse(reader.GetString(10));
			return pt;
		}
		
		private IDataReader ExecuteSQLQuery(string sql)
		{
			Trace("Executing Query");
			Trace(sql);
			IDbCommand cmd = m_Connection.CreateCommand();
			cmd.CommandText = sql;
			IDataReader rdr = cmd.ExecuteReader();
			return rdr;
			
		}

		public override List<Waypoint> GetChildWaypoints (string[] cachecode)
		{
			Trace("Getting Children for " + cachecode);
			StringBuilder sql = new StringBuilder(GET_WPTS);
			sql.Append(String.Format(WHERE_PARENT, ArrayToSQL(cachecode)));
			IDataReader rdr = ExecuteSQLQuery(sql.ToString());
			List<Waypoint> pts = new List<Waypoint>();
			while (rdr.Read())
			{
				pts.Add(BuildWaypoint(rdr));
			}
			return pts;
		}

		public override CacheAttribute[] GetAttributes (string cachecode)
		{
			Trace("Getting Attributes for " + cachecode);
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_ATTRIBUTES, cachecode));
			List<CacheAttribute> attrs = new List<CacheAttribute>();
			while (rdr.Read())
			{
				CacheAttribute attr = new CacheAttribute();
				attr.ID = rdr.GetString(0);
				string val = rdr.GetString(1);
				attr.Include = bool.Parse(val);
				attr.AttrValue = rdr.GetString(2);
				attrs.Add(attr);
			}
			return attrs.ToArray();
		}
		
		public override Dictionary<string, List<CacheAttribute>> GetAttributesMulti(string[] cachecodes)
		{
			Trace("Getting Attributes Multi");
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_ATTRIBUTES_MULTI, ArrayToSQL(cachecodes)));
			Dictionary<string, List<CacheAttribute>> attrs = new Dictionary<string, List<CacheAttribute>>();
			while (rdr.Read())
			{
				CacheAttribute attr = new CacheAttribute();
				attr.ID = rdr.GetString(0);
				string val = rdr.GetString(1);
				attr.Include = bool.Parse(val);
				attr.AttrValue = rdr.GetString(2);
				string code = rdr.GetString(3);
				if (!attrs.ContainsKey(code))
					attrs.Add(code, new List<CacheAttribute>());
				attrs[code].Add(attr);
			}
			return attrs;
		}

		public override List<CacheLog> GetCacheLogs (string cachecode)
		{
			Trace("Getting Cache Logs for " + cachecode);
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_LOGS, cachecode));
			List<CacheLog> logs = new List<CacheLog>();
			while (rdr.Read())
			{
				CacheLog log = new CacheLog();
				log.CacheCode = cachecode;
				log.LogDate = DateTime.Parse(rdr.GetString(0));
				log.LoggedBy = rdr.GetString(1);
				log.LogMessage = rdr.GetString(2);
				log.LogStatus = rdr.GetString(3);
				log.FinderID = rdr.GetString(4);
				String encoded = rdr.GetString(5);
				log.Encoded = Boolean.Parse(encoded);
				log.LogID = rdr.GetString(6);
				log.LogKey = rdr.GetString(7);
				logs.Add(log);
			}
			return logs;
		}
		
		public override CacheLog GetCacheLogByKey(string key)
		{
			Trace("Getting Cache Log  " + key);
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_LOG_BY_KEY, key));
			CacheLog log = new CacheLog();
			while (rdr.Read())
			{
				log.LogKey = key;
				log.LogDate = DateTime.Parse(rdr.GetString(0));
				log.LoggedBy = rdr.GetString(1);
				log.LogMessage = rdr.GetString(2);
				log.LogStatus = rdr.GetString(3);
				log.FinderID = rdr.GetString(4);
				String encoded = rdr.GetString(5);
				log.Encoded = Boolean.Parse(encoded);
				log.LogID = rdr.GetString(6);
				log.CacheCode = rdr.GetString(7);
			}
			return log;
		}
		
		public override Dictionary<string, List<CacheLog>> GetCacheLogsMulti(string[] cachecodes)
		{
			Trace("Getting Cache Logs multi");
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_LOGS_MULTI, ArrayToSQL(cachecodes)));
			Dictionary<string, List<CacheLog>> logmap = new Dictionary<string, List<CacheLog>>();
			while (rdr.Read())
			{
				CacheLog log = new CacheLog();
				log.LogDate = DateTime.Parse(rdr.GetString(0));
				log.LoggedBy = rdr.GetString(1);
				log.LogMessage = rdr.GetString(2);
				log.LogStatus = rdr.GetString(3);
				log.FinderID = rdr.GetString(4);
				String encoded = rdr.GetString(5);
				log.Encoded = Boolean.Parse(encoded);
				log.LogID = rdr.GetString(6);
				log.LogKey = rdr.GetString(7);
				log.CacheCode = rdr.GetString(8);
				if (logmap.ContainsKey(log.CacheCode))
				{
					logmap[log.CacheCode].Add(log);
				}
				else
				{
					logmap.Add(log.CacheCode, new List<CacheLog>());
					logmap[log.CacheCode].Add(log);
				}
			}
			return logmap;
		}

		public override TravelBug[] GetTravelBugs (string cachecode)
		{
			Trace("Getting Travel bugs for " + cachecode);
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_TB, cachecode));
			List<TravelBug> bugs = new List<TravelBug>();
			while (rdr.Read())
			{
				TravelBug bug = new TravelBug();
				bug.ID = rdr.GetString(0);
				bug.Ref = rdr.GetString(1);
				bug.Name = rdr.GetString(2);
				bug.Cache = cachecode;
				bugs.Add(bug);
			}
			return bugs.ToArray();
		}
		
		public override Dictionary<string, List<TravelBug>> GetTravelBugMulti(string[] cachecodes)
		{
			Trace("Getting Travel bugs multi ");
			IDataReader rdr = ExecuteSQLQuery(String.Format(GET_TB_MULTI, ArrayToSQL(cachecodes)));
			Dictionary<string, List<TravelBug>> bugs = new Dictionary<string, List<TravelBug>>();
			while (rdr.Read())
			{
				TravelBug bug = new TravelBug();
				bug.ID = rdr.GetString(0);
				bug.Ref = rdr.GetString(1);
				bug.Name = rdr.GetString(2);
				bug.Cache = rdr.GetString(3);
				if (!bugs.ContainsKey(bug.Cache))
				{
					bugs.Add(bug.Cache, new List<TravelBug>());
				}
				bugs[bug.Cache].Add(bug);
			}
			return bugs;
		}

		public override DateTime GetLastFound (string cachecode)
		{
			Trace("Getting last found date for " + cachecode);
			Object val = ExecuteSQLSingleValue(String.Format(LAST_FOUND, cachecode));
			if (val != null)
				return DateTime.Parse(val.ToString());
			return DateTime.MinValue;
		}

		public override DateTime GetLastFoundBy (string cachecode, string finderId)
		{
			
			Trace("Getting last found date for " + cachecode +" by " + finderId);
			Object val = ExecuteSQLSingleValue(String.Format(LAST_FOUND_BY, cachecode, finderId));
			if (val != null)
				return DateTime.Parse(val.ToString());
			return DateTime.MinValue;
		}

		public override DateTime GetLastDNFBy (string cachecode, string finderId)
		{
			Trace("Getting last dnf date for " + cachecode +" by " + finderId);
			Object val = ExecuteSQLSingleValue(String.Format(LAST_DNF_BY, cachecode, finderId));
			if (val != null)
				return DateTime.Parse(val.ToString());
			return DateTime.MinValue;
		}

		public override CacheLog GetLastLogBy (string cachecode, string finderId)
		{
			throw new System.NotImplementedException ();
		}

		public override CacheLog GetLastFindLogBy (string cachecode, string finderId)
		{
				
			Trace("Getting last found log for " + cachecode +" by " + finderId);
			IDataReader rdr = ExecuteSQLQuery(String.Format(LAST_FOUND_BY, cachecode, finderId));
			CacheLog log = new CacheLog();
			while (rdr.Read())
			{
				
				log.LogDate = DateTime.Parse(rdr.GetString(0));
				log.LoggedBy = rdr.GetString(1);
				log.LogMessage = rdr.GetString(2);
				log.LogStatus = rdr.GetString(3);
				log.FinderID = rdr.GetString(4);
				String encoded = rdr.GetString(5);
				log.Encoded = Boolean.Parse(encoded);
				log.LogID = rdr.GetString(6);
				log.LogKey = rdr.GetString(7);
				log.CacheCode = cachecode;
			}
			return log;
		}

		public override CacheLog GetLastLog (string cachecode)
		{
			throw new System.NotImplementedException ();
		}


		private void ExecuteSQLCommand (String sql)
		{
			Trace ("Executing SQL:");
			Trace (sql);
			IDbCommand cmd = m_Connection.CreateCommand ();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery ();
			cmd.Dispose ();
			cmd = null;
		}


		public override void StartUpdate ()
		{
			Trace ("Starting DB Update");
			if (m_Transaction != null)
				throw new Exception ("ENGINE ERR: Already in a transaction!");
			m_Transaction = m_Connection.BeginTransaction ();
		}

		public override void CancelUpdate ()
		{
			if (m_Transaction == null)
				return;
			Trace ("Cancelling DB Update");
			m_Transaction.Rollback ();
			m_Transaction.Dispose ();
			m_Transaction = null;
			
		}

		public override void CompleteUpdate ()
		{
			Trace ("Completing DB Update");
			m_Transaction.Commit ();
			m_Transaction.Dispose ();
			m_Transaction = null;
		}

		public override void PurgeAllLogs (string[] cachecodes)
		{
			Trace ("Purging Logs");
			ExecuteSQLCommand (String.Format (DELETE_LOGS, ArrayToSQL (cachecodes)));
		}
		
		public override void PurgeLogsByKey(string[] logkeys)
		{
			Trace ("Purging Logs By Key");
			ExecuteSQLCommand (String.Format (DELETE_LOGS_BY_KEY, ArrayToSQL (logkeys)));
		}



		public override void PurgeAllAttributes (string[] cachecodes)
		{
			Trace ("Purging Attributes");
			ExecuteSQLCommand (String.Format (DELETE_ATTRIBUTES, ArrayToSQL (cachecodes)));
		}

		public override void PurgeAllTravelBugs (string[] cachecodes)
		{
			Trace ("Purging TravelBugs");
			ExecuteSQLCommand (String.Format (DELETE_TBS, ArrayToSQL (cachecodes)));
		}

		public override void AddBookmarkList (string listName)
		{
			Trace("Add Bookmark list");
			String sql = String.Format(ADD_BMRK, listName);
			ExecuteSQLCommand(sql);
		}

		public override void AddBoormarkEntry (string listName, string cacheCode)
		{
			Trace("Add Bookmark entry");
			String sql = String.Format(BOOKMARK_CACHE, cacheCode, listName);
			ExecuteSQLCommand(sql);
		}

		public override void RemoveBookmarkEntry (string listName, string cacheCode)
		{
			Trace("Remove Bookmark");
			String sql = String.Format(REMOVE_CACHE_FROM_BOOKMARK, cacheCode, listName);
			ExecuteSQLCommand(sql);
		}


		public override void RemoveBookmarkList (string listName)
		{
			Trace("Remove Bookmark List");
			String sql = String.Format(REMOVE_BOOKMARK, listName);
			ExecuteSQLCommand(sql);
		}

		public override string[] GetBookmarkLists ()
		{
			List<string> bmrkList = new List<string>();
			IDataReader rdr = ExecuteSQLQuery(GET_BMRKS);
			while (rdr.Read())
			{
				bmrkList.Add(rdr.GetString(0));
			}
			return bmrkList.ToArray();
		}


		public override void AddLog (string parent, CacheLog log)
		{
			Trace("Adding log for " + parent);
			String insert = String.Format(ADD_LOG, parent, log.LogDate.ToString("o"), Escape(log.LoggedBy),
			                                Escape(log.LogMessage), Escape(log.LogStatus), log.FinderID, 
			                                log.Encoded.ToString(), log.LogID, log.LogKey);
			String update = String.Format(UPDATE_LOG, parent, log.LogDate.ToString("o"), Escape(log.LoggedBy),
			                                Escape(log.LogMessage), Escape(log.LogStatus), log.FinderID, 
			                                log.Encoded.ToString(), log.LogID, log.LogKey);
			InsertOrUpdate(update, insert);
		}
		
		public override void AddWaypointOrCache (Waypoint pt, bool lockSymbol, bool partialUpdate)
		{
			double lat = pt.Lat;
			double lon = pt.Lon;
			if (pt is Geocache)
			{
				lat = (pt as Geocache).OrigLat;
				lon = (pt as Geocache).OrigLon;
			}
			
				
			string insert = String.Format(INSERT_WPT, Escape(pt.Name), lat.ToString(CultureInfo.InvariantCulture), lon.ToString(CultureInfo.InvariantCulture), pt.URL, 
			                                Escape(pt.URLName), Escape(pt.Desc), pt.Symbol, pt.Type,
			                                pt.Time.ToString("o"), pt.Parent, pt.Updated.ToString("o"));
			string update;
			if (lockSymbol)
				update = String.Format(UPDATE_WPT_NO_SYM, Escape(pt.Name), lat.ToString(CultureInfo.InvariantCulture), lon.ToString(CultureInfo.InvariantCulture), pt.URL, 
			                                Escape(pt.URLName), Escape(pt.Desc), pt.Type,
			                                pt.Time.ToString("o"), pt.Parent, pt.Updated.ToString("o"));	
			else
				update = String.Format(UPDATE_WPT, Escape(pt.Name), lat.ToString(CultureInfo.InvariantCulture), lon.ToString(CultureInfo.InvariantCulture), pt.URL, 
			                                Escape(pt.URLName), Escape(pt.Desc), pt.Symbol, pt.Type,
			                                pt.Time.ToString("o"), pt.Parent, pt.Updated.ToString("o"));			
			InsertOrUpdate (update, insert);
			if (pt is Geocache)
			{
				AddGeocache(pt as Geocache, partialUpdate);
			}

		}
		
		private void AddGeocache(Geocache cache, bool partialUpdate)
		{
			Trace("Adding as geocache");
			Trace("Partial update:" + partialUpdate.ToString());
			string insert = String.Format(INSERT_GC, cache.Name, Escape(cache.CacheName), cache.CacheID, 
			                                Escape(cache.CacheOwner), cache.OwnerID, Escape(cache.PlacedBy), 
			                                cache.Difficulty.ToString(CultureInfo.InvariantCulture), cache.Terrain.ToString(CultureInfo.InvariantCulture), Escape(cache.Country), 
			                                Escape(cache.State),cache.TypeOfCache.ToString(), 
			                                Escape(cache.ShortDesc), Escape(cache.LongDesc),
			                                Escape(cache.Hint), cache.Container, cache.Archived.ToString(),
			                                cache.Available.ToString(), Escape(cache.Notes), cache.CheckNotes.ToString(),
			                              	cache.CorrectedLat.ToString(CultureInfo.InvariantCulture), cache.CorrectedLon.ToString(CultureInfo.InvariantCulture),
			                              	cache.DNF, cache.FTF, cache.User1, cache.User2, cache.User3, cache.User4);
			cache.ClearCorrectedFlag = false;
			string update;
			if (partialUpdate)
			{
				update = String.Format(ADD_EXISTING_GC, cache.Name, Escape(cache.CacheName), cache.CacheID, 
			                                Escape(cache.CacheOwner), cache.OwnerID, Escape(cache.PlacedBy), 
			                                cache.Difficulty.ToString(CultureInfo.InvariantCulture), cache.Terrain.ToString(CultureInfo.InvariantCulture), Escape(cache.Country), 
			                                Escape(cache.State),cache.TypeOfCache.ToString(), 
			                                Escape(cache.ShortDesc), Escape(cache.LongDesc),
			                                Escape(cache.Hint), cache.Container, cache.Archived.ToString(),
			                                cache.Available.ToString(), cache.CheckNotes.ToString(),
			                              	cache.CorrectedLat.ToString(CultureInfo.InvariantCulture), cache.CorrectedLon.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				update =  String.Format(UPDATE_GC, cache.Name, Escape(cache.CacheName), cache.CacheID, 
			                                Escape(cache.CacheOwner), cache.OwnerID, Escape(cache.PlacedBy), 
			                                cache.Difficulty.ToString(CultureInfo.InvariantCulture), cache.Terrain.ToString(CultureInfo.InvariantCulture), Escape(cache.Country), 
			                                Escape(cache.State),cache.TypeOfCache.ToString(), 
			                                Escape(cache.ShortDesc), Escape(cache.LongDesc),
			                                Escape(cache.Hint), cache.Container, cache.Archived.ToString(),
			                                cache.Available.ToString(), Escape(cache.Notes), cache.CheckNotes.ToString(),
			                              	cache.CorrectedLat.ToString(CultureInfo.InvariantCulture), cache.CorrectedLon.ToString(CultureInfo.InvariantCulture), 
				                        	cache.DNF, cache.FTF, cache.User1, cache.User2, cache.User3, cache.User4);
			}
			InsertOrUpdate (update, insert);

		}
	
		private void InsertOrUpdate(string update, string insert)
		{
			Trace("Starting Insert or Update...");
			Trace(update);
			IDbCommand cmd = m_Connection.CreateCommand();
			cmd.CommandText = update;
			int changed = cmd.ExecuteNonQuery();
			if (0 == changed)
			{
				Trace("Doing Insert");
				Trace(insert);
				cmd.CommandText = insert;
				cmd.ExecuteNonQuery();
			}
			cmd.Dispose();
			cmd = null;
		}

		public override void DeleteWaypoint (string name)
		{
			Trace("Deleting Waypoint:" + name);
			bool inTrans = m_Transaction != null ?true:false;
			if (!inTrans)
				StartUpdate();
			String sql = String.Format(DELETE_GC, name);
			ExecuteSQLCommand(sql);
			sql = String.Format(DELETE_WPT, name);
			ExecuteSQLCommand(sql);
			sql = String.Format(DELETE_ATTRIBUTES, "'" + name + "'");
			ExecuteSQLCommand(sql);
			sql= String.Format(DELETE_LOGS, "'" + name + "'");
			ExecuteSQLCommand(sql);
			sql = String.Format(DELETE_TBS, "'" + name + "'");
			ExecuteSQLCommand(sql);
			if (!inTrans)
				CompleteUpdate();
		}


		public override void AddTravelBug (string parent, TravelBug bug)
		{
			Trace("Adding travel bug for " + bug);
			ExecuteSQLCommand(String.Format(ADD_TB, parent, bug.ID, bug.Ref, Escape(bug.Name)));
		}

		public override void AddAttribute (string parent, CacheAttribute attribute)
		{
			Trace("Adding attribute for " + parent);
			ExecuteSQLCommand(String.Format(ADD_ATTRIBUTE, parent, attribute.ID, attribute.Include.ToString(), 
			                                attribute.AttrValue));
		}

		private String BuildWhereClause (Hashtable m_criteria, string[] ownerIDs)
		{
			StringBuilder builder = new StringBuilder ();
			String terrain_val = m_criteria[FilterList.KEY_TERRAIN_VAL] as string;
			String terrain_op = m_criteria[FilterList.KEY_TERRAIN_OP] as string;
			if (!String.IsNullOrEmpty (terrain_val)) {
				builder.Append (" AND GEOCACHE.terrain ");
				builder.Append (terrain_op);
				builder.Append (" ");
				builder.Append (terrain_val);
			}
			
			String diff_val = m_criteria[FilterList.KEY_DIFF_VAL] as string;
			String diff_op = m_criteria[FilterList.KEY_DIFF_OP] as string;
			if (!String.IsNullOrEmpty (diff_val)) {
				builder.Append (" AND GEOCACHE.difficulty ");
				builder.Append (diff_op);
				builder.Append (" ");
				builder.Append (diff_val);
			}
			
			List<String> cacheTypes = m_criteria[FilterList.KEY_CACHETYPE] as List<String>;
			if (null != cacheTypes) {
				builder.Append (" AND GEOCACHE.type IN (");
				IEnumerator<String> ct = cacheTypes.GetEnumerator ();
				bool firstDone = false;
				while (ct.MoveNext ()) {
					if (!firstDone)
						firstDone = true;
					else
						builder.Append (",");
					builder.Append ("'");
					builder.Append (ct.Current);
					builder.Append ("'");
				}
				builder.Append (")");
			}
			
			List<String> contTypes = m_criteria[FilterList.KEY_CONTAINER] as List<String>;
			if (null != contTypes) {
				builder.Append (" AND GEOCACHE.container IN (");
				IEnumerator<String> ct = contTypes.GetEnumerator ();
				bool firstDone = false;
				while (ct.MoveNext ()) {
					if (!firstDone)
						firstDone = true;
					else
						builder.Append (",");
					builder.Append ("'");
					builder.Append (ct.Current);
					builder.Append ("'");
				}
				builder.Append (")");
			}
			
			String placedBy = m_criteria[FilterList.KEY_PLACEDBY] as string;
			if (null != placedBy) {
				builder.Append (" AND GEOCACHE.placedby == '");
				builder.Append (Escape(placedBy));
				builder.Append ("'");
			}
			
			string description = m_criteria[FilterList.KEY_DESCRIPTION] as string;
			if (null != description) {
				String[] words = description.Split (' ');
				builder.Append (" AND (GEOCACHE.longdesc LIKE");
				Boolean firstDone = false;
				foreach (String word in words) {
					if (!firstDone)
						firstDone = true;
					else
						builder.Append (" OR GEOCACHE.longdesc LIKE");
					builder.Append ("'% ");
					builder.Append (Escape(word));
					builder.Append ("%'");
				}
				builder.Append (')');
			}
			
			BuildStatusFilter (m_criteria, builder, ownerIDs);
			
			if (m_criteria.ContainsKey (FilterList.KEY_COUNTRY)) {
				builder.Append (" AND GEOCACHE.country LIKE '");
				builder.Append (m_criteria[FilterList.KEY_COUNTRY] as string);
				builder.Append ("'");
			}
			
			if (m_criteria.ContainsKey (FilterList.KEY_STATE)) {
				builder.Append (" AND GEOCACHE.state LIKE '");
				builder.Append (m_criteria[FilterList.KEY_STATE] as string);
				builder.Append ("'");
			}
			
			if (m_criteria.ContainsKey (FilterList.KEY_FOUNDAFTER) || m_criteria.ContainsKey (FilterList.KEY_FOUNDBEFORE) || m_criteria.ContainsKey (FilterList.KEY_FOUNDON)) {
				builder.Append (" AND WAYPOINT.symbol == 'Geocache Found'");
			}
			
			if (m_criteria.Contains (FilterList.KEY_NOTES)) {
				builder.Append (" AND GEOCACHE.notes NOT NULL AND GEOCACHE.notes != ''");
			}
			
			if (m_criteria.Contains (FilterList.KEY_CORRECTED)) {
				builder.Append (" AND GEOCACHE.corlat NOT NULL AND GEOCACHE.corlat != '-1'");
			}
			
			if (m_criteria.Contains (FilterList.KEY_NOCORRECTED)) {
				builder.Append (" AND  (GEOCACHE.corlat IS NULL OR GEOCACHE.corlat ='-1')");
			}
			if (m_criteria.Contains (FilterList.KEY_FTF)) {
				builder.Append (" AND Geocache.ftf == '" + ((bool)m_criteria[FilterList.KEY_FTF]).ToString () + "'");
			}
			if (m_criteria.Contains (FilterList.KEY_DNF)) {
				builder.Append (" AND Geocache.dnf == '" + ((bool)m_criteria[FilterList.KEY_DNF]).ToString () + "'");
			}
			if (m_criteria.Contains (FilterList.KEY_U1)) {
				builder.Append (" AND Geocache.user1 LIKE '%" + Escape(m_criteria[FilterList.KEY_U1] as string) + "%'");
			}
			if (m_criteria.Contains (FilterList.KEY_U2)) {
				builder.Append (" AND Geocache.user2 LIKE '%" + Escape(m_criteria[FilterList.KEY_U2] as string) + "%'");
			}
			if (m_criteria.Contains (FilterList.KEY_U3)) {
				builder.Append (" AND Geocache.user3 LIKE '%" + Escape(m_criteria[FilterList.KEY_U3] as string) + "%'");
			}
			if (m_criteria.Contains (FilterList.KEY_U4)) {
				builder.Append (" AND Geocache.user4 LIKE '%" + Escape(m_criteria[FilterList.KEY_U4] as string) + "%'");
			}
			if (m_criteria.Contains (FilterList.KEY_CACHE_SOURCE)) {
				List<string> sources = m_criteria[FilterList.KEY_CACHE_SOURCE] as List<string>;
				builder.Append (" AND (");
				bool isFirst = false;
				foreach (string source in sources) {
					if (!isFirst)
						isFirst = true;
					else
						builder.Append (" OR ");
					builder.Append ("GEOCACHE.name LIKE '");
					builder.Append (source);
					builder.Append ("%'");
				}
				builder.Append (")");
			}
			if (m_criteria.Contains (FilterList.KEY_CACHE_NAME)) {
				builder.Append (" AND (Geocache.name LIKE '%" + Escape(m_criteria[FilterList.KEY_CACHE_NAME] as string) + "%'");
				builder.Append ("OR Geocache.fullname LIKE '%" + Escape(m_criteria[FilterList.KEY_CACHE_NAME] as string) + "%')");
			}
			return builder.ToString ();
		}
		
		private static void BuildStatusFilter (Hashtable m_criteria, StringBuilder builder, string[] ownerIDs)
		{
			Boolean[] status = m_criteria[FilterList.KEY_STATUS] as Boolean[];
			if (status != null) 
			{
				if (status[0] && status[1] && !status[2])
				{
					builder.Append (" AND (GEOCACHE.owner NOT IN (");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" AND GEOCACHE.ownerID NOT IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append ("))");
				}
				else if (status[0] && ! status[1] && status[2])
				{
					builder.Append (" AND (WAYPOINT.symbol == 'Geocache Found'");
					builder.Append (" OR GEOCACHE.owner IN (");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" OR GEOCACHE.ownerID IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append ("))");
				}
				else if (status[0] && ! status[1] && !status[2])
				{
					builder.Append (" AND WAYPOINT.symbol == 'Geocache Found'");
				}
				else if (!status[0] && status[1] && status[2])
				{
					builder.Append (" AND WAYPOINT.symbol != 'Geocache Found'");
				}
				else if (!status[0] && status[1] && !status[2])
				{
					builder.Append (" AND (WAYPOINT.symbol != 'Geocache Found'");
					builder.Append (" AND GEOCACHE.owner NOT IN (");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" AND GEOCACHE.ownerID NOT IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append ("))");
				}
				else if (!status[0] && !status[1] && status[2])
				{
					builder.Append (" AND (GEOCACHE.owner IN (");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" OR GEOCACHE.ownerID IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append ("))");
				}
				
				
				/*if (!status[0] && status[1])
					builder.Append (" AND WAYPOINT.symbol != 'Geocache Found'");
				if (status[0] && !status[1])
					builder.Append (" AND WAYPOINT.symbol == 'Geocache Found'");
				if (!status[2]) {
					builder.Append (" AND GEOCACHE.owner NOT IN (");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" AND GEOCACHE.ownerID NOT IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
				} else if (status[2] && (!status[0] || !status[1])) {
					builder.Append (" OR GEOCACHE.owner IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
					builder.Append (" OR GEOCACHE.ownerID IN(");
					builder.Append (ArrayToSQL(ownerIDs));
					builder.Append (")");
				}*/
				if (status[3] && !status[4] && status[5])
					builder.Append (" AND (GEOCACHE.available == 'True' OR Geocache.archived == 'True')");
				if (!status[3] && !status[4] && status[5])
					builder.Append (" AND GEOCACHE.available == 'False' AND Geocache.archived == 'True'");
				if (!status[3] && status[4] && !status[5])
					builder.Append (" AND GEOCACHE.available == 'False' AND Geocache.archived == 'False'");
				if (status[3] && !status[4] && !status[5])
					builder.Append (" AND GEOCACHE.available == 'True'");
				if (status[3] && status[4] && !status[5])
					builder.Append (" AND (GEOCACHE.available == 'True' OR Geocache.archived == 'False')");
				if (!status[3] && status[4] && status[5])
					builder.Append (" AND GEOCACHE.available == 'False'");
			}
		}
		
		private bool PostFilter (Geocache cache, double lat, double lon)
		{
			if (m_Advanced != null)
			{
				if (m_Advanced.Contains(FilterList.KEY_PLACEBEFORE))
					if (cache.Time >= ((DateTime) m_Advanced.GetCriteria(FilterList.KEY_PLACEBEFORE)))
						return false;
				if (m_Advanced.Contains(FilterList.KEY_PLACEAFTER))
					if (cache.Time <= ((DateTime) m_Advanced.GetCriteria(FilterList.KEY_PLACEAFTER)))
						return false;
				if (m_Advanced.Contains(FilterList.KEY_INFOBEFORE))
					if (cache.Updated >= ((DateTime) m_Advanced.GetCriteria(FilterList.KEY_INFOBEFORE)))
						return false;
				if (m_Advanced.Contains(FilterList.KEY_INFOAFTER))
					if (cache.Updated <= ((DateTime) m_Advanced.GetCriteria(FilterList.KEY_INFOAFTER)))
						return false;
				if (m_Advanced.Contains(FilterList.KEY_INFO_DAYS))
				{
					int days = (int) m_Advanced.GetCriteria(FilterList.KEY_INFO_DAYS);
					DateTime dt = DateTime.Now.Subtract(new TimeSpan(days, 0,0,0));
					if (cache.Updated <= dt)
						return false;						
				}
				if (m_Advanced.Contains(FilterList.KEY_INFO_NDAYS))
				{
					int days = (int) m_Advanced.GetCriteria(FilterList.KEY_INFO_NDAYS);
					DateTime dt = DateTime.Now.Subtract(new TimeSpan(days, 0,0,0));
					if (cache.Updated >= dt)
						return false;						
				}
				if (m_Advanced.Contains(FilterList.KEY_DIST))
				{
					double filterlat = lat;
					double filterlon = lon;
					if (m_Advanced.Contains(FilterList.KEY_DIST_LAT))
						lat = (double) m_Advanced.GetCriteria(FilterList.KEY_DIST_LAT);
					if (m_Advanced.Contains(FilterList.KEY_DIST_LON))
						lon = (double) m_Advanced.GetCriteria(FilterList.KEY_DIST_LON);
					
					double limit = (double) m_Advanced.GetCriteria(FilterList.KEY_DIST);
					double dist = Utilities.calculateDistance(cache.Lat, filterlat, cache.Lon, filterlon);
					string op = m_Advanced.GetCriteria(FilterList.KEY_DIST_OP) as String;
					if (op == "<=")
						if (dist > limit)
							return false;
					if (op == ">=")
						if (dist < limit)
							return false;
					if (op == "==")
						if (dist != limit)
							return false;
				}
			}
			if (m_Global.Contains(FilterList.KEY_DIST))
			{
				double limit = (double) m_Global.GetCriteria(FilterList.KEY_DIST);
				double dist = Utilities.calculateDistance(cache.Lat, lat, cache.Lon, lon);
				if (dist > limit)
					return false;
			}
			return true;
		}
	}
}
