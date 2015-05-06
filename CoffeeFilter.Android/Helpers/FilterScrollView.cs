using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CoffeeFilter.Helpers
{
	public class ScrolledEventArgs : EventArgs
	{
		public int X { get; set; }

		public int Y { get; set; }

		public int OldX { get; set; }

		public int OldY { get; set; }
	}

	public delegate void ScrolledEventDelegate (object sender, ScrolledEventArgs args);

	public class FilterScrollView : ScrollView
	{
		public event ScrolledEventDelegate OnScrolledEvent;

		public FilterScrollView (Context context) :
			base (context)
		{
		}

		public FilterScrollView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
		}

		public FilterScrollView (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
		}

		protected override void OnScrollChanged (int l, int t, int oldl, int oldt)
		{
			base.OnScrollChanged (l, t, oldl, oldt);
			if (OnScrolledEvent == null)
				return;

			OnScrolledEvent (this, new ScrolledEventArgs {
				X = l,
				Y = t,
				OldX = oldl,
				OldY = oldt
			});
		}
	}
}

