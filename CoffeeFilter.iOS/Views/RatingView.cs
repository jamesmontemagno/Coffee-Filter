using System;

using Foundation;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class RatingView : UIView
	{
		public int Rating { 
			set {
				var count = 0;
				foreach (var starView in Subviews) {
					if (count < value)
						((UIImageView)starView).Image = UIImage.FromBundle ("star");
					else
						starView.Hidden = true;
				}
			}
		}

		public RatingView (IntPtr handle) : base (handle)
		{
		}
	}
}
