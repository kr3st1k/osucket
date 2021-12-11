using System.Text.Json.Serialization;

namespace osucket.Calculations.Models
{
	public class OsuKey
	{
		[JsonPropertyName("ck1")]
		public int CountKeyLeft { get; set; }
		[JsonPropertyName("ck2")]
		public int CountKeyRight { get; set; }
		[JsonPropertyName("cm1")]
		public int CountMouseLeft { get; set; }
		[JsonPropertyName("cm2")]
		public int CountMouseRight { get; set; }
		
		[JsonPropertyName("bk1")]
		public bool PressedKeyLeft { get; set; }
		[JsonPropertyName("bk2")]
		public bool PressedKeyRight { get; set; }
		[JsonPropertyName("bm1")]
		public bool PressedMouseLeft { get; set; }
		[JsonPropertyName("bm2")]
		public bool PressedMouseRight { get; set; }

		public bool IsEnabled { get; set; }

	}
}