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
using System.Collections;
using System.Collections.Generic;
using Gtk;
using GLib;
using System.Reflection;
using System.Runtime.InteropServices;
using ocmengine;

namespace ocmgtk
{


	public class CacheStoreModel : GLib.Object, TreeModelImplementor, TreeSortable
	{
		List<Geocache> caches;
		
		private class ModelSorter : IComparer<Waypoint>, IComparer<Geocache>
		{

			double orig_lat, orig_lon;

			public ModelSorter (double lat, double lon)
			{
				orig_lat = lat;
				orig_lon = lon;
			}
			
			public int Compare (Geocache obj1, Geocache obj2)
			{
				return Compare(obj1 as Waypoint, obj2 as Waypoint);
			}

			public int Compare (Waypoint obj1, Waypoint obj2)
			{
				double d1 = Utilities.calculateDistance (orig_lat, obj1.Lat, orig_lon, obj1.Lon);
				double d2 = Utilities.calculateDistance (orig_lat, obj2.Lat, orig_lon, obj2.Lon);
				if (d2 > d1)
					return -1; else if (d2 == d1)
					return 0;
				else
					return 1;
			}
		}
		
		
		public const int TYPE_COL = 0;
		public const int CODE_COL = 1;
		public const int DIST_COL = 2;
		public const int TITLE_COL = 3;
		
		private OCMApp m_App;
		public OCMApp App
		{
			set { m_App = value;}
		}

		public CacheStoreModel ()
		{
			caches = new List<Geocache>(10000);
		}
		
		public List<Geocache> Caches
		{
			get { return caches;}
		}
		
		public void Add(Geocache cache)
		{
			caches.Add(cache);
		}
		
		public void Resort(double lat, double lon)
		{
			caches.Sort(new ModelSorter(lat, lon));
		}
		
		public void Clear()
		{
			caches.Clear();
		}

		object GetNodeAtPath (TreePath path)
		{
			if (path.Indices.Length > 0) {
				if (caches.Count > path.Indices[0])
				{
					Geocache cache = caches[path.Indices[0]];
						return cache;
				}
			}
			return null;				
		}

		Hashtable node_hash = new Hashtable ();

		public TreeModelFlags Flags {
			get { return TreeModelFlags.ListOnly; }
		}

		public int NColumns {
			get { return 1; }
		}

		public GLib.GType GetColumnType (int col)
		{
			GLib.GType result = GLib.GType.Object;
			return result;
		}

		TreeIter IterFromNode (object node)
		{
			GCHandle gch;
			if (node_hash[node] != null)
				gch = (GCHandle)node_hash[node];
			else
				gch = GCHandle.Alloc (node);
			TreeIter result = TreeIter.Zero;
			result.UserData = (IntPtr)gch;
			return result;
		}

		object NodeFromIter (TreeIter iter)
		{
			if (iter.UserData == IntPtr.Zero)
				return null;
			GCHandle gch = (GCHandle)iter.UserData;
			return gch.Target;
		}

		TreePath PathFromNode (object node)
		{
			if (node == null)
				return new TreePath ();
			
			object work = node;
			TreePath path = new TreePath ();
			path.PrependIndex(caches.IndexOf(work as Geocache));
			return path;
		}

		public bool GetIter (out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			iter = TreeIter.Zero;
			object node = GetNodeAtPath (path);
			if (node == null)
				return false;
			iter = IterFromNode (node);
			return true;
		}

		public TreePath GetPath (TreeIter iter)
		{
			object node = NodeFromIter (iter);
			if (node == null)
				throw new ArgumentException ("iter");
			
			return PathFromNode (node);
		}

		public void GetValue (TreeIter iter, int col, ref GLib.Value val)
		{
			object node = NodeFromIter (iter);
			if (node == null)
			{
				val = GLib.Value.Empty;
				return;
			}
			val = new GLib.Value(node as Geocache);
		}

		public bool IterNext (ref TreeIter iter)
		{
			object node = NodeFromIter (iter);
			if (node == null)
				return false;
			
			int idx;
			idx = caches.IndexOf(node as Geocache) + 1;
			if (idx < caches.Count)
			{
				iter = IterFromNode(caches[idx]);
				return true;
			}
			return false;
		}

		public bool IterChildren (out TreeIter child, TreeIter parent)
		{
			child = TreeIter.Zero;
			return false;
		}

		public bool IterHasChild (TreeIter iter)
		{
			return false;
		}

		public int IterNChildren (TreeIter iter)
		{
			if (caches.Count > 0)
				if (iter.Equals(TreeIter.Zero))
					return caches.Count;
			System.Console.WriteLine(caches.Count);
			return 0;
		}

		public bool IterNthChild (out TreeIter child, TreeIter parent, int n)
		{
			child = TreeIter.Zero;
			return false;
		}

		public bool IterParent (out TreeIter parent, TreeIter child)
		{
			parent = TreeIter.Zero;
			return false;
		}

		public void RefNode (TreeIter iter)
		{
		}

		public void UnrefNode (TreeIter iter)
		{
		}
		
		

		
		#region TreeSortable implementation
		public event EventHandler SortColumnChanged;	
		TreeIterCompareFunc[] sortFuncs = new TreeIterCompareFunc[4];
		TreeIterCompareFunc defaultSort = null;
		SortType m_SortType = SortType.Ascending;
		int m_SortedColumn = -1;
		
		public void SetSortFunc (int sort_column_id, TreeIterCompareFunc sort_func)
		{
			sortFuncs[sort_column_id] = sort_func;
		}
		
		
		public void SetSortColumnId (int sort_column_id, SortType order)
		{
			m_SortType = order;
			m_SortedColumn = sort_column_id;
			if (this.SortColumnChanged != null)
				this.SortColumnChanged(this, new EventArgs());
		}
		
		
		public void ChangeSortColumn ()
		{
			System.Console.WriteLine("CHANGE!");
		}
		
		public bool isSorted()
		{
			if (m_SortedColumn >= 0)
				return true;
			return false;
		}
		
		
		public bool GetSortColumnId (out int sort_column_id, out SortType order)
		{
			sort_column_id = m_SortedColumn;
			order = m_SortType;
			return true;
		}
		
		public void SetDefaultSortFunc (TreeIterCompareFunc sort_func, IntPtr user_data, Gtk.DestroyNotify destroy)
		{
			defaultSort = sort_func;
		}
		
		
		public void SetSortFunc (int sort_column_id, TreeIterCompareFunc sort_func, IntPtr user_data, Gtk.DestroyNotify destroy)
		{
			sortFuncs[sort_column_id] = sort_func;
		}
		
		
		public TreeIterCompareFunc DefaultSortFunc {
			set {
				defaultSort = value;
			}
		}
		
		
		public bool HasDefaultSortFunc {
			get {
				if (defaultSort != null)
					return true;
				return false;
			}
		}
		
		#endregion
	}
	
}
