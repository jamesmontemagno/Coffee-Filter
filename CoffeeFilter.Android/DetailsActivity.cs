using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using CoffeeFilter.Fragments;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;
using com.refractored;

namespace CoffeeFilter
{
	[Activity (Label = "Details", ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetailsActivity : BaseActivity
	{
		Android.Support.V7.Widget.ShareActionProvider actionProvider;
		DetailsAdapter adapter;
		ViewPager pager;
		PagerSlidingTabStrip tabs;
		DetailsViewModel viewModel;

		protected override int LayoutResource {
			get {
				return Resource.Layout.details;
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			viewModel = ServiceContainer.Resolve<DetailsViewModel> ();

			adapter = new DetailsAdapter (this, SupportFragmentManager, viewModel);
			pager = FindViewById<ViewPager> (Resource.Id.pager);
			tabs = FindViewById<PagerSlidingTabStrip> (Resource.Id.tabs);
			pager.Adapter = adapter;
			tabs.SetViewPager (pager);
			pager.OffscreenPageLimit = 3;

			var layoutParameters = new Android.Support.V7.Widget.Toolbar.LayoutParams (
				ViewGroup.LayoutParams.WrapContent,
				ViewGroup.LayoutParams.WrapContent,
				(int)(GravityFlags.Top | GravityFlags.Right));
			
			var progressBar = new ProgressBar (this, null, Android.Resource.Attribute.ProgressBarStyleSmallTitle) {
				Indeterminate = true,
				LayoutParameters = layoutParameters
			};

			SupportActionBar.SetDisplayShowCustomEnabled (true);

			SupportActionBar.CustomView = progressBar;
			SupportActionBar.Title = viewModel.Place.Name;

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "details" },
				{ "name", viewModel.Place.Name }
			});
			#endif

			viewModel.RefreshPlace ().ContinueWith ((result) => {
				RunOnUiThread (() => {
					SupportActionBar.CustomView.Visibility = ViewStates.Gone;
					if (!result.Result) {
						Toast.MakeText (this, Resource.String.unable_to_get_details, ToastLength.Long).Show ();
						return;
					}
					adapter.NotifyDataSetChanged ();
					SupportInvalidateOptionsMenu ();
				});
			});
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			ServiceContainer.RemoveScope ();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Android.Resource.Id.Home)
				Finish ();

			return base.OnOptionsItemSelected (item);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			//change menu_share1 to your name
			MenuInflater.Inflate (Resource.Menu.menu_details, menu);
			var shareItem = menu.FindItem (Resource.Id.action_share);
			var provider = MenuItemCompat.GetActionProvider (shareItem);
			actionProvider = provider.JavaCast<Android.Support.V7.Widget.ShareActionProvider> ();
			var intent = new Intent (Intent.ActionSend);
			intent.SetType ("text/plain");
			var shareText = string.Format (Resources.GetString (Resource.String.share_text), viewModel.Place.Name);
			if (!string.IsNullOrWhiteSpace (viewModel.Place.Website))
				shareText += " " + viewModel.Place.Website;
			
			intent.PutExtra (Intent.ExtraText, shareText);
			actionProvider.SetShareIntent (intent);
			return base.OnCreateOptionsMenu (menu);
		}
	}

	public class DetailsAdapter : FragmentStatePagerAdapter
	{
		string[] titles;
		DetailsViewModel viewModel;

		public override int Count {
			get {
				return titles.Length;
			}
		}

		public DetailsAdapter (Context context, Android.Support.V4.App.FragmentManager fm, DetailsViewModel viewModel) : base (fm)
		{
			titles = context.Resources.GetTextArray (Resource.Array.sections);
			this.viewModel = viewModel;
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			return new Java.Lang.String (titles [position]);
		}

		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			switch (position) {
			case 0:
				return PlaceDetailsFragment.NewInstance (viewModel.Place, viewModel.Position);
			case 1:
				return PlaceReviewsFragment.CreateNewInstance (viewModel.Place);
			case 2:
				return PlacePhotosFragment.CreateNewInstance (viewModel.Place);
			}
			return null;
		}

		public override int GetItemPosition (Java.Lang.Object frag)
		{
			return PositionNone;
		}
	}
}

