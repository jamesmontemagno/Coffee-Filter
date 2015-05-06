using System;
using System.Collections.Generic;

using Foundation;
using WatchKit;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class ReviewRowController : NSObject
	{
		List<WKInterfaceImage> stars;

		public Review Review {
			set {
				stars = new List<WKInterfaceImage> (new WKInterfaceImage[] { star_0, star_1, star_2, star_3, star_4 });
				var count = 0;

				foreach (var starView in stars) {
					starView.SetHidden (count > value.Rating);
					count++;
				}

				ReviewLabel.SetText (string.IsNullOrEmpty (value.Text) ? "Rating only" : value.Text);
				AuthorNameLabel.SetText (value.AuthorName);
				DateLabel.SetText (DateTimeUtils.ParseUnixTime (value.Time).ToString ("d"));
			}
		}
	}
}

