// 
//  Copyright 2011  Kyle Campbell
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
using Gtk;
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class LocationWidget : Gtk.Bin
	{
		Gtk.Entry m_DirectEntry = new Gtk.Entry();
		Gtk.Label m_DirectLabel = new Gtk.Label(Catalog.GetString("Coordinate:"));
		
		public delegate void ChangedEventHandler(object sender, EventArgs args);
		public event ChangedEventHandler Changed;
		
		bool m_IsDirect = false;
		
		public double Latitude
		{
			get 
			{ 
				if (m_IsDirect)
				{
					return Utilities.ParseCoordString(m_DirectEntry.Text)[0].GetDecimalDegrees();
				}
				return latWidget.getCoordinate();
			}
			set { 
				latWidget.SetCoordinate(value, true);
				if (m_IsDirect)
					m_DirectEntry.Text = Utilities.getCoordStringCN(latWidget.getCoordinate(), lonWidget.getCoordinate());
			}
		}
		
		public double Longitude
		{
			get 
			{ 
				if (m_IsDirect)
					return Utilities.ParseCoordString(m_DirectEntry.Text)[1].GetDecimalDegrees();
				return lonWidget.getCoordinate();
			}
			set { 
				lonWidget.SetCoordinate(value, false);
				if (m_IsDirect)
					m_DirectEntry.Text = Utilities.getCoordStringCN(latWidget.getCoordinate(), lonWidget.getCoordinate());

			}
		}
		
		public bool IsValid
		{
			get
			{ 
				if (m_IsDirect)
				{
					try
					{
						Utilities.ParseCoordString(m_DirectEntry.Text)[0].GetDecimalDegrees();
					}
					catch (Exception e)
					{
						OCMApp.ShowException(e);
						return false;
					}
					return true;
				}
				if (lonWidget.ValidateEntry() && latWidget.ValidateEntry())
				{
					return true;
				}
				return false;
			}
		}
		
		protected virtual void OnEditClicked (object sender, System.EventArgs e)
		{
			if (!m_IsDirect)
				SetDirectMode ();
			else
				SetHelperMode();
		}
		
		private void SetHelperMode ()
		{
			latWidget.SetCoordinate(Utilities.ParseCoordString(m_DirectEntry.Text)[0].GetDecimalDegrees(), true);
			lonWidget.SetCoordinate(Utilities.ParseCoordString(m_DirectEntry.Text)[1].GetDecimalDegrees(), false);
			
			Gtk.Table.TableChild props;
			widgetTable.Remove(m_DirectLabel);
			widgetTable.Remove(m_DirectEntry);
			widgetTable.Add(latWidget);
			widgetTable.Add(lonWidget);
			widgetTable.Add(lonLabel);
			widgetTable.Add(latLabel);
			props = ((Gtk.Table.TableChild)(this.widgetTable[latLabel]));
			props.TopAttach = 0;
			props.LeftAttach = 0;
			props.RightAttach = 1;
			props.BottomAttach = 1;	
			props.XOptions = AttachOptions.Fill;
			latLabel.Show();
			props = ((Gtk.Table.TableChild)(this.widgetTable[latWidget]));
			props.TopAttach = 0;
			props.LeftAttach = 1;
			props.RightAttach = 2;
			props.BottomAttach = 1;	
			props.XOptions = AttachOptions.Fill;
			latWidget.Show();
			props = ((Gtk.Table.TableChild)(this.widgetTable[lonLabel]));
			props.TopAttach = 1;
			props.LeftAttach = 0;
			props.RightAttach = 1;
			props.BottomAttach = 2;	
			props.XOptions = AttachOptions.Fill;
			lonLabel.Show();
			props = ((Gtk.Table.TableChild)(this.widgetTable[lonWidget]));
			props.TopAttach = 1;
			props.LeftAttach = 1;
			props.RightAttach = 2;
			props.BottomAttach = 2;	
			props.XOptions = AttachOptions.Fill;
			lonWidget.Show();
			m_IsDirect = false;
		}
		
		public void SetDirectMode ()
		{
			Gtk.Table.TableChild props;
			widgetTable.Remove(latLabel);
			widgetTable.Remove(lonLabel);
			widgetTable.Remove(latWidget);
			widgetTable.Remove(lonWidget);
			widgetTable.Add(m_DirectEntry);
			widgetTable.Add(m_DirectLabel);
			props = ((Gtk.Table.TableChild)(this.widgetTable[m_DirectLabel]));
			props.TopAttach = 0;
			props.LeftAttach = 0;
			props.RightAttach = 1;
			props.BottomAttach = 1;	
			props.XOptions = AttachOptions.Shrink;
			m_DirectLabel.Show();
			props = ((Gtk.Table.TableChild)(this.widgetTable[m_DirectEntry]));
			props.TopAttach = 0;
			props.LeftAttach = 1;
			props.RightAttach = 2;
			props.BottomAttach = 1;	
			props.XOptions = AttachOptions.Shrink;
			m_DirectEntry.Text = Utilities.getCoordStringCN(latWidget.getCoordinate(), lonWidget.getCoordinate());
			m_DirectEntry.TooltipText = Catalog.GetString("Coordinates must be typed in using English formatting");
			m_DirectEntry.Show();
			m_IsDirect = true;
		}
		
		
		public LocationWidget ()
		{
			this.Build ();
			m_DirectEntry.WidthChars = 40;
			latWidget.Changed += HandleLatWidgetChanged;
			lonWidget.Changed += HandleLonWidgetChanged;
			m_DirectEntry.Changed += HandleM_DirectEntryChanged;
		}

		void HandleM_DirectEntryChanged (object sender, EventArgs e)
		{
			if (m_IsDirect && Changed != null)
				Changed(this, new EventArgs());
		}

		void HandleLonWidgetChanged (object sender, EventArgs args)
		{
			if (Changed != null)
				Changed(this, new EventArgs());
		}

		void HandleLatWidgetChanged (object sender, EventArgs args)
		{
			if (Changed != null)
				Changed(this, new EventArgs());
		}
	}
}
