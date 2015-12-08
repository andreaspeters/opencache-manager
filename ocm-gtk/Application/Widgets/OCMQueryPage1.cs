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

namespace ocmgtk
{

	[System.ComponentModel.ToolboxItem(true)]
	public partial class OCMQueryPage1 : Gtk.Bin
	{
		#region Properties
		public String TerrValue {
			get {
				if (terrainCheck.Active)
					return terrValue.ActiveText;
				else
					return null;
			}
			set {
				if (value != null) {
					terrainCheck.Active = true;
					if (value.Equals ("1"))
						terrValue.Active = 0; else if (value.Equals ("1.5"))
						terrValue.Active = 1; else if (value.Equals ("2"))
						terrValue.Active = 2; else if (value.Equals ("2.5"))
						terrValue.Active = 3; else if (value.Equals ("3"))
						terrValue.Active = 4; else if (value.Equals ("3.5"))
						terrValue.Active = 5; else if (value.Equals ("4"))
						terrValue.Active = 6; else if (value.Equals ("4.5"))
						terrValue.Active = 7; else if (value.Equals ("5"))
						terrValue.Active = 8;
				} else {
					terrValue.Active = 0;
				}
			}
		}

		public String TerrOperator {
			get {
				if (!terrainCheck.Active)
					return null;
				switch (terrRange.Active) {
				case 0:
					return ">";
				case 1:
					return ">=";
				case 2:
					return "==";
				case 3:
					return "<=";
				default:
					return "<";
				}
			}
			set {
				if (value == null) {
					terrainCheck.Active = false;
					return;
				}
				if (value == ">")
					terrRange.Active = 0; else if (value == ">=")
					terrRange.Active = 1; else if (value == "==")
					terrRange.Active = 2; else if (value == "<=")
					terrRange.Active = 3; else if (value == "<")
					terrRange.Active = 4;
			}
		}

		public String DifficultyValue {
			get {
				if (diffCheck.Active)
					return diffValue.ActiveText;
				else
					return null;
			}
			set {
				if (value != null) {
					diffCheck.Active = true;
					if (value.Equals ("1"))
						diffValue.Active = 0; else if (value.Equals ("1.5"))
						diffValue.Active = 1; else if (value.Equals ("2"))
						diffValue.Active = 2; else if (value.Equals ("2.5"))
						diffValue.Active = 3; else if (value.Equals ("3"))
						diffValue.Active = 4; else if (value.Equals ("3.5"))
						diffValue.Active = 5; else if (value.Equals ("4"))
						diffValue.Active = 6; else if (value.Equals ("4.5"))
						diffValue.Active = 7; else if (value.Equals ("5"))
						diffValue.Active = 8;
				} else {
					diffCheck.Active = false;
				}
			}
		}

		public String DifficultyOperator {
			get {
				if (!diffCheck.Active)
					return null;
				switch (diffRange.Active) {
				case 0:
					return ">";
				case 1:
					return ">=";
				case 2:
					return "==";
				case 3:
					return "<=";
				default:
					return "<";
				}
			}
			set {
				if (value == null) {
					diffCheck.Active = false;
					return;
				}
				if (value == ">")
					diffRange.Active = 0; else if (value == ">=")
					diffRange.Active = 1; else if (value == "==")
					diffRange.Active = 2; else if (value == "<=")
					diffRange.Active = 3; else if (value == "<")
					diffRange.Active = 4;
			}
		}

		public List<String> SelectedCacheTypes {
			get {
				if (allCacheRadio.Active)
					return null;
				List<String> types = new List<String> ();
				if (tradCheck.Active)
					types.Add ("TRADITIONAL");
				if (multiCheck.Active)
					types.Add ("MULTI");
				if (apeCheck.Active)
					types.Add ("APE");
				if (unkCheck.Active)
					types.Add ("MYSTERY");
				if (lettCheck.Active)
					types.Add ("LETTERBOX");
				if (evntCheck.Active)
					types.Add ("EVENT");
				if (wherigoCheck.Active)
					types.Add ("WHERIGO");
				if (megaCheck.Active)
					types.Add ("MEGAEVENT");
				if (citoCheck.Active)
					types.Add ("CITO");
				if (earCheck.Active)
					types.Add ("EARTH");
				if (advCheck.Active)
					types.Add ("MAZE");
				if (virtCheck.Active)
					types.Add ("VIRTUAL");
				if (webCheck.Active)
					types.Add ("WEBCAM");
				if (locCheck.Active)
					types.Add ("REVERSE");
				return types;
			}
			set {
				if (value == null)
				{
					allCacheRadio.Active = true;
					return;
				}				
				selCacheRadio.Active = true;
				IEnumerator<String> ctype = value.GetEnumerator();
				while (ctype.MoveNext())
				{
					if (ctype.Current == "TRADITIONAL")
						tradCheck.Active = true;
					else if (ctype.Current == "MULTI")
						multiCheck.Active = true;
					else if (ctype.Current == "APE")
						apeCheck.Active = true;
					else if (ctype.Current == "MYSTERY")
						unkCheck.Active = true;
					else if (ctype.Current == "LETTERBOX")
						lettCheck.Active = true;
					else if (ctype.Current == "EVENT")
						evntCheck.Active = true;
					else if (ctype.Current == "WHERIGO")
						wherigoCheck.Active = true;
					else if (ctype.Current == "MEGAEVENT")
						megaCheck.Active = true;
					else if (ctype.Current == "CITO")
						citoCheck.Active = true;
					else if (ctype.Current == "EARTH")
						earCheck.Active = true;
					else if (ctype.Current == "MAZE")
						advCheck.Active = true;
					else if (ctype.Current == "MEGAEVENT")
						megaCheck.Active = true;
					else if (ctype.Current == "VIRTUAL")
						virtCheck.Active = true;
					else if (ctype.Current == "WEBCAM")
						webCheck.Active = true;
					else if (ctype.Current == "REVERSE")
						locCheck.Active = true;
				}
			}
		}
		
		public List<String> CacheSources
		{
			get {
				if (!providerCheck.Active)
					return null;
				List<String> sources = new List<String>();
				if (gcComCheck.Active)
					sources.Add("GC");
				if (openCacheCheck.Active)
					sources.Add("O");
				if (NaviCacheCheck.Active)
					sources.Add("NC");
				if (terraCachingCheck.Active)
				{
					sources.Add("TC");
					sources.Add("LC");
				}
				if (wayMarkingCheck.Active)
				{
					sources.Add("WM");
				}
				return sources;
			}
			set
			{
				providerCheck.Active = true;
				if (value.Contains("GC"))
					gcComCheck.Active = true;
				if (value.Contains("O"))
					openCacheCheck.Active = true;
				if (value.Contains("NC"))
					NaviCacheCheck.Active = true;
				if (value.Contains("TC"))
					terraCachingCheck.Active = true;
				if (value.Contains("WM"))
					wayMarkingCheck.Active = true;
			}
		}

		#endregion Properties

		public OCMQueryPage1 ()
		{
			this.Build ();
		}

		protected virtual void OnDiffToggle (object sender, System.EventArgs e)
		{
			diffRange.Sensitive = diffCheck.Active;
			diffValue.Sensitive = diffCheck.Active;
		}

		protected virtual void OnTerrToggle (object sender, System.EventArgs e)
		{
			
			terrRange.Sensitive = terrainCheck.Active;
			terrValue.Sensitive = terrainCheck.Active;
		}

		protected virtual void OnCacheToggle (object sender, System.EventArgs e)
		{
			selCacheFrame.Sensitive = selCacheRadio.Active;
		}
		protected virtual void OnSourceToggle (object sender, System.EventArgs e)
		{
			sourceFrame.Sensitive = providerCheck.Active;
		}
		
		
	}
}
