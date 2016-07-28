using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent.Quests
{
    public class QuestParams : Dictionary<string, string>
    {
    }

    public static class DictionaryHelpers
    {
        public static int GetInt(this Dictionary<string, string> dict, string key)
        {
            string text;
            int val;
            if (!dict.TryGetValue(key, out text) || !int.TryParse(text, out val))
            {
                val = 0;
            }
            return val;
        }

        public static string Get(this Dictionary<string, string> dict, string key)
        {
            string text;
            if (!dict.TryGetValue(key, out text))
            {
                return null;
            }
            return text;
        }
    }
}
