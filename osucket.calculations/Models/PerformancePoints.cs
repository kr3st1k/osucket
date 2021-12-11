using System.Text.Json.Serialization;

namespace osucket.Calculations.Models
{
	public class PerformancePoints
	{
		[JsonPropertyName("pp")]
		public double Performance { get; set; }
		[JsonPropertyName("fcpp")]
		public double FullComboPerformance { get; set; }
		[JsonPropertyName("sspp")]
		public double SuperSkillPerformance { get; set; }
	}
}