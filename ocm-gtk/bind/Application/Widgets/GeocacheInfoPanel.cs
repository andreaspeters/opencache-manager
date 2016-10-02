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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Gtk;
using Gdk;
using Mono.Unix;
using ocmengine;
using System.Text;

namespace ocmgtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeocacheInfoPanel : Gtk.Bin
	{
		static string START_BIG = "<span font='bold 12'>";
		static string END_BIG = "</span>";

		static string FOUND_DATE = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkgreen'>You have already found this cache on {0}</span>");
		static string FOUND = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkgreen'>You have already found this cache</span>");
		static string FTF_DATE = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkgreen'>You were first to find this cache on {0}</span>");
		static string FTF = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkgreen'>You were first to find this cache</span>");
		static string DNF_DATE = Catalog.GetString ("<span font='bold italic 10' fgcolor='blue'>You didn't find this cache on {0}</span>");
		static string DNF = Catalog.GetString ("<span font='bold italic 10' fgcolor='blue'>You didn't find this cache</span>");
		static string MINE = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkgreen'>You own this cache</span>");
		static string UNAVAILABLE = Catalog.GetString ("<span font='bold italic 10' fgcolor='red'>This cache is temporarily unavailable, check the logs for more information.</span>");
		static string ARCHIVED = Catalog.GetString ("<span font='bold italic 10' fgcolor='red'>This cache has been archived, check the logs for more information.</span>");
		static string CHECK_LOGS = Catalog.GetString ("<span font='bold italic 10' fgcolor='darkorange'>This cache has a recent DNF or requires maintenance, check the logs for more information</span>");
		
		private static Pixbuf STAR_ICON = new Pixbuf ("./icons/scalable/star.svg", 16, 16);
		private static Pixbuf ESTAR_ICON = new Pixbuf ("./icons/scalable/star_empty.svg", 16, 16);
		private static Pixbuf HSTAR_ICON = new Pixbuf ("./icons/scalable/halfstar.svg", 16, 16);
		
		OCMApp m_app;
		Geocache m_cache;
		OCMMainWindow m_Win;
		LogViewerWidget m_Logs;

		public OCMApp App {
			set { m_app = value; }
		}
		
		public LogViewerWidget Logs
		{
			set { m_Logs = value;}
		}
		
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}

		private IConfig AppConfig {
			get { return m_app.AppConfig; }
		}

		private ACacheStore CacheStore {
			get { return m_app.CacheStore; }
		}

		public GeocacheInfoPanel ()
		{
			this.Build ();
			SetCache(null);
		}

		public void SetCache (Geocache cache)
		{
			m_cache = cache;
			if (cache == null) {
				SetInactive ();
				return;
			}
			this.Sensitive = true;
			cacheCodeLabel.Markup = START_BIG + cache.Name + ": " + END_BIG;
			cacheNameLabel.Markup = START_BIG + GLib.Markup.EscapeText (cache.CacheName) + END_BIG;
			setDifficulty (cache.Difficulty);
			setTerrain (cache.Terrain);
			setCacheIcon (cache.TypeOfCache);
			if ((m_app.AppConfig.ShowNewCaches) && ((DateTime.Now - cache.Time) <= (new TimeSpan(m_app.AppConfig.NewCacheInterval,0,0,0,0))))
			{
					dateLabel.Markup = "<span background='light green'>" + cache.Time.ToShortDateString () + "</span>";
			}
			else
			{
				dateLabel.Text = cache.Time.ToShortDateString ();
			}
			
			if (m_app.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_app.AppConfig.StaleCacheInterval,0,0,0,0)))
					infoDateLabel.Markup = "<span background='gold'>" + cache.Updated.ToShortDateString () + "</span>";
				else
					infoDateLabel.Text = cache.Updated.ToShortDateString ();
			}
			else
			{
				infoDateLabel.Text = cache.Updated.ToShortDateString ();
			}
			SetUserDataFields (cache);
			SetLastFoundByYou (cache);
			DateTime lastFound = m_Logs.GetLastFound();
			if (lastFound == DateTime.MinValue)
				lfoundLabel.Text = Catalog.GetString ("Unknown");
			else
				lfoundLabel.Text = lastFound.ToShortDateString ();
			DateTime lastFoundByYou = DateTime.MinValue;
			foreach (string ownerID in m_app.OwnerIDs) {
				lastFoundByYou = m_Logs.GetLastFoundBy(ownerID);
				if (lastFoundByYou != DateTime.MinValue)
					break;
			}
			DateTime lastDNF = DateTime.MinValue;
			foreach (string ownerID in m_app.OwnerIDs) {
				lastDNF = m_Logs.GetLastDNFBy(ownerID);
				if (lastDNF != DateTime.MinValue)
					break;
			}
			if (cache.Found && lastFoundByYou == DateTime.MinValue) {
				if (!cache.FTF)
					statusLabel.Markup = FOUND;
				else
					statusLabel.Markup = FTF;
			} else if (cache.Found) {
				if (!cache.FTF)
					statusLabel.Markup = String.Format (FOUND_DATE, lastFoundByYou.ToShortDateString ());
				else
					statusLabel.Markup = String.Format (FTF_DATE, lastFoundByYou.ToShortDateString ());
			} else if (cache.Archived)
				statusLabel.Markup = ARCHIVED; else if (!cache.Available)
				statusLabel.Markup = UNAVAILABLE; else if (cache.DNF && lastDNF == DateTime.MinValue)
				statusLabel.Markup = DNF; else if (cache.DNF)
				statusLabel.Markup = String.Format (DNF_DATE, lastDNF.ToShortDateString ()); else if (m_app.OwnerIDs.Contains (cache.OwnerID) || m_app.OwnerIDs.Contains (cache.CacheOwner))
				statusLabel.Markup = MINE; else if (cache.CheckNotes)
				statusLabel.Markup = CHECK_LOGS;
			else
				statusLabel.Markup = String.Empty;
			setCacheType (cache.TypeOfCache);
			placedByLabel.Text = cache.PlacedBy;
			cacheSizeLabel.Text = cache.Container;
			
			if (cache.State.Trim () != String.Empty) {
				countryLabel.Markup = String.Format (Catalog.GetString ("<b>Location: </b> {0},{1}"), cache.State, cache.Country);
			} else if (cache.Country.Trim () != String.Empty) {
				countryLabel.Markup = String.Format (Catalog.GetString ("<b>Location: </b> {0}"), cache.Country);
			} else {
				countryLabel.Text = String.Empty;
			}
			
			setCoordinate (cache);
			
			CacheAttribute[] attrs = CacheStore.GetAttributes (cache.Name);
			StringBuilder bldr = new StringBuilder ();
			Gtk.Table.TableChild props;
			foreach (Gtk.Widget child in attrTable.Children) {
				attrTable.Remove (child);
			}
			if (attrs.Length <= 0) {
				bldr.Append (Catalog.GetString ("None"));
				attrTable.Add (attrLabel);
				props = ((Gtk.Table.TableChild)(this.attrTable[attrLabel]));
				props.TopAttach = 0;
				props.LeftAttach = 0;
				props.RightAttach = 1;
				props.BottomAttach = 1;
				attrLabel.Markup = bldr.ToString ();
				attrLabel.Show ();
			} else {
				ShowAttrIcons (attrs, bldr);
			}
			
		}

		private void SetLastFoundByYou (Geocache cache)
		{
			DateTime lastDate = DateTime.MinValue;
			foreach (string ownerId in m_app.OwnerIDs) {
				lastDate = m_Logs.GetLastFoundBy(ownerId);
				if (lastDate != DateTime.MinValue)
					break;
			}
			if (lastDate == DateTime.MinValue)
				lastFoundDateLabel.Text = Catalog.GetString ("Never");
			else
				lastFoundDateLabel.Text = lastDate.ToShortDateString ();
		}

		private void SetUserDataFields (Geocache cache)
		{
			if (!String.IsNullOrEmpty (cache.User1))
				uData1.Text = cache.User1;
			else
				uData1.Text = Catalog.GetString ("None");
			if (!String.IsNullOrEmpty (cache.User2))
				uData2.Text = cache.User2;
			else
				uData2.Text = Catalog.GetString ("None");
			if (!String.IsNullOrEmpty (cache.User3))
				uData3.Text = cache.User3;
			else
				uData3.Text = Catalog.GetString ("None");
			if (!String.IsNullOrEmpty (cache.User4))
				uData4.Text = cache.User4;
			else
				uData4.Text = Catalog.GetString ("None");
		}

		private void SetInactive ()
		{
			this.Sensitive = false;
			cacheCodeLabel.Markup = START_BIG + Catalog.GetString ("NO CACHE SELECTED") + END_BIG;
			cacheNameLabel.Markup = String.Empty;
			setDifficulty (0);
			setTerrain (0);
			dateLabel.Text = String.Empty;
			infoDateLabel.Text = string.Empty;
			statusLabel.Markup = String.Empty;
			placedByLabel.Text = String.Empty;
			cacheSizeLabel.Text = String.Empty;
			coordinateLabel.Text = String.Empty;
			distance_label.Text = String.Empty;
			cacheTypeLabel.Text = String.Empty;
			countryLabel.Text = String.Empty;
			attrLabel.Markup = Catalog.GetString ("None");
			origCoord.Markup = String.Empty;
		}

		private void ShowAttrIcons (CacheAttribute[] attrs, StringBuilder bldr)
		{
			Gtk.Table.TableChild props;
			bool isFirst = true;
			uint colCount = 0;
			foreach (CacheAttribute attr in attrs) {
				Pixbuf buf;
				if (attr.Include)
					buf = IconManager.GetYAttrIcon (attr.AttrValue);
				else
					buf = IconManager.GetNAttrIcon (attr.AttrValue);
				if (buf != null) {
					Gtk.Image img = new Gtk.Image ();
					img.Pixbuf = buf;
					img.TooltipText = Catalog.GetString (attr.AttrValue);
					attrTable.Add (img);
					props = ((Gtk.Table.TableChild)(this.attrTable[img]));
					props.TopAttach = 0;
					props.LeftAttach = colCount;
					props.RightAttach = colCount + 1;
					props.BottomAttach = 1;
					props.XOptions = AttachOptions.Shrink;
					img.Show ();
					colCount++;
					continue;
				}
				
				if (isFirst)
					isFirst = false;
				else
					bldr.Append (", ");
				if (!attr.Include) {
					bldr.Append ("<span fgcolor='red' strikethrough='true'>");
					bldr.Append (attr.AttrValue);
					bldr.Append ("</span>");
				} else {
					bldr.Append (attr.AttrValue);
				}
			}
			Label filler = new Label ("");
			attrTable.Add (filler);
			props = ((Gtk.Table.TableChild)(this.attrTable[filler]));
			props.TopAttach = 0;
			props.LeftAttach = colCount;
			props.RightAttach = colCount + 1;
			props.BottomAttach = 1;
			props.XOptions = AttachOptions.Expand;
			filler.Show ();
			
			if (bldr.Length > 0) {
				attrTable.Add (attrLabel);
				props = ((Gtk.Table.TableChild)(this.attrTable[attrLabel]));
				props.TopAttach = 1;
				props.LeftAttach = 0;
				props.RightAttach = colCount + 1;
				props.BottomAttach = 2;
				attrLabel.Markup = bldr.ToString ();
				attrLabel.Show ();
			}
		}

		public void setDifficulty (double diff)
		{
			
			diff_i1.Pixbuf = ESTAR_ICON;
			diff_i2.Pixbuf = ESTAR_ICON;
			diff_i3.Pixbuf = ESTAR_ICON;
			diff_i4.Pixbuf = ESTAR_ICON;
			diff_i5.Pixbuf = ESTAR_ICON;
			
			if (diff > 0.5 && diff < 1)
				diff_i1.Pixbuf = HSTAR_ICON;
			if (diff >= 1)
				diff_i1.Pixbuf = STAR_ICON;
			if (diff >= 1.5 && diff < 2)
				diff_i2.Pixbuf = HSTAR_ICON;
			if (diff >= 2)
				diff_i2.Pixbuf = STAR_ICON;
			if (diff >= 2.5 && diff < 3)
				diff_i3.Pixbuf = HSTAR_ICON;
			if (diff >= 3)
				diff_i3.Pixbuf = STAR_ICON;
			if (diff >= 3.5 && diff < 4)
				diff_i4.Pixbuf = HSTAR_ICON;
			if (diff >= 4)
				diff_i4.Pixbuf = STAR_ICON;
			if (diff >= 4.5 && diff < 5)
				diff_i5.Pixbuf = HSTAR_ICON;
			if (diff >= 5)
				diff_i5.Pixbuf = STAR_ICON;
		}

		public void setTerrain (double diff)
		{
			
			terr_i1.Pixbuf = ESTAR_ICON;
			terr_i2.Pixbuf = ESTAR_ICON;
			terr_i3.Pixbuf = ESTAR_ICON;
			terr_i4.Pixbuf = ESTAR_ICON;
			terr_i5.Pixbuf = ESTAR_ICON;
			
			if (diff > 0.5 && diff < 1)
				terr_i1.Pixbuf = HSTAR_ICON;
			if (diff >= 1)
				terr_i1.Pixbuf = STAR_ICON;
			if (diff >= 1.5 && diff < 2)
				terr_i2.Pixbuf = HSTAR_ICON;
			if (diff >= 2)
				terr_i2.Pixbuf = STAR_ICON;
			if (diff >= 2.5 && diff < 3)
				terr_i3.Pixbuf = HSTAR_ICON;
			if (diff >= 3)
				terr_i3.Pixbuf = STAR_ICON;
			if (diff >= 3.5 && diff < 4)
				terr_i4.Pixbuf = HSTAR_ICON;
			if (diff >= 4)
				terr_i4.Pixbuf = STAR_ICON;
			if (diff >= 4.5 && diff < 5)
				terr_i5.Pixbuf = HSTAR_ICON;
			if (diff >= 5)
				terr_i5.Pixbuf = STAR_ICON;
		}

		public void setCacheIcon (Geocache.CacheType type)
		{
			Pixbuf icon = IconManager.GetLargeCacheIcon(type);
			cacheIcon.Pixbuf = icon;
			
		}

		public void setCoordinate (Geocache cache)
		{
			
			
			coordinateLabel.Markup = "<span font='bold 10'>" + Utilities.getCoordString (cache.Lat, cache.Lon) + "</span>";
			if (cache.HasCorrected)
				origCoord.Markup = Catalog.GetString ("<i>Original: ") + Utilities.getCoordString (cache.OrigLat, cache.OrigLon) + "</i>";
			else
				origCoord.Markup = String.Empty;
			;
			
			
			double distance = Utilities.calculateDistance (m_app.CentreLat, cache.Lat, m_app.CentreLon, cache.Lon);
			double bearing = Utilities.calculateBearing (m_app.CentreLat, cache.Lat, m_app.CentreLon, cache.Lon);
			
			string bmarker = Catalog.GetString ("N");
			if (bearing > 22.5 && bearing <= 67.5)
				bmarker = Catalog.GetString ("NE"); else if (bearing > 67.5 && bearing <= 112.5)
				bmarker = Catalog.GetString ("E"); else if (bearing > 112.5 && bearing <= 157.5)
				bmarker = Catalog.GetString ("SE"); else if (bearing > 157.5 && bearing <= 202.5)
				bmarker = Catalog.GetString ("S"); else if (bearing > 202.5 && bearing <= 247.5)
				bmarker = Catalog.GetString ("SW"); else if (bearing > 247.5 && bearing <= 292.5)
				bmarker = Catalog.GetString ("W"); else if (bearing > 292.5 && bearing <= 337.5)
				bmarker = Catalog.GetString ("NW");
			
			
			if (AppConfig.ImperialUnits) {
				distance = Utilities.KmToMiles (distance);
				distance_label.Markup = Catalog.GetString (String.Format (Catalog.GetString ("<span font='bold italic 10'>({0} miles {1} from {2})</span>"), distance.ToString ("0.00"), bmarker, m_app.CenterName));
			} else {
				distance_label.Markup = Catalog.GetString (String.Format (Catalog.GetString ("<span font='bold italic 10'>({0} km {1} from {2})</span>"), distance.ToString ("0.00"), bmarker, m_app.CenterName));
			}
		}

		private void setCacheType (Geocache.CacheType ctype)
		{
			switch (ctype) {
			case Geocache.CacheType.APE:
				cacheTypeLabel.Text = Catalog.GetString ("Project A.P.E");
				break;
			case Geocache.CacheType.CITO:
				cacheTypeLabel.Text = Catalog.GetString ("Cache In Trash Out Event");
				break;
			case Geocache.CacheType.EARTH:
				cacheTypeLabel.Text = Catalog.GetString ("Earth Cache");
				break;
			case Geocache.CacheType.EVENT:
				cacheTypeLabel.Text = Catalog.GetString ("Event Cache");
				break;
			case Geocache.CacheType.LETTERBOX:
				cacheTypeLabel.Text = Catalog.GetString ("Letterbox Hybrid");
				break;
			case Geocache.CacheType.MAZE:
				cacheTypeLabel.Text = Catalog.GetString ("Geo Adventures Maze");
				break;
			case Geocache.CacheType.MEGAEVENT:
				cacheTypeLabel.Text = Catalog.GetString ("Mega Event");
				break;
			case Geocache.CacheType.MULTI:
				cacheTypeLabel.Text = Catalog.GetString ("Multi Cache");
				break;
			case Geocache.CacheType.MYSTERY:
				cacheTypeLabel.Text = Catalog.GetString ("Unknown Cache");
				break;
			case Geocache.CacheType.OTHER:
				cacheTypeLabel.Text = Catalog.GetString ("Undefined Cache Type");
				break;
			case Geocache.CacheType.REVERSE:
				cacheTypeLabel.Text = Catalog.GetString ("Locationless Cache");
				break;
			case Geocache.CacheType.TRADITIONAL:
				cacheTypeLabel.Text = Catalog.GetString ("Traditional Cache");
				break;
			case Geocache.CacheType.VIRTUAL:
				cacheTypeLabel.Text = Catalog.GetString ("Virtual Cache");
				break;
			case Geocache.CacheType.WEBCAM:
				cacheTypeLabel.Text = Catalog.GetString ("Webcam Cache");
				break;
			case Geocache.CacheType.WHERIGO:
				cacheTypeLabel.Text = Catalog.GetString ("Wherigo Cache");
				break;
			case Geocache.CacheType.GENERIC:
				cacheTypeLabel.Text = Catalog.GetString ("Geocache");
				break;
			default:
				cacheTypeLabel.Text = "NOT_DEFINED";
				break;
			}
		}
		protected virtual void OnClickView (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start (m_cache.URL.ToString ());
		}

		protected virtual void OnClickLog (object sender, System.EventArgs e)
		{
			m_Win.LogFind();
		}
	}
}
