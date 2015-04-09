
using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CoffeeFilter.Fragments;
using Android.Support.V4.App;
using Android.Support.V4.View;
using com.refractored;
using CoffeeFilter.Shared.ViewModels;
using CoffeeFilter.Shared.Helpers;
using System.Collections.Generic;
using Android.Content.PM;

namespace CoffeeFilter
{
	[Activity (Label = "Details", ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetailsActivity : BaseActivity
	{
		protected override int LayoutResource {
			get {
				return Resource.Layout.details;
			}
		}
			
		private DetailsAdapter adapter;
		private ViewPager pager;
		private PagerSlidingTabStrip tabs;
		private DetailsViewModel viewModel;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			viewModel = ServiceContainer.Resolve<DetailsViewModel> ();

			adapter = new DetailsAdapter(this,SupportFragmentManager, viewModel);
			pager = FindViewById<ViewPager> (Resource.Id.pager);
			tabs = FindViewById<PagerSlidingTabStrip> (Resource.Id.tabs);
			pager.Adapter = adapter;
			tabs.SetViewPager (pager);
			pager.OffscreenPageLimit = 3;

			var progressBar = new ProgressBar (this, null, Android.Resource.Attribute.ProgressBarStyleSmallTitle);
			progressBar.Indeterminate = true;
			progressBar.LayoutParameters = new Android.Support.V7.Widget.Toolbar.LayoutParams(
				ViewGroup.LayoutParams.WrapContent,
				ViewGroup.LayoutParams.WrapContent,
				(int)(GravityFlags.Top | GravityFlags.Right));
			SupportActionBar.SetDisplayShowCustomEnabled (true);

			SupportActionBar.CustomView = progressBar;
			SupportActionBar.Title = viewModel.Place.Name;

			#if !DEBUG
			Xamarin.Insights.Track("AppNav-Details", new Dictionary<string,string>
			{
				{"page", "details"},
				{"name", viewModel.Place.Name},
			});
			#endif

			viewModel.RefreshPlace ().ContinueWith ((result) => {

				RunOnUiThread(()=>
					{
						SupportActionBar.CustomView.Visibility = ViewStates.Gone;
						if(!result.Result){
							Toast.MakeText(this, Resource.String.unable_to_get_details, ToastLength.Long).Show();
							return;
						}
						adapter.NotifyDataSetChanged();
						SupportInvalidateOptionsMenu();
					});
			});
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			ServiceContainer.RemoveScope ();
		}
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)           
			{
			case Android.Resource.Id.Home:
				Finish ();
				break;
			}

			return base.OnOptionsItemSelected(item);
		}

		Android.Support.V7.Widget.ShareActionProvider actionProvider;
		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			//change menu_share1 to your name
			this.MenuInflater.Inflate(Resource.Menu.menu_details, menu);
			var shareItem = menu.FindItem(Resource.Id.action_share);
			var provider = MenuItemCompat.GetActionProvider(shareItem);
			actionProvider = provider.JavaCast<Android.Support.V7.Widget.ShareActionProvider>();
			var intent = new Intent(Intent.ActionSend);
			intent.SetType("text/plain");
			var shareText = string.Format (Resources.GetString (Resource.String.share_text), viewModel.Place.Name);
			if (!string.IsNullOrWhiteSpace (viewModel.Place.Website))
				shareText += " " + viewModel.Place.Website;
			
			intent.PutExtra(Intent.ExtraText, shareText);
			actionProvider.SetShareIntent(intent);
			return base.OnCreateOptionsMenu(menu);
		}
	}

	public class DetailsAdapter : FragmentStatePagerAdapter{
		private  string[] Titles;
		private DetailsViewModel viewModel;
		public DetailsAdapter(Context context, Android.Support.V4.App.FragmentManager fm, DetailsViewModel viewModel) : base(fm)
		{
			Titles = context.Resources.GetTextArray (Resource.Array.sections);
			this.viewModel = viewModel;
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			return new Java.Lang.String (Titles [position]);
		}
		#region implemented abstract members of PagerAdapter
		public override int Count {
			get {
				return Titles.Length;
			}
		}
		#endregion
		#region implemented abstract members of FragmentPagerAdapter
		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			switch (position) {
			case 0:
				return PlaceDetailsFragment.NewInstance (viewModel.Place, viewModel.Position);
			case 1:
				return PlaceReviewsFragment.NewInstance (viewModel.Place);
			case 2:
				return PlacePhotosFragment.NewInstance (viewModel.Place);
			}
			return null;
		}
		#endregion

		public override int GetItemPosition (Java.Lang.Object frag)
		{
				return PositionNone;
		}
	}
}

