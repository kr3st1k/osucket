using System.Collections.Generic;
using osucket.Calculations.Enums;
using System.Text.Json.Serialization;
using Mods = OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods;

namespace osucket.Calculations.Models
{
	public class GamePlay
	{
		[JsonPropertyName("mode")]
		public RuleSet GameMode { get; set; }
		[JsonPropertyName("c300")]
		public int Count300 { get; set; }
		[JsonPropertyName("c100")]
		public int Count100 { get; set; }
		[JsonPropertyName("c50")]
		public int Count50 { get; set; }
		[JsonPropertyName("cMiss")]
		public int CountMiss { get; set; }
		[JsonPropertyName("cSB")]
		public int CountSliderBreaks { get; set; }
		[JsonPropertyName("cGeki")]
		public int CountGeki { get; set; }
		[JsonPropertyName("cKatu")]
		public int CountKatu { get; set; }
		[JsonPropertyName("combo")]
		public int Combo { get; set; }
		[JsonPropertyName("maxCombo")]
		public int MaxCombo { get; set; }
		[JsonPropertyName("score")]
		public int Score { get; set; }
		public object RawLeaderboard { get; set; }
		[JsonPropertyName("acc")]
		public double Accuracy { get; set; }
		[JsonPropertyName("HP")]
		public double Health { get; set; }
		[JsonPropertyName("HPSmooth")]
		public double HealthSmooth { get; set; }
		[JsonPropertyName("pp")]
		public PerformancePoints PerformancePoints { get; set; }
		[JsonPropertyName("UR")]
		public double UnstableRate { get; set; }
		[JsonPropertyName("username")]
		public string Username { get; set; }
		
		public bool IsReplay { get; set; }
		
		public List<int> HitError { get; set; }
		[JsonPropertyName("keyOverlay")]
		public OsuKey OsuKey { get; set; }
		[JsonPropertyName("mods")]
		public Enums.Mods Mods { get; set; }
	}
}