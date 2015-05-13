using System;
using Xamarin.UITest;
using System.Linq;

namespace CoffeeFilter.UITests
{
	public static class PredefinedSteps
	{
		public static void SwipeLeft (this IApp app, float strength = 0.8f)
		{
			var screen = app.Query (c => c.Index (0)).FirstOrDefault ();
			var y = screen.Rect.CenterY;
			var halfStrength = strength * 0.5;
			var fromX = (float)(screen.Rect.Width * (0.5 + halfStrength));
			var toX = (float)(screen.Rect.Width * (0.5 - halfStrength));

			app.DragCoordinates (fromX, y, toX, y);
		}
	}
}
