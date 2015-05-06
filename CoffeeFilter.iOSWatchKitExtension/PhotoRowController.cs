using System;

using Foundation;

using CoffeeFilter.Shared;
using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class PhotoRowController : NSObject
	{
		public async void LoadPhoto (string url)
		{
			if (string.IsNullOrEmpty (url))
				return;

			try {
				ShowLoadingAnimation (true);
				var imageData = await ResourceLoader.DefaultLoader.GetImageData (url);
				ShowLoadingAnimation (false);
				PhotoView.SetImage (NSData.FromArray (imageData));
			} catch (Exception ex) {
				#if !DEBUG
				ex.Data ["call"] = "load photo";
				Xamarin.Insights.Report (ex);
				#endif
			}
		}

		void ShowLoadingAnimation (bool show)
		{
			LoadingAnimation.SetHidden (!show);

			if (show) {
				LoadingAnimation.SetImage ("loading_");
				LoadingAnimation.StartAnimating (new NSRange (0, 12), 2.0, Int32.MaxValue);
			} else {
				LoadingAnimation.StopAnimating ();
			}
		}
	}
}

