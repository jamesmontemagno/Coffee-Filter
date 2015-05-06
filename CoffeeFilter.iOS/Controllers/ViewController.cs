using System;
using System.Collections.Generic;

using UIKit;
using CoreGraphics;
using Foundation;
using CoreLocation;
using MapKit;
using CoreAnimation;

using CoffeeFilter.Shared.ViewModels;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.iOS
{
	public partial class ViewController : UIViewController
	{
		const int MetersInKilometer = 1000;

		UIColor tintColor = UIColor.FromRGB (239, 235, 233);
		bool error;
		CoffeeFilterViewModel viewModel;
		List<PlaceView> placeViews;
		MKPointAnnotation currentPlaceAnnotation;
		Place currentPlace;
		CoffeeAnimationView coffeeAnimation;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			placeViews = new List<PlaceView> ();

			coffeeAnimation = CoffeeAnimationView.GetView (this);

			NavigationController.NavigationBar.TintColor = tintColor;
			var appearance = UIBarButtonItem.AppearanceWhenContainedIn (typeof (UINavigationBar));
			appearance.SetTitleTextAttributes (new UITextAttributes { TextColor = tintColor, Font = UIFont.FromName ("HelveticaNeue-Light", 20f) },
				UIControlState.Normal);

			MapView.ShowsUserLocation = true;
			ScrollView.Scrolled += OnScrolled;

			try {
				viewModel = ServiceContainer.Resolve<CoffeeFilterViewModel> ();
			} catch {
				viewModel = new CoffeeFilterViewModel();
				ServiceContainer.Register<CoffeeFilterViewModel> (viewModel);
			}

			PlaceView.ButtonAction += OnPlaceSelected;

			var searchButton = NavigationItem.RightBarButtonItem;
			var navigationButton = new UIBarButtonItem (UIImage.FromBundle ("near"), UIBarButtonItemStyle.Plain, OpenMaps);
			NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] { searchButton, navigationButton }, false);

			RefreshData (true);
		}

		public virtual void OnScrolled (object sender, EventArgs e)
		{
			// Update the page when more than 50% of the previous/next page is visible
			nfloat pageWidth = ScrollView.Frame.Width;
			int page = (int)NMath.Floor ((ScrollView.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;

			PageControl.CurrentPage = page;
			currentPlace = viewModel.Places [page];
			SetAnnotationView (currentPlace);
		}

		partial void UpdatePlaces (UIBarButtonItem sender)
		{
			RefreshData (true);
		}

		partial void OpenSearch (UIBarButtonItem sender)
		{
			var searchViewController = new SearchViewController ();
			searchViewController.OnSearch += (searchString) => RefreshData (true, searchString);
			NavigationController.PushViewController (searchViewController, true);
		}

		void SetAnnotationView (Place place)
		{
			if (currentPlaceAnnotation != null)
				MapView.RemoveAnnotation (currentPlaceAnnotation);
			
			currentPlaceAnnotation = new MKPointAnnotation {
				Coordinate = new CLLocationCoordinate2D (place.Geometry.Location.Latitude, place.Geometry.Location.Longitude)	
			};

			MapView.AddAnnotation (currentPlaceAnnotation);
		}

		void OpenMaps (object sender, EventArgs e)
		{
			if (viewModel.Places == null || viewModel.Places.Count == 0)
				return;
			
			viewModel.NavigateToShop (currentPlace);
		}

		void ShowProgress (bool show)
		{
			NavigationController.NavigationBar.UserInteractionEnabled = !show;
			MapView.Hidden = show;

			if (show) {
				View.AddSubview (coffeeAnimation);
				coffeeAnimation.StartAnimation ();
			} else {
				coffeeAnimation.StopAnimation ();

				if (error)
					MapView.Hidden = true;
				else
					coffeeAnimation.RemoveFromSuperview ();
			}
		}

		void ClearViews ()
		{
			foreach (var placeView in placeViews)
				placeView.RemoveFromSuperview ();
		}

		async void RefreshData (bool forceRefresh = false, string search = null)
		{
			error = false;
			ShowProgress (true);
			PageControl.CurrentPage = 0;
			ClearViews ();

			try {
				var alert = new UIAlertView (string.Empty, string.Empty, null, "OK", null);
				if (!viewModel.IsConnected) {
					alert.Message = "no_network".LocalizedString ("Network connection failure message");
					alert.Show ();
					error = true;
					return;
				}

				await viewModel.GetLocation (forceRefresh);
				if (viewModel.Position == null) {
					alert.Message = "unable_to_get_locations".LocalizedString ("Places request failure message");
					alert.Show ();
					error = true;
					return;
				}

				var items = await viewModel.GetPlaces (search);
				if (items == null || items.Count == 0) {
					alert.Message = "nothing_open".LocalizedString ("Places request failure if everything is already closed");
					alert.Show ();
					error = true;
					return;
				}

				InvokeOnMainThread (() => {
					var location = new CLLocationCoordinate2D {
						Latitude = viewModel.Position.Latitude,
						Longitude = viewModel.Position.Longitude
					};
					ZoomInToMyLocation (location);
					InitScrollView ();
					currentPlace = viewModel.Places [0];
					SetAnnotationView (currentPlace);
				});
			} finally {
				ShowProgress (false);
			}
		}

		void ZoomInToMyLocation (CLLocationCoordinate2D location)
		{
			var delta = GetFarthestDistance (location.Latitude, location.Longitude);
			var region = MKCoordinateRegion.FromDistance (location, delta, delta);
			MapView.SetRegion (region, false);
		}

		double GetFarthestDistance (double latitude, double longitude)
		{
			var farthest = viewModel.Places [viewModel.Places.Count - 1];
			var distance = farthest.GetDistance (latitude, longitude,
				               CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Kilometers);
			distance += 2; // Add two more distance units to display all map annotations properly
			return distance * MetersInKilometer;
		}

		void InitScrollView ()
		{
			int count = 0;
			foreach (var place in viewModel.Places) {
				var placeView = NSBundle.MainBundle.LoadNib ("PlaceView", this, null).GetItem<PlaceView> (0);
				placeView.BackgroundColor = UIColor.Clear;
				placeView.Frame = new CGRect (ScrollView.Frame.Width * count, View.Frame.Y, ScrollView.Frame.Width, ScrollView.Frame.Height);
				placeView.PopulateWithData (viewModel.Places [count]);
				ScrollView.AddSubview (placeView);

				placeViews.Add (placeView);
				count++;
			}

			ScrollView.ContentSize = new CGSize (ScrollView.Frame.Width * viewModel.Places.Count, ScrollView.Frame.Height);
		}

		async void OnPlaceSelected ()
		{
			var detailsViewModel = new DetailsViewModel {
				Position = viewModel.Position,
				Place = currentPlace
			};

			ShowProgress (true);

			bool success = await detailsViewModel.RefreshPlace ();

			if (!success) {
				var alert = new UIAlertView (string.Empty, "unable_to_get_details".LocalizedString ("Details request failure message"), null, "OK", null);
				alert.Show ();
				return;
			}

			ShowProgress (false);

			ServiceContainer.RegisterScoped<DetailsViewModel> (detailsViewModel);
			var placeDetailsViewController = (PlaceDetailsViewController)Storyboard.InstantiateViewController ("PlaceDetailsViewController");
			NavigationController.PushViewController (placeDetailsViewController, false);
		}
	}
}

