using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using UIKit;

using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOS
{
	public partial class DetailsViewController : UITableViewController
	{
		DetailsViewModel viewModel;
		float contentBottomOffset = 20.0f;

		public DetailsViewController (IntPtr handle) : base (handle)
		{
			TabBarItem.Title = "details".LocalizedString ("Name of the details tab");
			TabBarItem.SetFinishedImages (UIImage.FromBundle ("information"), UIImage.FromBundle ("information"));
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			TableView.ContentInset = new UIEdgeInsets { Bottom = contentBottomOffset };
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			viewModel = ServiceContainer.Resolve<DetailsViewModel> ();
			SetInformation ();
			SetOpenDays ();
			SetLocation ();
			SetTableHeader ();

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "info" },
				{ "name", viewModel.Place.Name },
			});
			#endif
		}

		async void SetTableHeader ()
		{
			if (!viewModel.Place.HasImage)
				return;

			var imageData = await ResourceLoader.DefaultLoader.GetImageData (viewModel.Place.Photos [0].ImageUrlLarge);

			using (var image = UIImage.LoadFromData (NSData.FromArray (imageData))) {
				var cropppedImage = image.CGImage.WithImageInRect (
					new CGRect ((image.CGImage.Width - TableView.Frame.Width) / 2.0f,
						image.CGImage.Height / 3.0f,
						TableView.Frame.Width,
						150.0f)
				);

				TableView.TableHeaderView = new UIImageView (new UIImage (cropppedImage));
			}
		}

		void SetInformation ()
		{
			InformationCell.PopulateWithData ();
		}

		void SetOpenDays ()
		{
			int count = 0;

			string[] daysOfWeek = new string[] { 
				"monday".LocalizedString ("Monday"), 
				"tuesday".LocalizedString ("Tuesday"), 
				"wednesday".LocalizedString ("Wednesday"), 
				"thursday".LocalizedString ("Thursday"), 
				"friday".LocalizedString ("Friday"), 
				"saturday".LocalizedString ("Saturday"), 
				"sunday".LocalizedString ("Sunday")
			};

			if (viewModel.Place.OpeningHours.WeekdayText == null || viewModel.Place.OpeningHours.WeekdayText.Count == 0) {
				OpenHoursCell.Hidden = true;
				return;
			}
				
			foreach (var view in OpenHoursCell.ContentView) {
				((OpenHoursView)view).DayLabel.Text = daysOfWeek [count];
				((OpenHoursView)view).HoursLabel.Text = viewModel.GetTime (viewModel.Place.OpeningHours.WeekdayText [count]);
				count++;
			}
		}

		void SetLocation ()
		{
			var placeLocation = viewModel.Place.Geometry.Location;
			var coordinates = new CLLocationCoordinate2D (placeLocation.Latitude, placeLocation.Longitude);

			ZoomInToMyLocation (coordinates);

			var placeAnnotation = new MKPointAnnotation {
				Coordinate = coordinates
			};

			MapView.AddAnnotation (placeAnnotation);
		}

		void ZoomInToMyLocation (CLLocationCoordinate2D location)
		{
			var delta = 2;
			var region = MKCoordinateRegion.FromDistance (location, delta, delta);
			MapView.SetRegion (region, false);
		}
	}
}
