using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Aspect
	{
		[DataMember (Name = "rating")]
		public int Rating { get; set; } = 0;

		[DataMember (Name = "type")]
		public string Type { get; set; } = string.Empty;
	}

	[DataContract]
	public class Review
	{
		[DataMember (Name = "aspects")]
		public List<Aspect> Aspects { get; set; } = new List<Aspect>();

		[DataMember (Name = "author_name")]
		public string AuthorName { get; set; } = string.Empty;

		[DataMember (Name = "author_url")]
		public string AuthorUrl { get; set; } = string.Empty;

		[DataMember (Name = "language")]
		public string Language { get; set; } = string.Empty;

		[DataMember (Name = "rating")]
		public int Rating { get; set; } = 0;

		[DataMember (Name = "text")]
		public string Text { get; set; } = string.Empty;

		[DataMember (Name = "time")]
		public double Time { get; set; } = DateTime.Now.Ticks;
	}
}

