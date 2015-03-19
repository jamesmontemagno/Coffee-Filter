using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using CoffeeFilter.Shared.Models;
using Android.Content.PM;
using Android.Support.V4.Widget;
using CoffeeFilter.Shared.ViewModels;
using Android.Runtime;
using Android.Content;
using CoffeeFilter.Fragments;
using CoffeeFilter.Shared.Helpers;

namespace CoffeeFilter
{
	#if UITest
	[Activity (Label = "Coffee Filter", ScreenOrientation = ScreenOrientation.Portrait,  MainLauncher = false, Icon = "@drawable/ic_launcher")]
	#else
	[Activity (Label = "Coffee Filter", ScreenOrientation = ScreenOrientation.Portrait,  MainLauncher = true, Icon = "@drawable/ic_launcher")]
	#endif
	public class MainActivity : BaseActivity, IOnMapReadyCallback
	{
		private SwipeRefreshLayout refresher;
		private CoffeeFilterViewModel viewModel;
		private ViewPager pager;
		private PlacesPagerAdapter adapter;
		private Marker marker;
		private MarkerOptions markerOptions;
		private MapView mapView;
		private Circle locationCircle;
		private CircleOptions circleOptions;
		private ImageView progressBar, errorImage;
		private AnimationDrawable coffeeProgress;
		GoogleMap googleMap;
		DateTime lastRefresh = DateTime.UtcNow;

		protected override int LayoutResource {
			get {
				return Resource.Layout.main;
			}
		}

		protected override void OnCreate (Bundle bundle)
		{

			base.OnCreate (bundle);

			#if !DEBUG
			Xamarin.Insights.Initialize("8da86f8b3300aa58f3dc9bbef455d0427bb29086", this);
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
				if(e.State == (int)ScrollState.TouchScroll)
					refresher.Enabled = false;
				else
					refresher.Enabled = true;
			};
			
			/*pager.PageScrolled += (sender, e) => {
				//TranslateMap(e.Position, e.PositionOffset);
				//Console.WriteLine("Offset: " + e.PositionOffset);
				//Console.WriteLine("Offset Pixels: " + e.PositionOffsetPixels);
				//Console.WriteLine("Position: " + e.Position);
			};*/

			refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
			refresher.SetColorScheme(Resource.Color.accent);
			refresher.Refresh += (sender, args) =>
			{
				RefreshData (true);
				refresher.PostDelayed(()=>{refresher.Refreshing = false;}, 250);
			};

			SupportActionBar.SetDisplayHomeAsUpEnabled (false);
			SupportActionBar.SetHomeButtonEnabled (false);
			mapView.GetMapAsync (this);
			CheckGooglePlayServices ();
		}
		const int ConnectionFailureResolutionRequest = 9000;
		bool CheckGooglePlayServices ()
		{
			var result = GooglePlayServicesUtil.IsGooglePlayServicesAvailable (this);
			if (result == ConnectionResult.Success)
				return true;
			var dialog = GooglePlayServicesUtil.GetErrorDialog (result,
				this,
				ConnectionFailureResolutionRequest);
			if (dialog != null) {
				var errorDialog = SupportErrorDialogFragment.NewInstance (dialog);
				errorDialog.Show (SupportFragmentManager, "Google Services Updates");
				return false;
			}

			Finish ();
			return false;
		}


		public void OnMapReady (GoogleMap googleMap)
		{
			this.googleMap = googleMap;
			this.googleMap.UiSettings.CompassEnabled = false;
			this.googleMap.UiSettings.MyLocationButtonEnabled = false;
			this.googleMap.UiSettings.MapToolbarEnabled = false;

			RefreshData ();
		}


		private void ShowProgress(bool busy)
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

		bool initMap;
		bool error;
		//int oldPosition;
		/*
		//still testing for now
		private void TranslateMap(int newPosition, float offset)
		{
			if (markerOptions == null || marker == null || offset <= 0.0f)
				return;

			if (newPosition == oldPosition) {
				newPosition++;
			}

			if (newPosition > viewModel.Places.Count)
				return;

			double lat, lng;
		

			var oldLat = viewModel.Places [oldPosition].Geometry.Location.Latitude;
			var oldLng = viewModel.Places [oldPosition].Geometry.Location.Longitude;
		

			var newLat = viewModel.Places [newPosition].Geometry.Location.Latitude;
			var newLng = viewModel.Places [newPosition].Geometry.Location.Longitude;

			if (oldLat > newLat) {
				lat = oldLat - ((oldLat - newLat) * offset);
			} else {
				lat = oldLat + ((newLat - oldLat) * offset);
			}

			if (oldLng > newLng) {
				lng = oldLng - ((oldLng - newLng) * offset);
			} else {
				lng = oldLng + ((newLng - oldLng) * offset);
			}

			marker.Position = new LatLng(lat, lng);
		}*/
		private double radius = 16;
		private void UpdateMap(int position)
		{
			if (googleMap == null)
				return;
			//oldPosition = position;
			try
			{
				MapsInitializer.Initialize(this);
			}
			catch(GooglePlayServicesNotAvailableException e) {
				Console.WriteLine ("Google Play Services not available:" + e);
				error = true;
				#if !DEBUG
				Xamarin.Insights.Report(e, "GPS", "Not Available");
				#endif
				return;
			}

			if(markerOptions == null)
			{
				markerOptions = new MarkerOptions();
			}

			if (circleOptions == null) {
				circleOptions = new CircleOptions ();
			}

			var place = viewModel.Places[position];
			var markerLatLong = new LatLng(place.Geometry.Location.Latitude,
				place.Geometry.Location.Longitude);


			if (marker == null) {
				markerOptions.SetTitle(place.Name);
				markerOptions.SetPosition(markerLatLong);
				marker = googleMap.AddMarker (markerOptions);
			}
			else{
				marker.Position = markerLatLong;
				marker.Title = place.Name;
			}
				

			var me = new LatLng (viewModel.Position.Latitude, viewModel.Position.Longitude);

			if (locationCircle == null) {
				circleOptions.InvokeCenter(me);
				circleOptions.InvokeRadius (radius);
				circleOptions.InvokeStrokeWidth (0);
				circleOptions.InvokeFillColor (Resources.GetColor (Resource.Color.accent));
				locationCircle = googleMap.AddCircle (circleOptions);
			} else {
				locationCircle.Center = me;
				locationCircle.Radius = radius;
			}

			/*var builder = new LatLngBounds.Builder ();
			builder.Include (me);
			builder.Include (markerLatLong);

			var finalBounds = builder.Build ();
		

			googleMap.MoveCamera(CameraUpdateFactory.NewLatLngBounds(finalBounds, 0));*/

			if (!initMap) {

				var farthest = viewModel.Places [viewModel.Places.Count - 1];
				var distance = farthest.GetDistance (me.Latitude, me.Longitude);
				//var zoom = 16.0 - (distance / .5);
				radius = 20.0 * (distance / .45);
				radius = Math.Min (radius, 40.0);
				locationCircle.Radius = radius;

				var points = new LatLngBounds.Builder ();
				points.Include (me);
				foreach (var p in viewModel.Places) {
					points.Include (new LatLng (p.Geometry.Location.Latitude, p.Geometry.Location.Longitude));
				}
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

		int initTry = 0;
		private void PostDelayInitMap(LatLngBounds bounds)
		{
			if (initTry == 4)
				return;

			mapView.PostDelayed (() => {
				initTry++;
				if(mapView.Width == 0)
					PostDelayInitMap(bounds);
				else
					googleMap.MoveCamera (CameraUpdateFactory.NewLatLngBounds (bounds, 0));
			}, 250);
		}
			
		protected override void OnStart ()
		{
			base.OnStart ();
			if (googleMap != null && lastRefresh.AddMinutes (5) < DateTime.UtcNow)
				RefreshData ();
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

		bool refreshing = false;
		//async void is bad, but we are firing and forgetting here, so no worries
		private async void RefreshData(bool forceRefresh = false, string search = null)
		{
			error = false;
			initMap = false;
			lastRefresh = DateTime.UtcNow;
			if (googleMap == null)
				return;

			ShowProgress (true);
			refreshing = true;

			this.InvalidateOptionsMenu ();
			try
			{
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

				RunOnUiThread(()=>{

					adapter.NotifyDataSetChanged();
					if(viewModel.Places.Count != 0){
						pager.CurrentItem = 0;
						UpdateMap(0);
					}
				});
			}
			catch{
			}
			finally{
				ShowProgress (false);
				refreshing = false;
				this.InvalidateOptionsMenu ();
			}
		}


		Android.Support.V7.Widget.SearchView searchView;
		public override bool OnCreateOptionsMenu(IMenu menu)
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
					RefreshData(true, args.Query.Trim());
					var view = sender as Android.Support.V7.Widget.SearchView;
					if(view != null)
						view.ClearFocus();
				};
					
			}
			return base.OnCreateOptionsMenu(menu);
		}
			

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (viewModel.IsBusy)
				return base.OnOptionsItemSelected (item);

			switch (item.ItemId) {
			case Resource.Id.action_navigation:
				if (viewModel.Places.Count == 0)
					return base.OnOptionsItemSelected (item);
				var current = viewModel.Places [pager.CurrentItem];
				viewModel.NavigateToShop (current);
				break;
			}

			return base.OnOptionsItemSelected (item);
		}
	}

	public class PlacesPagerAdapter : FragmentStatePagerAdapter
	{
		public CoffeeFilterViewModel ViewModel { get; set; }
		public PlacesPagerAdapter(Android.Support.V4.App.FragmentManager fm, CoffeeFilterViewModel vm) : base(fm)
		{
			this.ViewModel = vm;
		}


		#region implemented abstract members of PagerAdapter
		public override int Count {
			get {
				return ViewModel == null || ViewModel.Places == null ? 0 : ViewModel.Places.Count;
			}
		}
		#endregion
		#region implemented abstract members of FragmentPagerAdapter
		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			var frag = PlaceFragment.NewInstance (ViewModel.Places[position], ViewModel.Position);
			frag.PlaceId = ViewModel.Places [position].PlaceId;
			return frag;
		}
		#endregion

		public override int GetItemPosition (Java.Lang.Object item)
		{
			return PositionNone;
		}

	}
}


