using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class SearchQueryResult
	{
		[DataMember(Name = "next_page_token")]
		public string NextPageToken { get; set; }
		[DataMember(Name = "results")]
		public List<Place> Places { get; set; }
		[DataMember(Name = "status")]
		public string Status { get; set; }
	}

	[DataContract]
	public class DetailQueryResult
	{
		[DataMember(Name = "result")]
		public Place Place { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }
	}
}

