using System;
using System.IO;
using System.IO.Compression;

namespace ocmgtk
{
	internal static class ZipArchiveHelper
	{
		public static string ExtractToTemporaryDirectory(string archivePath)
		{
			string destination = Path.Combine(Path.GetTempPath(), "ocm_zip_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(destination);
			try
			{
				ExtractToDirectory(archivePath, destination);
				return destination;
			}
			catch
			{
				TryDeleteDirectory(destination);
				throw;
			}
		}

		public static void ExtractToDirectory(string archivePath, string destination)
		{
			ZipFile.ExtractToDirectory(archivePath, destination, overwriteFiles: true);
		}

		public static void TryDeleteDirectory(string path)
		{
			if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
				return;

			try
			{
				Directory.Delete(path, recursive: true);
			}
			catch
			{
				// Temporary import cleanup must not turn a handled import error into a crash.
			}
		}
	}
}
