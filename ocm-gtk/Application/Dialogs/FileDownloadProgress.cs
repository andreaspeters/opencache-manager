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
using System.Net;
using ocmengine;
using Mono.Unix;
using System.IO;
using System.Collections.Generic;

namespace ocmgtk
{

	/// <summary>
	/// Downloads file(s) from the internet
	/// </summary>
	public partial class FileDownloadProgress : Gtk.Dialog
	{		
		bool m_cancel = false;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public FileDownloadProgress ()
		{
			this.Build ();
		}
		
		public void Start(Dictionary<string,string[]> files)
		{
			
				int iCount = 1;
				int iFailed = 0;
				int total = files.Count;
				this.Show();
				OCMApp.UpdateGUIThread();
				foreach (string url in files.Keys)
				{
					if (m_cancel)
						break;
					try
					{
						DownloadFile (files[url][0], files[url][1], total, ref iCount);
					}
					catch (Exception e)
					{
						System.Console.WriteLine("FAILED:" + files[url][0]);
						progBar.Text = String.Format(Catalog.GetString("Download Failed for {0}"),files[url][0]);
						iCount++;
						iFailed++;
					}
				}
				if (m_cancel)
					progBar.Text = Catalog.GetString("Cancelled");
				else
					progBar.Text = String.Format(Catalog.GetString("Complete, {0} files failed to download"), iFailed);
				buttonOk.Show();
				buttonCancel.Hide();
			
		}
		
		/// <summary>
		/// Starts downloading a list of files
		/// </summary>
		/// <param name="files">
		/// A list of file urls<see cref="List<System.String>"/>
		/// </param>
		/// <param name="destFolder">
		/// The destination directory <see cref="System.String"/>
		/// </param>
		public void Start(List<string> files, string destFolder)
		{
			try
			{
				int iCount = 1;
				int total = files.Count;
				this.Show();
				OCMApp.UpdateGUIThread();
				foreach (string url in files)
				{
					if (m_cancel)
						break;
					DownloadFile (url, destFolder, total, ref iCount);
					
				}
				if (m_cancel)
					progBar.Text = Catalog.GetString("Cancelled");
				else
					progBar.Text = Catalog.GetString("Complete");
				buttonOk.Show();
				buttonCancel.Hide();
			}
			catch (Exception e)
			{
				this.Hide();
				OCMApp.ShowException(e);
			}
		}
		
		private void DownloadFile (string url, string destFolder, int total, ref int iCount)
		{
			WebRequest req = WebRequest.Create(url);
			WebResponse resp = req.GetResponse();
			string fName = Utilities.GetShortFileNameNoExtension(url);
			if (fName == null)
			{
				// Skip this file
				iCount++;
				return;
			}
			
			System.IO.FileStream fs = new FileStream(destFolder + "/" + fName, FileMode.Create);
			System.IO.Stream webstream = resp.GetResponseStream();
			fileLabel.Markup = String.Format(Catalog.GetString("File {0} of {1}"), iCount, total);
			long totalKB = resp.ContentLength/8;
			long totalBytes = resp.ContentLength;
			long currProg = 0;
			byte[] buff = new byte[4096];
			for(;;)
			{
				OCMApp.UpdateGUIThread();
				long currKB = currProg/8;
				progBar.Text = String.Format(Catalog.GetString("Received {0} bytes of {1} bytes"), currKB, totalKB);
				double progress = (((double) currProg / (double) totalBytes) + (double) iCount) / (double) total;
				if (progress > 1)
					progress = 1;
				if (progBar.Fraction >= 0)
					progBar.Fraction = progress;
				int read = webstream.Read(buff, 0, 4096);
				currProg = currProg + read;
				if (read > 0 && !m_cancel)
				{
					fs.Write(buff, 0, read);
				}
				else
				{
					break;
				}
			}
			fs.Close();
			webstream.Close();
			iCount ++;
			return;
		}
		
		protected virtual void OnOKClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnCancelCLick (object sender, System.EventArgs e)
		{
			m_cancel = true;
		}
	}
}
