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
using System.Text.RegularExpressions;
using Mono.Unix;

namespace ocmgtk
{


	public partial class ScanWaypointsDialog : Gtk.Dialog
	{
		
		private WaypointWidget m_widget = null;
		private MatchCollection m_matches = null;

		public ScanWaypointsDialog (int foundCount, WaypointWidget widget, MatchCollection matches)
		{
			this.Build ();
			this.msgLabel.Text = String.Format(Catalog.GetString("OCM found {0} waypoints in the cache description. " +
				"You can add them all automatically, or review the matches one by one."), foundCount); 
			m_widget = widget;
			m_matches = matches;
		}
		
		protected virtual void OnAllClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			m_widget.AutoGenerateChildren(m_matches);
		}
		
		protected virtual void OnCancelClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnReviewClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			ReviewWaypointDialog dlg = new ReviewWaypointDialog(m_widget);
			dlg.Matches = m_matches;
			dlg.Run();	
			m_widget.App.RefreshAll();
		}
	}
}
