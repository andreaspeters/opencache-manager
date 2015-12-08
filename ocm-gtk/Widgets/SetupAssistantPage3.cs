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

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class SetupAssistantPage3 : Gtk.Bin
	{
		protected virtual void OnAccountInfoClicked (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start ("http://www.geocaching.com/account/default.aspx");
		}
		
		public double HomeLat
		{
			get { return latEntry.getCoordinate();}
		}
		
		public double HomeLon
		{
			get { return lonEntry.getCoordinate();}
		}
		
		public string MemberID
		{
			get { return ownerEntry.Text;}
		}

		public SetupAssistantPage3 ()
		{
			this.Build ();
			latEntry.SetCoordinate(0, true);
			lonEntry.SetCoordinate(0, false);
		}
	}
}
