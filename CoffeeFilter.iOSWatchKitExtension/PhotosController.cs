using System;
using System.Collections.Generic;

using WatchKit;
using Foundation;

using Newtonsoft.Json;

using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class PhotosController : WKInterfaceController
	{
		List<Photo> photos;

		public override void Awake (NSObject context)
		{
			var viewModel = ServiceContainer.Resolve <DetailsViewModel> ();
			photos = viewModel.Place.Photos;
			LoadTableRows ();

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "photos" },
				{ "name", viewModel.Place.Name }
			});
			#endif
		}

		void LoadTableRows ()
		{
			PhotosTable.SetNumberOfRows (photos.Count, "default");

			for (var i = 0; i < photos.Count; i++) {
				var photoRow = (PhotoRowController)PhotosTable.GetRowController (i);
				photoRow.LoadPhoto (photos [i].ImageUrl);
			}
		}
	}
}
