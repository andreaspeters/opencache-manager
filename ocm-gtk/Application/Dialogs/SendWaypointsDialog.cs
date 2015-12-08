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
using ocmengine;
using Mono.Unix;
using Gtk;

namespace ocmgtk
{

	/// <summary>
	/// Dialog and code to send waypoints to a GPS unit
	/// </summary>
	public partial class SendWaypointsDialog : Gtk.Dialog
	{
		GPSBabelWriter writer = new GPSBabelWriter ();
		double total = 0;
		double count = 0;
		bool m_autoClose = false;
				
		public bool noCancel = false;
		public bool done = false;

		/// <summary>
		/// If true, close the dialog immediately on close
		/// </summary>
		public bool AutoClose {
			set { m_autoClose = value; }	
		}
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public SendWaypointsDialog ()
		{
			this.Build ();
			writer.StartSend += HandleWriterStartSend;
			writer.Complete += HandleWriterComplete;
			writer.WriteWaypoint += HandleWriterWriteWaypoint;
		}

		/// <summary>
		/// Sends Geocaches to a device, always including child waypoints
		/// </summary>
		/// <param name="caches">
		/// A list of geocaches <see cref="List<Geocache>"/>
		/// </param>
		/// <param name="profile">
		/// A GPS profile <see cref="GPSProfile"/>
		/// </param>
		/// <param name="store">
		/// The source CacheStore <see cref="ACacheStore"/>
		/// </param>
		public void Start (List<Geocache> caches, GPSProfile profile, ACacheStore store)
		{
			Start(caches, profile, true, store);
		}
		
		/// <summary>
		/// Sends Geocaches to a device
		/// </summary>
		/// <param name="caches">
		/// A list of geocaches <see cref="List<Geocache>"/>
		/// </param>
		/// <param name="profile">
		/// A gps profiles <see cref="GPSProfile"/>
		/// </param>
		/// <param name="includeChildren">
		/// If true, include child waypoints<see cref="System.Boolean"/>
		/// </param>
		/// <param name="store">
		/// The source cache store <see cref="ACacheStore"/>
		/// </param>
		public void Start (List<Geocache> caches, GPSProfile profile, bool includeChildren, ACacheStore store)
		{
			try {
				total = caches.Count + 1;
				if (profile.CacheLimit != -1 && profile.CacheLimit < caches.Count)
					total = profile.CacheLimit + 1;
				writer.Limit = profile.CacheLimit;
				writer.BabelFile = profile.OutputFile;
				writer.BabelFormat = profile.BabelFormat;
				writer.DescMode = profile.DescMode;
				writer.NameMode = profile.NameMode;
				writer.LogLimit = profile.LogLimit;
				writer.IncludeAttributes = profile.IncludeAttributes;
				writer.OtherBabelParams = profile.OtherProperties;
				writer.IncludeChildren = includeChildren;
				writer.ForcePlainText = profile.ForcePlainText;
				OCMApp.UpdateGUIThread();
				writer.WriteToGPS (caches, profile.WaypointMappings, store);				
				this.Show ();
			} catch (Exception e) {
				this.Hide ();
				OCMApp.ShowException(e);
			}
		}

		/// <summary>
		/// Closes this window
		/// </summary>
		private void CloseMe() {
			this.Hide ();
			this.Dispose ();
		}

		
		/// <summary>
		/// Cancels the send operation
		/// </summary>
		/// <returns>
		/// True, if the user confirms cancelling <see cref="System.Boolean"/>
		/// </returns>
		private bool Cancel ()
		{
			MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.OkCancel,
			                                      Catalog.GetString("Are you sure you want to cancel?"));
			if ((int) ResponseType.Ok == dlg.Run())
			{
				writer.Cancel();
				dlg.Hide();
				return true;
			}
			dlg.Hide();
			return false;
		}
		
		
#region Event Handlers
		void HandleWriterWriteWaypoint (object sender, EventArgs args)
		{
			count++;
			double fraction = (count) / total;
			writeProgress.Fraction = fraction;
			this.infoLabel.Markup = "<i>" + (args as WriteEventArgs).Message + "</i>";
			writeProgress.Text = fraction.ToString ("0%");
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
		}

		void HandleWriterComplete (object sender, EventArgs args)
		{
			if ((args as WriteEventArgs).Message == "Cancelled")
			{
				this.Hide();
				return;
			}
			if (m_autoClose) {
				CloseMe();
				return;
			}
			writeProgress.Fraction = 1;
			writeProgress.Text = Catalog.GetString("Complete");
			done = true;
			this.infoLabel.Markup = String.Format(Catalog.GetString("<i>Send Complete:{0} geocaches transferred</i>"), count);
			closeButton.Show ();
			buttonCancel.Hide ();
		}

		void HandleWriterStartSend (object sender, EventArgs args)
		{
			this.infoLabel.Markup = Catalog.GetString("<i>Sending Geocaches to Device</i>");
			noCancel = true;
			buttonCancel.Sensitive = false;
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
		}
		
		protected virtual void OnCloseClick (object sender, System.EventArgs e)
		{
			CloseMe();
		}
		
		protected virtual void OnDelete (object o, Gtk.DeleteEventArgs args)
		{
			if (done)
				return;
			if (noCancel || !Cancel())
				args.RetVal = true;
		}
		
		protected virtual void OnCancelClick (object sender, System.EventArgs e)
		{
			Cancel ();
		}
#endregion
	}
}
