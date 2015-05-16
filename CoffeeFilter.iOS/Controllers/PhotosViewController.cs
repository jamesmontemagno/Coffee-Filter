using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using CoreGraphics;

using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOS
{
	public partial class PhotosViewController : UICollectionViewController
	{
		const string ReuseIdentifier = "PhotoCellIdentifier";

		List<Photo> photos;

		public PhotosViewController (IntPtr handle) : base(handle)
		{
			TabBarItem.Title = "photos".LocalizedString("Name of the photos tab");

			TabBarItem.Image = UIImage.FromBundle("photos");
			TabBarItem.SelectedImage = UIImage.FromBundle("photos");

			photos = new List<Photo> ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);

			var viewModel = ServiceContainer.Resolve<DetailsViewModel>();

			if (CollectionViewFlow != null) {
				var itemDimension = View.Frame.Width / 2f - 10f; // 10f is the offset for collection view items
				CollectionViewFlow.ItemSize = new CGSize (itemDimension, itemDimension);
			}

			if (viewModel.Place.Photos != null && viewModel.Place.Photos.Count != 0) {
				photos = viewModel.Place.Photos;
			} else {
				var warnginView = WarningMessageView.GetView("no_photos".LocalizedString("If place have no photos"), this);
				View = warnginView;
			}

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "photos" },
				{ "name", viewModel.Place.Name },
			});
			#endif
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear(animated);
			PhotoCell.ClearImages();
		}

		public override nint NumberOfSections (UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return photos.Count;
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (PhotoCell)CollectionView.DequeueReusableCell(ReuseIdentifier, indexPath);
			cell.SetImage(photos[indexPath.Row].ImageUrl);
			return cell;
		}
	}
}
