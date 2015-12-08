// 
//  Copyright 2011  campbelk
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
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class CleanupConfirmPage : Gtk.Bin
	{
		public CleanupConfirmPage ()
		{
			this.Build ();
		}
		
		public void Prepare()
		{
			CleanupManager mgr = (this.Parent as CleanupAssistant).Manager;
			mgr.logLimit =  (this.Parent as CleanupAssistant).page2.LogLimit;
			cleanupPreview.Text = String.Empty;
			if (mgr.purgeLogs)
			{
				mgr.BuildLogsToDelete();
				cleanupPreview.Text += String.Format(Catalog.GetString("\n\n{0} logs will be purged."), mgr.DeleteCount);
			}
			if (mgr.backupDB)
			{
				cleanupPreview.Text += Catalog.GetString("\n\nDatabase will be backed up.");
			}
			if (mgr.compactDB)
			{
				cleanupPreview.Text += Catalog.GetString("\n\nDatabase will be compacted.");
			}
		}
	}
}
