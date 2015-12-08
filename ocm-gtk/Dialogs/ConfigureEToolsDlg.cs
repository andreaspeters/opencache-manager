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
using Mono.Unix;

namespace ocmgtk
{


	public partial class ConfigureEToolsDlg : Gtk.Dialog
	{
		EToolList m_toolList;
		ListStore m_treemodel;
		TreeModelSort m_sortmodel;

		public ConfigureEToolsDlg ()
		{
			this.Build ();
		}
		
		public ConfigureEToolsDlg (EToolList tools)
		{
			this.Build ();
			m_toolList = tools;
			BuildTree();
		}
		
		private void BuildTree()
		{
			CellRendererText cmdname_cell = new CellRendererText ();
			CellRendererText cmdscript_cell = new CellRendererText ();
			TreeViewColumn cmdname = new TreeViewColumn (Catalog.GetString ("Name"), cmdname_cell);
			TreeViewColumn cmdscript = new TreeViewColumn (Catalog.GetString ("Exec"), cmdscript_cell);
			commandView.AppendColumn(cmdname);
			commandView.AppendColumn(cmdscript);
			m_treemodel = new ListStore(typeof (ExternalTool));
			m_sortmodel = new TreeModelSort(m_treemodel);
			cmdname.SetCellDataFunc (cmdname_cell, new TreeCellDataFunc (RenderCommandName));
			cmdscript.SetCellDataFunc(cmdscript_cell, new TreeCellDataFunc(RenderCommandExec));
			commandView.Model = m_sortmodel;
			commandView.Selection.Changed += HandleCommandViewSelectionChanged;
			
			foreach(ExternalTool tool in m_toolList.ToolArray)
			{
				m_treemodel.AppendValues(tool);
			}
		}

		void HandleCommandViewSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			
			if (((TreeSelection)sender).GetSelected (out model, out iter)) {
				editButton.Sensitive = true;
				removeButton.Sensitive = true;
			} else {
				editButton.Sensitive = false;
				removeButton.Sensitive = false;
			}
		}
		
		private void RenderCommandName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ExternalTool tool = (ExternalTool)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Markup = tool.Name;
		}
		
		private void RenderCommandExec (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ExternalTool tool = (ExternalTool)model.GetValue (iter, 0);
			CellRendererText text = cell as CellRendererText;
			text.Markup = tool.Command;
		}

		protected virtual void OnAddTool (object sender, System.EventArgs e)
		{
			AddCommandDialog dlg = new AddCommandDialog();
			dlg.Parent = this;
			if ((int) ResponseType.Ok ==  dlg.Run())
			{
				m_treemodel.AppendValues(dlg.ETool);
				m_toolList.AddTool(dlg.ETool);
			}
		}
		
		private ExternalTool GetSelectedTool ()
		{
			Gtk.TreeIter itr;
			Gtk.TreeModel model;
			if (commandView.Selection.GetSelected (out model, out itr)) {
				return (ExternalTool)model.GetValue (itr, 0);
			}
			return null;
		}
				
		protected virtual void OnCloseClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnDeleteClicked (object sender, System.EventArgs e)
		{
			ExternalTool tool = GetSelectedTool();
			MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Question,
			                                      ButtonsType.YesNo, Catalog.GetString("Are you sure you wish to delete '{0}'"),
			                                      tool.Name);
			if ((int) ResponseType.Yes == dlg.Run())
			{
				m_toolList.DeleteTool(tool.Name);
				RefreshList ();
			}
			dlg.Hide();
		}
		
		private void RefreshList ()
		{
			m_treemodel.Clear();
			foreach(ExternalTool ntool in m_toolList.ToolArray)
			{
				m_treemodel.AppendValues(ntool);
			}
		}
		
		protected virtual void OnEditClicked (object sender, System.EventArgs e)
		{
			ExternalTool tool = GetSelectedTool();
			AddCommandDialog dlg = new AddCommandDialog();
			dlg.Parent = this;
			dlg.ETool = tool;
			if ((int) ResponseType.Ok ==  dlg.Run())
			{
				m_toolList.DeleteTool(tool.Name);
				m_toolList.AddTool(dlg.ETool);
				RefreshList();
			}
		}
		
		
		
		
		
	}
}
