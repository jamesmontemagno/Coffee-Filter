using System;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class RatingView : UIView
	{
		public int Rating { 
			set {

				for (int i = 0; i < Subviews.Length; i++) {

					var imageView = Subviews?[i] as UIImageView;
					if (imageView != null)
						imageView.Image = (imageView.Tag <= value) ? UIImage.FromBundle("i_star_rate") : null;
				}
			}
		}

		public RatingView (IntPtr handle) : base(handle)
		{
		}
	}
}