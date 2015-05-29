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

					#if UITest
					var result = GetPlaceDetailsForTests();
					if(string.IsNullOrWhiteSpace(result)) {
						client.Timeout = TimeSpan.FromSeconds(10);
						result = await client.GetStringAsync(requestUri);
					}
					#else
					client.Timeout = TimeSpan.FromSeconds (10);
					var result = await client.GetStringAsync (requestUri);
					#endif
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

		string GetPlaceDetailsForTests ()
		{
			return "{\"html_attributions\":[],\"result\":{\"address_components\":[{\"long_name\":\"332\",\"short_name\":\"332\",\"types\":[\"street_number\"]},{\"long_name\":\"West El Camino Real\",\"short_name\":\"W El Camino Real\",\"types\":[\"route\"]},{\"long_name\":\"Sunnyvale\",\"short_name\":\"Sunnyvale\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"California\",\"short_name\":\"CA\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"United States\",\"short_name\":\"US\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"94087\",\"short_name\":\"94087\",\"types\":[\"postal_code\"]}],\"adr_address\":\"\\u003cspan class=\\\"street-address\\\"\\u003e332 West El Camino Real\\u003c/span\\u003e, \\u003cspan class=\\\"locality\\\"\\u003eSunnyvale\\u003c/span\\u003e, \\u003cspan class=\\\"region\\\"\\u003eCA\\u003c/span\\u003e \\u003cspan class=\\\"postal-code\\\"\\u003e94087\\u003c/span\\u003e, \\u003cspan class=\\\"country-name\\\"\\u003eUnited States\\u003c/span\\u003e\",\"formatted_address\":\"332 West El Camino Real, Sunnyvale, CA 94087, United States\",\"formatted_phone_number\":\"(408) 736-6859\",\"geometry\":{\"location\":{\"lat\":37.367716,\"lng\":-122.036448}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"94aea20b97c3cd0cca8ae8e80b180020bd4c39b3\",\"international_phone_number\":\"+1 408-736-6859\",\"name\":\"Espresso Vivace\",\"opening_hours\":{\"open_now\":true,\"periods\":[{\"close\":{\"day\":0,\"time\":\"2200\"},\"open\":{\"day\":0,\"time\":\"0500\"}},{\"close\":{\"day\":1,\"time\":\"2200\"},\"open\":{\"day\":1,\"time\":\"0500\"}},{\"close\":{\"day\":2,\"time\":\"2200\"},\"open\":{\"day\":2,\"time\":\"0500\"}},{\"close\":{\"day\":3,\"time\":\"2200\"},\"open\":{\"day\":3,\"time\":\"0500\"}},{\"close\":{\"day\":4,\"time\":\"2200\"},\"open\":{\"day\":4,\"time\":\"0500\"}},{\"close\":{\"day\":5,\"time\":\"2230\"},\"open\":{\"day\":5,\"time\":\"0500\"}},{\"close\":{\"day\":6,\"time\":\"2200\"},\"open\":{\"day\":6,\"time\":\"0500\"}}],\"weekday_text\":[\"Monday: 5:00 am – 10:00 pm\",\"Tuesday: 5:00 am – 10:00 pm\",\"Wednesday: 5:00 am – 10:00 pm\",\"Thursday: 5:00 am – 10:00 pm\",\"Friday: 5:00 am – 10:30 pm\",\"Saturday: 5:00 am – 10:00 pm\",\"Sunday: 5:00 am – 10:00 pm\"]},\"photos\":[{\"height\":1265,\"html_attributions\":[\"\\u003ca href=\\\"https://plus.google.com/109863918985393414306\\\"\\u003eArya Vishin\\u003c/a\\u003e\"],\"photo_reference\":\"CnRnAAAAulf6AN25GyNC6-ivVWJycZMKwTuZPL0gXYIMnnuttXqogkwANczxVobLzEF1db4_BuwL-oRICjd-9-4N2ccuYDHYbEjgaqC_KJhKzteEp8rm3O2d3lmaG-vtdyL5w0xCDwQoTxYegW1i_43BjIgrERIQ7QLVnA-RR2qrA4ZKpr14bRoUfCisCjLPBshgz1eLftGodABE3Ww\",\"width\":949}],\"place_id\":\"ChIJb0b7nWG2j4ARkDpIFK-5Nw8\",\"price_level\":2,\"rating\":3.5,\"reference\":\"CmRcAAAAuz9921mbFbncozmsZKxHd6v-5qRn4WvEnvyDmMkdDizmxdjHmiaifnQArqgiHkmL7-jISTijRDOu9tKOSO68NSa63Eug8fXNHVmDz3OJF_vCai8gQhcEPyM5mGERp66VEhBF4C0w4Q0Grd0gg6EdyKVgGhSjV0vnxvDVRNKItOQb2QcZsmOfrQ\",\"reviews\":[{\"aspects\":[{\"rating\":3,\"type\":\"overall\"}],\"author_name\":\"Andres Villa\",\"author_url\":\"https://plus.google.com/109292601574998586183\",\"language\":\"en\",\"rating\":5,\"text\":\"great staff they remembered my name and drink right away (2nd visit),,, i usually go at 1130 or 12, no wait time!!\",\"time\":1432539513},{\"aspects\":[{\"rating\":2,\"type\":\"overall\"}],\"author_name\":\"Aniket Jain\",\"author_url\":\"https://plus.google.com/107847850261794100195\",\"language\":\"en\",\"rating\":4,\"text\":\"Pretty large location but limited staff. Always a delay in getting my order. \",\"time\":1421988866},{\"aspects\":[{\"rating\":1,\"type\":\"overall\"}],\"author_name\":\"Mariam Rajabi\",\"author_url\":\"https://plus.google.com/107061692823230488876\",\"language\":\"en\",\"rating\":3,\"text\":\"Very spacious, you can always find a seat. However, not a good place to study, it's noisy and I always end up smelling like smoke when I stay there for a couple of hours.\",\"time\":1397423742},{\"aspects\":[{\"rating\":0,\"type\":\"overall\"}],\"author_name\":\"Aimee Lopez\",\"author_url\":\"https://plus.google.com/100712465750567278144\",\"language\":\"en\",\"rating\":1,\"text\":\"Great location but staff is pretty rude \",\"time\":1421111295},{\"aspects\":[{\"rating\":1,\"type\":\"food\"},{\"rating\":1,\"type\":\"decor\"},{\"rating\":0,\"type\":\"service\"}],\"author_name\":\"Shawn Besabella\",\"author_url\":\"https://plus.google.com/101232748487985782850\",\"language\":\"en\",\"rating\":3,\"text\":\"Normally, I would just write my standard Starbucks review, however this place on my first few visits has fallen below my expectations of Starbucks stores.\\n\\nThis is a pretty large location, however every time I've gone, usually during a rush hour, they insist on having only 1 person at the register. Mind you, this is when I've seen 5 people working at once.\\n\\nAlso, each time I've waiting on a drink from a barista, the barista took way longer than it should take to complete an order. I have pretty good patience, but I can recognize when a barista is not working at the optimal Starbucks speed.\\n\\nLastly, good luck parking in this lot. It is hell.\",\"time\":1360993703}],\"scope\":\"GOOGLE\",\"types\":[\"cafe\",\"food\",\"establishment\"],\"url\":\"https://plus.google.com/105544054342943026820/about?hl=en-US\",\"user_ratings_total\":31,\"utc_offset\":-420,\"vicinity\":\"332 West El Camino Real, Sunnyvale\",\"website\":\"http://starbucks.com/\"},\"status\":\"OK\"}";
		}
	}
}

