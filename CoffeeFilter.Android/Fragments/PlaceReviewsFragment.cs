using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using CoffeeFilter.Shared.Models;
using Android.Runtime;
using CoffeeFilter.Shared.Helpers;
using System.Linq;
using System;


namespace CoffeeFilter.Fragments
{
	public class PlaceReviewsFragment : Fragment
	{
		public Place Place { get; set; }
		public static PlaceReviewsFragment NewInstance(Place place)
		{
			var f = new PlaceReviewsFragment () {
				Place = place
			};
			var b = new Bundle ();

			f.Arguments = b;
			return f;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var root = inflater.Inflate(Resource.Layout.fragment_reviews, container, false);
			var grid = root.FindViewById<GridView> (Resource.Id.grid);
			grid.Adapter = new ReviewsAdapter (Activity, Place.Reviews == null ? new Review[0] : Place.Reviews.OrderByDescending(r => r.Time).ToArray ());
			if (Place.Reviews == null || Place.Reviews.Count == 0) {
				root.FindViewById<TextView> (Resource.Id.none).Visibility = ViewStates.Visible;
			}
			return root;
		}
	}

	class ReviewsAdapter : BaseAdapter
	{

		Context context;
		readonly Review[] reviews;
		public ReviewsAdapter(Context context, Review[] reviews)
		{
			this.context = context;
			this.reviews = reviews;
		}


		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			ReviewAdapterViewHolder holder = null;

			if (view != null)
				holder = view.Tag as ReviewAdapterViewHolder;

			if (holder == null)
			{
				holder = new ReviewAdapterViewHolder();
				var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
				//replace with your item and your holder items
				//comment back in
				view = inflater.Inflate(Resource.Layout.item_review, parent, false);
				holder.Review = view.FindViewById<TextView>(Resource.Id.item_review);
				holder.Name = view.FindViewById<TextView>(Resource.Id.item_author_name);
				holder.Date = view.FindViewById<TextView>(Resource.Id.item_date);
				holder.Rating = view.FindViewById<RatingBar>(Resource.Id.item_rating);
				view.Tag = holder;
			}

			var review = reviews [position];
			holder.Review.Text = string.IsNullOrWhiteSpace(review.Text) ? context.Resources.GetString(Resource.String.rating_only) : review.Text;
			holder.Name.Text = review.AuthorName;
			holder.Date.Text = DateTimeUtils.UnixTimeToDateTime (review.Time).ToString ("D");
			if (review.Rating < 0)
				holder.Rating.Rating = 0.0F;
			else
				holder.Rating.Rating = (float)Math.Max(review.Rating, 5.0);
		return view;
		}

		//Fill in cound here, currently 0
		public override int Count
		{
			get
			{
				return reviews.Length;
			}
		}

	}

	class ReviewAdapterViewHolder : Java.Lang.Object
	{
		//Your adapter views to re-use
		public TextView Review { get; set; }
		public TextView Date { get; set; }
		public TextView Name { get; set; }
		public RatingBar Rating { get; set; }
	}
}

