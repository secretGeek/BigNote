using System.Collections.Generic;

namespace BigNote.Models
{
    public class Replacement
    {
        public Replacement(string item, string separatorChars = "`")
        {
            var parts = item.Split(separatorChars.ToCharArray());
            ReplaceThis = parts[0];
            WithThis = parts[1];
        }

        public string ReplaceThis { get; set; }
        public string WithThis { get; set; }

        internal static List<Replacement> CreateList(string s)
        {
            var result = new List<Replacement>();
            if (string.IsNullOrWhiteSpace(s))
                return result;

            var parts = s.Split(",".ToCharArray());
            foreach (var item in parts)
            {
                if (item.IndexOf("`") > 0)
                {
                    var replacement = new Replacement(item);
                    result.Add(replacement);
                }
            }

            return result;
        }
    }
}
