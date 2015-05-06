using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using UIKit;
using CoffeeFilter.Shared;

namespace CoffeeFilter.iOS
{
	public partial class PhotoCell : UICollectionViewCell
	{
		static Dictionary<string, UIImage> images = new Dictionary<string, UIImage> ();

		public static void ClearImages ()
		{
			images.Clear ();
		}

		public PhotoCell (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithFrame:")]
		public PhotoCell (CGRect frame) : base (frame)
		{
			PhotoView.Image = UIImage.FromBundle ("placeholder");
		}

		public async void SetImage (string url)
		{
			UIImage image = null;

			if (!images.ContainsKey (url)) {
				var spinner = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray);
				spinner.StartAnimating ();
				spinner.Center = new CGPoint (PhotoView.Frame.Width / 2f, PhotoView.Frame.Height / 2f);
				ContentView.AddSubview (spinner);

				var imageData = await ResourceLoader.DefaultLoader.GetImageData (url);
				image = UIImage.LoadFromData (NSData.FromArray (imageData));

				spinner.StopAnimating ();
				spinner.RemoveFromSuperview ();

				images.Add (url, image);
			} else {
				image = images [url];
			}

			PhotoView.ContentMode = UIViewContentMode.ScaleAspectFill;
			PhotoView.Image = image;
		}
	}
}
