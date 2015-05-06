using System;

using UIKit;
using Foundation;

namespace CoffeeFilter.iOS
{
	public partial class CoffeeAnimationView : UIView
	{
		public static CoffeeAnimationView GetView (UIViewController owner)
		{
			var coffeeAnimationView = NSBundle.MainBundle.LoadNib ("CoffeeAnimationView", owner, null).GetItem<CoffeeAnimationView> (0);
			coffeeAnimationView.Frame = owner.View.Frame;
			coffeeAnimationView.SetupCoffeeAnimation ();
			return coffeeAnimationView;
		}

		public CoffeeAnimationView (IntPtr handle) : base (handle)
		{
		}

		void SetupCoffeeAnimation ()
		{
			var animationImages = new UIImage[] {
				UIImage.FromBundle ("ic_mug1"),
				UIImage.FromBundle ("ic_mug2"),
				UIImage.FromBundle ("ic_mug3"),
				UIImage.FromBundle ("ic_mug4"),
				UIImage.FromBundle ("ic_mug5"),
				UIImage.FromBundle ("ic_mug4"),
				UIImage.FromBundle ("ic_mug3"),
				UIImage.FromBundle ("ic_mug2"),
				UIImage.FromBundle ("ic_mug1")
			};

			CoffeeImage.AnimationImages = animationImages;
			CoffeeImage.AnimationRepeatCount = int.MaxValue;
			CoffeeImage.AnimationDuration = 3.0;
			CoffeeImage.Image = UIImage.FromBundle ("ic_sadcoffee");
		}

		public void StartAnimation ()
		{
			CoffeeImage.StartAnimating ();
		}

		public void StopAnimation ()
		{
			CoffeeImage.StopAnimating ();
		}
	}
}

