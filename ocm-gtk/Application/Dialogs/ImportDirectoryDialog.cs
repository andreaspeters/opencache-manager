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
using System.Collections.Generic;
using ocmengine;
using Gtk;

namespace ocmgtk
{


	public partial class ImportDirectoryDialog : Gtk.Dialog
	{
		private OCMMainWindow m_Win;
		
		public ImportDirectoryDialog ()
		{
			this.Build ();
		}
		
		public ImportDirectoryDialog (OCMMainWindow win)
		{
			this.Build ();
			m_Win = win;
			UpdateBookMarkCombo();
		}
		
		public string Directory
		{
			get { return dirChooser.Filename;}
			set { 
				System.Console.WriteLine(value);
				dirChooser.SetCurrentFolder(value);
			}
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
		
		public bool DeleteOnCompletion
		{
			set { deleteCheck.Active = value;}
			get { return deleteCheck.Active;}
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
		
		protected virtual void OnAddClicked (object sender, System.EventArgs e)
		{
			m_Win.AddBookmark();
			UpdateBookMarkCombo();
		}
		
		private void UpdateBookMarkCombo()
		{
			ListStore model = bmCombo.Model as ListStore;
			model.Clear();
			string[] bookmarks = m_Win.App.Bookmarks.Bookmarks;
			foreach(string bmrk in bookmarks)
			{
				bmCombo.AppendText(bmrk);
			}
			bmCombo.Active = 0;
			bmCombo.Show();
		}
		protected virtual void OnBmrkToggle (object sender, System.EventArgs e)
		{
			bmCombo.Sensitive = addToListCheck.Active;
			addBmrkButton.Sensitive = addToListCheck.Active;
		}
	}
}
