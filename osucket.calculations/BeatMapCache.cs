using System.IO;
using osucket.Calculations.OsuPerformanceCalculator;
using osuTK.Graphics.OpenGL;

namespace osucket.Calculations
{
	internal class MapCache
	{
		public (string, WorkingBeatmap) WorkingBeatMap { get; private set; }
		public MapCache(string file) => InitializeBeatMap(file);
		private void InitializeBeatMap(string file) => WorkingBeatMap = (file,new WorkingBeatmap(File.OpenRead(file)));
		public WorkingBeatmap this[string name]
		{
			get
			{
				if (WorkingBeatMap.Item1 != name) InitializeBeatMap(name);
				return WorkingBeatMap.Item2;
			}
		}
	}
}