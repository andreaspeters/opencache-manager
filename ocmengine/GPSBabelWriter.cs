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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using ocmengine;

namespace ocmengine
{


	public class GPSBabelWriter
	{
		private int m_Limit = -1;
		public int Limit {
			set { m_Limit = value; }
		}

		private string m_format = "garmin";
		public string BabelFormat {
			set { m_format = value; }
		}
		private string m_file = "usb:";
		public string BabelFile {
			set { m_file = value; }
		}
		
		private int m_LogLimit = -1;
		public int LogLimit{
			set { m_LogLimit = value;}
		}
		
		private bool m_forcePlainText = false;
		public bool ForcePlainText
		{
			set { m_forcePlainText = value;}
		}
		
		private WaypointDescMode m_descMode = WaypointDescMode.DESC;
		public WaypointDescMode DescMode
		{
			set { m_descMode = value;}
		}
		
		private WaypointNameMode m_nameMode = WaypointNameMode.NAME;
		public WaypointNameMode NameMode
		{
			set { m_nameMode = value;}
		}
		
		private bool m_incAttr = true;
		public bool IncludeAttributes
		{
			set {m_incAttr = value;}
		}
		
		private string m_otherBabelParams;
		public string OtherBabelParams
		{
			set {m_otherBabelParams = value;}
		}
		
		private bool m_includeChildren = true;
		public bool IncludeChildren
		{
			set { m_includeChildren = value;}
		}
		
		private GPXWriter writer;

		public event WriteEventHandler WriteWaypoint;
		public event WriteEventHandler StartSend;
		public event WriteEventHandler Complete;
		public delegate void WriteEventHandler (object sender, EventArgs args);


		public void WriteToGPS (List<Geocache> cacheList, Dictionary<string,string> waypointmappings, ACacheStore store)
		{
			writer = new GPXWriter ();
			writer.NameMode = m_nameMode;
			writer.DescriptionMode = m_descMode;
			writer.Limit = m_Limit;
			writer.LogLimit = m_LogLimit;
			writer.IncludeChildWaypoints = true;
			writer.WriteAttributes = m_incAttr;
			if (m_format == "OCM_GPX") {
				if (m_file == "%auto%")
				{
					string dBShort = Utilities.GetShortFileName(store.StoreName);
					dBShort = dBShort.Substring(0, dBShort.Length - 4);
					dBShort+= ".gpx";
					m_file = "/media/GARMIN/Garmin/GPX/" + dBShort;
				}
				writer.HTMLOutput = HTMLMode.GARMIN;
				writer.UseOCMPtTypes = true;
				WriteGPXFile (cacheList, waypointmappings, store);
				return;
			}
			else if (m_format == "delgpx")
			{
				writer.HTMLOutput = HTMLMode.PLAINTEXT;
				writer.UseOCMPtTypes = false;
				if (m_file == "%auto%")
				{
					string dBShort = Utilities.GetShortFileName(store.StoreName);
					dBShort = dBShort.Substring(0, dBShort.Length - 4);
					dBShort+= ".gpx";
					m_file = "/media/EM_USERMAPS/waypoints/" + dBShort;
				}
				WriteGPXFile (cacheList, waypointmappings, store);
				return;
			}
			
			if (m_format == "edge")
			{
				writer.UseOCMPtTypes = true;
				writer.IncludeGroundSpeakExtensions = false;
				WriteGPXFile (cacheList, waypointmappings, store);
				return;
			}
			
			writer.IncludeGroundSpeakExtensions = true;
			writer.UseOCMPtTypes = true;
			writer.IncludeChildWaypoints = m_includeChildren;
			if (m_format == "garmin_gpi")
			{
				writer.UseOCMPtTypes = false;
				writer.IncludeGroundSpeakExtensions = false;
				if (m_forcePlainText)
					writer.HTMLOutput = HTMLMode.PLAINTEXT;
				else
					writer.HTMLOutput = HTMLMode.GARMIN;
			}			
			else if (m_format == "garmin")
			{
				writer.IncludeGroundSpeakExtensions = false;
			}		
			writer.Complete += HandleWriterComplete;
			String tempFile = Path.GetTempFileName ();
			writer.WriteWaypoint += HandleWriterWriteWaypoint;
			writer.WriteGPXFile (tempFile, cacheList, waypointmappings, store);
			this.StartSend (this, new WriteEventArgs ("Sending Waypoints to GPS"));
			StringBuilder builder = new StringBuilder ();
			builder.Append ("-i gpx -f ");
			builder.Append (tempFile);
			builder.Append (" -o ");
			builder.Append (m_format);
			if (!String.IsNullOrEmpty(m_otherBabelParams))
			{
				builder.Append(",");
				builder.Append(m_otherBabelParams);
			}
			builder.Append (" -F \"");
			builder.Append (m_file.Replace("\\","\\\\").Replace("\"","\\\"").Replace("$","\\$").Replace("`","\\`"));
			builder.Append("\"");

			if (writer.Cancel)
			{
				throw new Exception ("Aborted");
			}
			ProcessStartInfo sp = new ProcessStartInfo();
			sp.Arguments = builder.ToString();
			sp.FileName = "gpsbabel";
			Process babel = Process.Start (sp);
			babel.WaitForExit ();
			if (babel.ExitCode != 0)
				throw new Exception ("Failed to send caches to GPS");
			this.Complete (this, new WriteEventArgs ("Complete"));
		}
		
		private void WriteGPXFile (List<Geocache> cacheList, Dictionary<string,string> waypointmappings, ACacheStore store)
		{
			writer.Complete += HandleWriterComplete;
			writer.WriteWaypoint += HandleWriterWriteWaypoint;
			writer.WriteGPXFile (m_file, cacheList, waypointmappings,store);
			this.Complete (this, new WriteEventArgs ("Complete"));
			return;
		}

		void HandleWriterWriteWaypoint (object sender, EventArgs args)
		{
			this.WriteWaypoint (this, args as WriteEventArgs);
		}

		void HandleWriterComplete (object sender, EventArgs args)
		{
			this.Complete (this, args as WriteEventArgs);
		}
		
		public void Cancel()
		{
			writer.Cancel = true;
		}
	}
}
