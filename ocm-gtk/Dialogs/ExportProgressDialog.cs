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
using Gtk;
using Mono.Unix;

namespace ocmgtk
{


	public partial class ExportProgressDialog : Gtk.Dialog
	{
		double total = 100;
		double count = 0;
		GPXWriter m_writer =null;
		
		private bool m_autoClose = false;
		public bool AutoClose
		{
			set { m_autoClose = value;}
		}
		
		public bool WaypointsOnly
		{
			set { m_writer.IncludeGroundSpeakExtensions = !value;}
		}
		
		private string m_command = null;
		public String CompleteCommand
		{
			set { m_command = value;}
		}
		
		public ExportProgressDialog (GPXWriter writer)
		{
			this.Build ();
			writer.WriteWaypoint += HandleWriterWriteWaypoint;
			writer.Complete += HandleWriterComplete;
			m_writer = writer;
		}
		
		public void Start(String filename, List<Geocache> list, Dictionary<string,string> wmappings, ACacheStore store)
		{
			total = list.Count;
			if (m_writer.Limit != -1 && m_writer.Limit < total)
				total = m_writer.Limit;
			fileLabel.Markup = Catalog.GetString("<b>File: </b>") + filename;
			m_writer.WriteGPXFile(filename, list, wmappings, store);
		}

		void HandleWriterComplete (object sender, EventArgs args)
		{
			if (m_autoClose)
			{
				this.Hide();
				if (m_command != null)
				{
					System.Diagnostics.Process.Start(Utilities.StringToStartInfo(m_command));
				}
				return;
			}
			
			okButton.Sensitive = true;
			buttonCancel.Sensitive = false;
			infoLabel.Markup = String.Format(Catalog.GetString("<i>{0} Geocaches exported.</i>"), count);
			writeProgress.Text = Catalog.GetString("Complete");
			okButton.Show();
			buttonCancel.Hide();
			okButton.GrabDefault();
		}

		void HandleWriterWriteWaypoint (object sender, EventArgs args)
		{
			count++;
			double fraction = count/total;
			writeProgress.Fraction = fraction;
			writeProgress.Text = fraction.ToString("0%");
			this.infoLabel.Markup = "<i>" + (args as WriteEventArgs).Message + "</i>";
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
			
		}
		
		protected virtual void OnCancel (object sender, System.EventArgs e)
		{
			DoCancel();
		}
		
		public void DoCancel()
		{
			Gtk.MessageDialog qdlg = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Warning,
			                                               Gtk.ButtonsType.YesNo, Catalog.GetString("Cancelling an export will result in an invalid GPX file.\nAre you sure?"));
			if ((int) ResponseType.Yes == qdlg.Run())
			{
				qdlg.Hide();
				qdlg.Dispose();
				this.Hide();
				this.Dispose();
				m_writer.Cancel = true;
				return;
			}
			qdlg.Hide();
			this.ShowNow();
		}		
		
		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			DoCancel();
		}
		protected virtual void OnOKClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Dispose();
		}
		
		
	}
}
