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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Mono.Unix;
using ocmengine;

namespace ocmengine
{
	
	
	public class Waypoint
	{
		protected double m_lat = 0;
		protected double m_lon = 0;
		private DateTime m_time = DateTime.Now;
		private string m_name = "";
		private string m_desc = "";
		private Uri m_uri = new Uri("http://www.geocaching.com");
		private string m_urlname = String.Empty;
		private string m_symbol = "Waypoint";
		private string m_type = "Waypoint";
		private string m_parent = null;
		private DateTime m_updated = DateTime.MinValue;
		private ACacheStore m_store = null;
		
		public string Name
		{
			get { return m_name;}
			set { m_name = value;}
		}
		
		public virtual double Lat
		{
			get { return m_lat;}
			set {m_lat = value;}
		}
		
		public virtual double Lon
		{
			get { return m_lon;}
			set { m_lon = value;}
		}
		
		public Uri URL
		{
			get { return m_uri; }
			set { m_uri = value; }
		}
		
		public string URLName
		{
			get { return m_urlname; }
			set { m_urlname = value; }
		}
		
		public string Desc
		{
			get { return m_desc; }
			set { m_desc = value; }
		}
		
		public string Symbol
		{
			get {return m_symbol; }
			set {m_symbol = value;}
		}
		
		public DateTime Time
		{
			get {return m_time;}
			set {m_time = value;}
		}
		
		public DateTime Updated
		{
			get {return m_updated;}
			set {m_updated = value;}
		}
		
		
		public string Type
		{
			get { return m_type;}
			set { m_type = value;}
		}
		
		public string Parent
		{
			get {return m_parent;}
			set {m_parent = value;}
		}
		
		
		public Waypoint()
		{
			
		}
		
		public void WriteToGPX(XmlWriter writer, GPXWriter gpx)
		{
			writer.WriteStartElement("wpt", GPXWriter.NS_GPX);
			WriteWPTDetails(writer, gpx);
			writer.WriteEndElement();
			if (!gpx.IncludeChildWaypoints)
				return;
			
			if ((this is Geocache) && ((this as Geocache).HasCorrected))
			{
				Geocache cache = this as Geocache;
				Waypoint orig = new Waypoint();
				orig.Name = cache.Name + "-ORIG";
				orig.Lat = cache.OrigLat;
				orig.Lon = cache.OrigLon;
				orig.Symbol = "Reference Point";
				orig.Type = "Waypoint|Reference Point";
				orig.Desc = Catalog.GetString("Original Location") + " - " + cache.CacheName;
				orig.WriteToGPX(writer, gpx);
			}
		}
		
		internal virtual void WriteWPTDetails(XmlWriter writer, GPXWriter gpx)
		{
			Geocache cache = null;
			if (this is Geocache)
			{
				cache = this as Geocache;
			}
			
			writer.WriteAttributeString("lat", this.Lat.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("lon", this.Lon.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("time", this.Time.ToString("o"));
			if (cache != null && gpx.NameMode == WaypointNameMode.NAME)
				writer.WriteElementString("name", cache.CacheName);
			else if (cache != null && gpx.NameMode == WaypointNameMode.SHORTCODE)
				writer.WriteElementString("name", this.Name.Substring(2));
			else
				writer.WriteElementString("name", this.Name);
			if (cache != null && gpx.DescriptionMode == WaypointDescMode.CODESIZEANDHINT)
			{
				if (!String.IsNullOrEmpty(cache.Container))
					writer.WriteElementString("desc", cache.Name + "/" + cache.Container.Substring(0, 3) + "/" + cache.Hint);
				else
					writer.WriteElementString("desc", cache.Name);
			}
			else if (cache != null && gpx.DescriptionMode == WaypointDescMode.CODESIZETYPE)
			{
				if (!String.IsNullOrEmpty(cache.Container))
					writer.WriteElementString("desc", cache.Name + "/" + cache.Container.Substring(0, 3) + "/" + cache.TypeOfCache.ToString());
				else
					writer.WriteElementString("desc", cache.Name + "/" + cache.TypeOfCache.ToString());
			}
			else if (cache != null && gpx.DescriptionMode == WaypointDescMode.FULL)
			{
				StringBuilder builder = new StringBuilder();				
				builder.Append("<br/><br/>" + cache.ShortDesc);
				builder.Append("<br/>" + cache.LongDesc);
				if (!String.IsNullOrEmpty(cache.Hint))
					builder.Append("<br/><br/>Hint: " + cache.Hint);
				builder.Append("<br/><br/>Logs:<br/>");
				List<CacheLog> logs = gpx.GetCacheLogs(cache.Name);
				int iCount = 0;
				foreach(CacheLog log in logs)
				{
					if (iCount >= gpx.LogLimit && gpx.LogLimit >= 0)
						break;
					builder.Append(log.toHTML());
					iCount++;
				}
				
				writer.WriteStartElement("desc");
				if (gpx.HTMLOutput == HTMLMode.GARMIN)
					writer.WriteCData(Utilities.HTMLtoGarmin(builder.ToString()));
				else if (gpx.HTMLOutput == HTMLMode.PLAINTEXT)
					writer.WriteCData(Utilities.HTMLtoText(builder.ToString()));
				else
					writer.WriteCData(builder.ToString());
				
				writer.WriteEndElement();	
			}
			else
			{
				writer.WriteElementString("desc", this.Desc);	
				writer.WriteElementString("cmt", this.Desc);
			}
			if (this.URL != null)
				writer.WriteElementString("url", this.URL.ToString());
			if (!String.IsNullOrEmpty(this.URLName))
				writer.WriteElementString("urlname", this.URLName);
			if (gpx.DescriptionMode == WaypointDescMode.FULL)
				writer.WriteElementString("sym", "Information");
			else if (!gpx.UseOCMPtTypes)
				writer.WriteElementString("sym", this.Symbol);
			else
			{				
					System.Console.WriteLine(this.Name);
					if (cache != null && cache.Symbol == "Geocache Found")
						writer.WriteElementString("sym", gpx.Mappings[this.Symbol]);					
					else if (cache != null && cache.TypeOfCache != Geocache.CacheType.GENERIC)
					{
						string key = "Geocache|" + Geocache.GetCTypeString(cache.TypeOfCache);
						writer.WriteElementString("sym", gpx.Mappings[key]);
					}
					else if (gpx.Mappings.ContainsKey(this.Type))
						writer.WriteElementString("sym", gpx.Mappings[this.Type]);
					else
						writer.WriteElementString("sym", this.Symbol);
			}
			writer.WriteElementString("type", this.Type);
			if (this.Parent != null)
				writer.WriteElementString("parent", "http://opencachemanage.sourceforge.net/schema1", this.Parent);
			if (cache != null && gpx.DescriptionMode == WaypointDescMode.FULL)
			{
				writer.WriteStartElement("extensions");
				writer.WriteStartElement("gpxx", "WaypointExtension", "http://www.garmin.com/xmlschemas/GpxExtensions/v3");
				writer.WriteElementString("gpxx:DisplayMode","SymbolAndName");
				writer.WriteStartElement("gpxx:Address");
				StringBuilder builder = new StringBuilder();
				builder.Append(Geocache.GetCTypeString(cache.TypeOfCache) + "\nBy: " + cache.PlacedBy);
				builder.Append("\n" + cache.Name + ":" + cache.CacheName);
				builder.Append("\n" + Utilities.getCoordString(cache.Lat, cache.Lon));
				builder.Append("\nT:" + cache.Terrain.ToString() + " D:" + cache.Difficulty.ToString() + " S:" + cache.Container);
				writer.WriteElementString("gpxx:PostalCode",builder.ToString());
				writer.WriteEndElement();
				writer.WriteEndElement();	
				writer.WriteEndElement();
			}
			else if (gpx.DescriptionMode == WaypointDescMode.FULL)
			{
				writer.WriteStartElement("extensions");
				writer.WriteStartElement("gpxx", "WaypointExtension", "http://www.garmin.com/xmlschemas/GpxExtensions/v3");
				writer.WriteElementString("gpxx:DisplayMode","SymbolAndName");
				writer.WriteStartElement("gpxx:Address");
				StringBuilder builder = new StringBuilder();
				builder.Append(this.Symbol + " for " + this.Parent);
				builder.Append("\n" + Utilities.getCoordString(this.Lat, this.Lon));
				writer.WriteElementString("gpxx:PostalCode",builder.ToString());
				writer.WriteEndElement();
				writer.WriteEndElement();	
				writer.WriteEndElement();
			}
		}
		
		public override string ToString ()
		{
			return string.Format("[Waypoint: Name={0}, Lat={1}, Lon={2}, URL={3}, URLName={4}, Des={5}, Symbol={6}, Time={7}, Type={8}]", Name, Lat, Lon, URL, URLName, Desc, Symbol, Time, Type);
		}

		
	}
}
