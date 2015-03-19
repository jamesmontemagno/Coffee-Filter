using System;
using System.Runtime.Serialization;
using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.Shared.Models
{

	[DataContract]
	public class Photo
	{
		[DataMember(Name = "height")]
		public int Height { get; set; }
		[DataMember(Name = "photo_reference")]
		public string Reference { get; set; }
		[DataMember(Name = "width")]
		public int Width { get; set; }

		[IgnoreDataMember]
		public string ImageUrl {
			get {
				return "https://maps.googleapis.com/maps/api/place/photo?maxwidth=500&photoreference=" + Reference + "&key=" + CoffeeFilterViewModel.APIKey;
			}
		}

		[IgnoreDataMember]
		public string ImageUrlLarge {
			get {
				return "https://maps.googleapis.com/maps/api/place/photo?maxwidth=800&photoreference=" + Reference + "&key=" + CoffeeFilterViewModel.APIKey;
			}
		}
	}

}

