using System;
using System.Collections.Generic;

using WatchKit;
using Foundation;
using CoreLocation;
using UIKit;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class InterfaceController : WKInterfaceController
	{
		CoffeeFilterViewModel ViewModel {
			get {
				CoffeeFilterViewModel viewModel;

				try {
					viewModel = ServiceContainer.Resolve<CoffeeFilterViewModel> ();
				} catch {
					viewModel = new CoffeeFilterViewModel ();
					ServiceContainer.Register<CoffeeFilterViewModel> (viewModel);
				}

				return viewModel;
			}
		}

		public InterfaceController (IntPtr handle) : base (handle)
		{
			#if !DEBUG
			Xamarin.Insights.Initialize ("8da86f8b3300aa58f3dc9bbef455d0427bb29086");
			#endif
		}

		public override void Awake (NSObject context)
		{
			base.Awake (context);
			RefreshPlaces ();
		}

		public override NSObject GetContextForSegue (string segueIdentifier, WKInterfaceTable table, nint rowIndex)
		{	
			ServiceContainer.Register<DetailsViewModel> (new DetailsViewModel {
				Place = ViewModel.Places [(int)rowIndex],
				Position = ViewModel.Position
			});

			return null;
		}

		void LoadTableRows ()
		{
			PlacesTable.SetNumberOfRows (ViewModel.Places.Count, "default");

			for (var i = 0; i < ViewModel.Places.Count; i++) {
				var placeRow = (PlaceRowController)PlacesTable.GetRowController (i);
				placeRow.RowText = ViewModel.Places [i].Name;
			}

			ShowAnimation (false);
		}

		async void UpdatePlaces ()
		{
			if (!ViewModel.IsConnected) {
				ShowAnimation (false);
				ShowWarning (true, "no_network".LocalizedString ("Network connection failure message"));
				return;
			}

			var success = await ViewModel.GetLocation (true);

			if (!success) {
				ShowAnimation (false);
				ShowWarning (true, "unable_to_get_locations".LocalizedString ("Places request failure message"));
				return;
			}

			await ViewModel.GetPlaces ();

			if (ViewModel.Places == null) {
				ShowAnimation (false);
				ShowWarning (true, "unable_to_get_locations".LocalizedString ("Places request failure message"));
				return;
			}

			if (ViewModel.Places.Count == 0) {
				ShowAnimation (false);
				ShowWarning (true, "nothing_open".LocalizedString ("There are no coffee shops nearby that are open. Try tomorrow. :("));
				return;
			}

			LoadTableRows ();
		}

		void ShowAnimation (bool show)
		{
			CoffeeAnimationView.SetHidden (!show);
			ImageGroup.SetHidden (!show);

			if (show) {
				ImageGroup.SetBackgroundColor (UIColor.FromRGB (239, 235, 233));
				CoffeeAnimationView.SetImage ("ic_mug");
				CoffeeAnimationView.StartAnimating (new NSRange (0, 9), 3.0, Int32.MaxValue);
			} else {
				ImageGroup.SetBackgroundColor (UIColor.Black);
				CoffeeAnimationView.StopAnimating ();
			}
		}

		void ShowWarning (bool show, string warningMessage = " ")
		{
			WarningGroup.SetHidden (!show);

			if (show) {
				TryAgainButton.SetTitle ("try_again".LocalizedString ("Title of the try again button"));
				WarningMessageLabel.SetText (warningMessage);
			}
		}

		void RefreshPlaces ()
		{
			ShowAnimation (true);
			UpdatePlaces ();
		}

		partial void TryAgainButtonClick ()
		{
			ShowWarning (false);
			RefreshPlaces ();
		}
	}
}

