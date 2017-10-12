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
	public partial class OCMQueryPage4 : Gtk.Bin
	{
		public bool HasNotes
		{
			get { return hasNotesCheck.Active;}
			set { hasNotesCheck.Active = value;}
		}
		
		public string ChildrenFilter
		{
			get
			{
				if (!hasChildCheck.Active)
					return null;
				switch (childCombo.Active)
				{
				case 0:
					return "Final Location";
				case 1:
					return "Parking Area";
				case 2:
					return "Question to Answer";
				case 3:
					return "Reference Point";
				case 4:
					return "Stages of a Multicache";
				case 5:
					return "Trailhead";
				default:
					return "Other";
				}
			}
			set
			{
				if (value == null)
					return;
				else if (value.Equals("Final Location"))
					childCombo.Active =  0;
				else if (value.Equals("Parking Area"))
					childCombo.Active =  1;
				else if (value.Equals("Question to Answer"))
					childCombo.Active =  2;
				else if (value.Equals("Reference Point"))
					childCombo.Active =  3;
				else if (value.Equals("Stages of a Multicache"))
					childCombo.Active =  4;
				else if (value.Equals("Trailhead"))
					childCombo.Active =  5;
				else 
					childCombo.Active =  6;
				hasChildCheck.Active = true;
			}
		}
		
		public string NoChildrenFilter
		{
			get
			{
				if (!hasNoChildCheck.Active)
					return null;
				switch (noChildCombo.Active)
				{
				case 0:
					return "Final Location";
				case 1:
					return "Parking Area";
				case 2:
					return "Question to Answer";
				case 3:
					return "Reference Point";
				case 4:
					return "Stages of a Multicache";
				case 5:
					return "Trailhead";
				default:
					return "Other";
				}
			}
			set
			{
				if (value == null)
					return;
				else if (value.Equals("Final Location"))
					noChildCombo.Active =  0;
				else if (value.Equals("Parking Area"))
					noChildCombo.Active =  1;
				else if (value.Equals("Question to Answer"))
					noChildCombo.Active =  2;
				else if (value.Equals("Reference Point"))
					noChildCombo.Active =  3;
				else if (value.Equals("Stages of a Multicache"))
					noChildCombo.Active =  4;
				else if (value.Equals("Trailhead"))
					noChildCombo.Active =  5;
				else 
					noChildCombo.Active =  6;
				hasNoChildCheck.Active = true;
			}
		}
		
		public bool HasCorrectedCoords
		{
			get { return hasCorrectedCheck.Active;}
			set { hasCorrectedCheck.Active = value;}
		}
		
		public bool DoesNotHaveCorrectedCoords
		{
			get { return noCorrectCheck.Active;}
			set { noCorrectCheck.Active = value;}
		}
		
		public string User1
		{
			get
			{ 
				if (ud1Check.Active)
					return uEntry1.Text;
				else
					return null;
			}
			set 
			{ 
				if (!String.IsNullOrEmpty(value))
				{
					uEntry1.Text = value;
					ud1Check.Active = true;
				}
			}
		}
		
		public string User2
		{
			get
			{ 
				if (ud2Check.Active)
					return uEntry2.Text;
				else
					return null;
			}
			set 
			{ 
				if (!String.IsNullOrEmpty(value))
				{
					uEntry2.Text = value;
					ud2Check.Active = true;
				}
			}
		}
		
		public string User3
		{
			get
			{ 
				if (ud3Check.Active)
					return uEntry3.Text;
				else
					return null;
			}
			set 
			{ 
				if (!String.IsNullOrEmpty(value))
				{
					uEntry3.Text = value;
					ud3Check.Active = true;
				}
			}
		}
		
		public string User4
		{
			get
			{ 
				if (ud4Check.Active)
					return uEntry4.Text;
				else
					return null;
			}
			set 
			{ 
				if (!String.IsNullOrEmpty(value))
				{
					uEntry4.Text = value;
					ud4Check.Active = true;
				}
			}
		}
		
		

		public OCMQueryPage4 ()
		{
			this.Build ();
		}
		
		protected virtual void OnHasNoChildToggle (object sender, System.EventArgs e)
		{
			noChildCombo.Sensitive = hasNoChildCheck.Active;
		}
		
		protected virtual void OnHasChildToggle (object sender, System.EventArgs e)
		{
			childCombo.Sensitive = hasChildCheck.Active;
		}
		
		protected virtual void OnU1Toggle (object sender, System.EventArgs e)
		{
			uEntry1.Sensitive = ud1Check.Active;
		}
		
		protected virtual void OnU2Toggle (object sender, System.EventArgs e)
		{
			uEntry2.Sensitive = ud2Check.Active;
		}
		
		protected virtual void OnU3Toggle (object sender, System.EventArgs e)
		{
			uEntry3.Sensitive = ud3Check.Active;
		}
		
		protected virtual void OnU4Toggle (object sender, System.EventArgs e)
		{
			uEntry4.Sensitive = ud4Check.Active;
		}
		
		
		
		
		
		
		
	}
}
