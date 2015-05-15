using System;
using UIKit;
using System.Linq;

namespace CoffeeFilter.iOS
{
	public partial class RatingView : UIView
	{
		public int Rating { 
			set {
				Console.WriteLine(value);

				for (int i = 0; i < Subviews.Length; i++) {
					
					var imageView = Subviews?[i] as UIImageView;
					if (imageView != null)
						imageView.Image = (imageView.Tag <= value) ? UIImage.FromBundle("i_star_rate") : null;
				}

//				var count = 0;
//				foreach (var starView in Subviews) {
//					if (count < value)
//						((UIImageView)starView).Image = UIImage.FromBundle("i_star_rate");
//					else
//						starView.Hidden = true;
//				}
			}
		}

		public RatingView (IntPtr handle) : base(handle)
		{
		}
	}
}