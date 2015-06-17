using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Place
	{
		[DataMember(Name = "address_components")]
		public List<AddressComponent> AddressComponents { get; set; } = new List<AddressComponent>();

		[DataMember(Name = "adr_address")]
		public string Address { get; set; } = string.Empty;

		[DataMember(Name = "formatted_address")]
		public string AddressFormatted { get; set; } = string.Empty;

		[DataMember(Name = "formatted_phone_number")]
		public string PhoneNumberFormatted { get; set; } = string.Empty;

		[DataMember(Name = "geometry")]
		public Geometry Geometry { get; set; } = new Geometry();

		[DataMember(Name = "icon")]
		public string Icon { get; set; } = string.Empty;

		[DataMember(Name = "id")]
		public string Id { get; set; } = string.Empty;

		[DataMember(Name = "international_phone_number")]
		public string InternationalPhoneNumber { get; set; } = string.Empty;

		[DataMember(Name = "name")]
		public string Name { get; set; } = string.Empty;

		[DataMember(Name = "opening_hours")]
		public OpeningHours OpeningHours { get; set; } = new OpeningHours();

		[DataMember(Name = "photos")]
		public List<Photo> Photos { get; set; } = new List<Photo>();

		[DataMember(Name = "place_id")]
		public string PlaceId { get; set; } = string.Empty;

		[DataMember(Name = "rating")]
		public double Rating { get; set; } = 0;

		[DataMember(Name = "reference")]
		public string Reference { get; set; } = string.Empty;

		[DataMember(Name = "scope")]
		public string Scope { get; set; } = string.Empty;

		[DataMember(Name = "types")]
		public List<string> Types { get; set; } = new List<string>();

		[DataMember(Name = "vicinity")]
		public string Vicinity { get; set; } = string.Empty;

		[DataMember(Name = "price_level")]
		public int? PriceLevel { get; set; } = 0;

		[DataMember(Name = "reviews")]
		public List<Review> Reviews { get; set; } = new List<Review>();

		[DataMember(Name = "url")]
		public string Url { get; set; } = string.Empty;

		[DataMember(Name = "user_ratings_total")]
		public int UserRatingsCount { get; set; } = 0;

		[DataMember(Name = "utc_offset")]
		public int UTCOffset { get; set; } = 0;

		[DataMember(Name = "website")]
		public string Website { get; set; } = string.Empty;

		[DataMember(Name = "permanently_closed")]
		public bool PermanentlyClosed { get; set; } = false;

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