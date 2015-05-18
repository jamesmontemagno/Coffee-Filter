using System;
using System.Collections.Generic;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

using CoreGraphics;
using Foundation;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class PhotosViewController : UICollectionViewController, IUICollectionViewDelegateFlowLayout
	{
		const string ReuseIdentifier = "PhotoCellIdentifier";

		static nfloat itemInset = 10;
		static nfloat itemDimen = (UIScreen.MainScreen.Bounds.Width / 2) - (itemInset * 1.5f);
		static CGSize itemSize = new CGSize (itemDimen, itemDimen);
		static UIEdgeInsets sectionInset = new UIEdgeInsets (74, itemInset, 59, itemInset);

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

			if (viewModel.Place.Photos != null && viewModel.Place.Photos.Count != 0) {
				photos = viewModel.Place.Photos;
			} else {
				var warningView = WarningMessageView.GetView("no_photos".LocalizedString("If place have no photos"), this);
				View = warningView;
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


		#region IUICollectionViewDelegateFlowLayout

		[Export("collectionView:layout:sizeForItemAtIndexPath:")]
		public CGSize GetSizeForItem (UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		{
			return itemSize;
		}

		[Export("collectionView:layout:minimumInteritemSpacingForSectionAtIndex:")]
		public nfloat GetMinimumInteritemSpacingForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return itemInset / 2;
		}

		[Export("collectionView:layout:minimumLineSpacingForSectionAtIndex:")]
		public nfloat GetMinimumLineSpacingForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return itemInset;
		}

		[Export("collectionView:layout:insetForSectionAtIndex:")]
		public UIEdgeInsets GetInsetForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return sectionInset;
		}

		#endregion
	}
}
