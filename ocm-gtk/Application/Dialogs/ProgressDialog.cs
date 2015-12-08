// 
//  Copyright 2010  campbelk
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
using System.IO;
using ocmengine;
using Mono.Unix;
using System.Diagnostics;
using System.Collections.Generic;

namespace ocmgtk
{


	public partial class ProgressDialog : Gtk.Dialog
	{

		private GPXParser m_parser;
		double m_total = 0;
		double m_progress = 0;
		double m_progressCount = 0;
		bool m_isMulti = false;
		bool m_autoClose = false;
		DateTime m_timeStart;
		public bool AutoClose
		{
			get { return m_autoClose;}
			set { m_autoClose = value;}
		}
		
		public ProgressDialog (GPXParser parser)
		{
			this.Build ();
			parser.ParseWaypoint += HandleParserParseWaypoint;
			parser.Complete += HandleParserComplete;
			m_parser = parser;
			multiFileLabel.Visible = false;
		}

		void HandleParserComplete (object sender, EventArgs args)
		{
			if (!m_isMulti)
				HandleCompletion ();
		}
		
		private void HandleCompletion ()
		{
			DateTime end = DateTime.Now;
			TimeSpan span = end.Subtract(m_timeStart);
			progressbar6.Text = Catalog.GetString("Complete");
			waypointName.Markup =  String.Format (Catalog.GetString("<i>Import complete, {0} waypoints processed in {1}:{2}:{3}</i>"), 
			                                      m_progress, span.Hours, span.Minutes, span.Seconds);
			okButton.Sensitive = true;
			okButton.Show();
			buttonCancel.Hide();
			buttonCancel.Sensitive = false;
			okButton.GrabDefault();
			if (m_autoClose)
				this.Hide();
		}
		
		
		public void StartMulti(String directoryPath, ACacheStore store, bool deleteOnCompletion)
		{
			m_isMulti = true;
			m_timeStart = DateTime.Now;
			string[] files = Directory.GetFiles(directoryPath);
			m_parser.StartUpdate(store);
			
			// Count total files
			m_progress = 0;
			m_progressCount = 0;
			m_total = 0;
			multiFileLabel.Visible = true;
			
			List<string> dirs = new List<string>();
			
			// Prescan for zip files and uncompress
			for (int i=0; i < files.Length; i++)
			{
				if (files[i].EndsWith(".zip"))
				{
					this.progressbar6.Text = Catalog.GetString("Unzipping");
					DirectoryInfo info = Directory.CreateDirectory(files[i].Substring(0, files[i].Length -4));
					dirs.Add(info.FullName);
					multiFileLabel.Text = Catalog.GetString("Unizpping");
					this.waypointName.Markup = "<i>" + Catalog.GetString("Unzipping") + ":" + files[i] + "</i>";
					while (Gtk.Application.EventsPending ())
						Gtk.Application.RunIteration (false);
					ProcessStartInfo start = new ProcessStartInfo();
					start.FileName = "unzip";
					start.Arguments = "-o \"" + files[i] + "\" -d \"" + info.FullName + "\"";
					Process unzip =  Process.Start(start);
					while (!unzip.HasExited)
					{
						// Do nothing until exit	
					}
					if (deleteOnCompletion)
					{
						File.Delete(files[i]);
					}
				}
			}
			
			// Rescan for all GPX files, including those uncompressed by ZIP files
			List<string> fileList = new List<string>();
			string[] directories = Directory.GetDirectories(directoryPath);
			BuildFileList (directoryPath, fileList);
			foreach (string dir in directories)
			{
				BuildFileList(dir, fileList);
			}
			
			int currCount = 0;
			foreach (string file in fileList)
			{
				if (file.EndsWith(".gpx"))
				{
					currCount++;
					//Clean out attributes,tbs,and logs that will be overwritten
					if (m_parser.Cancel)
						return;
					FileStream fs =  System.IO.File.OpenRead (file);
					m_parser.clearForImport(fs, store);
					fs.Close();
					// Need to reopen the file
					fs =  System.IO.File.OpenRead (file);
					multiFileLabel.Text = String.Format(Catalog.GetString("Processing File {0} of {1}"), currCount, fileList.Count);
					ParseFile(fs, store);
					fs.Close();
					if (deleteOnCompletion)
						File.Delete(file);
				}
			}
			
			if (deleteOnCompletion)
			{
				foreach (string dir in dirs)
				{
					Directory.Delete(dir);
				}
			}
			m_parser.EndUpdate(store);
			HandleCompletion();
		}
		
		private void BuildFileList (string dirPath,  List<string> fileList)
		{
			string[] files = Directory.GetFiles(dirPath);
			for (int i=0; i < files.Length; i++)
			{
				if (files[i].EndsWith(".gpx") || files[i].EndsWith(".loc"))
				{
					FileStream fs =  System.IO.File.OpenRead (files[i]);
					int total = m_parser.parseTotal(fs);
					m_total += total;
					fileList.Add(files[i]);
					fs.Close();
				}
			}
			return;
		}


		public void Start (String filename, ACacheStore store)
		{
			this.Show ();
			try {
				m_parser.StartUpdate(store);
				FileStream stream = File.OpenRead(filename);
				m_total = m_parser.PreParseForSingle(stream, store);
				stream.Close();
				stream = File.OpenRead(filename);
				m_progress = 0;
				m_progressCount = 0;
				m_timeStart = DateTime.Now;
				ParseFile (stream, store);
				stream.Close();
				if (!m_parser.Cancel)
					m_parser.EndUpdate(store);
			} catch (Exception e) {
				this.Hide ();
				OCMApp.ShowException(e);
				this.Dispose ();
			}
		}
		
		private void ParseFile (FileStream fs, ACacheStore store)
		{
			
				fileLabel.Markup = Catalog.GetString("<b>File: </b>") + fs.Name;
				m_parser.parseGPXFile (fs, store);
		}

		void HandleParserParseWaypoint (object sender, EventArgs args)
		{
			m_progress++;
			m_progressCount++;
			double fraction = (double)(m_progress / m_total);
			this.progressbar6.Text = (fraction).ToString ("0%");
			progressbar6.Fraction = fraction;
			this.waypointName.Markup = "<i>" + (args as ParseEventArgs).Message + "</i>";
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
			m_progressCount = 0;
		}

		protected virtual void OnCancel (object sender, System.EventArgs e)
		{
			DoCancel ();
		}

		private void DoCancel ()
		{
			m_parser.Cancel = true;
			this.Hide ();
			String message = Catalog.GetString("Import cancelled, all changes reverted.");
			Gtk.MessageDialog dlg = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Info, 
			                                               Gtk.ButtonsType.Ok, message);
			dlg.Run ();
			dlg.Hide ();
			dlg.Dispose ();
		}
		protected virtual void OnCancel (object o, Gtk.DeleteEventArgs args)
		{
			DoCancel ();
		}
		
		protected virtual void OnButton179Clicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Dispose();
		}
		
		
		
	}
}
