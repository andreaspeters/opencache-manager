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

namespace ocmengine
{


	public class CacheAttribute
	{
	
		string m_ID;
		bool m_include;
		string m_value;
		
		public string ID
		{
			get { return m_ID;}
			set { m_ID = value;}
		}
		
		public bool Include
		{
			get { return m_include;}
			set { m_include = value;}
		}
		
		public string AttrValue
		{
			get { return m_value;}
			set { m_value = value;}
		}
		
		
		public CacheAttribute ()
		{
		}
	}
}
