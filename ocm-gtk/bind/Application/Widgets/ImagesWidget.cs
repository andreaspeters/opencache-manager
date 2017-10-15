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
using System.IO;
using System.Collections.Generic;
using System.Net;
using Mono.Unix;
using ocmengine;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;

namespace ocmgtk
{

	/// <summary>
	/// This class displays a list of images on the file system associated
	/// with the particular caches
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ImagesWidget : Gtk.Bin
	{
		ListStore m_model;
		Geocache m_cache;
		OCMMainWindow m_Win;
		
		/// <summary>
		/// Reference to Main Window
		/// </summary>
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ImagesWidget ()
		{
			this.Build ();
			imagesView.SelectionChanged += HandleImagesViewSelectionChanged;
		}
		
		public bool HasImages
		{
			get { 
				
				if (m_model == null)
					return false;
				TreeIter first;
				if (!m_model.GetIterFirst(out first))
					return false;
				if (m_model.GetValue(first, 0) != null)
					return true;
				return false;
			}
		}
		
		/// <summary>
		/// Called when a cache is selected in the cache list
		/// </summary>
		/// <param name="cache">
		/// The selected geocache <see cref="Geocache"/>
		/// </param>
		public void SetCache(Geocache cache)
		{
			m_cache = cache;
			m_model = new ListStore(typeof(Pixbuf), typeof(string), typeof(string));
			if (cache == null)
			{
				this.Sensitive = false;
				return;
			}
			this.Sensitive = true;
			string imagesFolder = GetImagesFolder ();
			fileLabel.Text = String.Format(Catalog.GetString("Images Folder: {0}"), imagesFolder);
			if(Directory.Exists(imagesFolder))
			{
				string[] files = Directory.GetFiles(imagesFolder);
				foreach(string file in files)
				{
					try
					{
						Pixbuf buf = new Pixbuf(file,256, 256);
						string[] filePath = file.Split('/');
						m_model.AppendValues(buf, filePath[filePath.Length -1],file);
					}
					catch (GLib.GException)
					{
						// Ignore invalid image files
					}
						
				}
			}
			imagesView.Model = m_model;
			imagesView.PixbufColumn = 0;
			imagesView.TextColumn = 1;
			imagesView.SelectionMode = SelectionMode.Single;
		}
		
		/// <summary>
		/// Gets the path of the local image folder. The image folder path will be
		/// (ocm data dir)/ocm_images/(name of db)/(cache code), i.e.
		/// /home/user/bob/ocm/ocm_images/bobsdb/GCABC1
		/// </summary>
		/// <returns>
		/// The image diectory path <see cref="System.String"/>
		/// </returns>
		private string GetImagesFolder()
		{
			string dbName = GetDBName ();
			return m_Win.App.AppConfig.DataDirectory + "/ocm_images/" + dbName + "/" +  m_Win.CacheList.SelectedCache.Name;
		}
		
		
		/// <summary>
		/// Gets the short name of the db without the .ocm extension
		/// </summary>
		/// <returns>
		/// The short name of the db <see cref="System.String"/>
		/// </returns>
		private string GetDBName ()
		{
			string dbFile = m_Win.App.CacheStore.StoreName;
			return Utilities.GetShortFileNameNoExtension(dbFile);
		}
		
		/// <summary>
		/// Searches the cache description for HTML IMG tags and downloads the file
		/// to the local file system
		/// </summary>
		public void GrabImages ()
		{
			string baseURL = String.Empty;
			if (m_cache.URL != null)
				baseURL = m_cache.URL.Scheme + "://" + m_cache.URL.Host; 
			const string IMG = "(<[Ii][Mm][Gg])([^sS][^rR]*)([Ss][Rr][Cc]\\s?=\\s?)\"([^\"]*)\"([^>]*>)";
			MatchCollection matches = Regex.Matches(m_Win.CacheList.SelectedCache.LongDesc, IMG);
			if (matches.Count == 0)
			{
				MessageDialog mdlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, Catalog.GetString("No Images Found."));
				mdlg.Run();
				mdlg.Hide();
				return;
			}
			
			List<string> files = new List<string>();
			string imagesFolder = GetImagesFolder();
			if (!Directory.Exists(imagesFolder))
				Directory.CreateDirectory(imagesFolder);
			
			foreach(Match match in matches)
			{
				string url = match.Groups[4].Value;
				if (!url.Contains("://"))
				{
					if (url.StartsWith("/"))
						url = baseURL + url;
					else
						url = baseURL + "/" + url;
				}
				files.Add(url);
			}
		
			FileDownloadProgress dlg = new FileDownloadProgress();
			dlg.Icon = m_Win.Icon;
			dlg.Start(files, imagesFolder);
			SetCache(m_Win.CacheList.SelectedCache);
		}

#region Event Handlers
		protected virtual void OnViewClick (object sender, System.EventArgs e)
		{
			TreeIter iter;
			if (imagesView.SelectedItems[0] != null) {
				m_model.GetIter(out iter, imagesView.SelectedItems[0]);
				string  file = (string)m_model.GetValue (iter, 2);
				ImageDialog dlg = new ImageDialog(file);
				dlg.Run();
			}
		}
		
		protected virtual void OnOpenFolderClick (object sender, System.EventArgs e)
		{
			string imagesFolder = GetImagesFolder();
			if (!Directory.Exists(imagesFolder))
				Directory.CreateDirectory(imagesFolder);
			System.Diagnostics.Process.Start(imagesFolder);
		}
		
		protected virtual void OnGrabImagesClick (object sender, System.EventArgs e)
		{
			GrabImages ();
		}
		
		protected virtual void OnDoubleClick (object o, Gtk.ItemActivatedArgs args)
		{
			TreeIter iter;
			if (imagesView.SelectedItems[0] != null) {
				m_model.GetIter(out iter, imagesView.SelectedItems[0]);
				string  file = (string)m_model.GetValue (iter, 2);
				ImageDialog dlg = new ImageDialog(file);
				dlg.Run();
			}
		}
		
		
		void HandleImagesViewSelectionChanged (object sender, EventArgs e)
		{
			if (imagesView.SelectedItems.Length >0)
				viewButton.Sensitive = true;
			else
				viewButton.Sensitive = false;
		}
		
		protected virtual void OnRefesh (object sender, System.EventArgs e)
		{
			SetCache(m_Win.CacheList.SelectedCache);
		}
#endregion
	}
}
