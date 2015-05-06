using System;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using UIKit;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.iOS
{
	public partial class ReviewsViewController : UITableViewController
	{
		const string ReuseIdentifier = "ReviewCellIdentifier";

		List<Review> reviews;

		public ReviewsViewController (IntPtr handle) : base (handle)
		{
			TabBarItem.Title = "reviews".LocalizedString ("Name of the reviews tab");
			TabBarItem.SetFinishedImages (UIImage.FromBundle ("user_chat"), UIImage.FromBundle ("user_chat"));
			reviews = new List<Review> ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			TableView.ContentInset = new UIEdgeInsets (65.0f, 0.0f, 50.0f, 0.0f);
			TableView.SeparatorColor = UIColor.Clear;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			var viewModel = ServiceContainer.Resolve<DetailsViewModel> ();

			if (viewModel.Place.Reviews == null || viewModel.Place.Reviews.Count == 0) {
				var warnginView = WarningMessageView.GetView ("no_reviews".LocalizedString ("If place have no reviews"), this);
				View = warnginView;
				return;
			}

			reviews = viewModel.Place.Reviews;

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "reviews" },
				{ "name", viewModel.Place.Name },
			});
			#endif
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (ReviewCell)TableView.DequeueReusableCell (ReuseIdentifier, indexPath);
			cell.SetReviewToDisplay (reviews [indexPath.Section]);
			return cell;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return 1;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return reviews.Count;
		}

		public override nfloat GetHeightForFooter (UITableView tableView, nint section)
		{
			return 5.0f;
		}

		public override UIView GetViewForFooter (UITableView tableView, nint section)
		{
			return new UIView {
				Alpha = 0.0f
			};
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var height = base.GetHeightForRow (tableView, indexPath);
			var font = UIFont.SystemFontOfSize (15.0f);

			var textView = new UITextView (new CGRect (0, 0, tableView.Frame.Width, Int32.MaxValue)) {
				Text = reviews [indexPath.Section].Text,
				Font = font
			};

			textView.SizeToFit ();

			if (font.LineHeight != textView.Frame.Height)
				height += textView.Frame.Height;

			return height;
		}
	}
}
