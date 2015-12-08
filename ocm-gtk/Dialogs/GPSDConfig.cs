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


	public partial class GPSDConfig : Gtk.Dialog
	{
		
		public int PollInterval
		{
			get{return Int32.Parse(pollEntry.Text);}
			set{pollEntry.Text = value.ToString();}
		}
		
		public bool GPSDOnStartup
		{
			get{return startupCheck.Active;}
			set{startupCheck.Active = value;}
		}
		
		public bool RecenterMap
		{
			get{return recenterCheck.Active;}
			set{recenterCheck.Active = value;}
		}
		

		public GPSDConfig ()
		{
			this.Build ();
		}
		
		protected virtual void OnOK (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		
	}
}
