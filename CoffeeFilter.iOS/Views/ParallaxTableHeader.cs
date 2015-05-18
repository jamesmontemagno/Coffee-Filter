using System;

using UIKit;
using CoreGraphics;

namespace CoffeeFilter.iOS
{
	public class ParallaxTableHeader : UIView
	{
		readonly UIImageView imageView;


		public UIImage Image {
			set { imageView.Image = value; }
		}


		public ParallaxTableHeader (CGRect tableViewFrame, nfloat maxHeight)
		{
			Frame = new CGRect (0, 0, tableViewFrame.Width, maxHeight);

			imageView = new UIImageView (new CGRect (0, maxHeight / 2, tableViewFrame.Width, maxHeight));
			imageView.ContentMode = UIViewContentMode.ScaleAspectFill;

			Add(imageView);
		}


		public void UpdateOffset (nfloat offsetY)
		{
			var over = offsetY <= nfloat.Epsilon;

			ClipsToBounds = !over;

			imageView.ClipsToBounds = over;

			var x = over ? offsetY : 0;
			var y = over ? offsetY : offsetY / 2.5f;
			var w = over ? Frame.Width + (NMath.Abs(offsetY) * 2) : Frame.Width;
			var h = over ? Frame.Height + NMath.Abs(offsetY) : Frame.Height;

			imageView.Frame = new CGRect (x, y, w, h);
		}
	}
}