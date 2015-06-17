using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

using CoffeeFilter.Fragments;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;
using Android.Gms.AppInvite;
using Android.Util;
using Android.Support.V4.Content;

namespace CoffeeFilter
{

	[Activity (Name="com.refractored.coffeeFilter.MainActivity", Label = "Coffee Filter", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = true, Icon = "@drawable/ic_launcher")]
	/*[IntentFilter(new[]{Intent.ActionView},
		Categories=new[]{Intent.CategoryDefault, Intent.CategoryBrowsable}, 
		DataScheme="http", 
		DataHost="motzcod.es")]*/
	public class MainActivity : BaseActivity, IOnMapReadyCallback
	{
		const int ConnectionFailureResolutionRequest = 9000;

		bool initMap;
		bool error;
		double radius = 16;
		int initTry = 0;
		bool refreshing = false;

		Android.Support.V7.Widget.SearchView searchView;
		SwipeRefreshLayout refresher;
		CoffeeFilterViewModel viewModel;
		ViewPager pager;
		PlacesPagerAdapter adapter;
		Marker marker;
		MarkerOptions markerOptions;
		MapView mapView;
		Circle locationCircle;
		CircleOptions circleOptions;
		ImageView progressBar, errorImage;
		AnimationDrawable coffeeProgress;
		GoogleMap googleMap;
		DateTime lastRefresh = DateTime.UtcNow;
		private BroadcastReceiver deepLinkReceiver = null;

		protected override int LayoutResource {
			get {
				return Resource.Layout.main;
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			#if DEBUG
			Xamarin.Insights.Initialize("ef02f98fd6fb47ce8624862ab7625b933b6fb21d", this);
			#else
			Xamarin.Insights.Initialize ("8da86f8b3300aa58f3dc9bbef455d0427bb29086", this);
			#endif

			mapView = FindViewById<MapView> (Resource.Id.map);
			mapView.OnCreate (bundle);

			mapView.Visibility = ViewStates.Invisible;
			viewModel = new CoffeeFilterViewModel ();
			ServiceContainer.Register<CoffeeFilterViewModel> (viewModel);
			adapter = new PlacesPagerAdapter (SupportFragmentManager, viewModel);
			pager = FindViewById<ViewPager> (Resource.Id.pager);
			pager.Adapter = adapter;
			progressBar = FindViewById<ImageView> (Resource.Id.progressBar);
			errorImage = FindViewById<ImageView> (Resource.Id.error);
			coffeeProgress = (AnimationDrawable)progressBar.Background;
			progressBar.Visibility = ViewStates.Gone;

			pager.PageSelected += (sender, e) => UpdateMap (e.Position);
			pager.PageScrollStateChanged += (sender, e) => {
				if (e.State == (int)ScrollState.TouchScroll)
					refresher.Enabled = false;
				else
					refresher.Enabled = true;
			};

			refresher = FindViewById<SwipeRefreshLayout> (Resource.Id.refresher);
			refresher.SetColorSchemeColors (Resource.Color.accent);
			refresher.Refresh += (sender, args) => {
				RefreshData (true);
				refresher.PostDelayed (() => {
					refresher.Refreshing = false;
				}, 250);
			};

			SupportActionBar.SetDisplayHomeAsUpEnabled (false);
			SupportActionBar.SetHomeButtonEnabled (false);
			mapView.GetMapAsync (this);
			CheckGooglePlayServices ();

			// No savedInstanceState, so it is the first launch of this activity
			if (bundle == null && AppInviteReferral.HasReferral(Intent)) {
				// In this case the referral data is in the intent launching the MainActivity,
				// which means this user already had the app installed. We do not have to
				// register the Broadcast Receiver to listen for Play Store Install information
				LaunchDeepLinkActivity(Intent);

			}
		}

		public void LaunchDeepLinkActivity(Intent intent) 
		{
			Log.Debug("MainActivity", "LaunchDeepLinkActivity:" + intent);
			var newIntent = new Intent (intent);
			newIntent.SetClass (this, typeof(DetailsActivity));
			StartActivity(newIntent);
		}

		void RegisterDeepLinkReceiver() 
		{
			// Create local Broadcast receiver that starts 
			//DeepLinkActivity when a deep link is found
			deepLinkReceiver = new InviteBroadcastReceiver(this);

			var intentFilter = new IntentFilter(GetString(Resource.String.action_deep_link));
			LocalBroadcastManager.GetInstance(this).RegisterReceiver(
				deepLinkReceiver, intentFilter);
		}

		void UnregisterDeepLinkReceiver() 
		{
			if (deepLinkReceiver == null)
				return;
			
			LocalBroadcastManager.GetInstance(this).UnregisterReceiver(deepLinkReceiver);

		}

		bool CheckGooglePlayServices ()
		{
			var result = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
			if (result == ConnectionResult.Success)
				return true;
			
			var dialog = GoogleApiAvailability.Instance.GetErrorDialog (this, result, ConnectionFailureResolutionRequest);
			
			if (dialog != null) {
				var errorDialog = SupportErrorDialogFragment.NewInstance (dialog);
				errorDialog.Show (SupportFragmentManager, "Google Services Updates");
				return false;
			}

			Finish ();
			return false;
		}

		public void OnMapReady (GoogleMap map)
		{
			googleMap = map;
			googleMap.UiSettings.CompassEnabled = false;
			googleMap.UiSettings.MyLocationButtonEnabled = false;
			googleMap.UiSettings.MapToolbarEnabled = false;

			RefreshData ();
		}

		void ShowProgress (bool busy)
		{
			if (error) {
				mapView.Visibility = ViewStates.Invisible;
				errorImage.Visibility = ViewStates.Visible;
				pager.Visibility = ViewStates.Gone;
			} else {
				errorImage.Visibility = ViewStates.Gone;
				mapView.Visibility = busy ? ViewStates.Invisible : ViewStates.Visible;
				pager.Visibility = busy ? ViewStates.Gone : ViewStates.Visible;
			}
			refresher.Enabled = !busy; 
			progressBar.Visibility = busy ? ViewStates.Visible : ViewStates.Gone;
			if (busy)
				coffeeProgress.Start ();
			else
				coffeeProgress.Stop ();
		}

		void UpdateMap (int position)
		{
			if (googleMap == null)
				return;
			
			try {
				MapsInitializer.Initialize (this);
			} catch (GooglePlayServicesNotAvailableException e) {
				Console.WriteLine ("Google Play Services not available:" + e);
				error = true;
				#if !DEBUG
				Xamarin.Insights.Report (e, "GPS", "Not Available");
				#endif
				return;
			}

			if (markerOptions == null)
				markerOptions = new MarkerOptions ();

			if (circleOptions == null)
				circleOptions = new CircleOptions ();

			var place = viewModel.Places [position];
			var markerLatLong = new LatLng (place.Geometry.Location.Latitude, place.Geometry.Location.Longitude);

			if (marker == null) {
				markerOptions.SetTitle (place.Name);
				markerOptions.SetPosition (markerLatLong);
				marker = googleMap.AddMarker (markerOptions);
			} else {
				marker.Position = markerLatLong;
				marker.Title = place.Name;
			}

			var userLocation = new LatLng (viewModel.Position.Latitude, viewModel.Position.Longitude);

			if (locationCircle == null) {
				circleOptions.InvokeCenter (userLocation);
				circleOptions.InvokeRadius (radius);
				circleOptions.InvokeStrokeWidth (0);
				circleOptions.InvokeFillColor (Resources.GetColor (Resource.Color.accent));
				locationCircle = googleMap.AddCircle (circleOptions);
			} else {
				locationCircle.Center = userLocation;
				locationCircle.Radius = radius;
			}

			if (!initMap) {
				var farthest = viewModel.Places [viewModel.Places.Count - 1];
				var distance = farthest.GetDistance (userLocation.Latitude, userLocation.Longitude);

				radius = 20.0 * (distance / .45);
				radius = Math.Min (radius, 40.0);
				locationCircle.Radius = radius;

				var points = new LatLngBounds.Builder ();
				points.Include (userLocation);
				foreach (var p in viewModel.Places)
					points.Include (new LatLng (p.Geometry.Location.Latitude, p.Geometry.Location.Longitude));
				
				var bounds = points.Build ();

				if (mapView.Width == 0) {
					initTry = 0;
					PostDelayInitMap (bounds);
				} else {
					googleMap.MoveCamera (CameraUpdateFactory.NewLatLngBounds (bounds, 0));
				}
				initMap = true;
			}
		}

		void PostDelayInitMap (LatLngBounds bounds)
		{
			if (initTry == 4)
				return;

			mapView.PostDelayed (() => {
				initTry++;
				if (mapView.Width == 0)
					PostDelayInitMap (bounds);
				else
					googleMap.MoveCamera (CameraUpdateFactory.NewLatLngBounds (bounds, 0));
			}, 250);
		}



		protected override void OnStart ()
		{
			base.OnStart ();

			RegisterDeepLinkReceiver ();

			if (googleMap != null && lastRefresh.AddMinutes (5) < DateTime.UtcNow)
				RefreshData ();


		}

		protected override void OnStop ()
		{
			base.OnStop ();
			UnregisterDeepLinkReceiver ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			mapView.OnResume ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			mapView.OnPause ();
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			mapView.OnDestroy ();
		}

		public override void OnLowMemory ()
		{
			base.OnLowMemory ();
			mapView.OnLowMemory ();
		}



		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			mapView.OnSaveInstanceState (outState);
		}

		//async void is bad, but we are firing and forgetting here, so no worries
		async void RefreshData (bool forceRefresh = false, string search = null)
		{
			error = false;
			initMap = false;
			lastRefresh = DateTime.UtcNow;
			if (googleMap == null)
				return;

			ShowProgress (true);
			refreshing = true;

			InvalidateOptionsMenu ();
			try {
				if (!viewModel.IsConnected) {
					Toast.MakeText (this, Resource.String.no_network, ToastLength.Short).Show ();
					error = true;
					return;
				}

				await viewModel.GetLocation (forceRefresh);
				if (viewModel.Position == null) {
					Toast.MakeText (this, Resource.String.unable_to_get_locations, ToastLength.Short).Show ();
					error = true;
					return;
				}

				var items = await viewModel.GetPlaces (search);
				if (items == null || items.Count == 0) {
					Toast.MakeText (this, Resource.String.nothing_open, ToastLength.Short).Show ();
					error = true;
				}

				RunOnUiThread (() => {
					adapter.NotifyDataSetChanged ();
					if (viewModel.Places.Count != 0) {
						pager.CurrentItem = 0;
						UpdateMap (0);
					}
				});
			} finally {
				ShowProgress (false);
				refreshing = false;
				InvalidateOptionsMenu ();
			}
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			//change menu_search to your name
			if (!refreshing) {
				this.MenuInflater.Inflate (Resource.Menu.menu_home, menu);

				if (viewModel.Places.Count == 0)
					menu.RemoveItem (Resource.Id.action_navigation);

				var searchItem = menu.FindItem (Resource.Id.action_search);
				var provider = MenuItemCompat.GetActionView (searchItem);
				searchView = provider.JavaCast<Android.Support.V7.Widget.SearchView> ();

				searchView.QueryTextSubmit += (sender, args) => {
					RefreshData (true, args.Query.Trim ());
					var view = sender as Android.Support.V7.Widget.SearchView;
					if (view != null)
						view.ClearFocus ();
				};
			}
			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (viewModel.IsBusy)
				return base.OnOptionsItemSelected (item);

			if (item.ItemId == Resource.Id.action_navigation) {
				if (viewModel.Places.Count == 0)
					return base.OnOptionsItemSelected (item);
				var current = viewModel.Places [pager.CurrentItem];
				viewModel.NavigateToShop (current);
			}

			return base.OnOptionsItemSelected (item);
		}
	}

	public class PlacesPagerAdapter : FragmentStatePagerAdapter
	{
		public CoffeeFilterViewModel ViewModel { get; set; }

		public override int Count {
			get {
				return ViewModel == null || ViewModel.Places == null ? 0 : ViewModel.Places.Count;
			}
		}

		public PlacesPagerAdapter (Android.Support.V4.App.FragmentManager fm, CoffeeFilterViewModel vm) : base (fm)
		{
			ViewModel = vm;
		}

		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			var frag = PlaceFragment.CreateNewInstance (ViewModel.Places [position], ViewModel.Position);
			frag.PlaceId = ViewModel.Places [position].PlaceId;
			return frag;
		}

		public override int GetItemPosition (Java.Lang.Object item)
		{
			return PositionNone;
		}
	}
}


