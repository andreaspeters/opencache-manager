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
using ocmengine;
using Gtk;

namespace ocmgtk
{

	/// <summary>
	/// Defines a plugin
	/// </summary>
	public interface IOCMAddin
	{
		/// <summary>
		/// Called when a user selects a cache
		/// </summary>
		/// <param name="cache">
		/// The current selected cache<see cref="Geocache"/>
		/// </param>
		void OnCacheSelected(Geocache cache);
		
		/// <summary>
		/// Called to create a menu item in the tools for this plugin
		/// </summary>
		/// <returns>
		/// A new menu item, or null if there is no menu item for this plugin <see cref="MenuItem"/>
		/// </returns>
		MenuItem CreateMenuItem();
		
		/// <summary>
		/// Called to create a context menu item in the tools menu for this plugin.
		/// </summary>
		/// <returns>
		/// A new menu item, or null if there is no menu item for this plugin<see cref="MenuItem"/>
		/// </returns>
		MenuItem CreateContextMenuItem();
		
		/// <summary>
		/// Called to create a new widet tab
		/// </summary>
		/// <returns>
		/// A <see cref="Widget"/>
		/// </returns>
		Widget CreateInfoWidget();
		
		/// <summary>
		/// Called when this addin is uninstalled
		/// </summary>
		void OnUninstall();
		
		/// <summary>
		/// Called when this addin is installed
		/// </summary>
		void OnInstall();
	}
}
