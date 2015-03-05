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
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Models;
using Android.Content.PM;

namespace CoffeeFilter
{
	#if UITest
	[Activity (Label = "Coffee Filter", ScreenOrientation = ScreenOrientation.Portrait,  MainLauncher = false, Icon = "@drawable/ic_launcher")]
	#else
	[Activity (Label = "Coffee Filter", ScreenOrientation = ScreenOrientation.Portrait,  MainLauncher = true, Icon = "@drawable/ic_launcher")]
	#endif
	public class MainActivity : ActionBarActivity, IOnMapReadyCallback
	{

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
		protected override void OnCreate (Bundle bundle)
		{

			base.OnCreate (bundle);

			#if !DEBUG
			Xamarin.Insights.Initialize("YourAPIKey", this);
			#endif

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.main);
			mapView = FindViewById<MapView> (Resource.Id.map);
			mapView.OnCreate (bundle);

			mapView.Visibility = ViewStates.Gone;
			viewModel = new CoffeeFilterViewModel ();
			adapter = new PlacesPagerAdapter (SupportFragmentManager, viewModel);
			pager = FindViewById<ViewPager> (Resource.Id.pager);
			pager.Adapter = adapter;
			progressBar = FindViewById<ImageView> (Resource.Id.progressBar);
			errorImage = FindViewById<ImageView> (Resource.Id.error);
			coffeeProgress = (AnimationDrawable)progressBar.Background;
			progressBar.Visibility = ViewStates.Gone;

			pager.PageSelected += (sender, e) => UpdateMap (e.Position);
			pager.PageScrolled += (sender, e) => {
				//TranslateMap(e.Position, e.PositionOffset);
				//Console.WriteLine("Offset: " + e.PositionOffset);
				//Console.WriteLine("Offset Pixels: " + e.PositionOffsetPixels);
				//Console.WriteLine("Position: " + e.Position);
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

		public async void OnMapReady (GoogleMap googleMap)
		{
			this.googleMap = googleMap;
			this.googleMap.UiSettings.CompassEnabled = false;
			this.googleMap.UiSettings.MyLocationButtonEnabled = false;
			this.googleMap.UiSettings.MapToolbarEnabled = false;

			await RefreshData ();
		}


		private void ShowProgress(bool busy)
		{
			if (error) {
				mapView.Visibility = ViewStates.Gone;
				errorImage.Visibility = ViewStates.Visible;
				pager.Visibility = ViewStates.Gone;
			} else {
				errorImage.Visibility = ViewStates.Gone;
				mapView.Visibility = busy ? ViewStates.Gone : ViewStates.Visible;
				pager.Visibility = busy ? ViewStates.Gone : ViewStates.Visible;
			}
			progressBar.Visibility = busy ? ViewStates.Visible : ViewStates.Gone;
			if (busy)
				coffeeProgress.Start ();
			else
				coffeeProgress.Stop ();
		}

		bool initMap;
		bool error;
		int oldPosition;
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
			oldPosition = position;
			try
			{
				MapsInitializer.Initialize(this);
			}
			catch(GooglePlayServicesNotAvailableException e) {
				Console.WriteLine ("Google Play Services not available");
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
				var zoom = 16.0 - (distance / .5);
				radius = 19.0 * (distance / .45);
				locationCircle.Radius = radius;
			
				googleMap.MoveCamera (CameraUpdateFactory.NewLatLngZoom(me, (int)zoom));
				initMap = true;
			}
		}
			
		private async Task RefreshData(bool forceRefresh = false)
		{
			error = false;
			initMap = false;
			if (googleMap == null)
				return;

			ShowProgress (true);
			try
			{
				if (!viewModel.IsConnected) {
					Toast.MakeText (this, "No Network Connection Available.", ToastLength.Short).Show ();
					error = true;
					return;
				}

				adapter.NotifyDataSetChanged ();

				await viewModel.GetLocation (forceRefresh);
				if (viewModel.Position == null) {
					Toast.MakeText (this, "Unable to get coffee locations.", ToastLength.Short).Show ();
					error = true;
					return;
				}

				var items = await viewModel.GetPlaces ();
				if (items == null || items.Count == 0) {
					Toast.MakeText (this, "There are no coffee shops nearby that are open. Try tomorrow. :(", ToastLength.Short).Show ();
					error = true;
				}

				RunOnUiThread(()=>{
					adapter.NotifyDataSetChanged ();

					if(viewModel.Places.Count != 0){
						pager.CurrentItem = 0;
						UpdateMap(0);
					}
				});
			}
			finally{
				ShowProgress (false);
			}
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.menu_home, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (viewModel.IsBusy)
				return base.OnOptionsItemSelected (item);

			switch (item.ItemId) {
			case Resource.Id.refresh:
				RefreshData (true);
				break;
			}

			return base.OnOptionsItemSelected (item);
		}

	}

	public class PlacesPagerAdapter : FragmentPagerAdapter
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
			return PlaceFragment.NewInstance (ViewModel.Places[position], ViewModel.Position);
		}
		#endregion
	}
}


