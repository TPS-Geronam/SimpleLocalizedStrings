using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SimpleLocalizedStrings.Example
{
    public class MyLocLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("The localized data scriptable objects." +
            " Would have been loaded by some kind of content loader before Start.")]
        List<LocalizedSO> localizedSOs = new();
        
        [SerializeField]
        bool overwriteOldEntriesOnReload = true;

        public void Load() => ProcessLocalizedSOs(localizedSOs);

        void ProcessLocalizedSOs(IEnumerable<LocalizedSO> localizedSOs)
        {
            foreach (var so in localizedSOs)
                _ = PopulateStringLocalizationTable(so);
        }

        async UniTask PopulateStringLocalizationTable(LocalizedSO so)
        {
            var locale = Utils.GetOrCreateLocale(so);
            var stringTable = await LocalizationSettings.StringDatabase.GetTableAsync(so.localStringTableCollectionName, locale);
            if (TryProcessFields(so, stringTable))
            {
                EditorUtility.SetDirty(stringTable);
                EditorUtility.SetDirty(stringTable.SharedData);
                Debug.Log($"{stringTable.name} imported from {so.name}");
            }
        }

        bool TryProcessFields(LocalizedSO so, StringTable stringTable)
        {
            bool atLeastOneProcessed = false;
            foreach (var field in so.GetType().GetFields())
            {
                var attribute = field.GetCustomAttribute<LocalizedData>();
                if (attribute == null)
                    continue;
                if (TryCreateTableEntryFromField(so, stringTable, field))
                    atLeastOneProcessed = true;
            }
            return atLeastOneProcessed;
        }

        bool TryCreateTableEntryFromField(LocalizedSO so, StringTable stringTable, FieldInfo field)
        {
            if (field.FieldType == typeof(string))
                return TryCreateTableStringEntry(so, stringTable, field);
            else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                return TryCreateTableArrayEntry(so, stringTable, field);
            
            Debug.LogError($"{stringTable.name}: unsupported field {field.Name} ({so.name})");
            return false;
        }

        bool TryCreateTableArrayEntry(LocalizedSO so, StringTable stringTable, FieldInfo field)
        {
            var fieldKey = Utils.GetEntryKeyForField(field);
            var fieldArray = (IEnumerable<string>)field.GetValue(so);
            var i = 0;
            foreach (var value in fieldArray)
            {
                _ = TryCreateTableEntry(stringTable, $"{fieldKey}_{i}", value);
                i++;
            }
            return false;
        }

        bool TryCreateTableStringEntry(LocalizedSO so, StringTable stringTable, FieldInfo field)
        {
            var fieldKey = Utils.GetEntryKeyForField(field);
            var fieldValue = (string)field.GetValue(so);
            return TryCreateTableEntry(stringTable, fieldKey, fieldValue);
        }

        bool TryCreateTableEntry(StringTable stringTable, string fieldKey, string fieldValue)
        {
            var existingEntry = stringTable.GetEntry(fieldKey);
            if (existingEntry == null)
            {
                stringTable.AddEntry(fieldKey, fieldValue);
                return true;
            }

            if (overwriteOldEntriesOnReload)
            {
                existingEntry.Value = fieldValue;
                return true;
            }

            Debug.LogError($"{stringTable.name}: key {fieldKey} already exists");
            return false;
        }
    }
}
