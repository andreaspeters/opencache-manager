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
using System.Collections.Generic;
using Gdk;
using ocmengine;

namespace ocmgtk
{


	public class IconManager
	{
		private static Dictionary<string, Pixbuf> imageCache = new Dictionary<string, Pixbuf>();
		private static Pixbuf TRADICON_S = new Pixbuf ("./icons/scalable/traditional.svg", 24, 24);
		private static Pixbuf LETTERICON_S = new Pixbuf ("./icons/scalable/letterbox.svg", 24, 24);
		private static Pixbuf MULTIICON_S = new Pixbuf ("./icons/scalable/multi.svg", 24, 24);
		private static Pixbuf MYSTERYICON_S = new Pixbuf ("./icons/scalable/unknown.svg", 24, 24);
		private static Pixbuf OTHERICON_S = new Pixbuf ("./icons/scalable/other.svg", 24, 24);
		public static Pixbuf FOUNDICON_S = new Pixbuf ("./icons/scalable/found.svg", 24, 24);
		private static Pixbuf EARTHICON_S = new Pixbuf ("./icons/scalable/earth.svg", 24, 24);
		private static Pixbuf CITOICON_S = new Pixbuf ("./icons/scalable/cito.svg", 24, 24);
		private static Pixbuf MEGAEVENT_S = new Pixbuf ("./icons/scalable/mega.svg", 24, 24);
		private static Pixbuf EVENT_S = new Pixbuf ("./icons/scalable/event.svg", 24, 24);
		private static Pixbuf WEBCAM_S = new Pixbuf ("./icons/scalable/webcam.svg", 24, 24);
		private static Pixbuf WHERIGO_S = new Pixbuf ("./icons/scalable/wherigo.svg", 24, 24);
		private static Pixbuf VIRTUAL_S = new Pixbuf ("./icons/scalable/virtual.svg", 24, 24);
		private static Pixbuf OWNED_S = new Pixbuf ("./icons/scalable/star.svg", 24, 24);
		private static Pixbuf GENERIC_S = new Pixbuf ("./icons/scalable/treasure.svg", 24, 24);
		private static Pixbuf PARKING_S = new Pixbuf ("./icons/scalable/parking.svg", 24, 24);
		private static Pixbuf TRAILHEAD_S = new Pixbuf ("./icons/scalable/trailhead.svg", 24, 24);
		private static Pixbuf GREENPIN_S = new Pixbuf ("./icons/scalable/greenpin.svg", 24, 24);
		private static Pixbuf BLUEPIN_S = new Pixbuf ("./icons/scalable/bluepin.svg", 24, 24);
		private static Pixbuf REDPIN_S = new Pixbuf ("./icons/scalable/pushpin.svg", 24, 24);
		public static Pixbuf CORRECTED_S = new Pixbuf ("./icons/scalable/corrected.svg", 24, 24);
		public static Pixbuf FTF_S = new Pixbuf ("./icons/scalable/ftf.svg", 24, 24);
		public static Pixbuf DNF_S = new Pixbuf ("./icons/scalable/dnf.svg", 24, 24);
		public static Pixbuf WRITENOTE_S = new Pixbuf ("./icons/scalable/write_note.svg", 24, 24);
		public static Pixbuf NEEDS_MAINT_S = new Pixbuf ("./icons/scalable/needs_maintenance.svg", 24, 24);
		
		
		private static Pixbuf TRADICON = new Pixbuf ("./icons/scalable/traditional.svg", 64, 64);
		private static Pixbuf LETTERICON = new Pixbuf ("./icons/scalable/letterbox.svg", 64, 64);
		private static Pixbuf MULTIICON = new Pixbuf ("./icons/scalable/multi.svg", 64, 64);
		private static Pixbuf MYSTERYICON = new Pixbuf ("./icons/scalable/unknown.svg", 64, 64);
		private static Pixbuf OTHERICON = new Pixbuf ("./icons/scalable/other.svg", 64, 64);
		private static Pixbuf EARTHICON = new Pixbuf ("./icons/scalable/earth.svg", 64, 64);
		private static Pixbuf CITOICON = new Pixbuf ("./icons/scalable/cito.svg", 64, 64);
		private static Pixbuf MEGAEVENT = new Pixbuf ("./icons/scalable/mega.svg", 64, 64);
		private static Pixbuf EVENT = new Pixbuf ("./icons/scalable/event.svg", 64, 64);
		private static Pixbuf WEBCAM = new Pixbuf ("./icons/scalable/webcam.svg", 64, 64);
		private static Pixbuf WHERIGO = new Pixbuf ("./icons/scalable/wherigo.svg", 64, 64);
		private static Pixbuf VIRTUAL = new Pixbuf ("./icons/scalable/virtual.svg", 64, 64);
		private static Pixbuf GENERIC = new Pixbuf ("./icons/scalable/treasure.svg", 64, 64);
		

		private static string TRAD_MI = "traditional.png";
		private static string CITO_MI = "cito.png";
		private static string EARH_MI = "earth.png";
		private static string LETTRBOX_MI = "letterbox.png";
		private static string EVENT_MI = "event.png";
		private static string MEGA_MI = "mega.png";
		private static string MULTI_MI = "multi.png";
		private static string OTHER_MI = "other.png";
		private static string OWNED_MI = "owned.png";
		private static string FOUND_MI = "found.png";
		private static string UNKNOWN_MI = "unknown.png";
		private static string VIRTUAL_MI = "virtual.png";
		private static string WEBCAM_MI = "webcam.png";
		private static string WHERIGO_MI = "wherigo.png";
		private static string GENERIC_MI = "treasure.png";
		private static string CORRECTED_MI = "corrected.png";
		private static string DNF_MI = "dnf.png";
		private static string FTF_MI = "ftf.png";
		
		public static Pixbuf GetLargeCacheIcon(Geocache.CacheType type)
		{
			
			switch (type) {
			case Geocache.CacheType.TRADITIONAL:
				return TRADICON;
			case Geocache.CacheType.MYSTERY:
				return MYSTERYICON;
			case Geocache.CacheType.MULTI:
				return MULTIICON;
			case Geocache.CacheType.LETTERBOX:
				return LETTERICON;
			case Geocache.CacheType.EARTH:
				return EARTHICON;
			case Geocache.CacheType.CITO:
				return CITOICON;
			case Geocache.CacheType.VIRTUAL:
				return VIRTUAL;
			case Geocache.CacheType.MEGAEVENT:
				return MEGAEVENT;
			case Geocache.CacheType.EVENT:
				return EVENT;
			case Geocache.CacheType.WEBCAM:
				return WEBCAM;
			case Geocache.CacheType.WHERIGO:
				return WHERIGO;
			case Geocache.CacheType.GENERIC:
				return GENERIC;
			default:
				return OTHERICON;
			}
		}
		
		
		/// <summary>
		/// Returns a PixBuf containing the 16x16 icon for the specified cache type
		/// </summary>
		/// <param name="type">
		/// Cache Type <see cref="Geocache.CacheType"/>
		/// </param>
		/// <returns>
		/// A pixbuf containing the icon <see cref="Pixbuf"/>
		/// </returns>
		public static Pixbuf GetSmallCacheIcon (Geocache.CacheType type)
		{
			switch (type) {
			case Geocache.CacheType.FOUND:
				return FOUNDICON_S;
			case Geocache.CacheType.TRADITIONAL:
				return TRADICON_S;
			case Geocache.CacheType.MYSTERY:
				return MYSTERYICON_S;
			case Geocache.CacheType.MULTI:
				return MULTIICON_S;
			case Geocache.CacheType.LETTERBOX:
				return LETTERICON_S;
			case Geocache.CacheType.EARTH:
				return EARTHICON_S;
			case Geocache.CacheType.CITO:
				return CITOICON_S;
			case Geocache.CacheType.VIRTUAL:
				return VIRTUAL_S;
			case Geocache.CacheType.MEGAEVENT:
				return MEGAEVENT_S;
			case Geocache.CacheType.EVENT:
				return EVENT_S;
			case Geocache.CacheType.WEBCAM:
				return WEBCAM_S;
			case Geocache.CacheType.WHERIGO:
				return WHERIGO_S;
			case Geocache.CacheType.MINE:
				return OWNED_S;
			case Geocache.CacheType.GENERIC:
				return GENERIC_S;
			default:
				return OTHERICON_S;
			}
		}
		
		public static Pixbuf GetSmallWaypointIcon (String symbol)
		{
			if (symbol.Equals ("Parking Area"))
				return PARKING_S; 
			else if (symbol.Equals ("Trailhead"))
				return TRAILHEAD_S; 
			else if (symbol.Equals ("Final Location"))
				return BLUEPIN_S;
			else if ((symbol.Equals("Other")) || symbol.Equals("Reference Point"))
				return GREENPIN_S;
			return REDPIN_S;
		}
		
		public static string GetStatusIcon(Geocache cache, OCMApp app, bool ignoreFound)
		{
			if (cache.Found && !ignoreFound)
			{
				if (cache.FTF)
					return FTF_MI;
				return FOUND_MI;
			}
			if ((app.OwnerIDs.Contains(cache.OwnerID)) ||(app.OwnerIDs.Contains(cache.CacheOwner)))
				return OWNED_MI;
			if ((cache.HasCorrected || cache.HasFinal))
			{
				if (app.AppConfig.SolvedModeState == SolvedMode.ALL)
					return CORRECTED_MI;
				else if (app.AppConfig.SolvedModeState == SolvedMode.PUZZLES &&
				         cache.TypeOfCache == Geocache.CacheType.MYSTERY)
					return CORRECTED_MI;
			}
			else if (cache.DNF && app.AppConfig.ShowDNFIcon)
			{
				return DNF_MI;
			}
			return null;
		}

		/// <summary>
		/// Gets the icon name for the corresponding cache type
		/// </summary>
		/// <param name="type">
		/// A geocache type <see cref="Geocache.CacheType"/>
		/// </param>
		/// <returns>
		/// Icon file name <see cref="System.String"/>
		/// </returns>
		public static string GetMapIcon (Geocache cache)
		{
			switch (cache.TypeOfCache) {
				case Geocache.CacheType.TRADITIONAL:
					return TRAD_MI;
				case Geocache.CacheType.MYSTERY:
					return UNKNOWN_MI;
				case Geocache.CacheType.MULTI:
					return MULTI_MI;
				case Geocache.CacheType.LETTERBOX:
					return LETTRBOX_MI;
				case Geocache.CacheType.EARTH:
					return EARH_MI;
				case Geocache.CacheType.CITO:
					return CITO_MI;
				case Geocache.CacheType.VIRTUAL:
					return VIRTUAL_MI;
				case Geocache.CacheType.MEGAEVENT:
					return MEGA_MI;
				case Geocache.CacheType.EVENT:
					return EVENT_MI;
				case Geocache.CacheType.WEBCAM:
					return WEBCAM_MI;
				case Geocache.CacheType.WHERIGO:
					return WHERIGO_MI;
				case Geocache.CacheType.GENERIC:
					return GENERIC_MI;
				default:
					return OTHER_MI;
			}
		}

		/// <summary>
		/// Returns the map icon for a given waypoint symbol
		/// </summary>
		/// <param name="symbol">
		/// Waypoint symbol name<see cref="String"/>
		/// </param>
		/// <returns>
		/// Icon file path <see cref="System.String"/>
		/// </returns>
		public static string GetMapIcon (String symbol)
		{
			if (symbol.Equals ("Parking Area"))
				return "parking.png"; 
			else if (symbol.Equals ("Trailhead"))
				return "trailhead.png"; 
			else if (symbol.Equals ("Final Location"))
				return "bluepin.png";
			else if ((symbol.Equals("Other")) || symbol.Equals("Reference Point"))
				return "greenpin.png";
			return "pushpin.png";
		}
		
		public static Pixbuf GetYAttrIcon(String attrname)
		{
			try
			{
				String val = attrname.Replace(' ', '_');
				val = val.Replace('/','_');
				string iconName = "./icons/scalable/attributes/yes_" +  val + ".svg";
				if (!imageCache.ContainsKey(iconName))
					imageCache[iconName] = new Pixbuf(iconName, 32,32);
				return imageCache[iconName];				
			}
			catch 
			{
				return null;
			}
		}
		
		public static Pixbuf GetNAttrIcon(String attrname)
		{
			try
			{
				String val = attrname.Replace(' ', '_');
				val = val.Replace('/','_');
				string iconName = "./icons/scalable/attributes/no_" + val + ".svg";
				if (!imageCache.ContainsKey(iconName))
					imageCache[iconName] = new Pixbuf(iconName, 32,32);
				return imageCache[iconName];
			}
			catch 
			{
				return null;
			}
		}
		
		public static Pixbuf GetDisAttrIcon(String attrname)
		{
			try
			{
				String val = attrname.Replace(' ', '_');
				val = val.Replace('/','_');
				string iconName = "./icons/scalable/attributes/dis_" + val + ".svg";
				if (!imageCache.ContainsKey(iconName))
					imageCache[iconName] = new Pixbuf(iconName, 32,32);
				return imageCache[iconName];
			}
			catch 
			{
				return null;
			}
		}
	}
}
