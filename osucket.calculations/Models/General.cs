using osucket.Calculations.Enums;
using OsuMemoryDataProvider;

namespace osucket.Calculations.Models
{
	internal class General
	{
		internal OsuMemoryStatus OsuMemoryStatus { get; set; }
		internal SubmissionStatus SubmissionStatus { get; set; }
		internal RuleSet MenuRuleSet { get; set; }

		internal string Gameplay { get; set; } //?
		internal string ResultScreen { get; set; } //?
		internal string Song { get; set; }
		internal string BackgroundFile { get; set; }
		internal string SkinName { get; set; }
		internal string SkinDirectory { get; set; }
		internal string MapDirectory { get; set; }
		internal string MapFile { get; set; }
		internal string MapDifficulty { get; set; }
		internal int AudioPosition { get; set; }
		internal int BeatMapId { get; set; }
		internal int MenuMods { get; set; }
		internal int CountRetries { get; set; }
		internal bool IsInterface { get; set; }
		internal double SongLength { get; set; }

	}
}