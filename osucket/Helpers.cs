using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace BotServer
{
    class Helpers
    {
        public static Dictionary<string, string> ParseQueryString(NameValueCollection queryString)
        {
            var parsed = new Dictionary<string, string>();

            foreach(string key in queryString.Keys)
                parsed.Add(key, queryString.Get(key));

            return parsed;
        }

        public static int ParseIntOr(string s, int d)
        {
            try
            {
                return int.Parse(s);
            }
            catch
            {
                return d;
            }
        }

        public static JObject NotEnoughArgumentsError(string[] missingArguments) => JObject.FromObject(new
        {
            error = "Not enough arguments",
            missing = missingArguments
        });
    }
}