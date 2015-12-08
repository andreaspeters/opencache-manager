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

namespace ocmgtk
{


	public partial class GPSConfiguration : Gtk.Dialog
	{
		GarminUSBWidget gusbwidet = new GarminUSBWidget ();
		GenericGPSWidget gpswidget = new GenericGPSWidget();
		GarminSerialWidget garswidget = new GarminSerialWidget();
		DeLormeWidget delwidget = new DeLormeWidget();
		GarminEdgeWidget edgeWidget = new GarminEdgeWidget();
		DelormeGPXWidget delgpxwidget = new DelormeGPXWidget();
		GPXWidget gpxwidget = new GPXWidget();
		GPXWidget magWidget = new GPXWidget();

		public IGPSConfig GPSConfig {
			get
			{
				switch (deviceCombo.Active)
				{
					case 0:
						return gpxwidget;
					case 1:
						return gusbwidet;
					case 2:
						return garswidget;
					case 3:
						return edgeWidget;
					case 4:
						return delwidget;
					case 5:
						return delgpxwidget;
					case 6:
						return magWidget;
					default:
						return gpswidget;
				}
			}
		}
		
		public Dictionary<string,string> GPSMappings
		{
			get{ return waypointWidget.GetMappings();}
		}
		
		public string ProfileName
		{
			get { return profileEntry.Text;}
			set {profileEntry.Text = value;}
		}
		
		public GPSConfiguration()
		{
			this.Build ();	
			gpxwidget.SetCacheLimit(-1);
			gpxwidget.SetOutputFile("/media/GARMIN/Garmin/GPX/geocaches.gpx");
			gpxwidget.SetLogLimit(-1);
			gpxwidget.SetIncludeAttributes(false);
			gpxwidget.FieldNotesFile = "/media/GARMIN/Garmin/geocache_visits.txt";
			magWidget.SetCacheLimit(10000);
			magWidget.SetOutputFile("/media/Magellan/Geocaches/geocaches.gpx");
			magWidget.FieldNotesFile = "/media/Magellan/Geocaches/logs.txt";
			magWidget.SetLogLimit(-1);
			magWidget.SetIncludeAttributes(false);
			deviceCombo.Active = 0;
			waypointWidget.PopulateMappings(null);
			ShowDeviceConfig();
		}
		
		public GPSConfiguration(GPSProfile profile)
		{
			this.Build ();	
			profileEntry.Text = profile.Name;
			if ((profile.BabelFormat == "garmin") && (profile.OutputFile == "usb:"))
			{
				gusbwidet.SetCacheLimit(profile.CacheLimit);
				gusbwidet.SetNameMode(profile.NameMode);
				gusbwidet.SetDescMode(profile.DescMode);
				deviceCombo.Active = 1;
			}
			else if (profile.BabelFormat == "garmin")
			{
				garswidget.SetCacheLimit(profile.CacheLimit);
				garswidget.SetOutputFile(profile.OutputFile);
				garswidget.SetNameMode(profile.NameMode);
				deviceCombo.Active = 2;
			}
			else if (profile.BabelFormat== "OCM_GPX")
			{
				gpxwidget.SetCacheLimit(profile.CacheLimit);
				gpxwidget.SetOutputFile(profile.OutputFile);
				gpxwidget.SetLogLimit(profile.LogLimit);
				gpxwidget.SetIncludeAttributes(profile.IncludeAttributes);
				gpxwidget.FieldNotesFile = profile.FieldNotesFile;
				deviceCombo.Active = 0;
				ShowDeviceConfig();
			}
			else if (profile.BabelFormat.StartsWith("delbin"))
			{
				delwidget.SetCacheLimit(profile.CacheLimit);
				delwidget.SetLogLimit(profile.LogLimit);
				delwidget.SetIncludeAttributes(profile.IncludeAttributes);
				deviceCombo.Active = 4;
			}
			else if (profile.BabelFormat == "edge")
			{
				edgeWidget.SetCacheLimit(profile.CacheLimit);
				edgeWidget.SetOutputFile(profile.OutputFile);
				edgeWidget.SetDescMode(profile.DescMode);
				edgeWidget.SetNameMode(profile.NameMode);
				deviceCombo.Active = 3;
			}
			else if (profile.BabelFormat == "delgpx")
			{
				delgpxwidget.SetCacheLimit(profile.CacheLimit);
				delgpxwidget.SetOutputFile(profile.OutputFile);
				delgpxwidget.SetLogLimit(profile.LogLimit);
				delgpxwidget.SetIncludeAttributes(profile.IncludeAttributes);
				delgpxwidget.FieldNotesFile = profile.FieldNotesFile;
				deviceCombo.Active = 5;
			}
			else 
			{	
				gpswidget.SetCacheLimit(profile.CacheLimit);
				gpswidget.SetOutputFile(profile.OutputFile);
				gpswidget.SetBabelFormat(profile.BabelFormat);
				gpswidget.SetDescMode(profile.DescMode);
				gpswidget.SetNameMode(profile.NameMode);
				deviceCombo.Active = 6;
			}
			waypointWidget.PopulateMappings(profile.WaypointMappings);
			this.ShowAll();
		}

		protected virtual void OnButtonClick (object sender, System.EventArgs e)
		{
			this.Hide ();
		}
		
		
		protected virtual void OnComboChange (object sender, System.EventArgs e)
		{
			ShowDeviceConfig ();
		}
		
		private void ShowDeviceConfig ()
		{
			Gtk.Table.TableChild props;
			
			foreach (Gtk.Widget child in table1.Children)
			{
				if (child != deviceCombo && child != deviceLabel 
				    && child != profLabel && child !=profileEntry)
					table1.Remove(child);
			}
			
			switch (deviceCombo.Active)
			{
				case 0:
					table1.Add (gpxwidget);
					props = ((Gtk.Table.TableChild)(this.table1[gpxwidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					gpxwidget.Show ();
					waypointWidget.Sensitive = true;
					break;
				case 1:
					table1.Add (gusbwidet);
					props = ((Gtk.Table.TableChild)(this.table1[gusbwidet]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					gusbwidet.Show ();
					waypointWidget.Sensitive = true;
					break;
				case 2:
					table1.Add (garswidget);
					props = ((Gtk.Table.TableChild)(this.table1[garswidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					garswidget.Show ();
					waypointWidget.Sensitive = true;
					break;
				case 3:
					table1.Add (edgeWidget);
					props = ((Gtk.Table.TableChild)(this.table1[edgeWidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					edgeWidget.Show ();
					waypointWidget.Sensitive = true;
					break;
				case 4:
					table1.Add(delwidget);
					props = ((Gtk.Table.TableChild)(this.table1[delwidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					delwidget.Show ();
					waypointWidget.Sensitive = false;
					break;
				case 5:
					table1.Add(delgpxwidget);
					props = ((Gtk.Table.TableChild)(this.table1[delgpxwidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					delgpxwidget.Show ();
					waypointWidget.Sensitive = false;
					break;
				case 6:
					table1.Add(magWidget);
					props = ((Gtk.Table.TableChild)(this.table1[magWidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					magWidget.Show ();
					waypointWidget.Sensitive = false;
					break;
				default:
					table1.Add (gpswidget);
					props = ((Gtk.Table.TableChild)(this.table1[gpswidget]));
					props.TopAttach = 2;
					props.RightAttach = 2;
					props.BottomAttach = 3;
					gpswidget.Show ();
					waypointWidget.Sensitive = true;
					break;
			}
		}		
	}
}
