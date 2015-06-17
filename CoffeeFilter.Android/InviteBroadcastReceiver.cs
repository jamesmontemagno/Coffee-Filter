using System;
using Android.Content;
using Android.Gms.AppInvite;

namespace CoffeeFilter
{
	class InviteBroadcastReceiver : BroadcastReceiver
	{
		readonly MainActivity activity;
		public InviteBroadcastReceiver(MainActivity activity)
		{
			this.activity = activity;
		}

		public override void OnReceive (Context context, Intent intent)
		{
			if (!AppInviteReferral.HasReferral (intent))
				return;

			activity.LaunchDeepLinkActivity (intent);
		}
	}
}

