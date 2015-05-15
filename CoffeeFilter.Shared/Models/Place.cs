using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Place
	{
		[DataMember(Name = "address_components")]
		public List<AddressComponent> AddressComponents { get; set; }

		[DataMember(Name = "adr_address")]
		public string Address { get; set; }

		[DataMember(Name = "formatted_address")]
		public string AddressFormatted { get; set; }

		[DataMember(Name = "formatted_phone_number")]
		public string PhoneNumberFormatted { get; set; }

		[DataMember(Name = "geometry")]
		public Geometry Geometry { get; set; }

		[DataMember(Name = "icon")]
		public string Icon { get; set; }

		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "international_phone_number")]
		public string InternationalPhoneNumber { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "opening_hours")]
		public OpeningHours OpeningHours { get; set; }

		[DataMember(Name = "photos")]
		public List<Photo> Photos { get; set; }

		[DataMember(Name = "place_id")]
		public string PlaceId { get; set; }

		[DataMember(Name = "rating")]
		public double Rating { get; set; }

		[DataMember(Name = "reference")]
		public string Reference { get; set; }

		[DataMember(Name = "scope")]
		public string Scope { get; set; }

		[DataMember(Name = "types")]
		public List<string> Types { get; set; }

		[DataMember(Name = "vicinity")]
		public string Vicinity { get; set; }

		[DataMember(Name = "price_level")]
		public int? PriceLevel { get; set; }

		[DataMember(Name = "reviews")]
		public List<Review> Reviews { get; set; }

		[DataMember(Name = "url")]
		public string Url { get; set; }

		[DataMember(Name = "user_ratings_total")]
		public int UserRatingsCount { get; set; }

		[DataMember(Name = "utc_offset")]
		public int UTCOffset { get; set; }

		[DataMember(Name = "website")]
		public string Website { get; set; }

		[DataMember(Name = "permanently_closed")]
		public bool PermanentlyClosed { get; set; }

		[IgnoreDataMember]
		public bool HasImage {
			get {
				return Photos != null && Photos.Count > 0;
			}
		}

		[IgnoreDataMember]
		public bool HasReviews {
			get {
				return Reviews != null && Reviews.Count > 0;
			}
		}

		[IgnoreDataMember]
		public string MainImage {
			get {
				if (!HasImage)
					return Icon;

				return Photos[0].ImageUrlLarge;
			}
		}

		public double GetDistance (double lat, double lng, GeolocationUtils.DistanceUnit unit = GeolocationUtils.DistanceUnit.Miles)
		{
			return GeolocationUtils.GetDistance(lat, lng, Geometry.Location.Latitude, Geometry.Location.Longitude, unit);
		}
	}
}