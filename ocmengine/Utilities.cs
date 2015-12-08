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
using Mono.Unix;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ocmengine
{
	
	
	public class Utilities
	{
		const string DEGREE_MINUTES = "^\\s*([N|S]){1}\\W*([0-9]+)\\W*([0-9]+\\.[0-9]+)'?,?\\s+([E|W]){1}\\W*([0-9]+)\\W*([0-9]+\\.[0-9]+)'?\\s*$";
		const string DEC_DEGREES_1= "^\\s*([N|S]){1}\\s*([0-9]+\\.[0-9]+)'?,?\\s+([E|W]){1}\\s*([0-9]+\\.[0-9]+)'?\\s*$";
		const string DEC_DEGREES_2 = "^\\s*([-0-9]+\\.[0-9]+),?\\s*([-0-9]+\\.[0-9]+)\\s*$";
		const string DMS = "^\\s*([N|S]){1}\\s*([0-9]+)\\W*([0-9]+)\\W*([0-9]+)\\W*([E|W]){1}\\W*([0-9]+)\\W*([0-9]+)\\W*([0-9]+)\\W*\\s*$";
		const string HTML =@"<[^>]*>";
		const string BR = @"<[Bb][Rr]\s?/?>";
		const string END_TR = @"</\s?[Tt][Rr]>";
		const string END_LI = @"</\s?[Ll][Ii]>";
		const string END_P = @"</\s?[Pp]>";
		const string IMG = @"<[Ii][Mm][Gg][^>]*>";
		
		/// <summary>
		/// See http://www.movable-type.co.uk/scripts/latlong.html
		/// </summary>
		/// <param name="lat1">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lat2">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lon1">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lon2">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public static double calculateDistance(double lat1, double lat2, double lon1, double lon2)
		{
			if ((lat1 == lat2) && (lon1 == lon2))
				return 0;
			int R = 6371;
			lat1 = toRad(lat1);
			lat2 = toRad(lat2);
			lon1 = toRad(lon1);
			lon2 = toRad(lon2);
			
			double d = Math.Acos(Math.Sin(lat1)*Math.Sin(lat2) + 
			Math.Cos(lat1)*Math.Cos(lat2) *Math.Cos(lon2-lon1)) * R;
			return d;
		}
		
		/// <summary>
		/// See http://www.movable-type.co.uk/scripts/latlong.html
		/// </summary>
		/// <param name="lat1">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lat2">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lon1">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <param name="lon2">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public static int calculateBearing(double lat1, double lat2, double lon1, double lon2)
		{
			lat1 = toRad(lat1);
			lat2 = toRad(lat2);
			lon1 = toRad(lon1);
			lon2 = toRad(lon2);
			double dLon = lon2-lon1;
			double y = Math.Sin(dLon) * Math.Cos(lat2);
			double x = Math.Cos(lat1)*Math.Sin(lat2) - Math.Sin(lat1)*Math.Cos(lat2)*Math.Cos(dLon);	
			double b = Math.Atan2(y, x);
			return (int) (toDegrees(b) + 360) % 360;
		}
				
		public static double toRad(double degrees)
		{
			return degrees * (Math.PI/180);
		}
		
		public static double toDegrees(double radians)
		{
			return radians * (180/Math.PI);
		}
		
		public static string getCoordString(double lat, double lon)
		{
			return getCoordString(new DegreeMinutes(lat), new DegreeMinutes(lon));
		}
		
		public static string getCoordStringCN(double lat, double lon)
		{
			return getLatStringCN(new DegreeMinutes(lat)) + " " + getLonStringCN(new DegreeMinutes(lon));
		}
		
		public static string getCoordString(DegreeMinutes lat, DegreeMinutes lon)
		{
			return getLatString(lat) + " " + getLonString(lon);
		}
		
		public static string getCoordStringCN(DegreeMinutes lat, DegreeMinutes lon)
		{
			return getLatStringCN(lat) + " " + getLonStringCN(lon);
		}
		
		public static string getLatString(DegreeMinutes lat)
		{
				
			String co_ordinate = "";
			
			if (lat.GetDecimalDegrees() > 0)
				co_ordinate += String.Format(Catalog.GetString("N {0}° {1}"), lat.Degrees,lat.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
			else
				co_ordinate += String.Format(Catalog.GetString("S {0}° {1}"), lat.Degrees * -1,  lat.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
				
			return co_ordinate;
		}
		
		public static string getLatStringCN(DegreeMinutes lat)
		{
				
			String co_ordinate = "";
			
			if (lat.GetDecimalDegrees() > 0)
				co_ordinate += String.Format("N {0}° {1}", lat.Degrees,lat.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
			else
				co_ordinate += String.Format("S {0}° {1}", lat.Degrees * -1,  lat.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
				
			return co_ordinate;
		}
		
		public static string getLonString(DegreeMinutes lon)
		{
			String co_ordinate = "";
			
			if (lon.GetDecimalDegrees() > 0)
				co_ordinate += String.Format(Catalog.GetString("  E {0}° {1}"), lon.Degrees, lon.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
			else
				co_ordinate += String.Format(Catalog.GetString("  W {0}° {1}"), lon.Degrees *-1 , lon.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
		
			return co_ordinate;
		}
		
		public static string getLonStringCN(DegreeMinutes lon)
		{
			String co_ordinate = "";
			
			if (lon.GetDecimalDegrees() > 0)
				co_ordinate += String.Format("  E {0}° {1}", lon.Degrees, lon.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
			else
				co_ordinate += String.Format("  W {0}° {1}", lon.Degrees *-1 , lon.Minutes.ToString("0.000", CultureInfo.InvariantCulture));
		
			return co_ordinate;
		}
		
		public static DegreeMinutes[] ParseCoordString(String val)
		{
			DegreeMinutes[] coord = new DegreeMinutes[2];
			if (Regex.IsMatch(val, DEC_DEGREES_1))
			{
				Match match = Regex.Match(val, DEC_DEGREES_1);
				bool signLat = match.Groups[1].Value == "S";
				double absLat = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
				System.Console.WriteLine(absLat.ToString());
				System.Console.WriteLine(match.Groups[2].Value);
				coord[0] = new DegreeMinutes(signLat ? -absLat: absLat);
					
		        bool signLon    = match.Groups[3].Value == "W";
		        double absLon    = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
		        coord[1] = new DegreeMinutes(signLon ? -absLon : absLon);
			}
			else if (Regex.IsMatch(val, DEGREE_MINUTES))
			{
				Match match = Regex.Match(val, DEGREE_MINUTES);
				int degLat = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
				bool isNeg = false;
				if (match.Groups[1].Value == "S")
				{
					degLat = degLat * -1;
					isNeg = true;
				}
				double minLat = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
				DegreeMinutes lat = new DegreeMinutes(degLat, minLat, isNeg);
				isNeg = false;
				int degLon = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
				if (match.Groups[4].Value == "W")
				{
					degLon = degLon * -1;
					isNeg = true;
				}
				double minLon = double.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
				DegreeMinutes lon = new DegreeMinutes(degLon, minLon, isNeg);
				
				coord[0] = lat;
				coord[1] = lon;		
			}
			else if (Regex.IsMatch(val, DEC_DEGREES_2))
			{
				Match match = Regex.Match(val, DEC_DEGREES_2);
				coord[0] = new DegreeMinutes(Double.Parse(match.Groups[1].Value));
				coord[1] = new DegreeMinutes(Double.Parse(match.Groups[2].Value));
			}
			else if (Regex.IsMatch(val, DMS))
			{
				throw new Exception("NOT_YET_IMPLEMENTED");
			}
			else
			{
				System.Console.WriteLine(val);
				throw new Exception(Catalog.GetString("Unknown Coordinate Format"));
			}
			return coord;
					
				/* bool signLat    = re1.cap(1) == "S";
        int degLat      = re1.cap(2).toInt();
        float minLat    = re1.cap(3).toDouble();

        GPS_Math_DegMin_To_Deg(signLat, degLat, minLat, lat);

        bool signLon    = re1.cap(4) == "W";
        int degLon      = re1.cap(5).toInt();
        float minLon    = re1.cap(6).toDouble();

        GPS_Math_DegMin_To_Deg(signLon, degLon, minLon, lon);
			}
			
			
			DegreeMinutes val = new DegreeMinutes(0.0);
			return val;
			/*
			 * bool GPS_Math_Str_To_Deg(const QString& str, float& lon, float& lat, bool silent)
{
    QRegExp re1("^\\s*([N|S]){1}\\W*([0-9]+)\\W*([0-9]+\\.[0-9]+)\\s+([E|W]){1}\\W*([0-9]+)\\W*([0-9]+\\.[0-9]+)\\s*$");

    QRegExp re2("^\\s*([N|S]){1}\\s*([0-9]+\\.[0-9]+)\\W*\\s+([E|W]){1}\\s*([0-9]+\\.[0-9]+)\\W*\\s*$");

    QRegExp re3("^\\s*([-0-9]+\\.[0-9]+)\\s+([-0-9]+\\.[0-9]+)\\s*$");

    QRegExp re4("^\\s*([N|S]){1}\\s*([0-9]+)\\W*([0-9]+)\\W*([0-9]+)\\W*([E|W]){1}\\W*([0-9]+)\\W*([0-9]+)\\W*([0-9]+)\\W*\\s*$");

    if(re2.exactMatch(str))
    {
        bool signLat    = re2.cap(1) == "S";
        float absLat    = re2.cap(2).toDouble();
        lat = signLat ? -absLat : absLat;

        bool signLon    = re2.cap(3) == "W";
        float absLon    = re2.cap(4).toDouble();
        lon = signLon ? -absLon : absLon;
    }
    else if(re1.exactMatch(str))
    {

        bool signLat    = re1.cap(1) == "S";
        int degLat      = re1.cap(2).toInt();
        float minLat    = re1.cap(3).toDouble();

        GPS_Math_DegMin_To_Deg(signLat, degLat, minLat, lat);

        bool signLon    = re1.cap(4) == "W";
        int degLon      = re1.cap(5).toInt();
        float minLon    = re1.cap(6).toDouble();

        GPS_Math_DegMin_To_Deg(signLon, degLon, minLon, lon);
    }
    else if(re3.exactMatch(str))
    {
        lat             = re3.cap(1).toDouble();
        lon             = re3.cap(2).toDouble();
    }
    else if(re4.exactMatch(str))
    {
        bool signLat    = re4.cap(1) == "S";
        int degLat    = re4.cap(2).toInt();
        int minLat    = re4.cap(3).toInt();
        int secLat    = re4.cap(4).toInt();

        GPS_Math_DegMinSec_To_Deg(signLat, degLat, minLat, secLat, lat);

        bool signLon    = re4.cap(5) == "W";
        int degLon    = re4.cap(6).toInt();
        int minLon    = re4.cap(7).toInt();
        int secLon    = re4.cap(8).toInt();

        GPS_Math_DegMinSec_To_Deg(signLon, degLon, minLon, secLon, lon);


    }
    else
    {
        if(!silent) QMessageBox::warning(0,QObject::tr("Error"),QObject::tr("Bad position format. Must be: \"[N|S] ddd mm.sss [W|E] ddd mm.sss\" or \"[N|S] ddd.ddd [W|E] ddd.ddd\""),QMessageBox::Ok,QMessageBox::NoButton);
        return false;
    }

    if(fabs(lon) > 180.0 || fabs(lat) > 90.0)
    {
        if(!silent) QMessageBox::warning(0,QObject::tr("Error"),QObject::tr("Position values out of bounds. "),QMessageBox::Ok,QMessageBox::NoButton);
        return false;
    }

    return true;
}*/
		}
		
		public static double KmToMiles(double km)
		{
			return km * 0.6214;
		}
		
		public static double MilesToKm(double mi)
		{
			return mi / 0.6214;
		}
		

		public static string HTMLtoText(String src)
		{
			src = System.Web.HttpUtility.HtmlDecode(src);
			if (src.Contains("<br"))
				src = src.Replace("\n"," ");
			src = src.Replace("<hr noshade/>", "\n----------\n");
			src = Regex.Replace(src, END_P, "\n\n");
			src = Regex.Replace(src, END_LI, "\n");
			src = Regex.Replace(src, END_TR, "\n");
			src = Regex.Replace(src, BR, "\n");
			src = Regex.Replace(src, IMG, "[Image]");
			src = Regex.Replace(src, HTML, String.Empty);
			return src;
		}
		
		public static string HTMLtoGarmin(String src)
		{
			return Utilities.HTMLtoText(src).Replace("\n", "<br/>");
		}
		
		public static System.Diagnostics.ProcessStartInfo StringToStartInfo(String cmd)
		{
			int index = cmd.IndexOf(" ");
			// Not every command has a " " in it, perhaps when there is no arguments.
			if (index <= 0) {
				return new System.Diagnostics.ProcessStartInfo(cmd);
			} else {
				string proc = cmd.Substring(0, index);
				string args = cmd.Substring(index);
				return new System.Diagnostics.ProcessStartInfo(proc, args);
			}
		}
		
		public static string GetShortFileName(string fullPath)
		{
			string[] dbPath = fullPath.Split('/');
			string dbName = dbPath[dbPath.Length - 1];
			if (String.IsNullOrEmpty(dbName))
				return null;
			return dbName;
		}
		
		public static string GetShortFileNameNoExtension(string fullPath)
		{
			string[] dbPath = fullPath.Split('/');
			string dbName = dbPath[dbPath.Length - 1];
			if (String.IsNullOrEmpty(dbName))
				return null;
			dbName = dbName.Substring(0, dbName.Length -4);
			return dbName;
		}
	}	
}
