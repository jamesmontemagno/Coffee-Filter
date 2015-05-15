using System;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;
using UIKit;
using Foundation;

namespace CoffeeFilter.iOS
{
	public partial class PlaceView : UIView
	{
		public static Action ButtonAction { get; set; }


		public PlaceView (IntPtr handle) : base(handle)
		{
			
		}


		public override void AwakeFromNib ()
		{
			base.AwakeFromNib();

			BackgroundColor = UIColor.Clear;
		}


		public void PopulateWithData (Place place)
		{
			var coffeeFilterViewModel = ServiceContainer.Resolve<CoffeeFilterViewModel>();
			DistanceLabel.Text = CoffeeFilterViewModel.GetDistanceToPlace(place, coffeeFilterViewModel.Position);

			if (place.Rating != 0) {
				RatingLabel.Text = place.Rating.ToString();
				StarImageView.Image = UIImage.FromBundle("i_star");
			} else {
				RatingLabel.Hidden = true;
				StarImageView.Hidden = true;
			}

			placeButton.SetTitle(place.Name, UIControlState.Normal);
		}

		partial void placeButtonTouchUpInside (NSObject sender) => ButtonAction?.Invoke();
	}
}