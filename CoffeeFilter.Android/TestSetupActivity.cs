
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using CoffeeFilter.UITests.Shared;
using System.Linq;

namespace CoffeeFilter
{
	#if UITest
	[Activity (Label = "TestSetupActivity", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = true, NoHistory = true)]			
	public class TestSetupActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.test_setup);


			var list = FindViewById<ListView> (Resource.Id.listView);
			list.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1,
				Android.Resource.Id.Text1,
				Enum.GetValues(typeof(UITestsHelpers.TestType))
				.Cast<UITestsHelpers.TestType>().Select(en => en.ToFriendlyString()).ToArray());

			list.ItemClick += (sender, e) => {
				UITestsHelpers.SelectedTest = (UITestsHelpers.TestType)e.Position;
				StartActivity(typeof(MainActivity));
				Finish();
			};
		}
	}
	#endif
}

