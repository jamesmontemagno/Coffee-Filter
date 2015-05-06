using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ExternalMaps.Plugin;
using Geolocator.Plugin.Abstractions;
using Newtonsoft.Json;

using CoffeeFilter.Shared.Models;

namespace CoffeeFilter.Shared.ViewModels
{
	public class DetailsViewModel : BaseViewModel
	{
		const string DetailsQueryUrl = "https://maps.googleapis.com/maps/api/place/details/json?placeid={0}&key=" + CoffeeFilterViewModel.APIKey;

		public Place Place { get; set; }

		public Position Position { get; set; }

		public string Distance {
			get {
				string distance = Place.GetDistance (Position.Latitude, Position.Longitude,
					CultureInfo.CurrentCulture.Name != "en-US" ?
					CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Kilometers :
					CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles).ToString ("##.###", CultureInfo.CurrentUICulture);

				return string.Format ("{0} {1} away", distance, CultureInfo.CurrentCulture.Name != "en-US" ? "km" : "mi.");
			}
		}

		public string ShortAddress {
			get {
				var addressShort = string.Empty;
				if (Place.AddressComponents != null) {
					var comp = Place.AddressComponents.FirstOrDefault (a => a.Types != null && a.Types.Contains ("street_number"));
					var comp2 = Place.AddressComponents.FirstOrDefault (a => a.Types != null && a.Types.Contains ("route"));
					if (comp != null && comp2 != null)
						addressShort = string.Format ("{0} {1}", comp.ShortName, comp2.ShortName);
				}

				return addressShort;
			}
		}

		public string OpenHours {
			get {
				var priceOpenDisplay = string.Empty;

				if (Place.PriceLevel.HasValue)
					for (int i = 0; i < Place.PriceLevel.Value; i++)
						priceOpenDisplay += "$";

				if (!string.IsNullOrWhiteSpace (priceOpenDisplay))
					priceOpenDisplay += " - ";

				if (Place.OpeningHours == null || Place.OpeningHours.Periods == null || Place.OpeningHours.WeekdayText.Count != 7) {
					priceOpenDisplay += "Open Now"; // TODO move to localized strings
				} else {

					var dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;

					if (dayOfWeek < 0)
						dayOfWeek = 6;

					var closeTime = GetTime (Place.OpeningHours.WeekdayText [dayOfWeek]);
					var closeIndex = closeTime.LastIndexOf ("–", StringComparison.InvariantCultureIgnoreCase);

					if (closeIndex != -1)
						closeTime = closeTime.Remove (0, closeIndex + 1).Trim ();

					priceOpenDisplay += string.Format ("Open until {0}", closeTime); // TODO move to localized strings
				}

				return priceOpenDisplay;
			}
		}

		public void NavigateToShop ()
		{
			#if !DEBUG
			Xamarin.Insights.Track ("Navigation", new Dictionary<string,string> {
				{"name", Place.Name},
				{"rating", Place.Rating.ToString()}
			});
			#endif

			CrossExternalMaps.Current.NavigateTo (Place.Name, Place.Geometry.Location.Latitude, Place.Geometry.Location.Longitude);
		}

		public async Task<bool> RefreshPlace ()
		{
			if (Place == null || IsBusy)
				return false;

			IsBusy = true;
			#if !DEBUG
			Xamarin.Insights.Track ("Details", "CoffeeShop", Place.Name);
			#endif
			
			var requestUri = string.Format (DetailsQueryUrl, Place.PlaceId);
			try {
				using (var client = new HttpClient (new ModernHttpClient.NativeMessageHandler())) {

					client.Timeout = TimeSpan.FromSeconds (10);
					var result = await client.GetStringAsync (requestUri);

					try {
						Place = JsonConvert.DeserializeObject<DetailQueryResult> (result).Place;
					} catch (Exception ex) {
						Console.WriteLine ("Unable to query" + ex);
						#if !DEBUG
						Xamarin.Insights.Report(ex, "DetailsViewModel", "GetDetails");
						#endif
						return false;
					} finally {
						client.Dispose();
					}
				}
			} catch (Exception ex) {
				Console.WriteLine ("Unable to query" + ex);
				#if !DEBUG
				Xamarin.Insights.Report(ex, "DetailsViewModel", "GetDetails");
				#endif
				return false;
			} finally {
				IsBusy = false;
			}

			return true;
		}

		public string GetTime (string toParse)
		{
			var index = toParse.IndexOf (":", System.StringComparison.InvariantCultureIgnoreCase);
			return index <= 0 ? toParse : toParse.Remove (0, index + 1).Trim ();
		}
	}
}

