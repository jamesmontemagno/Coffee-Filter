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
		public List<string> WeekdayText { get; set; } = new List<string>();

		[DataMember (Name = "periods")]
		public List<Period> Periods { get; set; } = new List<Period>();
	}

	[DataContract]
	public class Close
	{
		[DataMember (Name = "day")]
		public int Day { get; set; } = 0;

		[DataMember (Name = "time")]
		public string Time { get; set; }  = string.Empty;
	}

	[DataContract]
	public class Open
	{
		[DataMember (Name = "day")]
		public int Day { get; set; } = 0;

		[DataMember (Name = "time")]
		public string Time { get; set; } = string.Empty;
	}

	[DataContract]
	public class Period
	{
		[DataMember (Name = "close")]
		public Close Close { get; set; } = new Close();

		[DataMember (Name = "open")]
		public Open Open { get; set; } = new Open();
	}
}

