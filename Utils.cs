using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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

        /// <summary>
        /// if this changes then GetEntryKeyForField(...) also needs to change<br />
        /// property format: myPropertyArray<br />
        /// table entry: myPropertyArray_0, myPropertyArray_1, ...
        /// </summary>
        public static string GetEntryKeyForSerializedProperty(SerializedProperty property, LocalizedData attribute)
        {
            /* explicit flag in attribute needed as string, string[], List<string> 
               will all be SerializedPropertyType.String and have property.isArray true */
            if (property.isArray && attribute.isArray)
            {
                var fieldInfo = property.GetUnderlyingField();
                // quick hack to get element index from name: e.g. "Element 0", "Element 1" ..
                var index = int.Parse(property.displayName.Split(" ")[1]);
                return $"{fieldInfo.Name}_{index}";
            }
            return property.name;
        }

        /// <summary>
        /// if this changes then GetEntryKeyForSerializedProperty(...) also needs to change<br />
        /// property format: myPropertyArray<br />
        /// table entry: myPropertyArray_0, myPropertyArray_1, ...
        /// </summary>
        public static string GetEntryKeyForField(FieldInfo fieldInfo)
        {
            //var name = FormatName(fieldInfo.Name, NameFormat.LowerSnake);
            var name = fieldInfo.Name; // better for JSON
            return name;
        }

        public static Locale GetOrCreateLocale(LocalizedSO so)
        {
            var locale = GetLocale(so.localIdCode);
            if (locale == null)
                return CreateAndRegisterLocale(so.localIdCode);
            return locale;
        }

        static Locale CreateAndRegisterLocale(string localIdCode)
        {
            var locale = Locale.CreateLocale(localIdCode);
            LocalizationSettings.AvailableLocales.AddLocale(locale);
            return locale;
        }

        static Locale GetLocale(string localIdCode) => LocalizationSettings.AvailableLocales.GetLocale(new(localIdCode));
    }
}
