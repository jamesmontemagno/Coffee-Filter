using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/*
* Location models from https://developers.google.com/places/documentation/search
*/
namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Location
	{
		[DataMember(Name = "lat")]
		public double Latitude { get; set; }
		[DataMember(Name = "lng")]
		public double Longitude { get; set; }
	}

	[DataContract]
	public class Geometry
	{
		[DataMember(Name = "location")]
		public Location Location { get; set; }
	}

	[DataContract]
	public class OpeningHours
	{
		[DataMember(Name = "open_now")]
		public bool IsOpen { get; set; }
		[DataMember(Name = "weekday_text")]
		public List<object> WeekdayText { get; set; }
	}

	[DataContract]
	public class Photo
	{
		[DataMember(Name = "height")]
		public int Height { get; set; }
		[DataMember(Name = "photo_reference")]
		public string Reference { get; set; }
		[DataMember(Name = "width")]
		public int Width { get; set; }
	}

	[DataContract]
	public class Place
	{
		[DataMember(Name = "geometry")]
		public Geometry Geometry { get; set; }
		[DataMember(Name = "icon")]
		public string Icon { get; set; }
		[DataMember(Name = "id")]
		public string Id { get; set; }
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


		public double GetDistance(double lat, double lng, CoffeeFilter.Shared.GeolocationUtils.DistanceUnit unit = CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles)
		{
			return GeolocationUtils.GetDistance (lat, lng, Geometry.Location.Latitude, Geometry.Location.Longitude, unit);
		}

		
	}

	[DataContract]
	public class QueryObject
	{
		[DataMember(Name = "next_page_token")]
		public string NextPageToken { get; set; }
		[DataMember(Name = "results")]
		public List<Place> Places { get; set; }
		[DataMember(Name = "status")]
		public string Status { get; set; }
	}
}

