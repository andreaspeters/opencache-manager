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
	
	public class DegreeMinutes
	{
		int m_degrees;
		double m_minutes;
		bool m_isNegative;
		
		public int Degrees
		{
			get { return m_degrees;}
		}
		
		public double Minutes
		{
			get { return m_minutes;}
		}
		
		public bool IsNegative
		{
			get { return m_isNegative; }
		}
		
		public DegreeMinutes(int degrees, double minutes, bool isNegative)
		{
			m_degrees = degrees;
			m_minutes = minutes;
			m_isNegative = isNegative;
		}
		
		public override string ToString ()
		{
			return string.Format("{0}° {1}", Degrees, Minutes);
		}
		
		public override bool Equals (object obj)
		{
			if (obj is DegreeMinutes)
			{
				DegreeMinutes other = obj as DegreeMinutes;
				if ((other.Degrees == Degrees)&&(Math.Round(other.Minutes, 5) == Math.Round(Minutes, 5)) && (other.IsNegative == m_isNegative))
				    return true;
			}
			return false;
		}
		
		public DegreeMinutes (double dd_coordinate)
		{		
			double degrees;
			double decimal_minutes;
			if (dd_coordinate >= 0)
			{
				degrees = Math.Floor(dd_coordinate);
				decimal_minutes = dd_coordinate - degrees;
				m_isNegative = false;
			}
			else
			{
				m_isNegative = true;
				degrees = Math.Ceiling(dd_coordinate);
				decimal_minutes = (dd_coordinate - degrees) * -1;
			}
			decimal_minutes = decimal_minutes * 60;
			m_degrees = (int) degrees;
			m_minutes = decimal_minutes;
		}
		
		public double GetDecimalDegrees()
		{
			if (!m_isNegative){
				return m_degrees + (m_minutes/60);
			}
			else
			{
				int deg = m_degrees * -1;
				double val = deg + (m_minutes/60);
				return val * -1;
			}
		}


	}
}
