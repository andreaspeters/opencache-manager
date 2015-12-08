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
using ocmengine;

namespace ocmgtk
{


	public partial class CopyMoveDialog : Gtk.Dialog
	{

		private OCMMainWindow m_Win;
		
		public string Filename
		{
			get { return chooser.Filename;}
			set { chooser.SetCurrentFolder(value);}
		}
		
		public CopyingProgress.ModeEnum Mode
		{
			get
			{
				if (visibleRadio.Active)
					return CopyingProgress.ModeEnum.VISIBLE;
				else if (selectedRadio.Active)
					return CopyingProgress.ModeEnum.SELECTED;
				else
					return CopyingProgress.ModeEnum.ALL;
			}
		}
		
		public CopyMoveDialog()
		{
			this.Build();
		}
		
		public CopyMoveDialog (OCMMainWindow win)
		{
			this.Build ();	
			m_Win = win;
			FileFilter filter = new FileFilter ();
			filter.Name = "OCM Databases";
			filter.AddPattern ("*.ocm");
			chooser.AddFilter (filter);
			chooser.SelectionChanged += HandleChooserSelectionChanged;
			buttonOk.Sensitive = false;
		}
		

		void HandleChooserSelectionChanged (object sender, EventArgs e)
		{
			if (chooser.Filename == m_Win.App.CacheStore.StoreName)
				buttonOk.Sensitive = false;
			else
				buttonOk.Sensitive = true;
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Dispose();
		}
		
		protected virtual void OKClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
	}
}
