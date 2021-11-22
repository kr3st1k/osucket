using System.IO;
using osucket.calculations.PPCalculator;

namespace osucket
{
    class MapCache
    {
      
        static public WorkingBeatmap GetBeatmap(string osuDir)
        {

            var map = new WorkingBeatmap(File.OpenRead(osuDir));

            return map;

        }



    }
}