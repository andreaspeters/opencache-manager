using System;

namespace ocmgtk
{
	internal static class OnlineLogUrl
	{
		internal static string FromCache(Uri cacheUrl, string cacheId)
		{
			if (cacheUrl == null)
				throw new ArgumentNullException("cacheUrl");

			string url = cacheUrl.ToString();
			if (url.Contains("opencaching"))
				return url.Replace("viewcache.php", "log.php");

			if (url.Contains("geocaching"))
				return "http://www.geocaching.com/seek/log.aspx?ID=" + cacheId;

			return url;
		}
	}
}
