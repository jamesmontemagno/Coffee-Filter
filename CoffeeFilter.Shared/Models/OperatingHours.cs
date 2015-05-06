using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoffeeFilter.Shared.Models
{
	[DataContract]
	public class OpeningHours
	{
		[DataMember (Name = "open_now")]
		public bool IsOpen { get; set; }

		[DataMember (Name = "weekday_text")]
		public List<string> WeekdayText { get; set; }

		[DataMember (Name = "periods")]
		public List<Period> Periods { get; set; }
	}

	[DataContract]
	public class Close
	{
		[DataMember (Name = "day")]
		public int Day { get; set; }

		[DataMember (Name = "time")]
		public string Time { get; set; }
	}

	[DataContract]
	public class Open
	{
		[DataMember (Name = "day")]
		public int Day { get; set; }

		[DataMember (Name = "time")]
		public string Time { get; set; }
	}

	[DataContract]
	public class Period
	{
		[DataMember (Name = "close")]
		public Close Close { get; set; }

		[DataMember (Name = "open")]
		public Open Open { get; set; }
	}
}

