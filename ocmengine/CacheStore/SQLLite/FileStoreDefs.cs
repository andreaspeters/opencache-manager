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

namespace ocmengine.SQLLite
{


	public partial class FileStore
	{
		const string CREATE_CACHE_TABLE = "CREATE TABLE GEOCACHE (available TEXT, archived TEXT, container TEXT, hint TEXT, longdesc TEXT, shortdesc TEXT, type TEXT, state TEXT, country TEXT, terrain TEXT, difficulty TEXT, placedby TEXT, name TEXT PRIMARY KEY, fullname TEXT, id TEXT, owner TEXT, ownerID TEXT, notes TEXT, checkNotes TEXT, corlat TEXT, corlon TEXT, dnf TEXT, ftf TEXT, user1 TEXT, user2 TEXT, user3 TEXT, user4 TEXT)";
		const string CREATE_LOGS_TABLE = "CREATE TABLE LOGS(cache TEXT, date text, loggedby TEXT, message TEXT, status TEXT, finderID TEXT, encoded TEXT, id TEXT, logkey TEXT PRIMARY KEY)";
		const string CREATE_TABLE_TBUGS = "CREATE TABLE TBUGS (cache TEXT, id TEXT, ref TEXT, name TEXT)";
		const string CREATE_TABLE_WPTS = "CREATE TABLE WAYPOINT (lastUpdate TEXT, parent TEXT, symbol TEXT, time TEXT, type TEXT, desc TEXT, urlname TEXT, url TEXT, lon TEXT, lat TEXT, name TEXT PRIMARY KEY)";
		const string CREATE_TABLE_BMRK = "CREATE TABLE BOOKMARKS (name TEXT PRIMARY KEY)";
		const string CREATE_TABLE_BMRK_CACHES = "CREATE TABLE BOOKMARKED_CACHES (bookmark TEXT, cachecode TEXT)";
		const string SQL_CONNECT = "Data Source=";
		const string INSERT_WPT = "INSERT INTO WAYPOINT (name,lat,lon,url,urlname,desc,symbol,type,time, parent, lastUpdate) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}', '{10}')";
		const string UPDATE_WPT = "UPDATE WAYPOINT SET lat='{1}', lon='{2}',url='{3}', urlname='{4}', desc='{5}', symbol='{6}', type='{7}', time='{8}', parent='{9}', lastUpdate='{10}' WHERE name='{0}'";
		const string UPDATE_WPT_NO_SYM = "UPDATE WAYPOINT SET lat='{1}', lon='{2}',url='{3}', urlname='{4}', desc='{5}', type='{6}', time='{7}', parent='{8}', lastUpdate='{9}' WHERE name='{0}'";
		const string WPT_EXISTS_CHECK = "SELECT COUNT(name) FROM WAYPOINT WHERE name='{0}'";
		const string GET_WPTS = "SELECT name, lat, lon, url, urlname, desc, symbol, type, time, parent, lastUpdate FROM WAYPOINT";
		const string DELETE_LOGS = "DELETE FROM LOGS where cache IN ({0})";
		const string DELETE_LOGS_BY_KEY = "DELETE FROM LOGS where logkey IN ({0})";
		const string DELETE_TBS = "DELETE FROM TBUGS where cache IN ({0})";
		const string ADD_LOG = "INSERT INTO LOGS (cache, date, loggedby, message, status, finderID, encoded, id, logkey) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}', '{7}','{8}')";
		const string UPDATE_LOG = "UPDATE LOGS SET cache='{0}', date='{1}', loggedby='{2}', message='{3}', status='{4}', " +
				" finderID ='{5}', encoded='{6}', id='{7}' where logkey='{8}'";
		const string ADD_TB = "INSERT INTO TBUGS(cache, id, ref, name) VALUES('{0}','{1}','{2}','{3}')";
		const string GET_TB = "SELECT id, ref, name FROM TBUGS WHERE cache='{0}'";
		const string GET_TB_MULTI = "SELECT id, ref, name, cache FROM TBUGS WHERE cache IN ({0})";
		const string WHERE_PARENT = " WHERE parent IN ({0})";
		const string GET_LOG_BY_KEY = "SELECT date, loggedby, message, status, finderID, encoded, id, cache FROM LOGS WHERE logkey='{0}'";
		const string GET_LOGS = "SELECT date, loggedby, message, status, finderID, encoded, id, logkey FROM LOGS WHERE cache='{0}' ORDER BY date DESC";
		const string GET_LOGS_MULTI = "SELECT date, loggedby, message, status, finderID, encoded, id, logkey, cache FROM LOGS WHERE cache IN ({0}) ORDER BY date DESC";
		const string LOG_STAT_SCAN = "SELECT status from LOGS WHERE cache='{0}' and date=(SELECT MAX(date) FROM LOGS WHERE cache='{0}')";
		const string UPDATE_GC_CHECKNOTE = "UPDATE GEOCACHE  SET checkNotes='{0}' WHERE name='{1}'";
		const string LAST_LOG_BY_YOU = "SELECT date from LOGS WHERE cache='{0}' and (finderID='{1}' or loggedBy='{1}') and date=(SELECT MAX(date) FROM LOGS WHERE cache='{0}' and (finderID='{1}' or loggedBy='{1}'))";
		const string LAST_FOUND_BY = "SELECT date, loggedby, message, status, finderID, encoded, id, logkey from LOGS WHERE cache='{0}' and (finderID='{1}' or loggedBy='{1}') and (status='Found it' or status='find' or status='Attended' or status='Webcam Photo Taken')";
		const string LAST_FOUND = "SELECT MAX(date) from LOGS  WHERE (status='Found it' or status='find' or status='Attended' or status='Webcam Photo Taken') and cache='{0}'  GROUP BY cache";
		const string LAST_FOUND_FILT = " SELECT MAX(date),cache FROM (SELECT CASE WHEN status IN ('Found it','find','Attended','Webcam Photo Taken') THEN date ELSE '2000-01-01T00:00:00' END AS date,cache FROM LOGS) GROUP BY cache";
		const string LAST_FOUND_ME_FILT = "SELECT MAX(date),cache FROM (SELECT CASE WHEN status IN ('Found it','find','Attended','Webcam Photo Taken') AND finderID IN ({0}) THEN date ELSE '2000-01-01T00:00:00' END AS date,cache FROM LOGS) GROUP BY cache";
		const string LAST_DNF_BY = "SELECT MAX(date) from LOGS WHERE cache='{0}' and (finderID ='{1}' or loggedBy='{1}') and (status='Didn''t find it') GROUP BY cache";
		const string INSERT_GC = "INSERT INTO GEOCACHE (name, fullname, id, owner, ownerID, placedby, difficulty, terrain, country, state, type, shortdesc, longdesc, hint, container, archived, available, notes, checkNotes, corlat, corlon, dnf, ftf, user1, user2, user3, user4)" + " VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}', '{17}', '{18}', '{19}', '{20}','{21}','{22}','{23}','{24}','{25}','{26}')";
		const string UPDATE_GC = "UPDATE GEOCACHE SET fullname='{1}', id='{2}', owner='{3}', ownerID='{4}',  placedby='{5}', difficulty='{6}', terrain='{7}', country='{8}',state='{9}',type='{10}',shortdesc='{11}',longdesc='{12}',hint='{13}',container='{14}',archived='{15}',available='{16}', notes='{17}', checkNotes='{18}', corlat='{19}', corlon='{20}',dnf='{21}',ftf='{22}',user1='{23}',user2='{24}',user3='{25}',user4='{26}' WHERE name='{0}'";
		// SAME AS UPDATE, BUT DOESN't OVERWRITE SPECIAL FIELDS but does overwrite found status
		const string ADD_EXISTING_GC = "UPDATE GEOCACHE SET fullname='{1}', id='{2}', owner='{3}', ownerID='{4}',  placedby='{5}', difficulty='{6}', terrain='{7}', country='{8}',state='{9}',type='{10}',shortdesc='{11}',longdesc='{12}',hint='{13}',container='{14}',archived='{15}',available='{16}', checkNotes='{17}' WHERE name='{0}'";
		const string GC_EXISTS_CHECK = "SELECT 1 FROM GEOCACHE WHERE name='{0}'";
		const string GET_GC = "SELECT  WAYPOINT.name, WAYPOINT.lat, WAYPOINT.lon, WAYPOINT.url, WAYPOINT.urlname, WAYPOINT.desc, WAYPOINT.symbol, WAYPOINT.type, WAYPOINT.time," 
			+ "GEOCACHE.fullname, GEOCACHE.id, GEOCACHE.owner, GEOCACHE.ownerID, GEOCACHE.placedby, GEOCACHE.difficulty, GEOCACHE.terrain, GEOCACHE.country, GEOCACHE.state,"
			+ "GEOCACHE.type, GEOCACHE.shortdesc, GEOCACHE.longdesc, GEOCACHE.hint, GEOCACHE.container, GEOCACHE.archived, GEOCACHE.available, WAYPOINT.lastUpdate, GEOCACHE.notes, GEOCACHE.checkNotes, GEOCACHE.corlat, GEOCACHE.corlon,"
			+	"GEOCACHE.dnf, GEOCACHE.ftf, GEOCACHE.user1, GEOCACHE.user2, GEOCACHE.user3, GEOCACHE.user4"
			+ " FROM GEOCACHE,WAYPOINT WHERE GEOCACHE.name = WAYPOINT.name";
		//
		const string FOUND_ONLY = " AND WAYPOINT.symbol = 'Geocache Found'";
		const string COUNT_GC = "SELECT COUNT(name) from GEOCACHE";
		const string COUNT_WPT = "SELECT COUNT(name) from WAYPOINT";
		const string FOUND = " WHERE SYMBOL='Geocache Found'";
		const string INACTIVE = " WHERE AVAILABLE='False'";
		const string DELETE_WPT = "DELETE FROM WAYPOINT WHERE name='{0}'";
		const string DELETE_GC = "DELETE FROM GEOCACHE WHERE name='{0}'";
		const string OR_PARENT = " OR parent='{0}'";
		const string SEPERATOR = ";";
		const string CREATE_TABLE_WPTS_TEMP = "CREATE TABLE WAYPOINT_TEMP (lastUpdate TEXT, parent TEXT, symbol TEXT, time TEXT, type TEXT, desc TEXT, urlname TEXT, url TEXT, lon TEXT, lat TEXT, name TEXT PRIMARY KEY)";
		const string CREATE_CACHE_TABLE_TEMP = "CREATE TABLE GEOCACHE (available TEXT, archived TEXT, container TEXT, hint TEXT, longdesc TEXT, shortdesc TEXT, type TEXT, state TEXT, country TEXT, terrain TEXT, difficulty TEXT, placedby TEXT, name TEXT PRIMARY KEY, fullname TEXT, id TEXT, owner TEXT, ownerID TEXT)";	
		const string CREATE_ATTRS_TABLE = "CREATE TABLE ATTRIBUTES(cachename TEXT, id TEXT, inc TEXT, value TEXT)";
		const string BMRK_FILTER = " AND GEOCACHE.name IN (SELECT cachecode FROM BOOKMARKED_CACHES WHERE bookmark = '{0}') ";
		const string BMRK_FILTER_COUNT = " WHERE GEOCACHE.name IN (SELECT cachecode FROM BOOKMARKED_CACHES WHERE bookmark = '{0}') ";
		const string GET_BMRKS = "SELECT name from BOOKMARKS";
		const string GET_ATTRIBUTES = "SELECT id, inc, value FROM ATTRIBUTES WHERE cachename='{0}'";
		const string GET_ATTRIBUTES_MULTI = "SELECT id, inc, value, cachename FROM ATTRIBUTES WHERE cachename IN({0})";
		const string ADD_ATTRIBUTE = "INSERT INTO ATTRIBUTES(cachename, id, inc, value) VALUES ('{0}','{1}','{2}','{3}')";
		const string DELETE_ATTRIBUTES = "DELETE FROM ATTRIBUTES WHERE cachename IN ({0})";
		const string ADD_BMRK = "INSERT INTO BOOKMARKS (name) VALUES ('{0}')";
		const string BOOKMARK_CACHE = "INSERT INTO BOOKMARKED_CACHES (cachecode, bookmark) VALUES('{0}','{1}')";
		const string REMOVE_CACHE_FROM_BOOKMARK = "DELETE FROM BOOKMARKED_CACHES WHERE cachecode='{0}' and bookmark = '{1}'";
		const string REMOVE_BOOKMARK = "DELETE FROM BOOKMARKED_CACHES WHERE bookmark = '{0}'; DELETE FROM BOOKMARKS WHERE name = '{0}'";
		const string GET_DB_VER = "SELECT VER FROM DB_VER";
		const string CREATE_DB_VER = "CREATE TABLE DB_VER (VER INTEGER PRIMARY KEY)";
		const string CLEAR_DB_VER = "DELETE FROM DB_VER";
		const string SET_DB_VER = "INSERT INTO DB_VER (VER) VALUES (5)";
		const string UPGRADE_GEOCACHE_V0_V1 = "ALTER TABLE GEOCACHE ADD COLUMN notes TEXT";
		const string UPGRADE_GEOCACHE_V1_V2 = "ALTER TABLE GEOCACHE ADD COLUMN checkNotes TEXT";
		const string UPGRADE_GEOCACHE_V2_V3 = "ALTER TABLE LOGS ADD COLUMN id TEXT";
		const string UPGRADE_GEOCACHE_V3_V4A = "ALTER TABLE GEOCACHE ADD COLUMN corlat TEXT";
		const string UPGRADE_GEOCACHE_V3_V4B = "ALTER TABLE GEOCACHE ADD COLUMN corlon TEXT";
		const string UPGRADE_GEOCACHE_V4_V5A = "ALTER TABLE GEOCACHE ADD COLUMN dnf TEXT";
		const string UPGRADE_GEOCACHE_V4_V5B = "ALTER TABLE GEOCACHE ADD COLUMN ftf TEXT";
		const string UPGRADE_GEOCACHE_V4_V5C = "ALTER TABLE GEOCACHE ADD COLUMN user1 TEXT";
		const string UPGRADE_GEOCACHE_V4_V5D = "ALTER TABLE GEOCACHE ADD COLUMN user2 TEXT";
		const string UPGRADE_GEOCACHE_V4_V5E = "ALTER TABLE GEOCACHE ADD COLUMN user3 TEXT";
		const string UPGRADE_GEOCACHE_V4_V5F = "ALTER TABLE GEOCACHE ADD COLUMN user4 TEXT";
		const string UPGRADE_GEOCACHE_V4_V5G = "ALTER TABLE LOGS RENAME TO OLD_LOGS";
		const string UPGRADE_GEOCACHE_V4_V5H = "SELECT cache, date, loggedby, message, status, finderID, encoded, id FROM OLD_LOGS";
		const string UPGRADE_GEOCACHE_V4_V5I = "DROP TABLE OLD_LOGS";
		const string VACUUM = "VACUUM";
		const string HASCHILDREN_LIST = "SELECT DISTINCT parent FROM WAYPOINT WHERE parent NOT NULL AND parent != ''";
		const string HASFINAL_LIST = "SELECT DISTINCT parent FROM WAYPOINT WHERE symbol=='Final Location' AND parent NOT NULL AND parent != ''";
		const string HAS_WPT_FILT= "SELECT DISTINCT parent FROM WAYPOINT WHERE symbol=='{0}' AND parent NOT NULL AND parent != ''";
	}
}
