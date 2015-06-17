using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class AddressComponent
	{
		[DataMember (Name = "long_name")]
		public string LongName { get; set; } = string.Empty;

		[DataMember (Name = "short_name")]
		public string ShortName { get; set; } = string.Empty;

		[DataMember (Name = "types")]
		public List<string> Types { get; set; } = new List<string>();
	}

	[DataContract]
	public class Location
	{
		[DataMember (Name = "lat")]
		public double Latitude { get; set; } = 0;

		[DataMember (Name = "lng")]
		public double Longitude { get; set; } = 0;
	}

	[DataContract]
	public class Geometry
	{
		[DataMember (Name = "location")]
		public Location Location { get; set; } = new Location();
	}
}

