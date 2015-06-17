using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using CoffeeFilter.Fragments;
using CoffeeFilter.Shared.Helpers;
using CoffeeFilter.Shared.ViewModels;
using com.refractored;
using Android.Gms.AppInvite;
using Android.Support.Design.Widget;
using Android.Gms.Common.Apis;
using Android.Gms.Common;

namespace CoffeeFilter
{
	[Activity (Label = "Details", ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetailsActivity : BaseActivity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener 
	{
		Android.Support.V7.Widget.ShareActionProvider actionProvider;
		DetailsAdapter adapter;
		ViewPager pager;
		PagerSlidingTabStrip tabs;
		DetailsViewModel viewModel;
		IGoogleApiClient client;
		Intent cachedInvitationIntent;
		const int REQUEST_INVITE = 0;
		bool needsRefresh = true;

		protected override int LayoutResource {
			get {
				return Resource.Layout.details;
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			try{
				viewModel = ServiceContainer.Resolve<DetailsViewModel> ();
			}
			catch{
				viewModel = new DetailsViewModel ();
				ServiceContainer.RegisterScoped (viewModel);
			}
			adapter = new DetailsAdapter (this, SupportFragmentManager, viewModel);
			pager = FindViewById<ViewPager> (Resource.Id.pager);
			tabs = FindViewById<PagerSlidingTabStrip> (Resource.Id.tabs);
			pager.Adapter = adapter;
			tabs.SetViewPager (pager);
			pager.OffscreenPageLimit = 3;

			var layoutParameters = new Android.Support.V7.Widget.Toolbar.LayoutParams (
				ViewGroup.LayoutParams.WrapContent,
				ViewGroup.LayoutParams.WrapContent,
				(int)(GravityFlags.Top | GravityFlags.Right));
			
			var progressBar = new ProgressBar (this, null, Android.Resource.Attribute.ProgressBarStyleSmallTitle) {
				Indeterminate = true,
				LayoutParameters = layoutParameters
			};

			SupportActionBar.SetDisplayShowCustomEnabled (true);

			SupportActionBar.CustomView = progressBar;
			SupportActionBar.Title = viewModel.Place == null ? string.Empty : viewModel.Place.Name;

			#if !DEBUG
			Xamarin.Insights.Track ("AppNav", new Dictionary<string,string> {
				{ "page", "details" },
				{ "name", SupportActionBar.Title }
			});
			#endif

			client = new GoogleApiClientBuilder (this)
				.AddConnectionCallbacks (this)
				.EnableAutoManage (this, 0, this)
				.AddApi (AppInviteClass.Api)
				.Build ();
		}

		private void RefreshData()
		{
			viewModel.RefreshPlace ().ContinueWith ((result) => {
				RunOnUiThread (() => {
					SupportActionBar.CustomView.Visibility = ViewStates.Gone;
					if (!result.Result) {
						Toast.MakeText (this, Resource.String.unable_to_get_details, ToastLength.Long).Show ();
						return;
					}
					SupportActionBar.Title = viewModel.Place == null ? string.Empty : viewModel.Place.Name;
					adapter.NotifyDataSetChanged ();
					SupportInvalidateOptionsMenu ();
				});
			});
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			ServiceContainer.RemoveScope ();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Android.Resource.Id.Home)
				Finish ();
			else if (item.ItemId == Resource.Id.action_invite)
				InviteClicked ();

			return base.OnOptionsItemSelected (item);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			//change menu_share1 to your name
			MenuInflater.Inflate (Resource.Menu.menu_details, menu);
			var shareItem = menu.FindItem (Resource.Id.action_share);
			var provider = MenuItemCompat.GetActionProvider (shareItem);
			actionProvider = provider.JavaCast<Android.Support.V7.Widget.ShareActionProvider> ();
			var intent = new Intent (Intent.ActionSend);
			intent.SetType ("text/plain");
			var shareText = string.Format (Resources.GetString (Resource.String.share_text), viewModel.Place.Name);
			if (!string.IsNullOrWhiteSpace (viewModel.Place.Website))
				shareText += " " + viewModel.Place.Website;
			
			intent.PutExtra (Intent.ExtraText, shareText);
			actionProvider.SetShareIntent (intent);
			return base.OnCreateOptionsMenu (menu);
		}

		void InviteClicked() {
			var intent = new AppInviteInvitation.IntentBuilder("Invite Friends to Coffee")
				.SetMessage("Join me at: " + viewModel.Place.Name + " with Coffee Filter")
				.SetDeepLink(Android.Net.Uri.Parse("http://motzcod.es/coffee/" + viewModel.Place.PlaceId + "," + viewModel.Position.Latitude + "," + viewModel.Position.Longitude))
				.Build();
			StartActivityForResult(intent, REQUEST_INVITE);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode == REQUEST_INVITE) {
				if (resultCode == Result.Ok) {
					// Check how many invitations were sent and show message to the user
					// The ids array contains the unique invitation ids for each invitation sent
					// (one for each contact select by the user). You can use these for analytics
					// as the ID will be consistent on the sending and receiving devices.
					//var ids = AppInviteInvitation.GetInvitationIds((int)resultCode, data);

				} else {
					// Sending failed or it was canceled, show failure message to the user
					ShowMessage("No coffee invites were sent.");
				}
			}
		}

		private void ShowMessage(String msg) {
			var container = FindViewById<ViewGroup>(Resource.Id.snackbar_layout);
			Snackbar.Make(container, msg, Snackbar.LengthShort).Show();
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			ProcessReferralIntent (Intent);
			needsRefresh = false;
		}

		void ProcessReferralIntent(Intent intent)
		{

			if (!AppInviteReferral.HasReferral (intent)) {
				RefreshData ();
				return;
			}

			var invitationId = AppInviteReferral.GetInvitationId (intent);
			var deepLink = AppInviteReferral.GetDeepLink (intent);

			Console.WriteLine ("Referral found: invitationId: " + invitationId + " deepLink: " + deepLink);

			var info = deepLink.Replace ("http://motzcod.es/coffee/", string.Empty);
			var infoArray = info.Split (new []{","}, StringSplitOptions.RemoveEmptyEntries);
			if (infoArray.Length == 3) {
				viewModel.Place.PlaceId = infoArray [0];
				double lat = 0;
				double lng = 0;
				double.TryParse (infoArray [1], out lat);
				double.TryParse (infoArray [2], out lng);
				viewModel.Position.Latitude = lat;
				viewModel.Position.Longitude = lng;
				RefreshData ();
			}

			if (client.IsConnected) {
				UpdateInvitationStatus (intent);
			}
			else {
				Console.WriteLine ("Warning: GoogleAPIClient not connect, can't update invitation just yet.");
				cachedInvitationIntent = intent;
			}
		}

		/// <summary>
		/// Update the install and conversion status of an invite intent
		/// </summary>
		/// <param name="intent">Intent.</param>
		private void UpdateInvitationStatus(Intent intent)
		{
			var invitationId = AppInviteReferral.GetInvitationId (intent);

			// Note: these  calls return PendingResult(s), so one could also wait to see
			// if this succeeds instead of using fire-and-forget, as is shown here
			if (AppInviteReferral.IsOpenedFromPlayStore (intent)) {
				AppInviteClass.AppInviteApi.UpdateInvitationOnInstall (client, invitationId);
			}

			// If your invitation contains deep link information such as a coupon code, you may
			// want to wait to call `convertInvitation` until the time when the user actually
			// uses the deep link data, rather than immediately upon receipt
			AppInviteClass.AppInviteApi.ConvertInvitation (client, invitationId);

		}

		public void OnConnected (Bundle connectionHint)
		{
			if (cachedInvitationIntent == null)
				return;

			UpdateInvitationStatus (cachedInvitationIntent);
			cachedInvitationIntent = null;
		}

		public void OnConnectionSuspended (int cause)
		{
		}

		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{
			Console.WriteLine ("GoogleApiClient - OnConnectionFailed: " + result.ErrorCode);

			if (result.ErrorCode == ConnectionResult.ApiUnavailable)
				Console.WriteLine ("OnConnectionFailed because an API was unavailable");
		}
	}

	public class DetailsAdapter : FragmentStatePagerAdapter
	{
		string[] titles;
		DetailsViewModel viewModel;

		public override int Count {
			get {
				return titles.Length;
			}
		}

		public DetailsAdapter (Context context, Android.Support.V4.App.FragmentManager fm, DetailsViewModel viewModel) : base (fm)
		{
			titles = context.Resources.GetTextArray (Resource.Array.sections);
			this.viewModel = viewModel;
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			return new Java.Lang.String (titles [position]);
		}

		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			switch (position) {
			case 0:
				return PlaceDetailsFragment.NewInstance (viewModel.Place, viewModel.Position);
			case 1:
				return PlaceReviewsFragment.CreateNewInstance (viewModel.Place);
			case 2:
				return PlacePhotosFragment.CreateNewInstance (viewModel.Place);
			}
			return null;
		}

		public override int GetItemPosition (Java.Lang.Object frag)
		{
			return PositionNone;
		}
	}
}

