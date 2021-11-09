using System.IO;
using osucket.PPCalculator;

namespace osucket
{
    class MapCache
    {
      
        static public ExpirableMap GetBeatmap(string osuDir)
        {

            var map = new WorkingBeatmap(File.OpenRead(osuDir));
            var expirable = new ExpirableMap
            {
                map = map,
                ID = 1
            };

            return expirable;

        }


        public class ExpirableMap
        {
            public WorkingBeatmap map;
            public int ID;
        }

    }
}