using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Aspect
	{
		[DataMember (Name = "rating")]
		public int Rating { get; set; }

		[DataMember (Name = "type")]
		public string Type { get; set; }
	}

	[DataContract]
	public class Review
	{
		[DataMember (Name = "aspects")]
		public List<Aspect> Aspects { get; set; }

		[DataMember (Name = "author_name")]
		public string AuthorName { get; set; }

		[DataMember (Name = "author_url")]
		public string AuthorUrl { get; set; }

		[DataMember (Name = "language")]
		public string Language { get; set; }

		[DataMember (Name = "rating")]
		public int Rating { get; set; }

		[DataMember (Name = "text")]
		public string Text { get; set; }

		[DataMember (Name = "time")]
		public double Time { get; set; }
	}
}

