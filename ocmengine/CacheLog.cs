/*
 Copyright 2009 Kyle Campbell
 Licensed under the Apache License, Version 2.0 (the "License"); 
 you may not use this file except in compliance with the License. 
 You may obtain a copy of the License at 
 
 		http://www.apache.org/licenses/LICENSE-2.0 
 
 Unless required by applicable law or agreed to in writing, software distributed under the License 
 is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
 implied. See the License for the specific language governing permissions and limitations under the License. 
*/
using System;
using System.Xml;
using System.IO;

namespace ocmengine
{
	
	
	public class CacheLog
	{
		DateTime m_logdate = DateTime.Now;
		const string LOG_PREFIX="groundspeak";
		string m_logged_by = "Unknown";
		string m_logmessage = "Unknown";
		string m_status = "Unknown";
		string m_finder_id = "Unknown";
		string m_logID = "-1";
		string m_logKey = "-1";
		string m_cacheCode = "Unknown";
		bool m_encoded = false;
		
		public DateTime LogDate
		{
			get { return m_logdate;}
			set {m_logdate = value;}
		}
		
		public string CacheCode
		{
			get { return m_cacheCode;}
			set { m_cacheCode = value;}
		}
		
		public string LoggedBy
		{
			get { return m_logged_by;}
			set { m_logged_by = value;}
		}
		
		
		public string LogMessage
		{
			get { return m_logmessage;}
			set { m_logmessage = value;}
		}
		
		public string  LogStatus
		{
			 get { return m_status;}
			 set { m_status = value;}
		}
		
		public string FinderID
		{
			get { return m_finder_id;}
			set { m_finder_id = value;}
		}
		
		public string LogID
		{
			get { return m_logID;}
			set { m_logID = value;
			}
		}
		
		public string LogKey
		{
			get { return m_logKey;}
			set { m_logKey = value;}
		}
		
		public bool Encoded
		{
			get { return m_encoded;}
			set { m_encoded = value;}
		}
				
		
		public CacheLog()
		{
		}
		
		public String toHTML()
		{
			string logHTML = "<div style='font-family:sans-serif;font-size:10pt'><hr/><table border='0' width='100%'><tr><td style='vertical-align:top;width:34;'>";
			if (m_status == "Found it"
					|| m_status == "find"
				    || m_status == "Attended"
				    || m_status == "Webcam Photo Taken")
			{
				logHTML += "<img src='found.png'></img>";
			}
			else if (m_status == "Didn't find it")
			{
				logHTML += "<img src='dnf.png'></img>";
			}
			else if (m_status == "Owner Maintenance" || m_status == "Needs Maintenance")
			{
				logHTML += "<img src='needs_maintenance.png'></img>";
			}
			else
			{
				logHTML += "<img src='write_note.png'></img>";
			}
			//logHTML += "<br/><img src='../icons/24x24/delete.png'></img>";
				
			logHTML += "</td><td><a href='ocm://deleteLog/"+ this.LogKey + "'>";
			logHTML += Mono.Unix.Catalog.GetString("Delete");
			logHTML += "</a>&nbsp;<a href='ocm://editLog/" + this.LogKey + "'>";
			logHTML += Mono.Unix.Catalog.GetString("Edit");
			logHTML += "</a><br/>";
			logHTML += "<b>Date:</b> ";
			logHTML += m_logdate.ToLongDateString();
			logHTML += "<br/><b>";
		
			logHTML += m_status;
			logHTML += "</b><br/>";
			logHTML += "<b>Logged By:</b> ";
			logHTML += m_logged_by;
			logHTML += "<hr/>";
			logHTML += m_logmessage;
			logHTML += "</td></tr></table><br/><br/></div>";
			return logHTML;
		}
		
		public override string ToString()
		{
			string logHTML = "Date:  ";
			logHTML += m_logdate.ToLongDateString();
			logHTML += "\n";
			logHTML += m_status;
			logHTML += "\n";
			logHTML += "Logged By: ";
			logHTML += m_logged_by;
			logHTML += "-----";
			logHTML += m_logmessage;
			logHTML += "\n\n";
			return logHTML;
		}
		
		public void WriteToGPX(XmlWriter writer)
		{
			writer.WriteStartElement(LOG_PREFIX,"log", GPXWriter.NS_CACHE);
			Random rand = new Random();
			if (!String.IsNullOrEmpty(m_logID))
				writer.WriteAttributeString("id", m_logID);
			else
			{
				rand.Next(50000);
				writer.WriteAttributeString("id", rand.Next(50000).ToString());
			}
			writer.WriteElementString(LOG_PREFIX,"date", GPXWriter.NS_CACHE, this.LogDate.ToString("o"));
			writer.WriteElementString(LOG_PREFIX,"type", GPXWriter.NS_CACHE, this.LogStatus);
			writer.WriteStartElement(LOG_PREFIX,"finder", GPXWriter.NS_CACHE);
			writer.WriteAttributeString("id", FinderID);
			writer.WriteString(LoggedBy);
			writer.WriteEndElement();
			writer.WriteStartElement(LOG_PREFIX,"text", GPXWriter.NS_CACHE);
			writer.WriteAttributeString("encoded", Encoded.ToString());
			writer.WriteString(LogMessage);
			writer.WriteEndElement();			
			writer.WriteEndElement();
		}
		
		public void WriteToFieldNotesFile(TextWriter writer)
		{
			writer.Write(m_cacheCode);
			writer.Write(",");
			writer.Write(m_logdate.ToString("o"));
			writer.Write(",");
			writer.Write(m_status);
			writer.Write(",\"");
			writer.Write(m_logmessage.Replace("\"","\"\""));
			writer.WriteLine("\"");
		}
	}
}
