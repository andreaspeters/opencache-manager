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
using System.Collections.Generic;
using ocmengine;
using Mono.Unix;
using Gtk;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class DescriptionWidget : Gtk.Bin
	{
		HTMLWidget descWidget, hintWidget;
		ListStore tbStore;
		OCMApp m_App;
		Geocache geocache;
		
		public OCMApp App
		{
			set { m_App = value;}
		}

		public DescriptionWidget ()
		{
			this.Build ();
			descWidget = new HTMLWidget ();
			hintWidget = new HTMLWidget();
			descAlign.Add(descWidget);			
			SetupTBList ();
		}

		public void SetCache (Geocache cache)
		{
			this.geocache = cache;
			if (cache == null)
			{
				descWidget.HTML = Catalog.GetString("<b>NO CACHE SELECTED</b>");
				return;
			}
			SetDescription (cache);
			SetTravelBugs(m_App.CacheStore.GetTravelBugs(cache.Name));
			if (String.IsNullOrEmpty (cache.Hint) || String.IsNullOrEmpty(cache.Hint.Trim())) {
				hintButton.Sensitive = false;
			} else {
				hintWidget.HTML = "<div style='font-family:sans-serif;font-size:10pt; background-color:#FFFFFF'>" + cache.Hint  + "</div>";
				hintButton.Sensitive = true;
			}
		}

		public void SetupTBList ()
		{
			tbStore = new ListStore (typeof(TravelBug));
			CellRendererText tbref_cell = new CellRendererText ();
			CellRendererText tbname_cell = new CellRendererText ();
			TreeViewColumn tbref_col = new TreeViewColumn (Catalog.GetString ("Ref"), tbref_cell);
			TreeViewColumn tbname_col = new TreeViewColumn (Catalog.GetString ("Name"),tbname_cell);
			tbref_col.SetCellDataFunc (tbref_cell, new TreeCellDataFunc(RenderRefCell));
			tbname_col.SetCellDataFunc (tbname_cell, new TreeCellDataFunc(RenderNameCell));
			tbugView.AppendColumn (tbref_col);
			tbugView.AppendColumn (tbname_col);
			tbugView.Model = tbStore;
		}

		private void RenderRefCell (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TravelBug bug = (TravelBug)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = bug.Ref;
		}

		private void RenderNameCell (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TravelBug bug = (TravelBug)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Text = bug.Name;
		}


		public void SetTravelBugs (TravelBug[] bugs)
		{
			tbStore.Clear ();
			foreach (TravelBug bug in bugs)
			{
				tbStore.AppendValues (bug);
			}
			if (bugs.Length == 0)
				tbugExpander.Sensitive = false;
			else
				tbugExpander.Sensitive = true;
		}

		public void SetDescription (Geocache cache)
		{
			String baseURL = cache.URL.Scheme + "://" + cache.URL.Host;
			descWidget.SetHTML("<div style='font-family:sans-serif;font-size:10pt; background-color:#FFFFFF'>" + cache.ShortDesc + "\n\n" 
				+ cache.LongDesc + "</div>", baseURL);
		}
		protected virtual void onHintClick (object sender, System.EventArgs e)
		{
			MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, geocache.Hint);
			dlg.Run();
			dlg.Hide();
		}
		
		
	}
}
