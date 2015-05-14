using System;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class PlaceView : UIView
	{
		public static Action ButtonAction { get; set; }

		public PlaceView (IntPtr handle) : base(handle)
		{
		}

		public void PopulateWithData (Place place)
		{
			var coffeeFilterViewModel = ServiceContainer.Resolve<CoffeeFilterViewModel>();
			DistanceLabel.Text = CoffeeFilterViewModel.GetDistanceToPlace(place, coffeeFilterViewModel.Position);

			if (place.Rating != 0) {
				RatingLabel.Text = place.Rating.ToString();
				StarImageView.Image = UIImage.FromBundle("ic_star");
			} else {
				RatingLabel.Hidden = true;
				StarImageView.Hidden = true;
			}

			placeButton.SetTitle(place.Name, UIControlState.Normal);

			placeButton.TouchUpInside += (sender, e) => {
				if (ButtonAction != null)
					ButtonAction();
			};
		}
	}
}
