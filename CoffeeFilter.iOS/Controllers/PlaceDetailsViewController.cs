using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using AddressBook;
using CoreFoundation;
using AddressBookUI;

using ExternalMaps.Plugin;
using Google.Maps;

using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;
using CoffeeFilter.Shared.Helpers;

namespace CoffeeFilter.iOS
{
	public partial class PlaceDetailsViewController : UITabBarController
	{
		DetailsViewModel viewModel;
		ABAddressBook addressBook;
		UIPopoverController shareController;
		UIBarButtonItem shareButton;
		UIBarButtonItem moreButton;

		public PlaceDetailsViewController (IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad();

			viewModel = ServiceContainer.Resolve<DetailsViewModel>();
			UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(0.46f, 0.27f, 0.13f);
			moreButton = new UIBarButtonItem (UIImage.FromBundle("more"), UIBarButtonItemStyle.Plain, OpenPlaceOptions);
			shareButton = new UIBarButtonItem (UIBarButtonSystemItem.Action, SharePlaceInfo);
			NavigationItem.RightBarButtonItems = new [] { moreButton, shareButton };

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "details" },
				{ "name", viewModel.Place.Name },
			});
			#endif
		}

		void SharePlaceInfo (object sender, EventArgs e)
		{
			var message = new NSString (string.Format("{0}\n{1}\n{2}", viewModel.Place.Name, viewModel.Place.Website, viewModel.ShortAddress));

			var activityController = new UIActivityViewController (new NSObject [] {
				message
			}, null);

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				shareController = new UIPopoverController (activityController);
				shareController.PresentFromBarButtonItem(shareButton, UIPopoverArrowDirection.Any, true);
			} else {
				PresentViewController(activityController, true, null);
			}
		}


		void OpenPlaceOptions (object sender, EventArgs e)
		{
			var showMapsButtonTitle = "open_maps".LocalizedString("Show maps button title");
			var streetViewButtonTitle = "street_view".LocalizedString("Street View button title");
			var makeCallButtonTitle = "make_a_call".LocalizedString("Make a call button title");
			var addToContactsButtonTitle = "add_to_contacts".LocalizedString("Make a call button title");
			var cancelButtonTitle = "cancel".LocalizedString("Cancel button title");

			ShowAlertViewController(showMapsButtonTitle, makeCallButtonTitle, addToContactsButtonTitle, streetViewButtonTitle, cancelButtonTitle);
		}


		void ShowAlertViewController (string showMapsButtonTitle, string makeCallButtonTitle, string addToContactsButtonTitle, string streetViewButtonTitle, string cancelButtonTitle)
		{
			var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

			alertController.AddAction(UIAlertAction.Create(streetViewButtonTitle, UIAlertActionStyle.Default, OpenStreetView));
			alertController.AddAction(UIAlertAction.Create(showMapsButtonTitle, UIAlertActionStyle.Default, ShowMaps));
			alertController.AddAction(UIAlertAction.Create(makeCallButtonTitle, UIAlertActionStyle.Default, MakeACall));
			alertController.AddAction(UIAlertAction.Create(addToContactsButtonTitle, UIAlertActionStyle.Default, AddContact));
			alertController.AddAction(UIAlertAction.Create(cancelButtonTitle, UIAlertActionStyle.Destructive, null));

			SetupPopover(alertController, View);

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				var moreController = new UIPopoverController (alertController);
				moreController.PresentFromBarButtonItem(moreButton, UIPopoverArrowDirection.Any, true);
			} else {
				PresentViewController(alertController, true, null);
			}
		}


		void ShowMaps (UIAlertAction action)
		{
			#if !DEBUG
			Xamarin.Insights.Track ("Navigation", new Dictionary<string, string> {
				{ "name", viewModel.Place.Name },
				{ "rating", viewModel.Place.ToString () }
			});
			#endif
			CrossExternalMaps.Current.NavigateTo(viewModel.Place.Name, viewModel.Position.Latitude, viewModel.Position.Longitude);
		}


		void OpenStreetView (UIAlertAction action)
		{
			var panoramaService = new PanoramaService ();
			var location = new CoreLocation.CLLocationCoordinate2D (viewModel.Place.Geometry.Location.Latitude,
				viewModel.Place.Geometry.Location.Longitude);
			panoramaService.RequestPanorama (location, PanoramaRequestCallback);
		}

		void PanoramaRequestCallback (Panorama panorama, NSError error)
		{
			if (error != null) {
				var alertController = UIAlertController.Create ("Warning",
					"Street view for this location is not available", UIAlertControllerStyle.Alert);
				alertController.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Destructive,
					_ => NavigationController.PopViewController (false)));
				return;
			}

			var streetViewController = new StreetViewController (viewModel.Place.Geometry.Location, viewModel.Place.Name);
			NavigationController.PushViewController(streetViewController, false);
		}

		void AddContact (UIAlertAction action)
		{
			try {
				#if !DEBUG
				Xamarin.Insights.Track ("Click", new Dictionary<string,string> {
					{ "item", "add contact" },
					{ "name", viewModel.Place.Name },
				});
				#endif
				NSError error;
				addressBook = ABAddressBook.Create(out error);
				CheckAddressBookAccess();
			} catch (Exception ex) {
				#if !DEBUG
				ex.Data ["call"] = "add contact";
				Xamarin.Insights.Report (ex);
				#endif
			}
		}


		void MakeACall (UIAlertAction action)
		{
			try {
				#if !DEBUG
				Xamarin.Insights.Track ("Click", new Dictionary<string,string> {
					{ "item", "phone" },
					{ "name", viewModel.Place.Name },
				});
				#endif

				if (string.IsNullOrEmpty(viewModel.Place.InternationalPhoneNumber)) {
					var alertController = UIAlertController.Create(string.Empty, "place_have_no_phone".LocalizedString("Alert message if place have no phone number"), UIAlertControllerStyle.Alert);
					alertController.AddAction(UIAlertAction.Create("ok".LocalizedString("OK title for button"), UIAlertActionStyle.Destructive, null));
					PresentViewController(alertController, true, null);
					return;
				}

				string phoneAppUrl = string.Format("telprompt://{0}", viewModel.Place.InternationalPhoneNumber.Replace(' ', '-'));
				var formattedString = new NSString (phoneAppUrl).CreateStringByReplacingPercentEscapes(NSStringEncoding.UTF8);
				using (var phoneaAppUrl = new NSUrl (formattedString))
					UIApplication.SharedApplication.OpenUrl(phoneaAppUrl);
			} catch (Exception ex) {
				#if !DEBUG
				ex.Data ["call"] = "phone";
				Xamarin.Insights.Report (ex);
				#endif
			}
		}


		void SetupPopover (UIAlertController alertController, UIView sourceView)
		{
			var popover = alertController.PopoverPresentationController;

			if (popover != null) {
				popover.SourceView = sourceView;
				popover.SourceRect = sourceView.Bounds;
			}
		}


		void CheckAddressBookAccess ()
		{
			switch (ABAddressBook.GetAuthorizationStatus()) {
			case ABAuthorizationStatus.Authorized:
				ShowNewContactViewController();
				break;
			case ABAuthorizationStatus.NotDetermined:
				RequestAddressBookAccess();
				break;
			case ABAuthorizationStatus.Denied:
			case ABAuthorizationStatus.Restricted:
				var alert = UIAlertController.Create(string.Empty,
					            "contacts_permission_request_denied".LocalizedString("Contacts permission request denied"),
					            UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("ok".LocalizedString("OK title for button"), UIAlertActionStyle.Default, null));
				PresentViewController(alert, true, null);
				break;
			default:
				break;
			}
		}


		void RequestAddressBookAccess ()
		{
			addressBook.RequestAccess((bool granted, NSError error) => {
				if (!granted)
					return;
				DispatchQueue.MainQueue.DispatchAsync(() => ShowNewContactViewController());
			});
		}


		async void ShowNewContactViewController ()
		{
			using (var contact = new ABPerson ()) {
				try {

					contact.FirstName = viewModel.Place.Name;

					if (viewModel.Place.HasImage) {
						var data = await ResourceLoader.DefaultLoader.GetImageData(viewModel.Place.Photos[0].ImageUrl);
						contact.Image = UIImage.LoadFromData(NSData.FromArray(data)).AsJPEG();
					}

					using (var phone = new ABMutableStringMultiValue ()) {
						phone.Add(viewModel.Place.PhoneNumberFormatted, ABLabel.Other);
						contact.SetPhones(phone);
					}

					using (var webSite = new ABMutableStringMultiValue ()) {
						webSite.Add(viewModel.Place.Website, ABLabel.Other);
						contact.SetUrls(webSite);
					}

					var upvc = new ABUnknownPersonViewController {
						DisplayedPerson = contact,
						AllowsAddingToAddressBook = true,
						AllowsActions = true,
						AlternateName = viewModel.Place.Name,
						Title = viewModel.Place.Name
					};

					NavigationController.PushViewController(upvc, true);
				} catch (Exception) {
					var alert = UIAlertController.Create("Error", "Could not create unknown user.", UIAlertControllerStyle.Alert);
					alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, null));
					PresentViewController(alert, true, null);
				}
			}
		}
	}
}
