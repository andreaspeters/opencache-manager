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


	[System.ComponentModel.ToolboxItem(true)]
	public partial class DateWidget : Gtk.Bin
	{
		protected virtual void OnCallClicked (object sender, System.EventArgs e)
		{
			CalendarWindow win = new CalendarWindow();
			win.Date = this.dateField.Text;
			win.Run();
			this.dateField.Text = win.Date;
		}
		
		bool m_IncludeTime = false;
		public bool IncludeTime
		{
			get{ return m_IncludeTime;}
			set { m_IncludeTime = value;}
		}
		
		
		
		public DateTime Date
		{
			get {
				try
				{
					return DateTime.Parse(dateField.Text);
				}
				catch (Exception e)
				{
					OCMApp.ShowException(e);
					return DateTime.MinValue;
				}
			}
			set
			{
				if (value == DateTime.MinValue)
					value = DateTime.Today;
				if (m_IncludeTime)
					dateField.Text = value.ToShortDateString() + " " + value.ToShortTimeString();
				else
					dateField.Text = value.ToShortDateString();
			}
		}
		

		public DateWidget ()
		{
			this.Build ();
			this.dateField.Text = System.DateTime.Today.ToShortDateString();
		}
	}
}
