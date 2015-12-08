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
using Gtk;
using Mono.Unix;
using ocmengine;

namespace ocmgtk
{


	public partial class ExportPOIDialog : Gtk.Dialog
	{
		IConfig m_config;
		
		public string FileName
		{
			get { return fileEntry.Text;}
			set { fileEntry.Text = value;}
		}
		
		public WaypointNameMode NameMode
		{
			get 
			{ 
				switch (nameCombo.Active)
				{
					case 0:
						return WaypointNameMode.CODE;
					default:
						return WaypointNameMode.NAME;
				}
			}
			set {
				if (value == WaypointNameMode.CODE)
					nameCombo.Active = 0;
				else
					nameCombo.Active = 1;
			}
		}
		
		public WaypointDescMode DescMode
		{
			
			get
			{
				switch(descCombo.Active)
				{
				case 0:
					return WaypointDescMode.DESC;
				case 1:
					return WaypointDescMode.CODESIZEANDHINT;
				case 2:
					return WaypointDescMode.CODESIZETYPE;
				default:
					return WaypointDescMode.FULL;
				}
			}
			set
			{
				if (value == WaypointDescMode.DESC)
					descCombo.Active = 0;
				else if (value == WaypointDescMode.CODESIZEANDHINT)
					descCombo.Active = 1;
				else if (value == WaypointDescMode.CODESIZETYPE)
					descCombo.Active = 2;
				else
					descCombo.Active = 3;
			}
		}
		
		public int CacheLimit
		{
			get
			{
				if (limitCaches.Active)
					return int.Parse(limitEntry.Text);
				else 
					return -1;
			}
			set
			{
				if (value > 0)
				{
					limitCaches.Active = true;
					limitEntry.Text = value.ToString();
				}
			}
		}
		
		public bool IncludeChildren
		{
			get
			{
				return includeChildrenCheck.Active;
			}
			set
			{
				includeChildrenCheck.Active = value;
			}
		}
		
		public string Category
		{
			get
			{
				return catagoryEntry.Text;
			}
			set
			{
				catagoryEntry.Text = value;
			}
		}
		
		public string BMPFile
		{
			get
			{
				if (includeBMPCheck.Active)
				{
					return bmpFile.Text;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					includeBMPCheck.Active = true;
					bmpFile.Text = value;
				}
			}
		}
		
		public double ProximityDistance
		{
			get
			{
				if (proximityAlertCheck.Active)
					return double.Parse(proxEntry.Text);
				return -1;
			}
			set
			{
				if (value > - 0)
				{
					proxEntry.Text = value.ToString();
					proximityAlertCheck.Active = true;
				}
				else
				{
					proximityAlertCheck.Active = false;
				}
					
			}
		}
		
		public string ProximityUnits
		{
			get
			{
				switch (proxCombo.Active)
				{
				case 0:
					return "m";
				default:
					return "s";
				}
			}
			set
			{
				if (value== "m")
					proxCombo.Active = 0;
				else
					proxCombo.Active = 1;
			}
		}
		
		public bool UsePlainText
		{
			get{ return useHTMLCheck.Active;}
			set{ useHTMLCheck.Active = value;}
		}
		
		public int LogLimit
		{
			get
			{
				if (logCheck.Active)
					return int.Parse(logEntry.Text);
				else
					return -1;
			}
			set
			{
				if (value > 0)
				{
					logCheck.Active = true;
					logEntry.Text = value.ToString();
				}
			}
		}
		
		public ExportPOIDialog (IConfig config)
		{
			this.Build ();
			m_config = config;
			fileEntry.Text = config.ExportPOIFile;
			BMPFile = config.ExportPOIBitmap;
			IncludeChildren = config.ExportPOIIncludeChildren;
			NameMode = config.ExportPOINameMode;
			DescMode = config.ExportPOIDescMode;
			Category = config.ExportPOICategory;
			CacheLimit = config.ExportPOICacheLimit;
			LogLimit = config.ExportPOILogLimit;
			ProximityUnits = config.ExportPOIProxUnits;
			ProximityDistance = config.ExportPOIProxDist;
			UsePlainText = config.ExportPOIForcePlain;
		}
		
		
		protected virtual void OnFileClick (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Select GPI location"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			dlg.CurrentName = "geocaches.gpi";
			FileFilter filter = new FileFilter ();
			filter.Name = "Garmin POI Database";
			filter.AddPattern ("*.gpi");			
			dlg.AddFilter (filter);			
			if (dlg.Run () == (int)ResponseType.Accept) {
				fileEntry.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		protected virtual void OnOKClick (object sender, System.EventArgs e)
		{
			m_config.ExportPOIFile = FileName;
			m_config.ExportPOIBitmap = BMPFile;
			m_config.ExportPOIIncludeChildren = IncludeChildren;
			m_config.ExportPOINameMode = NameMode;
			m_config.ExportPOIDescMode = DescMode;
			m_config.ExportPOICategory = Category;
			m_config.ExportPOICacheLimit = CacheLimit;
			m_config.ExportPOIForcePlain = UsePlainText;
			m_config.ExportPOIProxDist = ProximityDistance;
			m_config.ExportPOIProxUnits = ProximityUnits;
			m_config.ExportPOILogLimit = LogLimit;
			this.Hide();
		}
		
		protected virtual void OnCancelClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
		
		protected virtual void OnBMPClick (object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog (Catalog.GetString ("Select BMP File"), null, FileChooserAction.Save, Catalog.GetString ("Cancel"), ResponseType.Cancel, Catalog.GetString ("Select"), ResponseType.Accept);
			dlg.SetCurrentFolder (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments));
			FileFilter filter = new FileFilter ();
			filter.Name = "BMP Files";
			filter.AddPattern ("*.bmp");			
			dlg.AddFilter (filter);			
			if (dlg.Run () == (int)ResponseType.Accept) {
				bmpFile.Text = dlg.Filename;
			}
			dlg.Destroy ();
		}
		
		protected virtual void OnProxToggle (object sender, System.EventArgs e)
		{
			proxCombo.Sensitive = proximityAlertCheck.Active;
			proxEntry.Sensitive = proximityAlertCheck.Active;
		}
		
		protected virtual void OnCacheToggle (object sender, System.EventArgs e)
		{
			limitEntry.Sensitive = limitCaches.Active;
		}
		
		protected virtual void OnBMPToggle (object sender, System.EventArgs e)
		{
			bmpButton.Sensitive = includeBMPCheck.Active;
			bmpFile.Sensitive = includeBMPCheck.Active;
		}
		
		protected virtual void OnLogToggle (object sender, System.EventArgs e)
		{
			logEntry.Sensitive = logCheck.Active;
		}
		
		
		
		
		
		
		
		
	}
}
