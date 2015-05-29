using System;
using System.Threading.Tasks;
using System.Linq;

using CoreGraphics;
using Foundation;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class CoffeeAnimationView : UIView
	{
		UIView coffee;
		UIImageView steam;
		CGRect fullCoffee = new CGRect (9, 28, 40, 40), emptyCoffee = new CGRect (9, 67, 40, 1);

		public bool ShowSadCoffee { get; set; }

		UIImage regularCoffeeImage;
		UIImage RegularCoffeeImage {
			get {
				if (regularCoffeeImage == null)
					regularCoffeeImage = UIImage.FromBundle("ic_mug5");

				return regularCoffeeImage;
			}
		}

		UIImage sadCoffeeImage;
		UIImage SadCoffeeImage {
			get {
				if (sadCoffeeImage == null)
					sadCoffeeImage = UIImage.FromBundle("ic_sadcoffee");

				return sadCoffeeImage;
			}
		}

		bool ShowSteamAndCoffee {
			set {
				CoffeeImage.Subviews.ToList().ForEach(x => x.Hidden = value);
			}
		}

		public CoffeeAnimationView (IntPtr handle) : base(handle)
		{
		}


		public static CoffeeAnimationView GetView (UIViewController owner)
		{			
			var coffeeAnimationView = NSBundle.MainBundle.LoadNib<CoffeeAnimationView>(owner);

			var navBar = owner.NavigationController.NavigationBar.Bounds.Height + 20;
			var frame = new CGRect (0, navBar, owner.View.Bounds.Width, owner.View.Bounds.Height - navBar);

			coffeeAnimationView.Frame = frame;

			coffeeAnimationView.SetupCoffeeAnimation();

			return coffeeAnimationView;
		}


		void SetupCoffeeAnimation ()
		{
			coffee = new UIView (emptyCoffee);
			coffee.BackgroundColor = UIColor.FromRGB(101f / 255f, 67f / 255f, 56f / 255f);
			coffee.Layer.CornerRadius = 2;

			steam = new UIImageView (new CGRect (0, 0, CoffeeImage.Bounds.Width, 21));
			steam.Image = UIImage.FromBundle("ic_mug_steam");
			steam.Alpha = 0;

			CoffeeImage.AddSubviews(coffee, steam);
		}


		public async override void RemoveFromSuperview ()
		{
			// give it a sec (or 3/10 of a sec) to start the 'real; push animation
			await Task.Delay(300);

			// fade this sucker out every time we remove from super
			await UIView.AnimateAsync(0.2, () => Alpha = 0);

			base.RemoveFromSuperview();
			coffee.Frame = emptyCoffee;
			steam.Alpha = 0;
		}

		public bool LoopAnimation { get; set;}
		public async Task StartAnimation ()
		{
			if (CoffeeImage.Image != RegularCoffeeImage) {
				ShowSteamAndCoffee = false;
				CoffeeImage.Image = RegularCoffeeImage;
			}

			await UIView.AnimateAsync(0.2, () => Alpha = 1);
			await UIView.AnimateAsync(1.0, () => {
				coffee.Frame = fullCoffee;
				steam.Alpha = 1;
			});
			await Task.Delay(100);
			await UIView.AnimateAsync(1.0, () => {
				coffee.Frame = emptyCoffee;
				steam.Alpha = 0;
			});

			if (ShowSadCoffee) {
				LoopAnimation = false;
				ShowSteamAndCoffee = true;
				CoffeeImage.Image = SadCoffeeImage;
				return;
			}

			if(LoopAnimation)
				StartAnimation ();
		}
	}
}