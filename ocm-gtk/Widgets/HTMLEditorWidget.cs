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
using Gtk;
using WebKit;

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class HTMLEditorWidget : Gtk.Bin
	{
		public delegate void TextChangedEventHandler(object sender, EventArgs args);
		public event TextChangedEventHandler TextChanged;
		
		//TODO: Use Webkit or gtkhtml for a fancier note widget
		
		public string Text
		{
			get { return editor.Buffer.Text;}
			set { editor.Buffer.Text = value;}
		}
		
		
		
		public HTMLEditorWidget ()
		{
			this.Build ();
			editor.Buffer.Changed += HandleEditorBufferChanged;
		}

		void HandleEditorBufferChanged (object sender, EventArgs e)
		{
			if (TextChanged != null)
				TextChanged(this, new EventArgs());
		}		
	}
}
