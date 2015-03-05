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
using Connectivity.Plugin;
using CoffeeFilter.UITests.Shared;

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

		/// <summary>
		/// Gets a value indicating whether there is connectivity.
		/// </summary>
		/// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
		public bool IsConnected
		{
			get {
				#if UITest
				return UITestsHelpers.SelectedTest != UITestsHelpers.TestType.NoConnection;
				#endif
				return CrossConnectivity.Current.IsConnected;
			}
		}

		public async Task<bool> GetLocation(bool forceRefresh = false)
		{
			if (Position == null || forceRefresh) {

				#if UITest
				Position = new Position{
					Latitude = 47.620061,
					Longitude = -122.330373
				};
				return true;
				#endif
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

			#if !UITest
			//got results within 5 minutes
			if (lastTry.AddMinutes (5) > DateTime.UtcNow)
				return Places;
			#endif

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
					#if UITest
					var result = GetUITestResults();
					if(string.IsNullOrWhiteSpace(result)) {
						client.Timeout = TimeSpan.FromSeconds(10);
						result = await client.GetStringAsync(query);
					}
					#else
						client.Timeout = TimeSpan.FromSeconds(10);
						var result = await client.GetStringAsync(query);
					#endif
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

		bool first = true;
		private string GetUITestResults()
		{
			switch (UITestsHelpers.SelectedTest) {
			case UITestsHelpers.TestType.ClosedCoffee:
				return "{\"html_attributions\":[],\"next_page_token\":\"NAXupVzLLc580jce5n1__-XTG2kpHbXPBcvk0Qr4ReJDOAvf_f_uXM87CzeOZgVOMPgGv2N5EuKzOcE_E13uekuMxq4bSKB_8emEfui6Pl_q8JgMh5KFjxvWXSdwaQVEytxFL5RzH03aE6aiz-itRRQaQ0TfDsyib1hm7CUeg9HApkft4lUMN3v2UDY8RyP22apCojOlBzObf2tZn6f_yFKOmIf-iOHfyFFpVIPzcIqx6_nKMucmQoVNmEqYUTuZ-WRAg_DAyyBpwZX5swBf3fjFJo-L3gg5nbcPhGnh7NTHdv1WXmb7qW3UarUnOH8DZjw2MqgY4SEFIn3pyXSc7JE17QbB9CWGIaFOnnMdcfXCtvlg4Y--j5oJvg6nTd\",\"results\":[{\"formatted_address\":\"227 Yale Avenue North, Seattle, WA 98109, United States\",\"geometry\":{\"location\":{\"lat\":47.620179,\"lng\":-122.330584}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"9050da096f1a773f2a8aca68cb91291523963cd9\",\"name\":\"Espresso Vivace\",\"opening_hours\":{\"open_now\":false,\"weekday_text\":[]},\"photos\":[{\"height\":853,\"html_attributions\":[],\"photo_reference\":\"CnRnAAAA9mRKKCaaT393yNJ0yHw7YrQkK3JbRmf5CBaD0SQrtNTLSzr4xqMCVGunk22o1QBM7hy_7KfiFex4bcFMgkVuJ1wkM4fG4qFPLbPRAJ_mjHHizeYSYEa7Qh9UQjJgyDvXTEG4M0WxOLm4r1txCJdo0RIQZTAdx9zx3_7X09BTy-a9xRoUBPd4rI2VlyDQpzjwr1jQAEGDPsI\",\"width\":1280}],\"place_id\":\"ChIJ1-NiqTYVkFQRO2XahxpM_-g\",\"price_level\":1,\"rating\":4.5,\"reference\":\"CnRjAAAAUejPAhORDn8-mm-OPCap6-GBqbu0rg6Ufx5SpWYZViplVwttA4eh_IHzXCwFHiRFO_kbu6MZG1iiQhuhcv1Vb-fo1-rX86UQ9LjSk_1gSwxX3ViG8x0w8pETJA0keu7xHmKGSz-VrDZaMTtVKR2hPRIQxKDmK2GEXqzejK6KceLyaxoUPO-8DrikzRY65mfvjwchFiYzJKE\",\"types\":[\"cafe\",\"food\",\"establishment\"]}],\"status\":\"OK\"}";
			case UITestsHelpers.TestType.OpenCoffee:
				return "{\"html_attributions\":[],\"next_page_token\":\"Fnu2gNAXupVzLLc580jce5n1__-XTG2kpHbXPBcvk0Qr4ReJDOAvf_f_uXM87CzeOZgVOMPgGv2N5EuKzOcE_E13uekuMxq4bSKB_8emEfui6Pl_q8JgMh5KFjxvWXSdwaQVEytxFL5RzH03aE6aiz-itRRQaQ0TfDsyib1hm7CUeg9HApkft4lUMN3v2UDY8RyP22apCojOlBzObf2tZn6f_yFKOmIf-iOHfyFFpVIPzcIqx6_nKMucmQoVNmEqYUTuZ-WRAg_DAyyBpwZX5swBf3fjFJo-L3gg5nbcPhGnh7NTHdv1WXmb7qW3UarUnOH8DZjw2MqgY4SEFIn3pyXSc7JE17QbB9CWGIaFOnnMdcfXCtvlg4Y--j5oJvg6nTd\",\"results\":[{\"formatted_address\":\"227 Yale Avenue North, Seattle, WA 98109, United States\",\"geometry\":{\"location\":{\"lat\":47.620179,\"lng\":-122.330584}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"9050da096f1a773f2a8aca68cb91291523963cd9\",\"name\":\"Espresso Vivace\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":853,\"html_attributions\":[],\"photo_reference\":\"CnRnAAAA9mRKKCaaT393yNJ0yHw7YrQkK3JbRmf5CBaD0SQrtNTLSzr4xqMCVGunk22o1QBM7hy_7KfiFex4bcFMgkVuJ1wkM4fG4qFPLbPRAJ_mjHHizeYSYEa7Qh9UQjJgyDvXTEG4M0WxOLm4r1txCJdo0RIQZTAdx9zx3_7X09BTy-a9xRoUBPd4rI2VlyDQpzjwr1jQAEGDPsI\",\"width\":1280}],\"place_id\":\"ChIJ1-NiqTYVkFQRO2XahxpM_-g\",\"price_level\":1,\"rating\":4.5,\"reference\":\"CnRjAAAAUejPAhORDn8-mm-OPCap6-GBqbu0rg6Ufx5SpWYZViplVwttA4eh_IHzXCwFHiRFO_kbu6MZG1iiQhuhcv1Vb-fo1-rX86UQ9LjSk_1gSwxX3ViG8x0w8pETJA0keu7xHmKGSz-VrDZaMTtVKR2hPRIQxKDmK2GEXqzejK6KceLyaxoUPO-8DrikzRY65mfvjwchFiYzJKE\",\"types\":[\"cafe\",\"food\",\"establishment\"]},{\"formatted_address\":\"1600 East Olive Way, Seattle, WA 98102, United States\",\"geometry\":{\"location\":{\"lat\":47.619401,\"lng\":-122.325054}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"65f59975d6fa765f66cba7818baea6551044a422\",\"name\":\"Starbucks\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":949,\"html_attributions\":[\"<a href=\"https://plus.google.com/105254155462724110663\">Jonah Kirangi</a>\"],\"photo_reference\":\"CnRoAAAAEm9EGf6pRTUiibJ29PPKfLP13GGjL19v-eD5BT0MFSbhD6s2XIaVHBxzjqr4xhLmkmz09qrTqU-hWl8Y37q_dbV40_YdQJGA7gJGUq0valLtgIEpZQBmoRUWOBxdg3rTckku9rS8QPzYBwgAYcMRmBIQamcuxCtURzXlP-lhHviAoBoUo166OKYVq76YzlOa03-1T0dxZjc\",\"width\":1265}],\"place_id\":\"ChIJjfnnrjMVkFQRSuZC1_ODJvk\",\"price_level\":2,\"rating\":,\"reference\":\"CmRdAAAAGA92tmNe1Kw7-z9IPZUWTXBF17wPVMdz-CGPBP2YUT67rLpQKoE8NrNDOA9lnJKZGUgPl0KE_NTaqw7ZRjwjNflci1yzdCqozXdgax-F1tcXQzfRP4jWlsx7izs66zRGEhD3PQ1m06poIOAMaxUvgtnOGhQcX5rMI--lwtoUxwO1pEDIdWBquA\",\"types\":[\"cafe\",\"food\",\"establishment\"]},{\"formatted_address\":\"422 Yale Avenue North, suite B, Seattle, WA 98109, United States\",\"geometry\":{\"location\":{\"lat\":47.622335,\"lng\":-122.330146}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/restaurant-71.png\",\"id\":\"03b8989ea39a823d2b50642128fae0c48bd4503b\",\"name\":\"Caffè Torino\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":2048,\"html_attributions\":[],\"photo_reference\":\"CnRnAAAA2TCUiCh-77BNyzc8VwijUKbhPz4Cv90WyURE1Qp0fA4p9xgFStBxYRv9frddEud2s8I6WiX3_XIETE5yIOoRJK5IA78v8YcM6ugaEN0_YVGEuk7pKcjeYc3ek82mJG_R0EEzDQw3TStzljUdVIXUXBIQ2LsOs4gU1ZzaVkU8N1-enBoUVengj32iXE3iVM7zeq0kWl4lssU\",\"width\":1365}],\"place_id\":\"ChIJl8KRGDEVkFQRPSY4FT8fZsc\",\"rating\":4,\"reference\":\"CnRhAAAAOCUvIrxI229vJsLZpi1BJlr9a-YODXCmuw290feNv5yElYEW3U07NskZrvkaXNtfknXJQxJNd0BkTQp4k1dE8l7q6ThzTknO_3BoIjScfVj7BLzARnqoRw7lbJAS92RObQ_Y9sPBZ35yXi6GY9khhhIQWnU2haYA1mbiK_8OYH4aHRoU8y1de48F9tDaVWF1RKxUUZpLjtI\",\"types\":[\"bakery\",\"store\",\"meal_takeaway\",\"cafe\",\"restaurant\",\"food\",\"establishment\"]}],\"status\":\"OK\"}";
			case UITestsHelpers.TestType.NoLocations:
				return "{\"html_attributions\":[],\"results\":[],\"status\":\"ZERO_RESULTS\"}";
			case UITestsHelpers.TestType.ParseError:
				throw new ArgumentNullException ();
			case UITestsHelpers.TestType.UserMoved:
				if (first) {
					first = false;
					return "{\"html_attributions\":[],\"results\":[],\"status\":\"ZERO_RESULTS\"}";
				} else {	
					return "{\"html_attributions\":[],\"next_page_token\":\"WyFnu2gNAXupVzLLc580jce5n1__-XTG2kpHbXPBcvk0Qr4ReJDOAvf_f_uXM87CzeOZgVOMPgGv2N5EuKzOcE_E13uekuMxq4bSKB_8emEfui6Pl_q8JgMh5KFjxvWXSdwaQVEytxFL5RzH03aE6aiz-itRRQaQ0TfDsyib1hm7CUeg9HApkft4lUMN3v2UDY8RyP22apCojOlBzObf2tZn6f_yFKOmIf-iOHfyFFpVIPzcIqx6_nKMucmQoVNmEqYUTuZ-WRAg_DAyyBpwZX5swBf3fjFJo-L3gg5nbcPhGnh7NTHdv1WXmb7qW3UarUnOH8DZjw2MqgY4SEFIn3pyXSc7JE17QbB9CWGIaFOnnMdcfXCtvlg4Y--j5oJvg6nTd\",\"results\":[{\"formatted_address\":\"227 Yale Avenue North, Seattle, WA 98109, United States\",\"geometry\":{\"location\":{\"lat\":47.620179,\"lng\":-122.330584}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"9050da096f1a773f2a8aca68cb91291523963cd9\",\"name\":\"Espresso Vivace\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":853,\"html_attributions\":[],\"photo_reference\":\"CnRnAAAA9mRKKCaaT393yNJ0yHw7YrQkK3JbRmf5CBaD0SQrtNTLSzr4xqMCVGunk22o1QBM7hy_7KfiFex4bcFMgkVuJ1wkM4fG4qFPLbPRAJ_mjHHizeYSYEa7Qh9UQjJgyDvXTEG4M0WxOLm4r1txCJdo0RIQZTAdx9zx3_7X09BTy-a9xRoUBPd4rI2VlyDQpzjwr1jQAEGDPsI\",\"width\":1280}],\"place_id\":\"ChIJ1-NiqTYVkFQRO2XahxpM_-g\",\"price_level\":1,\"rating\":4.5,\"reference\":\"CnRjAAAAUejPAhORDn8-mm-OPCap6-GBqbu0rg6Ufx5SpWYZViplVwttA4eh_IHzXCwFHiRFO_kbu6MZG1iiQhuhcv1Vb-fo1-rX86UQ9LjSk_1gSwxX3ViG8x0w8pETJA0keu7xHmKGSz-VrDZaMTtVKR2hPRIQxKDmK2GEXqzejK6KceLyaxoUPO-8DrikzRY65mfvjwchFiYzJKE\",\"types\":[\"cafe\",\"food\",\"establishment\"]},{\"formatted_address\":\"1600 East Olive Way, Seattle, WA 98102, United States\",\"geometry\":{\"location\":{\"lat\":47.619401,\"lng\":-122.325054}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/cafe-71.png\",\"id\":\"65f59975d6fa765f66cba7818baea6551044a422\",\"name\":\"Starbucks\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":949,\"html_attributions\":[\"<a href=\"https://plus.google.com/105254155462724110663\">Jonah Kirangi</a>\"],\"photo_reference\":\"CnRoAAAAEm9EGf6pRTUiibJ29PPKfLP13GGjL19v-eD5BT0MFSbhD6s2XIaVHBxzjqr4xhLmkmz09qrTqU-hWl8Y37q_dbV40_YdQJGA7gJGUq0valLtgIEpZQBmoRUWOBxdg3rTckku9rS8QPzYBwgAYcMRmBIQamcuxCtURzXlP-lhHviAoBoUo166OKYVq76YzlOa03-1T0dxZjc\",\"width\":1265}],\"place_id\":\"ChIJjfnnrjMVkFQRSuZC1_ODJvk\",\"price_level\":2,\"rating\":4.3,\"reference\":\"CmRdAAAAGA92tmNe1Kw7-z9IPZUWTXBF17wPVMdz-CGPBP2YUT67rLpQKoE8NrNDOA9lnJKZGUgPl0KE_NTaqw7ZRjwjNflci1yzdCqozXdgax-F1tcXQzfRP4jWlsx7izs66zRGEhD3PQ1m06poIOAMaxUvgtnOGhQcX5rMI--lwtoUxwO1pEDIdWBquA\",\"types\":[\"cafe\",\"food\",\"establishment\"]},{\"formatted_address\":\"422 Yale Avenue North, suite B, Seattle, WA 98109, United States\",\"geometry\":{\"location\":{\"lat\":47.622335,\"lng\":-122.330146}},\"icon\":\"http://maps.gstatic.com/mapfiles/place_api/icons/restaurant-71.png\",\"id\":\"03b8989ea39a823d2b50642128fae0c48bd4503b\",\"name\":\"Caffè Torino\",\"opening_hours\":{\"open_now\":true,\"weekday_text\":[]},\"photos\":[{\"height\":2048,\"html_attributions\":[],\"photo_reference\":\"CnRnAAAA2TCUiCh-77BNyzc8VwijUKbhPz4Cv90WyURE1Qp0fA4p9xgFStBxYRv9frddEud2s8I6WiX3_XIETE5yIOoRJK5IA78v8YcM6ugaEN0_YVGEuk7pKcjeYc3ek82mJG_R0EEzDQw3TStzljUdVIXUXBIQ2LsOs4gU1ZzaVkU8N1-enBoUVengj32iXE3iVM7zeq0kWl4lssU\",\"width\":1365}],\"place_id\":\"ChIJl8KRGDEVkFQRPSY4FT8fZsc\",\"rating\":4,\"reference\":\"CnRhAAAAOCUvIrxI229vJsLZpi1BJlr9a-YODXCmuw290feNv5yElYEW3U07NskZrvkaXNtfknXJQxJNd0BkTQp4k1dE8l7q6ThzTknO_3BoIjScfVj7BLzARnqoRw7lbJAS92RObQ_Y9sPBZ35yXi6GY9khhhIQWnU2haYA1mbiK_8OYH4aHRoU8y1de48F9tDaVWF1RKxUUZpLjtI\",\"types\":[\"bakery\",\"store\",\"meal_takeaway\",\"cafe\",\"restaurant\",\"food\",\"establishment\"]}],\"status\":\"OK\"}";
				}
			}
			return string.Empty;
		}
	}
}

