using System;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;

using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class PlaceDetailsCell : UITableViewCell
	{
		public PlaceDetailsCell (IntPtr handle) : base(handle)
		{
		}

		public void PopulateWithData ()
		{
			var detailsViewModel = ServiceContainer.Resolve<DetailsViewModel>();

			PlaceName.Text = detailsViewModel.Place.Name;
			PlacePhone.Text = detailsViewModel.Place.PhoneNumberFormatted;
			PlaceDistance.Text = detailsViewModel.Distance;
			PlaceAddress.Text = detailsViewModel.ShortAddress;
			PlaceOpenHours.Text = detailsViewModel.OpenHours;
		}
	}
}
