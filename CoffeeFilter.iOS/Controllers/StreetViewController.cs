using System;
using System.Collections.Generic;

using UIKit;
using Foundation;
using CoreLocation;

using Google.Maps;

using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.iOS
{
	public class StreetViewController : UIViewController
	{
		CLLocationCoordinate2D location;

		public StreetViewController (Location position, string title)
		{
			location = new CLLocationCoordinate2D (position.Latitude, position.Longitude);
			Title = title;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var panoView = new PanoramaView ();
			panoView.MoveNearCoordinate (location);
			View = panoView;

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "panorama" }
			});
			#endif
		}
	}
}

