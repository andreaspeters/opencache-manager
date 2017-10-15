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
using ocmengine;
using Gtk;
using Mono.Unix;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotesWidget : Gtk.Bin
	{
		Geocache current = null;
		OCMMainWindow m_Win = null;
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}
		
		public NotesWidget ()
		{
			this.Build ();
			editorWidget.TextChanged += HandleEditorWidgetTextChanged;
		}

		void HandleEditorWidgetTextChanged (object sender, EventArgs args)
		{
			saveButton.Sensitive = true;
		}
		
		public void SetCache (Geocache cache)
		{
			if (saveButton.Sensitive == true)
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, Catalog.GetString("You have unsaved notes, do you wish to save them?"));
				if ((int)ResponseType.Yes ==  dlg.Run())
				{
					SaveNotes();
				}
				dlg.Hide();
			}
			current = cache;
			if (cache != null)
			{
				editorWidget.Text = cache.Notes;
				editorWidget.Sensitive = true;
			}
			else
			{
				editorWidget.Sensitive = false;
				editorWidget.Text = String.Empty;
			}
			saveButton.Sensitive = false;
		}
		
		protected virtual void OnSaveNotes (object sender, System.EventArgs e)
		{
			SaveNotes ();
		}
		
		private void SaveNotes ()
		{
			current.Notes = editorWidget.Text;
			m_Win.App.CacheStore.AddWaypointOrCache (current, true, false);
			saveButton.Sensitive = false;
			m_Win.QueueDraw();
		}
		
		
	}
}
