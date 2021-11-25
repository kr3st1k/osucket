using System.IO;
using osucket.Calculations.OsuPerformanceCalculator;

namespace osucket.Calculations
{
	internal static class MapCache
	{
		private static WorkingBeatmap _workingBeatMap;
		public static WorkingBeatmap GetBeatMap(string file) => _workingBeatMap ??= new WorkingBeatmap(File.OpenRead(file));
	}
}