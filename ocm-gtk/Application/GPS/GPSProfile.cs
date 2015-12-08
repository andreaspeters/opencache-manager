// 
//  Copyright 2011  Kyle Campbell
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

namespace ocmgtk
{


	[Serializable]
	public class GPSProfile
	{
		public int CacheLimit = -1;
		public string OutputFile = null;
		public string BabelFormat = null;
		public string FieldNotesFile = null;
		public string Name = null;
		public int LogLimit = -1;
		public string OtherProperties = null;
		public bool IncludeAttributes = true;
		public bool ForcePlainText = false;
		public WaypointNameMode NameMode = WaypointNameMode.CODE;
		public WaypointDescMode DescMode = WaypointDescMode.DESC;
		public Dictionary<string, string> WaypointMappings = null;
		public DateTime LastFieldNoteScan = DateTime.MinValue;		

		public GPSProfile ()
		{
		}
	}
}
