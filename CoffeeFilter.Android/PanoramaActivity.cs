using System.Collections.Generic;

using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;

namespace CoffeeFilter
{
	[Activity (Label = "Panorama Activity", ScreenOrientation = ScreenOrientation.Portrait)]			
	public class PanoramaActivity : AppCompatActivity, IOnStreetViewPanoramaReadyCallback
	{
		LatLng latlng;
		StreetViewPanoramaView streetViewPanoramaView;
		StreetViewPanorama streetPanorama;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.panorama);
			var lat = Intent.GetDoubleExtra ("lat", 37.7977);
			var lng = Intent.GetDoubleExtra ("lng", -122.40);

			latlng = new LatLng (lat, lng);
			streetViewPanoramaView = FindViewById<StreetViewPanoramaView> (Resource.Id.panorama);
			streetViewPanoramaView.OnCreate (bundle);
			streetViewPanoramaView.GetStreetViewPanoramaAsync (this);

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "panorama" }
			});
			#endif
		}

		public override void OnWindowFocusChanged (bool hasFocus)
		{
			base.OnWindowFocusChanged (hasFocus);
			if (hasFocus && (int)Build.VERSION.SdkInt >= 19) {
				Window.DecorView.SystemUiVisibility = 
					(StatusBarVisibility)(SystemUiFlags.LayoutStable
						| SystemUiFlags.HideNavigation
						| SystemUiFlags.LayoutFullscreen 
						| SystemUiFlags.Fullscreen
						| SystemUiFlags.ImmersiveSticky);
			}
		}

		public void OnStreetViewPanoramaReady (StreetViewPanorama panorama)
		{
			streetPanorama = panorama;
			streetPanorama.UserNavigationEnabled = true;
			streetPanorama.StreetNamesEnabled = true;
			streetPanorama.PanningGesturesEnabled = true;
			streetPanorama.ZoomGesturesEnabled = true;

			streetPanorama.SetPosition (latlng);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			streetViewPanoramaView.OnResume ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			streetViewPanoramaView.OnPause ();
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			streetViewPanoramaView.OnDestroy ();
		}

		public override void OnLowMemory ()
		{
			base.OnLowMemory ();
			streetViewPanoramaView.OnLowMemory ();
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			streetViewPanoramaView.OnSaveInstanceState (outState);
		}
	}
}

