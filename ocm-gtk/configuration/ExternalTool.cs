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
using System.Diagnostics;
using System.Collections.Generic;
using ocmengine;
using Mono.Unix;
using Gtk;

namespace ocmgtk
{

	[Serializable]
	public class ExternalTool
	{	
		
		string m_command, m_name;
		public String Command
		{
			get { return m_command;}
			set { m_command = value;}
		}
		
		public String Name
		{
			get { return m_name;}
			set { m_name = value;}
		}
		
		public ExternalTool()
		{
		}
		
		public ExternalTool (String name, String cmd)
		{
			m_command = cmd;
			m_name = name;
		}
		
		public void RunCommand(OCMMainWindow win)
		{
			string cmd = m_command;
			
			if (cmd.Contains("%gpxgc%"))
			{
				GPXWriter writer = new GPXWriter();
				String tempPath = System.IO.Path.GetTempPath();
				String tempFile = tempPath + "ocm_export.gpx";
				ExportProgressDialog dlg = new ExportProgressDialog(writer);	
				dlg.AutoClose = true;
				dlg.Title = Catalog.GetString("Preparing GPX File");
				dlg.WaypointsOnly = false;
				dlg.CompleteCommand = m_command.Replace("%gpxgc%", tempFile);
				dlg.Start(tempFile, win.CacheList.UnfilteredCaches, GPSProfileList.GetDefaultMappings(), win.App.CacheStore);
			}
			else if (cmd.Contains("%gpx%"))
			{
				GPXWriter writer = new GPXWriter();
				String tempPath = System.IO.Path.GetTempPath();
				String tempFile = tempPath + "ocm_export.gpx";
				ExportProgressDialog dlg = new ExportProgressDialog(writer);	
				dlg.AutoClose = true;
				dlg.Title = Catalog.GetString("Preparing GPX File");
				dlg.WaypointsOnly = true;
				dlg.CompleteCommand = m_command.Replace("%gpx%", tempFile);
				dlg.Start(tempFile, win.CacheList.UnfilteredCaches, GPSProfileList.GetDefaultMappings(), win.App.CacheStore);
			}
			else if (cmd.Contains("%selected%"))
			{
				if (win.CacheList.SelectedCache == null)
				{
					MessageDialog edlg = new MessageDialog(null, DialogFlags.Modal,
					                                      MessageType.Error, ButtonsType.Ok, 
					                                      Catalog.GetString("No cache selected."));
					edlg.Run();
					edlg.Hide();
					edlg.Dispose();
					return;
				}
				GPXWriter writer = new GPXWriter();
				String tempPath = System.IO.Path.GetTempPath();
				String tempFile = tempPath + "ocm_export.gpx";
				ExportProgressDialog dlg = new ExportProgressDialog(writer);	
				dlg.AutoClose = true;
				dlg.Title = Catalog.GetString("Preparing GPX File");
				dlg.WaypointsOnly = true;
				dlg.CompleteCommand = m_command.Replace("%selected%", tempFile);
				List<Geocache> cache = new List<Geocache>();
				cache.Add(win.CacheList.SelectedCache);
				dlg.Start(tempFile, cache, GPSProfileList.GetDefaultMappings(), win.App.CacheStore);
			}
			else if (cmd.Contains("%names%") && win.CacheList.UnfilteredCaches.Count>0 )
			{
				String names = "";
				foreach( Geocache cache in win.CacheList.UnfilteredCaches ) 
				{
					if( names.Length>0 )
					{
						names += ' ';
					}
					names += cache.Name;
				}
				Process.Start(Utilities.StringToStartInfo(cmd.Replace("%names%", names)));
			}
			else if (cmd.Contains("%selectedname%"))
			{
				if (win.CacheList.SelectedCache == null)
				{
					MessageDialog edlg = new MessageDialog(null, DialogFlags.Modal,
					                                      MessageType.Error, ButtonsType.Ok, 
					                                      Catalog.GetString("No cache selected."));
					edlg.Run();
					edlg.Hide();
					edlg.Dispose();
					return;
				}
				Process.Start(Utilities.StringToStartInfo(cmd.Replace("%selectedname%", win.CacheList.SelectedCache.Name)));
			}
			else if (cmd.Contains("%finds%"))
			{
				GPXWriter writer = new GPXWriter();
				writer.IsMyFinds = true;
				writer.MyFindsOwner = win.App.OwnerIDs[0];
				String tempPath = System.IO.Path.GetTempPath();
				String tempFile = tempPath + "ocm_finds.gpx";
				ExportProgressDialog dlg = new ExportProgressDialog(writer);	
				dlg.AutoClose = true;
				dlg.Title = Catalog.GetString("Preparing GPX File");
				dlg.WaypointsOnly = true;
				dlg.CompleteCommand = m_command.Replace("%finds%", tempFile);
				//dlg.Start(tempFile, Engine.getInstance().Store.GetFinds(), GPSProfileList.GetDefaultMappings(), mon.CacheStore);
			}
			else
			{
				Process.Start(Utilities.StringToStartInfo(m_command));
			}
		}
	}
}
