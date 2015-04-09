using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using CoffeeFilter.Shared.Models;
using Android.Gms.MapsSdk.Model;
using Android.Gms.MapsSdk;
using Android.Content;
using System.Linq;
using System;
using ExternalMaps.Plugin;
using Geolocator.Plugin.Abstractions;
using System.Globalization;
using System.Collections.Generic;
using CoffeeFilter.Helpers;


namespace CoffeeFilter.Fragments
{
	public class PlaceDetailsFragment : Fragment, IOnMapReadyCallback
	{
		MapView mapFragment;
		GoogleMap map;

		public Place Place { get; set; }

		public Position Position { get; set; }

		public static PlaceDetailsFragment NewInstance (Place place, Position position)
		{
			var f = new PlaceDetailsFragment () {
				Place = place,
				Position = position
			};
			var b = new Bundle ();

			f.Arguments = b;
			return f;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
		}


		FilterScrollView mainScroll;
		ImageView image;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate (Resource.Layout.fragment_details, container, false);
			mapFragment = root.FindViewById<MapView> (Resource.Id.map);
			mapFragment.OnCreate (savedInstanceState);


			var name = root.FindViewById<TextView> (Resource.Id.name);
			var reviews = root.FindViewById<TextView> (Resource.Id.rating);
			image = root.FindViewById<ImageView> (Resource.Id.image);
			var address = root.FindViewById<TextView> (Resource.Id.address);
			var priceHours = root.FindViewById<TextView> (Resource.Id.price_hours);
			var phone = root.FindViewById<TextView> (Resource.Id.phone_number);
			var website = root.FindViewById<TextView> (Resource.Id.website);
			var googlePlus = root.FindViewById<TextView> (Resource.Id.google_plus);
			var monday = root.FindViewById<TextView> (Resource.Id.monday);
			var tuesday = root.FindViewById<TextView> (Resource.Id.tuesday);
			var wednesday = root.FindViewById<TextView> (Resource.Id.wednesday);
			var thursday = root.FindViewById<TextView> (Resource.Id.thursday);
			var friday = root.FindViewById<TextView> (Resource.Id.friday);
			var saturday = root.FindViewById<TextView> (Resource.Id.saturday);
			var sunday = root.FindViewById<TextView> (Resource.Id.sunday);
			var allHours = root.FindViewById<LinearLayout> (Resource.Id.all_hours);
			var allWeb = root.FindViewById<LinearLayout> (Resource.Id.all_web);
			var distance = root.FindViewById<TextView> (Resource.Id.distance);
			mainScroll = root.FindViewById<FilterScrollView> (Resource.Id.main_scroll);


			var dis = Place.GetDistance (Position.Latitude,
				          Position.Longitude, CultureInfo.CurrentCulture.Name != "en-US" ? 
				CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Kilometers :
					CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles).ToString ("##.###", CultureInfo.CurrentUICulture);

			distance.Text = string.Format (Resources.GetString (Resource.String.distance_away), dis);

			if (string.IsNullOrWhiteSpace (Place.Rating.ToString ()))
				reviews.Visibility = ViewStates.Gone;
			else
				reviews.Text = string.Format (Resources.GetString (Resource.String.based_on_review), Place.Rating, Place.UserRatingsCount);
			
			name.Text = Place.Name;
			phone.Text = Place.PhoneNumberFormatted;
			if (!string.IsNullOrWhiteSpace (phone.Text)) {
				phone.Clickable = true;
				phone.Click += (sender, e) => {
					try {
						#if !DEBUG
						Xamarin.Insights.Track ("Click", new Dictionary<string,string> {
							{ "item", "phone" },
							{ "name", Place.Name },
						});
						#endif
						var uri = Android.Net.Uri.Parse ("tel:" + (string.IsNullOrWhiteSpace (Place.InternationalPhoneNumber) ? phone.Text : Place.InternationalPhoneNumber));
						var intent = new Intent (Intent.ActionView, uri); 
						Activity.StartActivity (intent);  
					} catch (Exception ex) {
						#if !DEBUG
						ex.Data ["call"] = "phone";
						Xamarin.Insights.Report (ex);
						#endif
					}
				};
			}

			
      address.Text = Place.DisplayAddress;

      priceHours.Text = Place.PriceOpenDisplay(Activity.Resources.GetString (Resource.String.open_now), Activity.Resources.GetString (Resource.String.open_until));

			if (Place.HasImage)
				Koush.UrlImageViewHelper.SetUrlDrawable (image, Place.MainImage);

      if (!Place.HasOpeningHours)
				allHours.Visibility = ViewStates.Gone;
			else {
        monday.Text = Place.DisplayHoursMonday;
        tuesday.Text = Place.DisplayHoursTuesday;
        wednesday.Text = Place.DisplayHoursWednesday;
        thursday.Text = Place.DisplayHoursThursday;
        friday.Text = Place.DisplayHoursFriday;
        saturday.Text = Place.DisplayHoursSaturday;
        sunday.Text = Place.DisplayHoursSunday;
			}

			if (string.IsNullOrWhiteSpace (Place.Website) && string.IsNullOrWhiteSpace (Place.Url))
				allWeb.Visibility = ViewStates.Gone;

			if (string.IsNullOrWhiteSpace (Place.Website)) {
				website.Visibility = ViewStates.Gone;
				root.FindViewById<TextView> (Resource.Id.website_header).Visibility = ViewStates.Gone;
			} else
				website.Text = Place.Website;

			if (string.IsNullOrWhiteSpace (Place.Url)) {
				googlePlus.Visibility = ViewStates.Gone;
				root.FindViewById<TextView> (Resource.Id.google_plus_header).Visibility = ViewStates.Gone;
			} else
				googlePlus.Text = "plus.google.com";

			website.Clickable = true;
			website.Click += (sender, e) => {
				try {
					#if !DEBUG
					Xamarin.Insights.Track ("Click", new Dictionary<string,string> {
						{ "item", "website" },
						{ "name", Place.Name },
					});
					#endif
					var intent = new Intent (Intent.ActionView);
					intent.SetData (Android.Net.Uri.Parse (Place.Website));
					Activity.StartActivity (intent);
				} catch (Exception ex) {
					#if !DEBUG
					ex.Data ["call"] = "website";
					Xamarin.Insights.Report (ex);
					#endif
				}
			};

			googlePlus.Clickable = true;
			googlePlus.Click += (sender, e) => {
				try {
					#if !DEBUG
					Xamarin.Insights.Track ("Click", new Dictionary<string,string> {
						{ "item", "google plus" },
						{ "name", Place.Name },
					});
					#endif
					var intent = new Intent (Intent.ActionView);
					intent.SetData (Android.Net.Uri.Parse (Place.Url));
					Activity.StartActivity (intent);
				} catch (Exception ex) {
					#if !DEBUG
					ex.Data ["call"] = "google+";
					Xamarin.Insights.Report (ex);
					#endif
				}
			};
				

			var panorama = root.FindViewById<Button> (Resource.Id.panorama);
			panorama.Click += (sender, e) => {
				var intent = new Intent (Activity, typeof(PanoramaActivity));
				intent.PutExtra ("lat", (double)Place.Geometry.Location.Latitude);
				intent.PutExtra ("lng", (double)Place.Geometry.Location.Longitude);
				Activity.StartActivity (intent);
			};

			if (!Place.HasImage) {
				var paddingLayout = root.FindViewById<LinearLayout> (Resource.Id.padding_layout);
				paddingLayout.SetPadding (0, 0, 0, 0);
			}

			return root;
		}

	

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			mapFragment.GetMapAsync (this);
			mainScroll.OnScrolledEvent += MainScroll_OnScrolledEvent;
		}

		public override void OnDestroyView ()
		{
			base.OnDestroyView ();
			mainScroll.OnScrolledEvent -= MainScroll_OnScrolledEvent;
		}

		void MainScroll_OnScrolledEvent (object sender, ScrolledEventArgs args)
		{
			image.TranslationY = -(int)((float)args.Y / 2.5F);
		}

		public void OnMapReady (GoogleMap googleMap)
		{
			this.map = googleMap;
			MapsInitializer.Initialize (Activity.ApplicationContext);

			this.map.MapClick += GoogleMap_MapClick;
			this.map.MarkerClick += Map_MarkerClick;

			this.map.UiSettings.CompassEnabled = false;
			this.map.UiSettings.MyLocationButtonEnabled = false;
			this.map.UiSettings.MapToolbarEnabled = false;
			var markerLatLong = new LatLng (Place.Geometry.Location.Latitude, Place.Geometry.Location.Longitude);
			var markerOptions = new MarkerOptions ();
      markerOptions.InvokeTitle (Place.Name);
      markerOptions.InvokePosition (markerLatLong);
			this.map.AddMarker (markerOptions);
			this.map.MoveCamera (CameraUpdateFactory.NewLatLng (markerLatLong));
		}

		void Map_MarkerClick (object sender, GoogleMap.MarkerClickEventArgs e)
		{
			MapClick ();
		}

		void MapClick ()
		{
			#if !DEBUG
			Xamarin.Insights.Track ("Navigation", new Dictionary<string, string> { {
					"name",
					Place.Name
				}, {
					"rating",
					Place.Rating.ToString ()
				}
			});
			#endif
			CrossExternalMaps.Current.NavigateTo (Place.Name, Place.Geometry.Location.Latitude, Place.Geometry.Location.Longitude);
		}

		void GoogleMap_MapClick (object sender, GoogleMap.MapClickEventArgs e)
		{
			MapClick ();
		}



		public override void OnResume ()
		{
			base.OnResume ();
			mapFragment.OnResume ();
		}

		public override void OnPause ()
		{
			base.OnPause ();
			mapFragment.OnPause ();
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			mapFragment.OnDestroy ();
		}

		public override void OnLowMemory ()
		{
			base.OnLowMemory ();
			mapFragment.OnLowMemory ();
		}

		public override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			mapFragment.OnSaveInstanceState (outState);
		}
	}
}

