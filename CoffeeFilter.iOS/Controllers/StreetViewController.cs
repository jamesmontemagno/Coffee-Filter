using System;
using System.Collections.Generic;

using Google.Maps;
using UIKit;

using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.iOS
{
	public class StreetViewController : UIViewController
	{
		Location position;
		PanoramaView panoView;


		public StreetViewController (Location position, string title)
		{
			this.position = position;
			Title = title;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			panoView = new PanoramaView ();
			View = panoView;
			panoView.MoveNearCoordinate (new CoreLocation.CLLocationCoordinate2D (position.Latitude, position.Longitude));

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "panorama" }
			});
			#endif
		}
	}
}

