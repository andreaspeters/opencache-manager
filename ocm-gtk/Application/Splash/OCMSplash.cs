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
using System.Timers;
using System.IO;
using Gdk;
using Gtk;
using Mono.Unix;
using ocmengine;


namespace ocmgtk
{


	public partial class OCMSplash : Gtk.Window
	{

		private CacheStoreModel m_Model = new CacheStoreModel();
		private OCMApp m_App = null;
		private int m_Count = 0;
		private int m_disabledOrArchivedCount =0;
		private int m_mineCount = 0;
		private int m_visibleCount = 0;
		private int m_foundCount = 0;
		private bool m_loadDone = false;
		private bool m_timerDone = false;
		bool showChristmasSplash = (System.DateTime.Now.Month == 12 && System.DateTime.Now.Day <= 26)? true:false;

		Timer animTimer = new Timer();

		string[] christmasBG = new string[]{"./icons/splash/christmas/AXmas.png", "./icons/splash/christmas/BXmas.png", "./icons/splash/christmas/CXmas.png",
			"./icons/splash/christmas/DXmas.png", "./icons/splash/christmas/EXmas.png", "./icons/splash/christmas/FXmas.png",
			"./icons/splash/christmas/GXmas.png",  "./icons/splash/christmas/HXmas.png", "./icons/splash/christmas/IXmas.png"};
		Pixbuf[] backgrounds = new Pixbuf[9];
		Pixbuf currentBackground;
		int animPos = 0;


		public OCMSplash () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			SetSplashBG ();

			this.versionLabel.Markup = String.Format(Catalog.GetString("<b><big>Version {0}</big></b>\n<small>Copyright (c) Kyle Campbell 2010-2013\nCopyright (c) Andreas Peters 2015-2016</small>"), OCMApp.GetOCMVersion());

			if (showChristmasSplash)
			{
				animTimer.Elapsed += HandleAnimTimerElapsed;
				animTimer.AutoReset = true;
				animTimer.Interval = 150;
				animTimer.Start();
			}

			Timer minWait = new Timer();
			minWait.Elapsed += HandleMinWaitElapsed;
			minWait.AutoReset = false;
			minWait.Interval = 1500;
			minWait.Start();
		}

		void SetSplashBG ()
		{
			string[] files;
			if (showChristmasSplash)
				files = Directory.GetFiles("./icons/splash/christmas");
			else
				files = Directory.GetFiles("./icons/splash");
			Random rand = new Random();
			int idx = rand.Next(files.Length);
			Pixbuf test = new Pixbuf(files[idx], 744, 600);
			currentBackground = test;
			this.AppPaintable = true;
			this.QueueDraw();

			if (showChristmasSplash)
			{
				for(int idx2 = 0; idx2 < christmasBG.Length; idx2++)
				{
					Pixbuf test2 = new Pixbuf(christmasBG[idx2], 744, 600);
					backgrounds[idx2] = test2;
				}
			}
		}

		void UpdateBG ()
		{
			int idx = animPos;
			currentBackground = backgrounds[idx];
			this.QueueDraw();
			OCMApp.UpdateGUIThread();
			animPos++;
			if (animPos >= backgrounds.Length)
				animPos = 0;
		}

		protected override bool OnDrawn (Cairo.Context context)
		{
			if (currentBackground != null)
			{
				Gdk.CairoHelper.SetSourcePixbuf(context, currentBackground, 0, 0);
				context.Paint();
			}
			return base.OnDrawn(context);
		}

		void HandleAnimTimerElapsed (object sender, ElapsedEventArgs e)
		{
			Application.Invoke(delegate {UpdateBG();});
		}

		void HandleMinWaitElapsed (object sender, ElapsedEventArgs e)
		{
			m_timerDone = true;
			Application.Invoke(delegate {CheckDone();});
		}

		void CheckDone()
		{
			if (m_timerDone && m_loadDone)
			{
				animTimer.Stop();
				this.Hide();
				m_App.ShowMainWindow();

			}

		}

		public void Preload(OCMApp app, QuickFilter filter)
		{
			m_App = app;

			if (filter != null)
			{
				m_App.CacheStore.GlobalFilters.AddFilterCriteria(FilterList.KEY_STATUS,
			                                           new bool[]{filter.Found, filter.NotFound, filter.Mine,
																	filter.Available, filter.Unavailable, filter.Archived});
				m_App.CacheStore.AdvancedFilters = filter.AdvancedFilters;
				if (filter.ComboFilter != null)
					m_App.CacheStore.CombinationFilter = filter.ComboFilter;
			}
			app.CacheStore.Complete += HandleAppCacheStoreComplete;
			app.CacheStore.ReadCache += HandleAppCacheStoreReadCache;
			if (app.AppConfig.UseGPSD)
				app.EnableGPS(false);
			app.CacheStore.GetUnfilteredCaches(app.CentreLat, app.CentreLon, app.OwnerIDs.ToArray());
		}

		void HandleAppCacheStoreReadCache (object sender, ocmengine.ReadCacheArgs args)
		{
			m_Model.Add(args.Cache);
			if (!args.Cache.Available)
				m_disabledOrArchivedCount++;
			else if (args.Cache.Archived)
				m_disabledOrArchivedCount++;
			if (args.Cache.Found)
				m_foundCount++;
			if (m_App.OwnerIDs.Contains(args.Cache.OwnerID) || m_App.OwnerIDs.Contains(args.Cache.PlacedBy))
				m_mineCount++;
			m_visibleCount++;

			m_Count++;
			if (m_Count == 50)
			{
				loadProgress.Pulse();
				OCMApp.UpdateGUIThread();
				m_Count = 0;
			} 
		}

		void HandleAppCacheStoreComplete (object sender, EventArgs args)
		{
			m_App.CacheStore.Complete -= HandleAppCacheStoreComplete;
			m_App.CacheStore.ReadCache -= HandleAppCacheStoreReadCache;
			m_App.SetInitalCacheModel(m_Model, m_visibleCount, m_foundCount, m_mineCount, m_disabledOrArchivedCount);
			loadProgress.Visible = false;
			m_loadDone = true;
			CheckDone();
		}
	}
}
