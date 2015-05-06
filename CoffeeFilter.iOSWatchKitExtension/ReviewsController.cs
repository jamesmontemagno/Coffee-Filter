using System;
using System.Collections.Generic;

using Foundation;
using WatchKit;

using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class ReviewsController : WKInterfaceController
	{
		List<Review> reviews;

		public override void Awake (NSObject context)
		{
			var viewModel = ServiceContainer.Resolve <DetailsViewModel> ();
			reviews = viewModel.Place.Reviews;
			LoadTableRows ();

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "reviews" },
				{ "name", viewModel.Place.Name }
			});
			#endif
		}

		void LoadTableRows ()
		{
			ReviewsTable.SetNumberOfRows (reviews.Count, "default");

			for (var i = 0; i < reviews.Count; i++) {
				var placeRow = (ReviewRowController)ReviewsTable.GetRowController (i);
				placeRow.Review = reviews [i];
			}
		}
	}
}
