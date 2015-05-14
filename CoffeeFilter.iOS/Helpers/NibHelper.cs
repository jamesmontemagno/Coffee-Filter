using System;
using Foundation;
using UIKit;

namespace CoffeeFilter.iOS
{
	public static class NibHelper
	{
		public static T LoadNib<T> (this NSBundle bundle, NSObject owner) where T : UIView
		{
			return bundle.LoadNib(typeof(T).Name, owner, null).GetItem<T>(0);
		}

		public static T Instantiate<T> (this UIStoryboard storyboard) where T : UIViewController
		{
			return storyboard.InstantiateViewController(typeof(T).Name) as T;
		}

		public static T Instantiate<T> (this UIStoryboard storyboard, string name) where T :  UIViewController
		{
			return storyboard.InstantiateViewController(name) as T;
		}

		public static UIViewController Instantiate (this UIStoryboard storyboard, string name)
		{
			return storyboard.InstantiateViewController(name) as UIViewController;
		}
	}
}