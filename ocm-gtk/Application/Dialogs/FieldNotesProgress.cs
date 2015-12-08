// 
//  Copyright 2011  campbelk
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
using Mono.Unix;
using ocmengine;

namespace ocmgtk
{

	/// <summary>
	/// Progress dialog for downloading field notes from a device
	/// </summary>
	public partial class FieldNotesProgress : Gtk.Dialog
	{
		OCMMainWindow m_Win;
		bool m_cancel = false;

		/// <summary>
		/// Used for GUI Builder only, use FieldNotesProgress(OCMMainWin win)
		/// </summary>
		public FieldNotesProgress ()
		{
			this.Build ();
		}
		
		/// <summary>
		/// Main Constructpr
		/// </summary>
		/// <param name="win">
		/// Parent Window <see cref="OCMMainWindow"/>
		/// </param>
		public FieldNotesProgress (OCMMainWindow win)
		{
			this.Build ();
			m_Win = win;
		}
		
		/// <summary>
		/// Connect to the specified profile and download all field notes
		/// </summary>
		/// <param name="profile">
		/// The GPS Profile to use <see cref="GPSProfile"/>
		/// </param>
		/// <param name="latestScan">
		/// Logs older then this date will be ignored <see cref="DateTime"/>
		/// </param>
		/// <param name="ownerId">
		/// The owner ID to use for marking the logs in the OCM DB <see cref="String"/>
		/// </param>
		/// <returns>
		/// The new datetime of the latest scan <see cref="DateTime"/>
		/// </returns>
		public DateTime ProcessFieldNotes(GPSProfile profile, DateTime latestScan, String ownerId)
		{
			try
			{
				statusLabel.Markup = Catalog.GetString("<i>Receiving from device...</i>");
				OCMApp.UpdateGUIThread();
				List<CacheLog> logs = FieldNotesHandler.GetLogs(profile.FieldNotesFile, ownerId);
				int iCount = 0;
				int iProgCount = 0;
				int iTotal = logs.Count;
				statusLabel.Markup = Catalog.GetString("<i>Processing Field Notes...</i>");
				m_Win.App.CacheStore.StartUpdate();
				DateTime newLatest = DateTime.MinValue;
				foreach(CacheLog log in logs)
				{
					if (m_cancel)
					{
						m_Win.App.CacheStore.CancelUpdate();
						return DateTime.MinValue;
					}
					double prog = (double)((double) iProgCount/ (double) iTotal);
					loadProgress.Fraction = prog;
					loadProgress.Text = prog.ToString("0%");
					iProgCount ++;
					OCMApp.UpdateGUIThread();
					if (log.LogDate <= latestScan)
					{
						System.Console.WriteLine("Skipping"  + latestScan);
						continue;
					}
					List<Geocache> cache = m_Win.App.CacheStore.GetCachesByName(new string[]{log.CacheCode});
					if (cache.Count > 0)
						UpdateCache(cache[0], log);
					if (newLatest < log.LogDate)
						newLatest = log.LogDate;
					iCount ++;
				}
				if (!m_cancel)
				{
					UpdateFNFile(logs);
					m_Win.App.CacheStore.CompleteUpdate();
				}
				else
					m_Win.App.CacheStore.CancelUpdate();
				buttonCancel.Visible = false;
				buttonClose.Visible = true;
				buttonView.Visible = true;
				loadProgress.Fraction = 1;
				statusLabel.Markup = String.Format(Catalog.GetString("<i>Scanned {0} Field Notes, {1} new.</i>"), iProgCount, iCount);
				loadProgress.Text = Catalog.GetString("Complete");
				return newLatest;
			}
			catch (Exception e)
			{
				m_Win.App.CacheStore.CancelUpdate();
				this.Hide();
				OCMApp.ShowException(e);
				return DateTime.MinValue;
			}
		}
		
		/// <summary>
		/// Updates Cache Status
		/// </summary>
		/// <param name="cache">
		/// A <see cref="Geocache"/>
		/// </param>
		/// <param name="log">
		/// A <see cref="CacheLog"/>
		/// </param>
		public void UpdateCache (Geocache cache, CacheLog log)
		{	
			if (cache == null)
				return;
			m_Win.App.CacheStore.AddLog (log.CacheCode, log);
			if (log.LogStatus == "Found it") {
				cache.DNF = false;
				cache.FTF = false;
				cache.Symbol = "Geocache Found";
				m_Win.App.CacheStore.AddWaypointOrCache (cache, false, false);
			} else if (log.LogStatus == "Didn't find it") {
				cache.DNF = true;
				cache.FTF = false;
				cache.Symbol = "Geocache";
				m_Win.App.CacheStore.AddWaypointOrCache (cache, false, false);
			} else if (log.LogStatus == "Needs Maintenance") {
				cache.CheckNotes = true;
			}
		}
		
		/// <summary>
		/// Writes out an updated field notes file
		/// </summary>
		private void UpdateFNFile(List<CacheLog> logs)
		{
			string fnFile = m_Win.App.AppConfig.FieldNotesFile;
			FieldNotesHandler.WriteToFile(logs, fnFile);
		}

#region Event Handlers
		protected virtual void OnViewClick (object sender, System.EventArgs e)
		{
			this.Hide();
			m_Win.ShowFieldNotes(true);
		}
		
		protected virtual void OnCloseClick (object sender, System.EventArgs e)
		{
			this.Hide();
			m_Win.RefreshCacheList();
		}
		
		protected virtual void OnCancelClick (object sender, System.EventArgs e)
		{
			this.Hide();
		}
#endregion
	}
}
