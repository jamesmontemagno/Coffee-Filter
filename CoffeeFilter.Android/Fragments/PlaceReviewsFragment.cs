using System;
using System.Linq;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.Fragments
{
	public class PlaceReviewsFragment : Fragment
	{
		public Place Place { get; set; }

		public static PlaceReviewsFragment CreateNewInstance (Place place)
		{
			return new PlaceReviewsFragment {
				Place = place,
				Arguments = new Bundle ()
			};
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate (Resource.Layout.fragment_reviews, container, false);
			var grid = root.FindViewById<GridView> (Resource.Id.grid);
			grid.Adapter = new ReviewsAdapter (Activity, Place.Reviews == null ? new Review[0] : Place.Reviews.OrderByDescending (r => r.Time).ToArray ());
			if (Place.Reviews == null || Place.Reviews.Count == 0)
				root.FindViewById<TextView> (Resource.Id.none).Visibility = ViewStates.Visible;
			
			return root;
		}
	}

	class ReviewsAdapter : BaseAdapter
	{
		Context context;
		readonly Review[] reviews;

		public override int Count {
			get {
				return reviews.Length;
			}
		}

		public ReviewsAdapter (Context context, Review[] reviews)
		{
			this.context = context;
			this.reviews = reviews;
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return position;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			ReviewAdapterViewHolder holder = null;

			if (view != null)
				holder = view.Tag as ReviewAdapterViewHolder;

			if (holder == null) {
				var inflater = context.GetSystemService (Context.LayoutInflaterService).JavaCast<LayoutInflater> ();
				view = inflater.Inflate (Resource.Layout.item_review, parent, false);

				holder = new ReviewAdapterViewHolder {
					Review = view.FindViewById<TextView> (Resource.Id.item_review),
					Name = view.FindViewById<TextView> (Resource.Id.item_author_name),
					Date = view.FindViewById<TextView> (Resource.Id.item_date),
					Rating = view.FindViewById<RatingBar> (Resource.Id.item_rating)	
				};

				view.Tag = holder;
			}

			var review = reviews [position];
			holder.Review.Text = string.IsNullOrWhiteSpace (review.Text) ? context.Resources.GetString (Resource.String.rating_only) : review.Text;
			holder.Name.Text = review.AuthorName;
			holder.Date.Text = DateTimeUtils.ParseUnixTime (review.Time).ToString ("D");
			if (review.Rating < 0)
				holder.Rating.Rating = 0.0F;
			else
				holder.Rating.Rating = (float)Math.Max (review.Rating, 5.0);
			
			return view;
		}
	}

	class ReviewAdapterViewHolder : Java.Lang.Object
	{
		public TextView Review { get; set; }

		public TextView Date { get; set; }

		public TextView Name { get; set; }

		public RatingBar Rating { get; set; }
	}
}

