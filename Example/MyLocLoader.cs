using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SimpleLocalizedSO.Example
{
    public class MyLocLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("The localized data scriptable objects." +
            " Would have been loaded by some kind of content loader before Start.")]
        List<LocalizedSO> localizedSOs = new();
        
        [SerializeField]
        bool overwriteOldEntriesOnReload = true;

        void OnEnable() => LocalizationSettings.SelectedLocaleChanged += Load;
        void OnDisable() => LocalizationSettings.SelectedLocaleChanged -= Load;
        void Load(Locale locale) => ProcessLocalizedSOs(localizedSOs);

        void ProcessLocalizedSOs(IEnumerable<LocalizedSO> localizedSOs)
        {
            foreach (var so in localizedSOs)
                _ = PopulateLocalizationTable(so);
        }

        async UniTask PopulateLocalizationTable(LocalizedSO so)
        {
            var locale = GetLocale(so.localIdCode);
            if (locale == null)
                locale = CreateAndRegisterLocale(so.localIdCode);
            
            var table = await LocalizationSettings.StringDatabase.GetTableAsync(so.localTableCollectionName, locale);
            if (TryProcessFields(so, table))
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
                Debug.Log($"{table.name} imported from {so.name}");
            }
        }

        bool TryProcessFields(LocalizedSO so, StringTable table)
        {
            bool atLeastOneProcessed = false;
            foreach (var field in so.GetType().GetFields())
            {
                if (field.GetCustomAttribute<LocalizedDataString>() == null)
                    continue;
                if (TryCreateTableEntryFromField(so, table, field))
                    atLeastOneProcessed = true;
            }
            return atLeastOneProcessed;
        }

        bool TryCreateTableEntryFromField(LocalizedSO so, StringTable table, FieldInfo field)
        {
            if (field.FieldType == typeof(string))
                return TryCreateTableStringEntry(so, table, field);
            else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                return TryCreateTableArrayEntry(so, table, field);

            Debug.LogError($"{table.name}: unsupported field {field.Name} ({so.name})");
            return false;
        }

        bool TryCreateTableArrayEntry(LocalizedSO so, StringTable table, FieldInfo field)
        {
            var fieldKey = GetEntryKeyForField(field);
            var fieldArray = (IEnumerable<string>)field.GetValue(so);
            var i = 0;
            foreach (var value in fieldArray)
            {
                _ = TryCreateTableEntry(table, $"{fieldKey}_{i}", value);
                i++;
            }
            return false;
        }

        bool TryCreateTableStringEntry(LocalizedSO so, StringTable table, FieldInfo field)
        {
            var fieldKey = GetEntryKeyForField(field);
            var fieldValue = (string)field.GetValue(so);
            return TryCreateTableEntry(table, fieldKey, fieldValue);
        }

        bool TryCreateTableEntry(StringTable table, string fieldKey, string fieldValue)
        {
            var existingEntry = table.GetEntry(fieldKey);
            if (existingEntry == null)
            {
                table.AddEntry(fieldKey, fieldValue);
                return true;
            }

            if (overwriteOldEntriesOnReload)
            {
                existingEntry.Value = fieldValue;
                return true;
            }

            Debug.LogError($"{table.name}: key {fieldKey} already exists");
            return false;
        }

        /// <summary>
        /// if this changes then LocalizedDataStringDrawer.GetEntryKey(..) also needs to change<br />
        /// property format: myPropertyArray<br />
        /// table entry: myPropertyArray_0, myPropertyArray_1, ...
        /// </summary>
        string GetEntryKeyForField(FieldInfo fieldInfo)
        {
            //var name = FormatName(fieldInfo.Name, NameFormat.LowerSnake);
            var name = fieldInfo.Name; // better for JSON
            return name;
        }

        Locale CreateAndRegisterLocale(string localIdCode)
        {
            var locale = Locale.CreateLocale(localIdCode);
            LocalizationSettings.AvailableLocales.AddLocale(locale);
            return locale;
        }

        Locale GetLocale(string localIdCode) => LocalizationSettings.AvailableLocales.GetLocale(new(localIdCode));
    }
}
