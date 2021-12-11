using System.IO;
using osucket.Calculations.OsuPerformanceCalculator;
using osuTK.Graphics.OpenGL;

namespace osucket.Calculations
{
	internal class MapCache
	{
		private static (string, WorkingBeatmap) WorkingBeatMap { get; set; }
		private static void InitializeBeatMap(string file) => WorkingBeatMap = (file,new WorkingBeatmap(File.OpenRead(file)));
		// public WorkingBeatmap this[string name]
		// {
		// 	get
		// 	{
		// 		if (WorkingBeatMap.Item1 != name) InitializeBeatMap(name);
		// 		return WorkingBeatMap.Item2;
		// 	}
		// }
		public static WorkingBeatmap GetBeatMap(string file)
		{
			if (file != WorkingBeatMap.Item1) InitializeBeatMap(file);
			return WorkingBeatMap.Item2;
		}
	}
}