using System.Text.Json.Serialization;
using osucket.Calculations.Enums;
using OsuMemoryDataProvider;

namespace osucket.Calculations.Models
{
	public class General
	{
		[JsonPropertyName("Status")]
		public OsuMemoryStatus OsuMemoryStatus { get; set; }
		[JsonPropertyName("beatmapStatus")]
		public SubmissionStatus SubmissionStatus { get; set; }
		[JsonPropertyName("mode")]
		public RuleSet MenuRuleSet { get; set; }

		public GamePlay Gameplay { get; set; } //?
		public GamePlay ResultScreen { get; set; } //?
		public string Song { get; set; }
		[JsonPropertyName("bg")]
		public string BackgroundFile { get; set; }
		public string SkinName { get; set; }
		[JsonPropertyName("SkinDir")]
		public string SkinDirectory { get; set; }
		[JsonPropertyName("mapDir")]
		public string MapDirectory { get; set; }
		[JsonPropertyName("osuFile")]
		public string MapFile { get; set; }
		[JsonPropertyName("StarRate")]
		public string MapDifficulty { get; set; }
		[JsonPropertyName("SongTime")]
		public int AudioPosition { get; set; }
		[JsonPropertyName("beatmap_id")]
		public int BeatMapId { get; set; }
		[JsonPropertyName("mods")]
		public int MenuMods { get; set; }
		[JsonPropertyName("cRetry")]
		public int CountRetries { get; set; }
		public bool IsInterface { get; set; }
		public double SongLength { get; set; }

	}
}