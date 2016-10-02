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

namespace ocmgtk
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class OCMQueryPage6 : Gtk.Bin
	{

		public OCMQueryPage6 ()
		{
			this.Build ();
		}
		
		public DateTime FoundOn
		{
			get
			{
				if (foundCheck.Active && (foundCombo.Active == 0))
					return foundDateEntry.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					foundCheck.Active = true;
					foundDateEntry.Date = value;
					foundCombo.Active =0;
				}
					
			}
		}
		
		public DateTime FoundBefore
		{
			get
			{
				if (foundCheck.Active && (foundCombo.Active == 2))
					return foundDateEntry.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					foundCheck.Active = true;
					foundDateEntry.Date = value;
					foundCombo.Active = 2;
				}
					
			}
		}
		
		public DateTime FoundAfter
		{
			get
			{
				if (foundCheck.Active && (foundCombo.Active == 1))
					return foundDateEntry.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					foundCheck.Active = true;
					foundDateEntry.Date = value;
					foundCombo.Active = 1;
				}
					
			}
		}
		
		public DateTime FoundAnyoneBefore
		{
			get
			{
				if (lFoundAnyOneCheck.Active && lFoundAnyoneCombo.Active == 0)
					return lFoundAnyoneDate.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					lFoundAnyOneCheck.Active = true;
					lFoundAnyoneDate.Date = value;
					lFoundAnyoneCombo.Active = 0;
				}
			}
		}
		
		public DateTime FoundAnyoneAfter
		{
			get
			{
				if (lFoundAnyOneCheck.Active && lFoundAnyoneCombo.Active == 1)
					return lFoundAnyoneDate.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					lFoundAnyOneCheck.Active = true;
					lFoundAnyoneDate.Date = value;
					lFoundAnyoneCombo.Active = 1;
				}
			}
		}
		
		public DateTime InfoAfter
		{
			get
			{
				if (lastUpdateCheck.Active && lUpdateDate.Active && lastUpdateCombo.Active == 1)
					return lastUpdateDate.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value == DateTime.MinValue)
					return;
				lastUpdateDate.Date = value;
				lastUpdateCombo.Active = 1;
				lastUpdateCheck.Active = true;
			}
		}
		
		public DateTime InfoBefore
		{
			get 
			{
				if (lastUpdateCheck.Active && lUpdateDate.Active && lastUpdateCombo.Active == 0)
					return lastUpdateDate.Date;
				return DateTime.MinValue;
			}
			set
			{
				if (value == DateTime.MinValue)
					return;
				lastUpdateDate.Date = value;
				lastUpdateCombo.Active = 0;
				lUpdateDate.Active = true;
				lastUpdateCheck.Active = true;
			}
		}
		
		public int InfoWithin
		{
			get
			{
				if (lUpdateWithin.Active && lastUpdateCheck.Active)
				{
					return int.Parse(lupdatewithinEntry.Text);
				}
				return -1;
			}
			set
			{
				if (value > 0)
				{
					lUpdateWithin.Active = true;
					lastUpdateCheck.Active = true;
					lupdatewithinEntry.Text = value.ToString();
				}
			}
		}
		
		public int InfoNotWithin
		{
			get
			{
				if (nUpdateWithin.Active && lastUpdateCheck.Active)
				{
					return int.Parse(nupdatedWithinEntry.Text);
				}
				return -1;
			}
			set
			{
				if (value > 0)
				{
					nUpdateWithin.Active = true;
					lastUpdateCheck.Active = true;
					nupdatedWithinEntry.Text = value.ToString();
				}
			}
		}
		
		protected virtual void OnFoundCheckToggle (object sender, System.EventArgs e)
		{
			foundDateEntry.Sensitive = foundCheck.Active;
			foundCombo.Sensitive = foundCheck.Active;
		}
		
		protected virtual void OnUpdateToggle (object sender, System.EventArgs e)
		{
			updateFrame.Sensitive = lastUpdateCheck.Active;
			
		}
		protected virtual void OnLUpdateToggle (object sender, System.EventArgs e)
		{
			lastUpdateCombo.Sensitive = lUpdateDate.Active;
			lastUpdateDate.Sensitive = lUpdateDate.Active;
		}
		
		protected virtual void OnLWithinToggle (object sender, System.EventArgs e)
		{
			lupdatewithinEntry.Sensitive = lUpdateWithin.Active;
		}
		
		protected virtual void OnNotWithinToggle (object sender, System.EventArgs e)
		{
			nupdatedWithinEntry.Sensitive = nUpdateWithin.Active;
		}
		
		protected virtual void OnLFoundAnyToggle (object sender, System.EventArgs e)
		{
			lFoundAnyoneCombo.Sensitive = lFoundAnyOneCheck.Active;
			lFoundAnyoneDate.Sensitive = lFoundAnyOneCheck.Active;
		}
		
		
		
		
		
	}
}
