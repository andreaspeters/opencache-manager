// 
//  Copyright 2010  campbelk
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

namespace ocmengine
{


	public class WaypointEnumerator : IEnumerator<Waypoint>
	{
		Waypoint m_current;
		IDataReader m_reader;
		IDbCommand m_command;
		IDbConnection m_conn;
		
		public Waypoint Current
		{
			get { return m_current;}
		}
		
		object System.Collections.IEnumerator.Current
		{
			get { return m_current;}
		}
		
		public WaypointEnumerator(IDbConnection conn, string sql)
		{
			m_command = conn.CreateCommand();
			m_command.CommandText = sql;
			m_reader = m_command.ExecuteReader();
		}
		
		public bool MoveNext()
		{
			if (m_reader.Read())
			{
				Waypoint pt = new Waypoint();
				pt.Name = m_reader.GetString(0);
				pt.Lat = m_reader.GetFloat(1);
				pt.Lon = m_reader.GetFloat(2);
				string url = m_reader.GetString(3);
				if (null != url)
					pt.URL = new Uri(url);
				pt.URLName = m_reader.GetString(4);
				pt.Desc = m_reader.GetString(5);
				pt.Symbol = m_reader.GetString(6);
				pt.Time = DateTime.Parse(m_reader.GetString(7));
				pt.Parent = m_reader.GetString(8);
				m_current = pt;
				return true;
			}
			else
			{
				CloseConnection();
			}
			return false;
		}
		
		void CloseConnection ()
		{
			if (null != m_reader)	
			{
				m_reader.Close();
				m_reader = null;
			}
			if (null != m_command)
			{
				m_command.Dispose();
				m_conn = null;
			}
			if (null != m_conn)
			{
				m_conn.Close();
				m_conn = null;
			}
		}
		
		public void Reset()
		{
			//Do nothing. Forward only
		}
		
		public void Dispose()
		{
			CloseConnection();
		}
		
		
	}
}
