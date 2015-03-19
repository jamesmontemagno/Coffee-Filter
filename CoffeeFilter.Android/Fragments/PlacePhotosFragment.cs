using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using CoffeeFilter.Shared.Models;
using Android.Widget;
using Android.Content;
using Android.Runtime;
using System.Linq;


namespace CoffeeFilter.Fragments
{
	public class PlacePhotosFragment : Fragment
	{
		public Place Place { get; set; }
		public static PlacePhotosFragment NewInstance(Place place)
		{
			var f = new PlacePhotosFragment () {
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
			var root = inflater.Inflate(Resource.Layout.fragment_photos, container, false);
			var grid = root.FindViewById<GridView> (Resource.Id.grid);
			grid.Adapter = new PhotosAdapter (Activity, Place.Photos == null ? new string[0] : Place.Photos.Select (p => p.ImageUrl).ToArray ());

			if (Place.Photos == null || Place.Photos.Count == 0) {
				root.FindViewById<TextView> (Resource.Id.none).Visibility = ViewStates.Visible;
			}
			return root;
		}
	}

	class PhotosAdapter : BaseAdapter
	{

		Context context;
		string[] photos;
		public PhotosAdapter(Context context, string[] photos)
		{
			this.context = context;
			this.photos = photos;
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
			PhotosAdapterViewHolder holder = null;

			if (view != null)
				holder = view.Tag as PhotosAdapterViewHolder;

			if (holder == null)
			{
				holder = new PhotosAdapterViewHolder();
				var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
				//replace with your item and your holder items
				//comment back in
				view = inflater.Inflate(Resource.Layout.item_photo, parent, false);
				holder.Image = view.FindViewById<ImageView>(Resource.Id.item_image);
				view.Tag = holder;
			}

			var image = photos [position];
			Koush.UrlImageViewHelper.SetUrlDrawable (holder.Image, image);

			return view;
		}

		//Fill in cound here, currently 0
		public override int Count
		{
			get
			{
				return photos.Length;
			}
		}

	}

	class PhotosAdapterViewHolder : Java.Lang.Object
	{
		//Your adapter views to re-use
		public ImageView Image { get; set; }
	}
}

