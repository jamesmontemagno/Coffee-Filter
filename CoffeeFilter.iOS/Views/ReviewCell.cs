using System;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;
using UIKit;

namespace CoffeeFilter.iOS
{
	public partial class ReviewCell : UITableViewCell
	{
		public ReviewCell (IntPtr handle) : base(handle)
		{
		}

		public void SetReviewToDisplay (Review review)
		{
			if (review == null) throw new ArgumentException ("review");
			
			AuthorNameLabel.Text = review.AuthorName;
			ReviewDateLabel.Text = DateTimeUtils.ParseUnixTime(review.Time).ToString("D");
			ReviewLabel.Text = string.IsNullOrEmpty(review.Text) ? "Rating only" : review.Text;
			RatingControl.Rating = review.Rating;
			SelectionStyle = UITableViewCellSelectionStyle.None;
		}
	}
}