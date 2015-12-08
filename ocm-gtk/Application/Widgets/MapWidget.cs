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
using System.Globalization;
using System.Text;
using ocmengine;
using Mono.Unix;
using Gdk;
using WebKit;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class MapWidget : Gtk.Bin
	{
		WebView m_View;
		List<string> m_PendingActions;
		Dictionary<string, Geocache> m_UnfilteredCaches;
		List<Waypoint> m_ChildWaypoints;
		OCMApp m_App;
		OCMMainWindow m_Win;
		bool m_Loaded;
		bool m_ShowNearby = false;
		bool m_ShowAllChildren = false;
		double m_Lat;
		double m_Lon;
		Geocache m_Selected;
		
		public OCMApp App
		{
			set {
				m_App = value;
				m_ShowAllChildren = m_App.AppConfig.ShowAllChildren;
				m_ShowNearby = m_App.AppConfig.ShowNearby;
				m_Lat = m_App.CentreLat;
				m_Lon = m_App.CentreLon;
			}
		}
		
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}
		
		public double MapLat
		{
			get { return m_Lat;}
		}
		
		public double MapLon
		{
			get { return m_Lon;}
		}
		
		private IConfig AppConfig
		{
			get { return m_App.AppConfig; }
		}
		
		public bool ShowNearby
		{
			get { return m_ShowNearby;}
			set { m_ShowNearby = value;}
		}
		
		public bool ShowAllChildren
		{
			get { return m_ShowAllChildren;}
			set { m_ShowAllChildren = value;}
		}
		
		private bool m_IgnoreFinds = false;
		public bool HideFindSymbol
		{
			set { m_IgnoreFinds = value;}
		}
		
		public MapWidget ()
		{
			this.Build ();
			m_Loaded = false;
			m_View = new WebView();
			m_View.WidthRequest = 100;
			m_View.HeightRequest = 100;
			m_UnfilteredCaches = new Dictionary<string, Geocache>();
			m_ChildWaypoints = new List<Waypoint>();
			mapScroll.AddWithViewport(m_View);
			m_PendingActions = new List<string>();
			m_View.LoadCommitted += HandleM_ViewLoadCommitted;
			m_View.LoadStarted += HandleViewLoadStarted;
			m_View.LoadFinished += HandleViewLoadFinished;
			m_View.LoadProgressChanged += HandleViewLoadProgressChanged;
			m_View.NavigationRequested += HandleM_ViewNavigationRequested;;
		}

		void HandleM_ViewLoadCommitted (object o, LoadCommittedArgs args)
		{
			mapProgress.Fraction = 0.10;
			mapProgress.Visible = true;
			OCMApp.UpdateGUIThread();
		}

		void HandleM_ViewNavigationRequested (object o, NavigationRequestedArgs args)
		{
			if (args.Request.Uri.StartsWith("ocm://"))
			{
				string[]  request = args.Request.Uri.Substring(6).Split('/');
				if (request[0].Equals("select"))
				{
					HighlightPoint(m_Lat, m_Lon);
					m_App.SelectCacheByName(request[1]);
				}
				else if (request[0].Equals("mapmoved"))
				{
					m_Lat = double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture);
					m_Lon = double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture);
					Refresh(m_Lat, m_Lon);
				}
				else if (request[0].Equals("setcentre"))
				{
					m_Win.SetLocation(Catalog.GetString("Map Point"), double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture));
				}
				else if (request[0].Equals("sethome"))
				{
					m_App.AppConfig.HomeLat = double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture);
					m_App.AppConfig.HomeLon =  double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture);
					m_Win.ResetToHome();
				}
				else if (request[0].Equals("addlocation"))
				{
					m_Win.AddLocation(double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture));
				}
				else if (request[0].Equals("addwaypoint"))
				{
					if (m_Win.CacheList.SelectedCache != null)
						m_App.AddChildWaypoint(double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture));
				}
				else if (request[0].Equals("correctcoordinate"))
				{
					if (m_Win.CacheList.SelectedCache != null)
						m_Win.CorrectCoordinates(double.Parse(request[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(request[2], System.Globalization.CultureInfo.InvariantCulture));					
				}
				args.Frame.StopLoading();
			}

		}

		void HandleViewLoadProgressChanged (object o, LoadProgressChangedArgs args)
		{
			mapProgress.Fraction = ((double) args.Progress/ 100d);
			OCMApp.UpdateGUIThread();
		}

		void HandleViewLoadFinished (object o, LoadFinishedArgs args)
		{
			m_Loaded = true;
			mapProgress.Visible = false;
			foreach(string script in m_PendingActions)
			{
				LoadScript(script);
			}
			m_PendingActions.Clear();
		}

		void HandleViewLoadStarted (object o, LoadStartedArgs args)
		{
			mapProgress.Fraction = 0.15;
			mapProgress.Visible = true;
			OCMApp.UpdateGUIThread();
		}
		
		public void Reload()
		{
			m_Loaded = false;
			m_View.LoadUri("file://" + System.Environment.CurrentDirectory + "/web/wpt_viewer.html?lat=" 
			               + m_App.AppConfig.LastLat.ToString (CultureInfo.InvariantCulture) 
			               + "&lon=" + m_App.AppConfig.LastLon.ToString (CultureInfo.InvariantCulture));
			AddMaps(AppConfig.OpenLayerMaps);
			LoadScript("setAutoSelectCache('" + AppConfig.AutoSelectCacheFromMap + "');");
		}
		
		public void ClearCaches()
		{
			m_UnfilteredCaches.Clear();
			ClearAllMarkers();
		}
		
		public void SetPopups(bool enabled)
		{
			if (enabled)
				LoadScript("togglePopups(true)");
			else
				LoadScript("togglePopups(false)");
		}
		
		public void AddGeocache(Geocache cache)
		{
			m_UnfilteredCaches.Add(cache.Name, cache);
		}
		
		public void SetInitalCaches(List<Geocache> caches)
		{
			foreach(Geocache cache in caches)
			{
				m_UnfilteredCaches.Add(cache.Name, cache);
			}
		}
		
		public void PanTo(double lat, double lon)
		{
			m_Lat = lat;
			m_Lon = lon;
			LoadScript("panTo(" + lat.ToString(CultureInfo.InvariantCulture) 
			           + "," + lon.ToString(CultureInfo.InvariantCulture) + ")");
		}
		
		public void ZoomTo(double lat, double lon)
		{
			m_Lat = lat;
			m_Lon = lon;
			LoadScript("zoomTo(" + lat.ToString(CultureInfo.InvariantCulture) 
			           + "," + lon.ToString(CultureInfo.InvariantCulture) + ")");
		}
		
		public void UpdateChildWaypoints()
		{
			List<string> parents = new List<string>();
			m_ChildWaypoints.Clear();
			m_ChildWaypoints = null;
			foreach(Geocache cache in m_UnfilteredCaches.Values)
			{
				parents.Add(cache.Name);
			}
			m_ChildWaypoints = m_App.CacheStore.GetChildWaypoints(parents.ToArray());
		}
		
		public void SetCache(Geocache cache)
		{
			ClearActiveMarkers();
			if (cache == null)
			{
				Refresh(m_Lat, m_Lon);
				return;
			}	
			AddMarker(cache, false);
			HighlightPoint(cache.Lat, cache.Lon);
			
			List<Waypoint> children = m_App.CacheStore.GetChildWaypoints(new string[]{cache.Name});
			foreach(Waypoint child in children)
			{
				AddChildWaypointMarker(child, false);
			}
			
		}
		
		public void AddMarker(Geocache cache, bool isExtra)
		{
			string iconModifier = String.Empty;
			string titleMode = String.Empty;
			
					
			if (m_App.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_App.AppConfig.StaleCacheInterval,0,0,0,0)))
					iconModifier = "stale-";
			}
			if (m_App.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_App.AppConfig.NewCacheInterval,0,0,0,0)))	
					iconModifier = "new-";
			}
			if (cache.Archived) 
			{
					if (iconModifier != "stale-")
						iconModifier = "archived-";
					titleMode = "archived";
			} 
			else if (!cache.Available) 
			{
					if (iconModifier != "stale-")
						iconModifier = "disabled-";
					titleMode = "disabled";
			}
			else if (cache.CheckNotes)
			{
				titleMode = "checknotes";
				if (iconModifier == String.Empty && m_App.AppConfig.ShowRecentDNF)
					iconModifier = "clogs-";
			}
			
			StringBuilder builder = new StringBuilder();
			builder.Append("<div style=font-size:10pt;>" );
			builder.Append(Catalog.GetString ("<b>A cache by:</b> "));
			builder.Append(cache.PlacedBy);
			builder.Append(Catalog.GetString (" <b>Hidden on: </b>"));
			builder.Append(cache.Time.ToShortDateString ());
			builder.Append(Catalog.GetString("<br><b>Difficulty: </b>"));
			builder.Append(cache.Difficulty);
			builder.Append(Catalog.GetString(" <b>Terrain: </b>"));
			builder.Append(cache.Terrain);
			builder.Append(Catalog.GetString("<br><b>Cache size: </b>"));
			builder.Append(cache.Container);
		/*	builder.Append (Catalog.GetString(" <b>Last Found</b>"));
			DateTime lastFound = m_App.CacheStore.GetLastFound(cache.Name);
			if (lastFound != DateTime.MinValue)
				builder.Append (lastFound.ToShortDateString());
			else
				builder.Append (Catalog.GetString("Never"));*/
			builder.Append("</div>");
			GenerateMarker(cache.Lat, cache.Lon, iconModifier, cache, builder.ToString(), titleMode, isExtra);
		}
		
		private void ClearAllMarkers()
		{
			LoadScript("clearAllMarkers()");
		}
		
		private void ClearActiveMarkers()
		{
			LoadScript("clearActiveMarkers()");
		}
		
		private void ClearExtraMarkers()
		{
			LoadScript("clearExtraMarkers()");
		}
		
		public void HighlightPoint(double lat, double lon)
		{
			LoadScript ("zoomToPoint(" + lat.ToString (CultureInfo.InvariantCulture) + "," + lon.ToString (CultureInfo.InvariantCulture) + ")");
		}
		
		private void GenerateMarker (double lat, double lon, string iconModifier, Geocache cache, string cachedesc, string mode, bool isExtra)
		{
			StringBuilder addMarkerReq = new StringBuilder ();
			if (isExtra)
				addMarkerReq.Append ("addExtraMarker(");
			else
				addMarkerReq.Append ("addMarker(");
			addMarkerReq.Append (lat.ToString (CultureInfo.InvariantCulture));
			addMarkerReq.Append (",");
			addMarkerReq.Append (lon.ToString (CultureInfo.InvariantCulture));
			addMarkerReq.Append (",'../icons/24x24/");
			addMarkerReq.Append (iconModifier);
			string statIcon = IconManager.GetStatusIcon(cache, m_App, m_IgnoreFinds);
			if (null != statIcon)
				addMarkerReq.Append(statIcon);
			else
				addMarkerReq.Append (IconManager.GetMapIcon (cache));
			addMarkerReq.Append ("',\"");
			addMarkerReq.Append (cache.Name);
			addMarkerReq.Append ("\",\"");
			addMarkerReq.Append (cache.CacheName.Replace ("\"", "'"));
			addMarkerReq.Append ("\",\"");
			addMarkerReq.Append (cachedesc.Replace ("\"", "''"));
			addMarkerReq.Append ("\",\"");
			addMarkerReq.Append (mode);
			addMarkerReq.Append ("\"");
			if (AppConfig.ShowDiffTerrIcon) {
				addMarkerReq.Append (",");
				addMarkerReq.Append (cache.Difficulty.ToString (CultureInfo.InvariantCulture));
				addMarkerReq.Append (",");
				addMarkerReq.Append (cache.Terrain.ToString (CultureInfo.InvariantCulture));
			}
			addMarkerReq.Append (")");
			LoadScript (addMarkerReq.ToString ());
		}
		
		private void LoadScript(string script)
		{
			if (!m_Loaded)
				m_PendingActions.Add(script);
			else
				m_View.ExecuteScript(script);
		}
		
		private void AddMaps(List<MapDescription> maps) 
		{
			foreach (MapDescription map in maps)
			{
				if (map.Active) 
					AddMap(map.Code);
			}
		}
			
		private void AddMap(string codeForMap) {
			LoadScript("addMapRenderer(" + codeForMap + "); ");
		}
		
		public void Refresh()
		{
			Refresh(m_Lat, m_Lon);
		}
		
		public void Refresh(double lat, double lon)
		{
			ClearExtraMarkers();
			m_Lat = lat;
			m_Lon = lon;
			
			if (m_ShowNearby)
			{
				List<Geocache> sortList= new List<Geocache>(m_UnfilteredCaches.Values);
				sortList.Sort(new CacheDistanceSorter(lat, lon));
				int iCount = 0;
				foreach(Geocache cache in sortList)
				{
					if (cache == m_Selected)
						continue;
					AddMarker(cache, true);
					iCount++;
					if (iCount == AppConfig.MapPoints)
						break;
				}
				if (m_ShowAllChildren)
				{
					m_ChildWaypoints.Sort(new CacheDistanceSorter(lat,lon));
					iCount = 0;
					foreach(Waypoint pt in m_ChildWaypoints)
					{
						if (m_Selected != null && pt.Parent == m_Selected.Name)
							continue;
						AddChildWaypointMarker (pt, true);
						iCount++;
						if (iCount == AppConfig.MapPoints)
							return;
					}
				}
			}
			
			

		}
		
		private void AddChildWaypointMarker (Waypoint pt, bool isExtra)
		{
			if (!m_UnfilteredCaches.ContainsKey(pt.Parent))
				return;
			Geocache cache = m_UnfilteredCaches[pt.Parent];
			string desc = pt.Desc.Replace ("\"", "''");
			desc = desc.Replace ("\n", "<br/>");
			string iconModifier = String.Empty;
			if (m_App.AppConfig.ShowStaleCaches)
			{
				if ((DateTime.Now - cache.Updated) > (new TimeSpan(m_App.AppConfig.StaleCacheInterval,0,0,0,0)))
					iconModifier = "stale-";
			}
			if (m_App.AppConfig.ShowNewCaches)
			{
				if ((DateTime.Now - cache.Time) <= (new TimeSpan(m_App.AppConfig.NewCacheInterval,0,0,0,0)))
					iconModifier = "new-";
			}
			if (cache.Archived) 
			{
					iconModifier = "archived-";
			} 
			else if (!cache.Available) 
			{
					iconModifier = "disabled-";
			}
			if (isExtra)
				LoadScript ("addExtraMarker(" + pt.Lat.ToString (CultureInfo.InvariantCulture) + "," + pt.Lon.ToString (CultureInfo.InvariantCulture) + ",'../icons/24x24/" + iconModifier + IconManager.GetMapIcon (pt.Symbol) + "',\"" + cache.Name + "\",\"" + cache.CacheName.Replace ("\"", "'") + "\",\"" + pt.Name + "-" + desc + "\")");
			else
				LoadScript ("addMarker(" + pt.Lat.ToString (CultureInfo.InvariantCulture) + "," + pt.Lon.ToString (CultureInfo.InvariantCulture) + ",'../icons/24x24/" + iconModifier + IconManager.GetMapIcon (pt.Symbol) + "',\"" + cache.Name + "\",\"" + cache.CacheName.Replace ("\"", "'") + "\",\"" + pt.Name + "-" + desc + "\")");

		}
		
		public class CacheDistanceSorter : IComparer<Waypoint>, IComparer<Geocache>
		{

			double orig_lat, orig_lon;

			public CacheDistanceSorter (double lat, double lon)
			{
				orig_lat = lat;
				orig_lon = lon;
			}
			
			public int Compare (Geocache obj1, Geocache obj2)
			{
				return Compare(obj1 as Waypoint, obj2 as Waypoint);
			}

			public int Compare (Waypoint obj1, Waypoint obj2)
			{
				double d1 = Utilities.calculateDistance (orig_lat, obj1.Lat, orig_lon, obj1.Lon);
				double d2 = Utilities.calculateDistance (orig_lat, obj2.Lat, orig_lon, obj2.Lon);
				if (d2 > d1)
					return -1; else if (d2 == d1)
					return 0;
				else
					return 1;
			}
		}

	}
}
