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
using ocmengine;
using System.Collections.Generic;
using Gtk;

namespace ocmgtk
{


	public partial class ImportDialog : Gtk.Dialog
	{
		private OCMMainWindow m_Win;
		
		public ImportDialog()
		{
			this.Build();
		}

		public ImportDialog (OCMMainWindow win)
		{
			this.Build ();
			m_Win = win;
			FileFilter filter = new FileFilter ();
			filter.Name = "Waypoint Files";
			filter.AddPattern ("*.gpx");
			filter.AddPattern ("*.loc");
			filter.AddPattern ("*.zip");
			fileWidget.AddFilter (filter);
			UpdateBookMarkCombo();
		}
		
		
		
		public string Filename
		{
			get { return fileWidget.Filename;}
		}
				
		
		public bool PreventStatusOverwrite
		{
			get { return statusCheck.Active;}
			set { statusCheck.Active = value;}
		}
		
		public bool PurgeOldLogs
		{
			get { return oldLogsCheck.Active;}
			set { oldLogsCheck.Active = value;}
		}
		
		public bool IgnoreExtraFields
		{
			get { return gsakFieldsCheck.Active;}
			set { gsakFieldsCheck.Active = value;}
		}
		
		public string ItemToDelete
		{
			get
			{
				if (addToListCheck.Active)
					return bmCombo.ActiveText;
				return null;
			}
			set
			{
				if (value != null)
				{
					ListStore store = bmCombo.Model as ListStore;
					TreeIter itr;
					store.GetIterFirst (out itr);
					if (!store.IterIsValid (itr))
						return;
					int iCount = 0;
					do
					{
						if (value == store.GetValue (itr, 0) as string)
						{
							bmCombo.Active = iCount;
							addToListCheck.Active = true;
							return;
						}
						iCount++;
					} while (store.IterNext (ref itr));
				}
			}
		}
		
		
		public void SetCurrentFolder(string folder)
		{
			fileWidget.SetCurrentFolder(folder);
		}
		
		protected virtual void OnOkClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnCancelClicked (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnFileActivated (object sender, System.EventArgs e)
		{
			this.Respond(ResponseType.Accept);
			this.Hide();
		}
		
		protected virtual void OnAddClicked (object sender, System.EventArgs e)
		{
			m_Win.AddBookmark();
			UpdateBookMarkCombo();
		}
		
		private void UpdateBookMarkCombo()
		{
			ListStore model = bmCombo.Model as ListStore;
			model.Clear();
			System.Console.WriteLine(m_Win.ToString());
			string[] bookmarks = m_Win.App.Bookmarks.Bookmarks;
			foreach(string bmrk in bookmarks)
			{
				bmCombo.AppendText(bmrk);
			}
			bmCombo.Active = 0;
			bmCombo.Show();
		}
		
		protected virtual void OnAddToBmrkToggle (object sender, System.EventArgs e)
		{
			bmCombo.Sensitive = addToListCheck.Active;
			addBmrkButton.Sensitive = addToListCheck.Active;
		}
		
		
	}
}