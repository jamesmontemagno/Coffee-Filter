using System;
using System.Threading.Tasks;
using CoffeeFilter.Shared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using ServiceStack.Text;
using Geolocator.Plugin.Abstractions;
using Geolocator.Plugin;
using System.Linq;

namespace CoffeeFilter.Shared
{
	public class CoffeeFilterViewModel
	{
		private DateTime lastTry;
		public CoffeeFilterViewModel()
		{
			Places = new List<Place> ();
			lastTry = DateTime.UtcNow.AddMinutes(-60);
		}
		const string APIKey = "YourAPIKey";
		const string Query = "https://maps.googleapis.com/maps/api/place/textsearch/json?query=coffee&&location={0},{1}&radius=800&types=cafe&key={2}";

		public Position Position { get; set; }

		public bool IsBusy {get;set;}

		public List<Place> Places{ get; set; }

		public async Task<bool> GetLocation(bool forceRefresh = false)
		{
			if (Position == null || forceRefresh) {

				try
				{
					Position = await CrossGeolocator.Current.GetPositionAsync(10000);
				}
				catch(Exception ex) {
					Console.WriteLine ("Unable to query location: " + ex);
				}
			}

			return Position != null;
		}

		public async Task<List<Place>> GetPlaces()
		{
			if (Position == null || IsBusy)
				return null;

			//got results within 5 minutes
			if (lastTry.AddMinutes (5) > DateTime.UtcNow)
				return Places;

			IsBusy = true;
			Places = new List<Place> ();
			var query = string.Format (Query,
				Position.Latitude.ToString(CultureInfo.InvariantCulture),
				Position.Longitude.ToString(CultureInfo.InvariantCulture),
				APIKey);

			try
			{
				using (var client = new HttpClient())
				{
					client.Timeout = TimeSpan.FromSeconds(10);
					var result = await client.GetStringAsync(query);
					try
					{
						var queryObject = result.FromJson<QueryObject> ();
						if(queryObject != null && queryObject.Places != null)
						{
							Places = queryObject.Places.Where(p=> p.OpeningHours != null && p.OpeningHours.IsOpen).OrderBy(p =>
								p.GetDistance(Position.Latitude,
									Position.Longitude)).ToList();

							lastTry = DateTime.UtcNow;
							return Places;
						}
					}
					catch(Exception ex) {
						Console.WriteLine ("Unable to query" + ex);
						#if !DEBUG
						Xamarin.Insights.Report(ex, "CoffeeViewModel", "GetPlaces");
						#endif
					}
				}
			}
			catch(Exception ex) {
				Console.WriteLine ("Unable to query" + ex);
				#if !DEBUG
				Xamarin.Insights.Report(ex, "CoffeeViewModel", "GetPlaces");
				#endif
			}
			finally {
				IsBusy = false;
			}

			return null;
		}
	}
}

