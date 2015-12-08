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

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class SymbolChooser : Gtk.Bin
	{
		public string SymbolName
		{
			get {return symbolCombo.Entry.Text;}
			set { symbolCombo.Entry.Text = value;}
		}
		
		public string Key
		{
			get { return symbolLabel.Text;}
		}
		
		public SymbolChooser (String key, String def)
		{
			this.Build ();
			symbolLabel.Text = key;
			symbolCombo.Entry.Text = def;
			this.ShowAll();
		}
	}
}
