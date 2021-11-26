using System.Collections.Generic;
using osucket.Calculations.Enums;
using Mods = OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods;

namespace osucket.Calculations.Models
{
	public class GamePlay
	{
		public RuleSet GameMode { get; set; }
		public int Count300 { get; set; }
		public int Count100 { get; set; }
		public int Count50 { get; set; }
		public int CountMiss { get; set; }
		public int CountGeki { get; set; }
		public int CountKatu { get; set; }
		public int Combo { get; set; }
		public int MaxCombo { get; set; }
		public int Score { get; set; }
		
		public double Accuracy { get; set; }
		public double Health { get; set; }
		public double HealthSmooth { get; set; }
		public PerformancePoints PerformancePoints { get; set; }
		public double UnstableRate { get; set; }
		
		public string Username { get; set; }
		
		public bool IsReplay { get; set; }
		
		public List<int> HitError { get; set; }
		
		public OsuKey OsuKey { get; set; }
		
		public Enums.Mods Mods { get; set; }
	}
}