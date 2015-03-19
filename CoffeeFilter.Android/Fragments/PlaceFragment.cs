using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using CoffeeFilter.Shared.Models;
using Geolocator.Plugin.Abstractions;
using System.Globalization;
using Android.Views;
using CoffeeFilter.Shared.Helpers;
using ExternalMaps.Plugin;
using CoffeeFilter.Shared.ViewModels;
using System.Linq;
using Android.Content;
using System.Collections.Generic;

namespace CoffeeFilter.Fragments
{
	public class PlaceFragment : Fragment
	{
		public string PlaceId { get; set; }
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
			b.PutString ("placeId", place.PlaceId);
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
			PlaceId = Arguments.GetString ("placeId");
		}

		Button placeName;
		public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate(Resource.Layout.fragment_place, container, false);
			var top = root.FindViewById<TextView> (Resource.Id.top);
			top.Text = distance + (CultureInfo.CurrentCulture.Name != "en-US" ? " km" : " mi.");

			placeName = root.FindViewById<Button> (Resource.Id.bottom);
			placeName.Text = name;

			var ratingText = root.FindViewById<TextView> (Resource.Id.rating);
			ratingText.Text = rating;
			var star = root.FindViewById<ImageView> (Resource.Id.star);

			star.Visibility = (string.IsNullOrWhiteSpace(rating) ? ViewStates.Invisible : ViewStates.Visible);
			return root;
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			placeName.Click += Info_Click;
		}

		public override void OnDestroyView ()
		{
			base.OnDestroyView ();
			placeName.Click -= Info_Click;
		}

		void Info_Click (object sender, System.EventArgs e)
		{
			var viewModel = ServiceContainer.Resolve<CoffeeFilterViewModel> ();
			ServiceContainer.AddScope ();
			ServiceContainer.RegisterScoped<DetailsViewModel> (new DetailsViewModel {
				Position = viewModel.Position,
				Place = viewModel.Places.FirstOrDefault (p => p.PlaceId == PlaceId)
			});

			var intent = new Intent (Activity, typeof(DetailsActivity));
			Activity.StartActivity (intent);
		}
	}
}

