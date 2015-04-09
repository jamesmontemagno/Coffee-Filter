using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/*
* Location models from https://developers.google.com/places/documentation/search
*/
using CoffeeFilter.Shared.ViewModels;
using System.Linq;


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

		[DataMember(Name="permanently_closed")]
		public bool PermanentlyClosed { get; set;}

		/*[IgnoreDataMember]
		public string StreetNumber {
			get;
		}

		[IgnoreDataMember]
		public string Street {
			get;
		}

		[IgnoreDataMember]
		public string City {
			get;
		}

		[IgnoreDataMember]
		public string State {
			get;
		}

		[IgnoreDataMember]
		public string PostalCode {
			get;
		}
			
		[IgnoreDataMember]
		public string Country {
			get;
		}*/

		[IgnoreDataMember]
		public bool HasImage
		{
			get { return (Photos != null && Photos.Count > 0); }
		}

		[IgnoreDataMember]
		public string MainImage
		{
			get {
				if (!HasImage)
					return Icon;

				return Photos [0].ImageUrlLarge;
			}
		}

    [IgnoreDataMember]
    public bool HasOpeningHours
    {
      get {
        return (OpeningHours != null && OpeningHours.WeekdayText.Count == 7);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursMonday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;
        
        return GetTime (OpeningHours.WeekdayText [0]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursTuesday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [1]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursWednesday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [2]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursThursday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [3]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursFriday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [4]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursSaturday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [5]);
      }
    }

    [IgnoreDataMember]
    public string DisplayHoursSunday
    {
      get{
        if (!HasOpeningHours)
          return string.Empty;

        return GetTime (OpeningHours.WeekdayText [6]);
      }
    }

		public double GetDistance(double lat, double lng, GeolocationUtils.DistanceUnit unit = CoffeeFilter.Shared.GeolocationUtils.DistanceUnit.Miles)
		{
			return GeolocationUtils.GetDistance (lat, lng, Geometry.Location.Latitude, Geometry.Location.Longitude, unit);
		}

    [IgnoreDataMember]
    public string DisplayAddress
    {
      get {
        var addressShort = string.Empty;
        if (this.AddressComponents != null) {
          var comp = this.AddressComponents.FirstOrDefault (a => a.Types != null && a.Types.Contains ("street_number"));
          var comp2 = this.AddressComponents.FirstOrDefault (a => a.Types != null && a.Types.Contains ("route"));
          if (comp != null && comp2 != null)
            addressShort = comp.ShortName + " " + comp2.ShortName;

        }
        return (string.IsNullOrWhiteSpace (addressShort) ? this.AddressFormatted : addressShort);

      }
    }


    public string PriceOpenDisplay(string openNow, string formatOpenUntil)
    {
        var priceOpenDisplay = string.Empty;
        if (PriceLevel.HasValue) {
          for (int i = 0; i < PriceLevel.Value; i++)
            priceOpenDisplay += "$";
        }

        if (!string.IsNullOrWhiteSpace (priceOpenDisplay)) {
          priceOpenDisplay += " - ";
        }


        if (OpeningHours == null || OpeningHours.Periods == null || OpeningHours.WeekdayText.Count != 7) {
          priceOpenDisplay += openNow;
        } else {

          var dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;
          if (dayOfWeek < 0)
            dayOfWeek = 6;

          var closeTime = GetTime (OpeningHours.WeekdayText [dayOfWeek]);
          var closeIndex = closeTime.LastIndexOf ("–", StringComparison.InvariantCultureIgnoreCase);
          if (closeIndex != -1)
            closeTime = closeTime.Remove (0, closeIndex + 1).Trim ();

        priceOpenDisplay += string.Format (formatOpenUntil, closeTime);
        }
        return priceOpenDisplay;

    }

    private string GetTime (string toParse)
    {
      var index = toParse.IndexOf (":", System.StringComparison.InvariantCultureIgnoreCase);
      if (index <= 0)
        return toParse;

      return toParse.Remove (0, index + 1).Trim ();
    }

		
	}


}

