// 
//  Copyright 2011  Florian Plähn
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
using System.Text;
using System.Collections.Generic;
using System.Xml.XPath;

namespace ocmgtk
{
	public class MapManager
	{
	/*
		private string m_baseDirectory;
		
		public MapManager (string baseDirectory) 
		{
			if (!Directory.Exists(baseDirectory)) {
				throw new DirectoryNotFoundException("Didn't find directory: " + baseDirectory);
			}
			m_baseDirectory = baseDirectory;
		}

	
		public void addMaps(BrowserWidget browserWidget) {
			foreach (string file in Directory.GetFiles(m_baseDirectory)) {
				if (file.EndsWith(".xml")) {
					XPathNavigator nav = new XPathDocument (file).CreateNavigator();
					
					XPathNodeIterator maps = nav.Select("/maps/map");
					while (maps.MoveNext()) {
						string code = maps.Current.SelectSingleNode("code").Value;
						int layer = maps.Current.SelectSingleNode("layer").ValueAsInt;
						browserWidget.AddMap(code, layer);	
					}
				}
			}
		}
	*/
		
		public static List<MapDescription> GetMapsFromFile(string file) {
			return GetMapsFromReader(File.OpenText(file));
		}
		
		public static List<MapDescription> GetMapsFromString(string xml) {
			return GetMapsFromReader(new StringReader(xml));
		}

		public static List<MapDescription> GetMapsFromReader(TextReader reader) {
			List<MapDescription> mapList = new List<MapDescription>();
			XPathNavigator nav = new XPathDocument(reader).CreateNavigator();
			XPathNodeIterator maps = nav.Select("/maps/map");
			while (maps.MoveNext()) {
				MapDescription map = new MapDescription();
				map.Name = maps.Current.SelectSingleNode("name").Value;
				map.Code = maps.Current.SelectSingleNode("code").Value;
				map.BaseLayer = maps.Current.SelectSingleNode("baseLayer").ValueAsBoolean;
				map.Covered = maps.Current.SelectSingleNode("covered").Value;
				map.Active = maps.Current.SelectSingleNode("active").ValueAsBoolean;
				mapList.Add(map);
			}
			return mapList;
		}

		public static String CreateXML(List<MapDescription> maps) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<maps>");
			foreach(MapDescription map in maps) {
				sb.AppendLine("\t<map>");
				sb.AppendLine("\t\t<name>" + map.Name + "</name>");
				sb.AppendLine("\t\t<code><![CDATA[" + map.Code + "]]></code>");
				sb.AppendLine("\t\t<baseLayer>" + map.BaseLayer.ToString().ToLower() + "</baseLayer>");
				sb.AppendLine("\t\t<covered>" + map.Covered + "</covered>");
				sb.AppendLine("\t\t<active>" + map.Active.ToString().ToLower() + "</active>");
				sb.AppendLine("\t</map>");
			}
			sb.AppendLine("</maps>");
			return sb.ToString();
		}
	}
}
