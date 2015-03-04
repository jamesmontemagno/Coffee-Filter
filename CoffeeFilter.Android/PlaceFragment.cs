using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using CoffeeFilter.Shared.Models;
using Geolocator.Plugin.Abstractions;
using ExternalMaps.Plugin;
using System.Globalization;
using System.Collections.Generic;

namespace CoffeeFilter
{
	public class PlaceFragment : Fragment
	{

		private string name, distance, rating;
		private double lat, lng;
		public static PlaceFragment NewInstance(Place place, Position position)
		{
			var f = new PlaceFragment ();
			var b = new Bundle ();
			b.PutString("name", place.Name);
			b.PutString ("distance", place.GetDistance(position.Latitude,
				position.Longitude, CultureInfo.CurrentCulture.Name != "en-US" ? 
				CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Kilometers :
				CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles).ToString("##.###", CultureInfo.CurrentUICulture));
			b.PutString ("rating", place.Rating.ToString ("#.#", CultureInfo.CurrentUICulture));
			b.PutDouble ("lat", place.Geometry.Location.Latitude);
			b.PutDouble ("lng", place.Geometry.Location.Longitude);
			f.Arguments = b;
			return f;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			name = Arguments.GetString ("name");
			distance = Arguments.GetString ("distance");
			rating = Arguments.GetString ("rating");
			lat = Arguments.GetDouble ("lat");
			lng = Arguments.GetDouble ("lng");
		}


		public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate(Resource.Layout.fragment_place, container, false);
			var top = root.FindViewById<TextView> (Resource.Id.top);
			top.Text = distance + (CultureInfo.CurrentCulture.Name != "en-US" ? " km" : " mi.");

			var bottom = root.FindViewById<TextView> (Resource.Id.bottom);
			bottom.Text = name;
			bottom.LongClickable = true;
			bottom.LongClick += HandleNavigationClick;

			var ratingText = root.FindViewById<TextView> (Resource.Id.rating);
			ratingText.Text = rating;
			return root;
		}

		void HandleNavigationClick (object sender, System.EventArgs e)
		{
			#if !DEBUG
			Xamarin.Insights.Track("Navigation", new Dictionary<string,string>
				{
					{"name", name},
					{"rating", rating},
					{"lat", lat.ToString()},
					{"lng", lng.ToString()}
				});
			#endif
			CrossExternalMaps.Current.NavigateTo (name, lat, lng);
		}
	}
}

