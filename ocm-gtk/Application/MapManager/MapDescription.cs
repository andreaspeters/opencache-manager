// 
//  Copyright 2011  tweety
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

namespace ocmgtk
{

	/// <summary>
	/// This class describes an OpenLayer Map for adding to the BrowserWidget/CacheMap.
	/// </summary>
	public class MapDescription
	{

		private string m_name;
		private string m_code;
		private bool m_active;
		private string m_covered;
		private bool m_baseLayer;

		public string Name {
			get { return m_name; }
			set { m_name = value; }
		}

		public string Code {
			get { return m_code; }
			set { m_code = value; }
		}

		public bool Active {
			get { return m_active; }
			set { m_active = value; }
		}

		public string Covered {
			get { return m_covered; }
			set { m_covered = value; }
		}

		public bool BaseLayer {
			get { return m_baseLayer; }
			set { m_baseLayer = value; }
		}

		public MapDescription ()
		{
		}

		public MapDescription (string name, string code, bool active, string covered, bool baseLayer)
		{
			m_name = name;
			m_code = code;
			m_active = active;
			m_covered = covered;
			m_baseLayer = baseLayer;
		}
	}
}
