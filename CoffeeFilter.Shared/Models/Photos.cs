using System;
using System.Runtime.Serialization;

using CoffeeFilter.Shared.ViewModels;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class Photo
	{
		const string RegularImageUrl = "https://maps.googleapis.com/maps/api/place/photo?maxwidth=500&photoreference={0}&key={1}";
		const string LargeImageUrl = "https://maps.googleapis.com/maps/api/place/photo?maxwidth=800&photoreference={0}&key={1}";

		[DataMember (Name = "height")]
		public int Height { get; set; }

		[DataMember (Name = "photo_reference")]
		public string Reference { get; set; }

		[DataMember (Name = "width")]
		public int Width { get; set; }

		[IgnoreDataMember]
		public string ImageUrl {
			get {
				return string.Format (RegularImageUrl, Reference, CoffeeFilterViewModel.APIKey);
			}
		}

		[IgnoreDataMember]
		public string ImageUrlLarge {
			get {
				return string.Format (LargeImageUrl, Reference, CoffeeFilterViewModel.APIKey);
			}
		}
	}
}

