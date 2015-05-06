using System.Linq;
using Android.Content;

using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.Fragments
{
	public class PlacePhotosFragment : Fragment
	{
		public Place Place { get; set; }

		public static PlacePhotosFragment CreateNewInstance (Place place)
		{
			return new PlacePhotosFragment {
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
			var root = inflater.Inflate (Resource.Layout.fragment_photos, container, false);
			var grid = root.FindViewById<GridView> (Resource.Id.grid);
			grid.Adapter = new PhotosAdapter (Activity, Place.Photos == null ? new string[0] : Place.Photos.Select (p => p.ImageUrl).ToArray ());

			if (Place.Photos == null || Place.Photos.Count == 0)
				root.FindViewById<TextView> (Resource.Id.none).Visibility = ViewStates.Visible;
			
			return root;
		}
	}

	class PhotosAdapter : BaseAdapter
	{
		string[] photos;
		Context context;

		public override int Count {
			get {
				return photos.Length;
			}
		}

		public PhotosAdapter (Context context, string[] photos)
		{
			this.context = context;
			this.photos = photos;
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
			PhotosAdapterViewHolder holder = null;

			if (view != null)
				holder = view.Tag as PhotosAdapterViewHolder;

			if (holder == null) {
				holder = new PhotosAdapterViewHolder ();
				var inflater = context.GetSystemService (Context.LayoutInflaterService).JavaCast<LayoutInflater> ();
				view = inflater.Inflate (Resource.Layout.item_photo, parent, false);
				holder.Image = view.FindViewById<ImageView> (Resource.Id.item_image);
				view.Tag = holder;
			}

			var image = photos [position];
			Koush.UrlImageViewHelper.SetUrlDrawable (holder.Image, image);

			return view;
		}
	}

	class PhotosAdapterViewHolder : Java.Lang.Object
	{
		public ImageView Image { get; set; }
	}
}

