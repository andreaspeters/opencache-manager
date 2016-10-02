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
using System.Text;
using System.Collections.Generic;
using ocmengine;

namespace ocmgtk
{

	[System.ComponentModel.ToolboxItem(true)]
	public partial class LogViewerWidget : Gtk.Bin
	{
		HTMLWidget m_logPane;
		List<CacheLog> m_logs = null;
		
		public OCMApp App
		{
			set { m_logPane.App = value;}
		}
		
		public OCMMainWindow Win
		{
			set { m_logPane.Win = value;}
		}

		public LogViewerWidget ()
		{
			this.Build ();
			m_logPane = new HTMLWidget ();
			logAlign.Add(m_logPane);
		}
		
		public DateTime GetLastFound()
		{
			DateTime lastFound = DateTime.MinValue;
			foreach(CacheLog log in m_logs)
			{
				if (log.LogDate > lastFound)
					lastFound = log.LogDate;
			}
			return lastFound;
		}

		
		public DateTime GetLastFoundBy(string user)
		{
			DateTime lastFound = DateTime.MinValue;
			foreach(CacheLog log in m_logs)
			{
				if (log.LogStatus == "Found it"
					|| log.LogStatus == "find"
				    || log.LogStatus == "Attended"
				    || log.LogStatus == "Webcam Photo Taken")
				{
					if ((log.LogDate > lastFound) &&
					    ((log.FinderID == user ) ||
					     (log.LoggedBy == user)))
						lastFound = log.LogDate;
				}			
				
			}
			return lastFound;
		}
		
		public DateTime GetLastDNFBy(string user)
		{
			DateTime lastFound = DateTime.MinValue;
			foreach(CacheLog log in m_logs)
			{
				if (log.LogStatus == "Didn't find it")
				{
					if ((log.LogDate > lastFound) &&
					    ((log.FinderID == user ) ||
					     (log.LoggedBy == user)))
						lastFound = log.LogDate;
				}			
				
			}
			return lastFound;
		}
		

		public void SetCacheLogs (List<CacheLog> logs)
		{
			m_logs = logs;
			StringBuilder builder = new StringBuilder ();
			builder.Append ("<HTML><BODY>");
			if (logs != null) {
				foreach (CacheLog log in logs) {
					builder.Append (log.toHTML ());
				}
			}
			else
			{
				builder.Append(Mono.Unix.Catalog.GetString("NO CACHE SELECTED"));
			}
			builder.Append ("</BODY></HTML>");
			m_logPane.SetHTML(builder.ToString(), "file://" + System.Environment.CurrentDirectory + "/icons/24x24/.");
		}
	}
}
