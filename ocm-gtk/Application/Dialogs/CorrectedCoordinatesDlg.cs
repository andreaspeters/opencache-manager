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

namespace ocmgtk
{


	public partial class CorrectedCoordinatesDlg : Gtk.Dialog
	{
		private Geocache m_cache;
		
		public double CorrectedLat
		{
			get { return coordEntry.Latitude;}
			set { 
				coordEntry.Latitude = value;
				m_IsCorrected = true;
			}
		}
		
		public double CorrectedLon
		{
			get { return coordEntry.Longitude;}
			set { 
				coordEntry.Longitude = value;
				m_IsCorrected = true;
			}
		}
		
		bool m_IsCorrected = false;
		public Boolean IsCorrected
		{
			get { return m_IsCorrected;}
		}
		
		private void SetActual(double lat, double lon)
		{
			String latStr = Utilities.getLatString(new DegreeMinutes(lat));
			String lonStr = Utilities.getLonString(new DegreeMinutes(lon));
			origLabel.Text = latStr + " " + lonStr;
		}
		
		public void SetCache(Geocache cache, bool isDirect)
		{
			m_cache = cache;
			if (isDirect)
				coordEntry.SetDirectMode();
			SetActual(cache.OrigLat, cache.OrigLon);
			if (cache.HasCorrected)
			{
				coordEntry.Latitude = cache.CorrectedLat;
				coordEntry.Longitude = cache.CorrectedLon;
				resetButton.Sensitive = true;
			}
			else
			{
				coordEntry.Latitude = cache.Lat;
				coordEntry.Longitude = cache.Lon;
			}	
			m_IsCorrected = cache.HasCorrected;
			coordEntry.Changed += HandleCoordEntryChanged;
		}

		void HandleCoordEntryChanged (object sender, EventArgs args)
		{
			resetButton.Sensitive = true;
			m_IsCorrected = true;
		}

		public CorrectedCoordinatesDlg ()
		{
			this.Build ();
		}
		protected virtual void OnResetClick (object sender, System.EventArgs e)
		{
			coordEntry.Latitude = m_cache.OrigLat;
			coordEntry.Longitude = m_cache.OrigLon;
			m_IsCorrected = false;
			resetButton.Sensitive = false;
		}
		
		protected virtual void OnOKClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnCancelClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		
		
		
	}
}
