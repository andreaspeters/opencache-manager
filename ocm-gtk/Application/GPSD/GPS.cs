
using org.freedesktop.DBus;
using NDesk.DBus;

namespace ocmgtk
{
	//pointless comment
	public class GPS
	{
		private Connection DbusConnection;
		public Gpsd gps;
		
		private double lastLat = 0;
		public double Lat
		{
			get { return lastLat;}
		}
		
		
		private double lastLon = 0;
		public double Lon
		{
			get { return lastLon;}
		}
		
		public GPS ()
		{
			DbusConnection = Bus.System;
			gps = DbusConnection.GetObject<Gpsd> ("org.gpsd", new ObjectPath ("/org/gpsd"));
			gps.fix += HandleGpsfix;
		}

		void HandleGpsfix (GPSFix fix)
		{;
			if (double.IsNaN(fix.latitude) || fix.latitude > 90 || fix.latitude < -90)
			{
				// Garbage coordinate. Ignore this reading;
				return;
			}
			else if (double.IsNaN(fix.longitude) || fix.longitude < -180 || fix.longitude > 180)
			{
				// Garbage coordinate. Ignore this reading;
				return;
			}
			lastLat = fix.latitude;
			lastLon = fix.longitude;
		}	
	}

	public struct GPSFix
	{
		public double time;
		public int mode;
		public double ept;
		public double latitude;
		public double longitude;
		public double eph;
		public double altitude;
		public double epv;
		public double track;
		public double epd;
		public double speed;
		public double eps;
		public double climb;
		public double epc;
	}

	public delegate void GPSPositionChangedHandler (GPSFix fix);

	[Interface("org.gpsd")]
	public interface Gpsd
	{
		event GPSPositionChangedHandler fix;
	}
}
