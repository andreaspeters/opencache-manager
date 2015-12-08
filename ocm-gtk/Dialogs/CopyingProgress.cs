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
using ocmengine.SQLLite;
using Mono.Unix;
using Gtk;
using System.Data;

namespace ocmgtk
{


	public partial class CopyingProgress : Gtk.Dialog
	{
		double total = 0;
		bool cancel = false;
		
		public enum ModeEnum {VISIBLE, SELECTED, ALL}
		
		public CopyingProgress ()
		{
			this.Build ();
		}
		
		public void StartDelete(List<Geocache> caches, OCMMainWindow win)
		{
			this.copyLabel.Markup = Catalog.GetString("<big><b>Deleting Geocaches</b></big>");
			this.Title = Catalog.GetString("Delete Caches....");
			targetDBLabel.Visible = false;
			TargetLabel.Visible = false;
			ACacheStore store = win.App.CacheStore;
			store.StartUpdate();
			double count = 0;
			total = caches.Count;
			foreach(Geocache cache in caches)
			{
				if (cancel)
				{
					store.CancelUpdate();
					this.Hide();
					this.Dispose();
					return;
				}
				count++;
				DeleteProgress(count, cache.Name);
				store.DeleteWaypoint(cache.Name);
				win.DeleteCacheImages(cache.Name);
				
			}
			statusLabel.Markup = Catalog.GetString("<i>Complete</i>");
			progressBar.Text = Catalog.GetString("Complete");
			buttonOk.Visible = true;
			buttonCancel.Visible = false;
			store.CompleteUpdate();			
		}
		
		public void Start (String targetDB, bool isMove, ModeEnum modeType, OCMMainWindow win)
		{
			if (isMove)
			{
				Title = Catalog.GetString ("Move Caches...");
				copyLabel.Markup = Catalog.GetString ("Moving Geocaches");
			}
			List<Geocache> caches;
			if (modeType == CopyingProgress.ModeEnum.VISIBLE)
			{
				caches = win.CacheList.UnfilteredCaches;
			}
			else if (modeType == CopyingProgress.ModeEnum.SELECTED)
			{
				caches = new List<Geocache> ();
				caches.Add (win.CacheList.SelectedCache);
			}
			else
			{
				caches = null;
				//caches = new List<Geocache> (UIMonitor.getInstance ().CacheStore.GetUnfilteredCaches(0d,0d,new string[0]));
			}
			ACacheStore target = new FileStore (targetDB);
			ACacheStore source = win.App.CacheStore;
			targetDBLabel.Text = targetDB;
			double count = 0;
			total = caches.Count;
			if (target.NeedsUpgrade)
			{
				MessageDialog dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
				                                      Catalog.GetString("The target database needs to be upgraded. " +
				                                      	"Please open the target database before trying to copy/move caches."));
				dlg.Run();
				dlg.Hide();
				this.Hide();
				return;
			}
			buttonOk.Visible = false;
			List<String> str = new List<String>();
			foreach(Geocache c in caches)
			{
				str.Add(c.Name);
			}
			
			target.StartUpdate();
			target.PurgeAllLogs(str.ToArray());
			target.PurgeAllAttributes(str.ToArray());
			target.PurgeAllTravelBugs(str.ToArray());
			foreach(Geocache cache in caches)
			{
				if (cancel)
				{
					target.CancelUpdate();
					this.Hide();
					this.Dispose();
					return;
				}
				
				count++;
				UpdateProgress(count, cache.Name);
				target.AddWaypointOrCache(cache, false, false);
				List<CacheLog> logs = source.GetCacheLogs(cache.Name);
				foreach(CacheLog log in logs)
				{
					target.AddLog(cache.Name, log);
				}
				
				List<Waypoint> children = source.GetChildWaypoints(new string[]{cache.Name});
				foreach (Waypoint child in children)
				{
					target.AddWaypointOrCache(child, false, false);
				}
				
				CacheAttribute[]  attributes = source.GetAttributes(cache.Name);
				foreach (CacheAttribute attribute in attributes)
				{
					target.AddAttribute(cache.Name,attribute);
				}
				
				if (isMove)
					source.DeleteWaypoint(cache.Name);
			}
			statusLabel.Markup = Catalog.GetString("<i>Complete</i>");
			progressBar.Text = Catalog.GetString("Complete");
			buttonOk.Visible = true;
			buttonCancel.Visible = false;
			target.CompleteUpdate();
		}
		
		private void UpdateProgress(double count, string name)
		{
			double fraction = count/total;
			progressBar.Fraction = fraction;
			progressBar.Text = fraction.ToString("0%");
			this.statusLabel.Markup = String.Format(Catalog.GetString("<i>Copying:{0}</i>"), name);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
		}
		
		private void DeleteProgress(double count, string name)
		{
			double fraction = count/total;
			progressBar.Fraction = fraction;
			progressBar.Text = fraction.ToString("0%");
			this.statusLabel.Markup = String.Format(Catalog.GetString("<i>Deleting:{0}</i>"), name);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (false);
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Hide();
			this.Dispose();
		}
		
		protected virtual void OnCancelClick (object sender, System.EventArgs e)
		{
			cancel = true;
		}
		
		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			cancel = true;
		}
		
		
		
	}
}
