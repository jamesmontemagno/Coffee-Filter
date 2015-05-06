using System;
using Foundation;

namespace CoffeeFilter.iOSWatchKitExtension
{
	public partial class PlaceRowController : NSObject
	{
		public string RowText {
			set {
				PlaceNameLabel.SetText (value);
			}
		}
	}
}

