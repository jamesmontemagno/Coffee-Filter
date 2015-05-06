using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using ExternalMaps.Plugin;
using Geolocator.Plugin.Abstractions;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.Fragments
{
	public class PlaceFragment : Fragment
	{
		string name, distance, rating;
		double lat, lng;
		Button placeName;

		public string PlaceId { get; set; }

		public static PlaceFragment CreateNewInstance (Place place, Position position)
		{
			var b = new Bundle ();
			b.PutString ("name", place.Name);
			b.PutString ("distance", place.GetDistance (position.Latitude,
				position.Longitude, CultureInfo.CurrentCulture.Name != "en-US" ? 
				CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Kilometers :
				CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles).ToString ("##.###", CultureInfo.CurrentUICulture));
			
			b.PutString ("rating", place.Rating.ToString ("#.#", CultureInfo.CurrentUICulture));
			b.PutDouble ("lat", place.Geometry.Location.Latitude);
			b.PutDouble ("lng", place.Geometry.Location.Longitude);
			b.PutString ("placeId", place.PlaceId);

			return new PlaceFragment {
				Arguments = b
			};
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

		public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate (Resource.Layout.fragment_place, container, false);
			var top = root.FindViewById<TextView> (Resource.Id.top);
			top.Text = distance + (CultureInfo.CurrentCulture.Name != "en-US" ? " km" : " mi.");

			placeName = root.FindViewById<Button> (Resource.Id.bottom);
			placeName.Text = name;

			var ratingText = root.FindViewById<TextView> (Resource.Id.rating);
			ratingText.Text = rating;
			var star = root.FindViewById<ImageView> (Resource.Id.star);

			star.Visibility = (string.IsNullOrWhiteSpace (rating) ? ViewStates.Invisible : ViewStates.Visible);
			return root;
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			placeName.Click += HandleInfoClick;
		}

		public override void OnDestroyView ()
		{
			base.OnDestroyView ();
			placeName.Click -= HandleInfoClick;
		}

		void HandleInfoClick (object sender, System.EventArgs e)
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

