using CoffeeFilter.Shared.Models;
using Geolocator.Plugin.Abstractions;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using ExternalMaps.Plugin;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoffeeFilter.Shared.ViewModels
{
	public class DetailsViewModel : BaseViewModel
	{
		public Place Place { get; set; }
		public Position Position { get; set; }
		private const string DetailsQueryUrl = "https://maps.googleapis.com/maps/api/place/details/json?placeid={0}&key=" + CoffeeFilterViewModel.APIKey;
		public DetailsViewModel ()
		{

		}

		public void NavigateToShop()
		{
			#if !DEBUG
			Xamarin.Insights.Track("Navigation", new Dictionary<string,string>
				{
					{"name", Place.Name},
					{"rating", Place.Rating.ToString()}
				});
			#endif
			CrossExternalMaps.Current.NavigateTo (Place.Name, Place.Geometry.Location.Latitude, Place.Geometry.Location.Longitude);

		}

		public async Task<bool> RefreshPlace()
		{
			if (Place == null || IsBusy)
				return false;

			IsBusy = true;
			#if !DEBUG
			Xamarin.Insights.Track("Details", "CoffeeShop", Place.Name);
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
					}
					finally
					{
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
	}
}

