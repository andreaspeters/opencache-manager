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
using ocmengine;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class OCMQueryPage3 : Gtk.Bin
	{
		OCMMainWindow m_Win;
		
		protected virtual void OnPlacedByToggle (object sender, System.EventArgs e)
		{
			placedEntry.Sensitive = placedByRadio.Active;
		}
		
		public String PlacedBy
		{
			get { 
				if (placedByRadio.Active)
					return placedEntry.Text;
				return null;
			}
			set
			{
				if (value == null)
					return;
				placedEntry.Text = value;
				placedByRadio.Active = true;
			}
		}
		
		public DateTime PlaceBefore
		{
			get
			{
				if (hiddenCheck.Active && hiddenCombo.Active == 0)
					return hiddenDateEntry.Date;
				return DateTime.MinValue;				
			}
			set
			{
				if (value == DateTime.MinValue)
					return;
				hiddenCheck.Active = true;
				hiddenCombo.Active = 0;
				hiddenDateEntry.Date = value;
			}
		}
		
		public DateTime PlaceAfter
		{
			get
			{
				if (hiddenCheck.Active && hiddenCombo.Active == 1)
					return hiddenDateEntry.Date;
				return DateTime.MinValue;			
			}
			set
			{
				if (value == DateTime.MinValue)
					return;
				hiddenCheck.Active = true;
				hiddenCombo.Active = 1;
				hiddenDateEntry.Date = value;
			}
		}
		
		
		
	
		public string Country
		{
			get { 
				if (countryCheck.Active)
					return countryEntry.Text;
				else
					return null;
				
			}
			set { 
				if (value != null)
				{
					countryEntry.Text = value;
					countryCheck.Active = true;
				}
				else
				{
					countryCheck.Active = false;
				}
			}
		}
		
		public string Province
		{
			get { 
				if (stateCheck.Active)
					return stateEntry.Text;
				else
					return null;
			}
			set { 
				if (value != null)
				{
					stateEntry.Text = value;
					stateCheck.Active = true;
				}
				else
				{
					stateCheck.Active = false;
				}
			}
		}
		
		public double Distance
		{
			get {
				if (distCheck.Active)
				{
					double dist = double.Parse(distEntry.Text);
					if (m_Win.App.AppConfig.ImperialUnits)
						dist = Utilities.MilesToKm(dist);
					return dist;
				}
				return -1;
			}
			set
			{
				if (value > 0)
				{
					distCheck.Active = true;
					distEntry.Text = value.ToString();
				}
			}
		}
		
		public string DistOp
		{
			get {
				switch (distCombo.Active)
				{
					case 0:
						return "<=";
					case 1:
						return ">=";
					default:
						return "==";
				}
			}
			set 
			{
				if (value == "<=")
					distCombo.Active = 0;
				else if (value == ">=")
					distCombo.Active = 1;
				else
					distCombo.Active = 2;
			}
		}
		
		public double DistLat
		{
			get
			{
				if (!distCheck.Active)
					return -1;
				if (locRadio.Active)
				{
					if (locationCombo.Active == 0)
						return m_Win.App.AppConfig.HomeLat;
					return m_locations.GetLocation(locationCombo.ActiveText).Latitude;
				}
				if (posRadio.Active)
				{
					return posLocation.Latitude;
				}
				return -1;
			}
			set
			{
				if (value != 0)
				{
					posRadio.Active = true;
					posLocation.Latitude = value;
				}
			}
		}
		
		public double DistLon
		{
			get
			{
				if (!distCheck.Active)
					return -1;
				if (locRadio.Active)
				{
					if (locationCombo.Active == 0)
						return m_Win.App.AppConfig.HomeLon;
					return m_locations.GetLocation(locationCombo.ActiveText).Longitude;
				}
				if (posRadio.Active)
				{
					return posLocation.Longitude;
				}
				return -1;
			}
			set
			{
				if (value != 0)
				{
					posRadio.Active = true;
					posLocation.Longitude = value;
				}
			}
		}
		
		LocationList m_locations;
		public OCMQueryPage3 ()
		{
			this.Build ();
		}
		
		public OCMMainWindow Mainwin
		{
			set
			{
				locationCombo.AppendText(Catalog.GetString("Home"));
				m_Win = value;
				if (m_Win.App.AppConfig.UseDirectEntryMode)
					posLocation.SetDirectMode();
				m_locations = m_Win.App.Locations;
				foreach (Location loc in m_locations.Locations)
				{
					locationCombo.AppendText(loc.Name);
				}
				locationCombo.Active = 0;
				if (m_Win.App.AppConfig.ImperialUnits)
				{
					distMeasureLabel.Text = Catalog.GetString("Mi");
				}
			}
		}
		
		protected virtual void OnCountryToggle (object sender, System.EventArgs e)
		{
			countryEntry.Sensitive = countryCheck.Active;
		}
			
		protected virtual void OnStateCheckToggle (object sender, System.EventArgs e)
		{
			stateEntry.Sensitive = stateCheck.Active;
		}
		
		protected virtual void OnHiddenToggle (object sender, System.EventArgs e)
		{
			hiddenCombo.Sensitive = hiddenCheck.Active;
			hiddenDateEntry.Sensitive = hiddenCheck.Active;
		}
		
		protected virtual void OnLocationToggle (object sender, System.EventArgs e)
		{
			locationCombo.Sensitive = locRadio.Active;
		}
		
		protected virtual void OnPositionToggle (object sender, System.EventArgs e)
		{
			posLocation.Sensitive = posRadio.Active;
		}
		
		protected virtual void OnDistanceToggle (object sender, System.EventArgs e)
		{
			distFrame.Sensitive = distCheck.Active;
		}
		
		
		
		
		
		
	}
}
