using System;
using System.Diagnostics;

namespace ocmgtk
{
	internal static class ExternalProcessLauncher
	{
		public static bool TryOpenExternalTarget(string target, Action<Exception> reportError, Action<Action> dispatchError)
		{
			if (String.IsNullOrWhiteSpace(target))
			{
				ReportExternalTargetError(new ArgumentException("The external target is empty.", "target"), reportError, dispatchError);
				return false;
			}

			if (!OperatingSystem.IsLinux())
			{
				return TryStart(new ProcessStartInfo
				{
					FileName = target,
					UseShellExecute = true
				}, reportError);
			}

			Process process = null;
			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = "gio",
					UseShellExecute = false,
					RedirectStandardError = true,
					CreateNoWindow = true
				};
				startInfo.ArgumentList.Add("open");
				startInfo.ArgumentList.Add(target);

				process = Process.Start(startInfo);
				if (process == null)
					throw new InvalidOperationException("The gio URI opener did not start.");

				var stderrTask = process.StandardError.ReadToEndAsync();
				process.Exited += delegate
				{
					int exitCode;
					string errorDetail;
					try
					{
						exitCode = process.ExitCode;
						errorDetail = stderrTask.GetAwaiter().GetResult().Trim();
					}
					catch (Exception ex)
					{
						process.Dispose();
						ReportExternalTargetError(new InvalidOperationException("Could not determine whether the default application opened '" + target + "'.", ex), reportError, dispatchError);
						return;
					}

					process.Dispose();
					if (exitCode != 0)
					{
						string message = "OpenCacheManager could not open '" + target + "' in the default application (gio exited with status " + exitCode + "). Make sure a default browser or handler is installed and configured.";
						if (!String.IsNullOrWhiteSpace(errorDetail))
							message += "\n\n" + errorDetail;
						ReportExternalTargetError(new InvalidOperationException(message), reportError, dispatchError);
					}
				};
				process.EnableRaisingEvents = true;
				return true;
			}
			catch (Exception ex)
			{
				if (process != null)
					process.Dispose();
				ReportExternalTargetError(new InvalidOperationException("OpenCacheManager could not start the default application for '" + target + "'. Make sure a default browser or handler is installed and configured.\n\n" + ex.Message, ex), reportError, dispatchError);
				return false;
			}
		}

		private static void ReportExternalTargetError(Exception error, Action<Exception> reportError, Action<Action> dispatchError)
		{
			Action report = delegate
			{
				try
				{
					if (reportError != null)
						reportError(error);
					else
						Console.Error.WriteLine(error.Message);
				}
				catch (Exception reportingException)
				{
					try
					{
						Console.Error.WriteLine(error.Message);
						Console.Error.WriteLine("Could not display the external target error: " + reportingException.Message);
					}
					catch
					{
					}
				}
			};

			try
			{
				if (dispatchError != null)
					dispatchError(report);
				else
					report();
			}
			catch (Exception dispatchException)
			{
				try
				{
					Console.Error.WriteLine(error.Message);
					Console.Error.WriteLine("Could not dispatch the external target error: " + dispatchException.Message);
				}
				catch
				{
				}
			}
		}

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
				InvalidOperationException reportedError = new InvalidOperationException(message, ex);
				try
				{
					if (reportError != null)
						reportError(reportedError);
					else
						Console.Error.WriteLine(reportedError.Message);
				}
				catch (Exception reportingException)
				{
					try
					{
						Console.Error.WriteLine(reportedError.Message);
						Console.Error.WriteLine("Could not display the external launch error: " + reportingException.Message);
					}
					catch
					{
					}
				}
				return false;
			}
		}
	}
}
