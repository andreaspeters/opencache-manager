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
using Gtk;

namespace ocmgtk
{


	public partial class NoDBDialog : Gtk.Dialog
	{
		public NoDBDialog ()
		{
			this.Build ();
		}
		protected virtual void OpenDBClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Respond(ResponseType.Yes);
		}
		
		protected virtual void QuitClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Respond(ResponseType.Cancel);
		}
		protected virtual void NewDBClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Respond(ResponseType.No);
		}
	}
}
