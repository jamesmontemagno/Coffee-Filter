using System;

using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class DetailsViewController : UITableViewController, IUIScrollViewDelegate
	{
		DetailsViewModel viewModel;

		nfloat tableHeaderHeight = 150, bottomContentInset = 19, topContentInset = 64;


		public DetailsViewController (IntPtr handle) : base(handle)
		{
			TabBarItem.Title = "details".LocalizedString("Name of the details tab");

			TabBarItem.Image = UIImage.FromBundle("information");
			TabBarItem.SelectedImage = UIImage.FromBundle("information");
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad();

			TableView.TableHeaderView = new ParallaxTableHeader (TableView.Bounds, tableHeaderHeight);
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);

			// scroll up to hide the header
			TableView.ContentInset = new UIEdgeInsets { Bottom = bottomContentInset, Top = topContentInset };

			TableView.SetContentOffset(new CGPoint (0, tableHeaderHeight - topContentInset), false);

			viewModel = ServiceContainer.Resolve<DetailsViewModel>();

			SetInformation();
			SetOpenDays();
			SetLocation();
			SetTableHeader();

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "info" },
				{ "name", viewModel.Place.Name },
			});
			#endif
		}


		[Export("scrollViewDidScroll:")]
		public void Scrolled (UIScrollView scrollView)
		{
			// compensate for the nav & status bar
			var offsetY = scrollView.ContentOffset.Y + topContentInset;

			((ParallaxTableHeader)TableView.TableHeaderView).UpdateOffset(offsetY);
		}


		async void SetTableHeader ()
		{
			if (!viewModel.Place.HasImage) {

				TableView.ContentInset = new UIEdgeInsets {
					Bottom = bottomContentInset, Top = -tableHeaderHeight + topContentInset
				};

				return;
			}

			var imageData = await ResourceLoader.DefaultLoader.GetImageData(viewModel.Place.Photos[0].ImageUrlLarge);

			using (var image = UIImage.LoadFromData(NSData.FromArray(imageData))) {

				var scale = TableView.Bounds.Width / image.Size.Width; 

				var size = CGAffineTransform.MakeScale(scale, scale).TransformSize(image.Size);

				Console.WriteLine($"Image Size: Before = {image.Size}, After = {size}");

				UIGraphics.BeginImageContextWithOptions(size, false, 0);

				image.Draw(new CGRect (CGPoint.Empty, size));

				((ParallaxTableHeader)TableView.TableHeaderView).Image = UIGraphics.GetImageFromCurrentImageContext();

				UIGraphics.EndImageContext();
			}
		}


		void SetInformation ()
		{
			InformationCell.PopulateWithData();
		}


		void SetOpenDays ()
		{
			int count = 0;

			var daysOfWeek = new [] { 
				"monday".LocalizedString("Monday"), 
				"tuesday".LocalizedString("Tuesday"), 
				"wednesday".LocalizedString("Wednesday"), 
				"thursday".LocalizedString("Thursday"), 
				"friday".LocalizedString("Friday"), 
				"saturday".LocalizedString("Saturday"), 
				"sunday".LocalizedString("Sunday")
			};

			if (viewModel.Place.OpeningHours.WeekdayText == null || viewModel.Place.OpeningHours.WeekdayText.Count == 0) {
				OpenHoursCell.Hidden = true;
				return;
			}
				
			foreach (var view in OpenHoursCell.ContentView) {
				((OpenHoursView)view).DayLabel.Text = daysOfWeek[count];
				((OpenHoursView)view).HoursLabel.Text = viewModel.GetTime(viewModel.Place.OpeningHours.WeekdayText[count]);
				count++;
			}
		}


		void SetLocation ()
		{
			var placeLocation = viewModel.Place.Geometry.Location;
			var coordinates = new CLLocationCoordinate2D (placeLocation.Latitude, placeLocation.Longitude);

			ZoomInToMyLocation(coordinates);

			var placeAnnotation = new MKPointAnnotation {
				Coordinate = coordinates
			};

			MapView.AddAnnotation(placeAnnotation);
		}


		void ZoomInToMyLocation (CLLocationCoordinate2D location)
		{
			const double delta = 2;
			var region = MKCoordinateRegion.FromDistance(location, delta, delta);
			MapView.SetRegion(region, false);
		}
	}
}
