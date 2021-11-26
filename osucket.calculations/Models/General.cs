using osucket.Calculations.Enums;
using OsuMemoryDataProvider;

namespace osucket.Calculations.Models
{
	public class General
	{
		public OsuMemoryStatus OsuMemoryStatus { get; set; }
		public SubmissionStatus SubmissionStatus { get; set; }
		public RuleSet MenuRuleSet { get; set; }

		public GamePlay Gameplay { get; set; } //?
		public GamePlay ResultScreen { get; set; } //?
		public string Song { get; set; }
		public string BackgroundFile { get; set; }
		public string SkinName { get; set; }
		public string SkinDirectory { get; set; }
		public string MapDirectory { get; set; }
		public string MapFile { get; set; }
		public string MapDifficulty { get; set; }
		public int AudioPosition { get; set; }
		public int BeatMapId { get; set; }
		public int MenuMods { get; set; }
		public int CountRetries { get; set; }
		public bool IsInterface { get; set; }
		public double SongLength { get; set; }

	}
}