using System;
using System.Diagnostics;

namespace ocmgtk
{
	internal static class ExternalProcessLauncher
	{
		public static bool TryStart(ProcessStartInfo startInfo, Action<Exception> reportError)
		{
			if (startInfo == null)
				throw new ArgumentNullException("startInfo");

			try
			{
				Process.Start(startInfo);
				return true;
			}
			catch (Exception ex)
			{
				string target = String.IsNullOrWhiteSpace(startInfo.FileName)
					? "the external application"
					: "'" + startInfo.FileName + "'";
				string message = "OpenCacheManager could not start " + target +
					". Make sure it is installed and available, and that a default handler is configured when opening a link or folder.\n\n" + ex.Message;
				if (reportError != null)
					reportError(new InvalidOperationException(message, ex));
				return false;
			}
		}
	}
}
