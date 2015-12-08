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

namespace ocmgtk
{


	public partial class AddLocationDialog : Gtk.Dialog
	{

		public Location NewLocation
		{
			get { 
				Location loc = new Location();
				loc.Name = nameEntry.Text;
				loc.Latitude = locationwidget1.Latitude;
				loc.Longitude = locationwidget1.Longitude;
				return loc;
			}
		}
		
		public LocationWidget CoordinateWidget
		{
			get
			{
				return locationwidget1;
			}
		}
		
		public string LocationName
		{
			get { return nameEntry.Text;}
		}
		
		public AddLocationDialog ()
		{
			this.Build ();
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
