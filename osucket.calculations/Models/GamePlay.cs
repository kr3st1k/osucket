using System.Collections.Generic;
using osucket.Calculations.Enums;
using Mods = OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods;

namespace osucket.Calculations.Models
{
	internal class GamePlay
	{
		internal RuleSet GameMode { get; set; }
		internal int Count300 { get; set; }
		internal int Count100 { get; set; }
		internal int Count50 { get; set; }
		internal int CountMiss { get; set; }
		internal int CountGeki { get; set; }
		internal int CountKatu { get; set; }
		internal int Combo { get; set; }
		internal int MaxCombo { get; set; }
		internal int Score { get; set; }
		
		internal double Accuracy { get; set; }
		internal double Health { get; set; }
		internal double HealthSmooth { get; set; }
		internal PerformancePoints PerformancePoints { get; set; }
		internal double UnstableRate { get; set; }
		
		internal string Username { get; set; }
		
		internal bool IsReplay { get; set; }
		
		internal List<int> HitError { get; set; }
		
		internal OsuKey OsuKey { get; set; }
		
		internal Enums.Mods Mods { get; set; }
	}
}