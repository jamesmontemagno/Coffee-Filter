using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using WatchKit;
using CoreLocation;
using MapKit;

using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;
using CoffeeFilter.Shared.Helpers;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class MapDetailController : WKInterfaceController
	{
		const int MetersInKilometer = 1000;

		DetailsViewModel viewModel;

		public MapDetailController (IntPtr handle) : base (handle)
		{
		}

		public override void Awake (NSObject context)
		{
			viewModel = ServiceContainer.Resolve <DetailsViewModel> ();

			RatingLabel.SetText (viewModel.Place.Rating.ToString ());
			DistanceLabel.SetText (CoffeeFilterViewModel.GetDistanceToPlace (viewModel.Place, viewModel.Position));
			PlaceNameLabel.SetText (viewModel.Place.Name);

			SetMapRegion ();
			AddMapAnnotation ();
			EnableButtons (false);
			UpdatePlaceInfo ();

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "details" },
				{ "name", viewModel.Place.Name }
			});
			#endif
		}

		void SetMapRegion ()
		{
			var placePosition = viewModel.Position;
			var userPosition = new CLLocationCoordinate2D (placePosition.Latitude, placePosition.Longitude);
			var distance = viewModel.Place.GetDistance (placePosition.Latitude, placePosition.Longitude, GeolocationUtils.DistanceUnit.Kilometers);
			distance += 6; // Add more distance units to display map annotations in screen bounds
			var region = MKCoordinateRegion.FromDistance (userPosition, distance * MetersInKilometer, distance * MetersInKilometer);
			MapView.SetRegion (region);
		}

		void AddMapAnnotation ()
		{
			var placePosition = viewModel.Position;
			MapView.AddAnnotation (
				new CLLocationCoordinate2D (placePosition.Latitude,placePosition.Longitude),
				WKInterfaceMapPinColor.Red
			);
		}

		async void UpdatePlaceInfo ()
		{	
			await viewModel.RefreshPlace ();

			var reviewsButtonTitle = viewModel.Place.HasReviews ?
				string.Format ("{0} {1}", viewModel.Place.Reviews.Count, "reviews".LocalizedString ("Title of the reviews button")) :
				"no_reviews".LocalizedString ("If place have no reviews");

			SetButton (ReviewsButton, reviewsButtonTitle, viewModel.Place.HasReviews);

			var photosButtonTitle = viewModel.Place.HasImage ?
				string.Format ("{0} {1}", viewModel.Place.Photos.Count, "photos".LocalizedString ("Title of the photos button")) :
				"no_photos".LocalizedString ("If place have no photos");

			SetButton (PhotosButton, photosButtonTitle, viewModel.Place.HasImage);
		}

		void EnableButtons (bool enabled)
		{
			ReviewsButton.SetEnabled (enabled);
			PhotosButton.SetEnabled (enabled);
		}

		void SetButton (WKInterfaceButton button, string title, bool enabled)
		{
			button.SetTitle (title);
			button.SetEnabled (enabled);
		}
	}
}
