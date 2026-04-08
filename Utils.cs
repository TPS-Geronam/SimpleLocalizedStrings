using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLocalizedSO
{
    public static class Utils
    {
        public enum NameFormat
        {
            UpperSnake,
            LowerSnake
        }

        public static string FormatName(string baseName, NameFormat format)
        {
            var name = CollapseWhitespaces(baseName);
            var sb = new StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                var s = GetCharFormatted(c, format);

                bool prevValid = i - 1 >= 0;
                bool prevWasDigit = prevValid && char.IsDigit(name[i - 1]);
                bool prevWasUpper = prevValid && char.IsUpper(name[i - 1]);
                bool isNewBreak = char.IsDigit(c) && !prevWasDigit || char.IsUpper(c) && !prevWasUpper;
                if (i != 0 && isNewBreak)
                    s = "_" + s;
                sb.Append(s);
            }
            return sb.ToString();
        }

        static string CollapseWhitespaces(string s) => Regex.Replace(s, @"\s+", "");

        static string GetCharFormatted(char c, NameFormat format) => format switch
        {
            NameFormat.UpperSnake => c.ToString().ToUpper(),
            _ => c.ToString().ToLower(),
        };
    }
}
