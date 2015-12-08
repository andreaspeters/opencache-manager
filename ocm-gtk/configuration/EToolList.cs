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
using System.IO;
using Gtk;
using Mono.Unix;
using System.Runtime.Serialization.Formatters.Binary;

namespace ocmgtk
{

	[Serializable]
	public class EToolList
	{
		private Dictionary<string, ExternalTool> m_tools = new Dictionary<string, ExternalTool>();
		
		[NonSerializedAttribute]
		OCMMainWindow m_Win;
		public OCMMainWindow MainWin
		{
			set { m_Win = value;}
		}
		
		
		public ExternalTool[] ToolArray
		{
			get{
				ExternalTool[] tools = new ExternalTool[m_tools.Count];
				m_tools.Values.CopyTo(tools, 0);
				return tools;
			}
			set{
				foreach(ExternalTool tool in value)
				{
					m_tools.Add(tool.Name, tool);
				}				
			}
		}

		public EToolList ()
		{
		}
		
		public void AddTool(ExternalTool tool)
		{
			if (m_tools.ContainsKey(tool.Name))
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
				                                      String.Format(Catalog.GetString("Are you sure you wish to " +
				                                      	"overwrite \"{0}\"?"), tool.Name));
				if ((int) ResponseType.Yes != dlg.Run())
				{
					dlg.Hide();
					return;
				}
				else
				{
					dlg.Hide();
					m_tools.Remove(tool.Name);
				}
			}
			m_tools.Add(tool.Name, tool);
			UpdateETFile();
		}
		
		public void DeleteTool(string name)
		{
			m_tools.Remove(name);
			UpdateETFile();
		}
		
		public static EToolList LoadEToolList()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!File.Exists(path + "/ocm/etools.oqf"))
			{
				return new EToolList();
			}
			FileStream fs = new FileStream(path + "/ocm/etools.oqf", FileMode.Open, FileAccess.Read);	
			BinaryFormatter ser = new BinaryFormatter();
			System.Object filters = ser.Deserialize(fs);
			fs.Close();
			return filters as EToolList;
		}
		
		private void UpdateETFile()
		{
			String path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);
			if (!Directory.Exists("ocm"))
				Directory.CreateDirectory(path + "/ocm");
			path = path + "/ocm";
			BinaryFormatter ser = new BinaryFormatter();
			FileStream fs = new FileStream(path + "/etools.oqf", FileMode.Create, FileAccess.ReadWrite);
			ser.Serialize(fs, this);
			fs.Close();
		}
		
		public Menu BuildEToolMenu()
		{
			Menu etMenu = new Menu();
			int iCount = 0;
			foreach (ExternalTool tool in m_tools.Values)
			{
				Gtk.Action action = new Gtk.Action(tool.Name, tool.Name);
				etMenu.Append(action.CreateMenuItem());
				action.Activated += HandleActionActivated;
				iCount ++;
				
			}
			return etMenu;
		}
		
		public ExternalTool GetTool(String name)
		{
			return m_tools[name];
		}
		
		void HandleActionActivated (object sender, EventArgs e)
		{
			m_tools[((sender) as Gtk.Action).Name].RunCommand(m_Win);
		}
	}
}
