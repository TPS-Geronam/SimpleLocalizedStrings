using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SimpleLocalizedStrings
{
    public static class LocalizationLoader
    {
        public static async UniTask ProcessLocalizedSOsAsync(IEnumerable<LocalizedSO> localizedSOs)
        {
            foreach (var so in localizedSOs)
                await ProcessLocalizedSOAsync(so);
        }

        public static async UniTask ProcessLocalizedSOAsync(LocalizedSO localizedSO)
        {
            var locale = Utils.GetOrCreateLocale(localizedSO);
            var stringTable = await LocalizationSettings.StringDatabase.GetTableAsync(localizedSO.localStringTableCollectionName, locale);
            if (TryProcessFields(localizedSO, stringTable))
            {
                EditorUtility.SetDirty(stringTable);
                EditorUtility.SetDirty(stringTable.SharedData);
                Debug.Log($"{stringTable.name} imported from {localizedSO.name}");
            }
        }

        static bool TryProcessFields(LocalizedSO localizedSO, StringTable stringTable)
        {
            bool atLeastOneProcessed = false;
            foreach (var field in localizedSO.GetType().GetFields())
            {
                var attribute = field.GetCustomAttribute<LocalizedData>();
                if (attribute == null)
                    continue;
                if (TryCreateTableEntryFromField(localizedSO, stringTable, field))
                    atLeastOneProcessed = true;
            }
            return atLeastOneProcessed;
        }

        static bool TryCreateTableEntryFromField(LocalizedSO localizedSO, StringTable stringTable, FieldInfo field)
        {
            if (field.FieldType == typeof(string))
                return TryCreateTableStringEntry(localizedSO, stringTable, field);
            else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                return TryCreateTableArrayEntry(localizedSO, stringTable, field);

            Debug.LogError($"{stringTable.name}: unsupported field {field.Name} ({localizedSO.name})");
            return false;
        }

        static bool TryCreateTableStringEntry(LocalizedSO localizedSO, StringTable stringTable, FieldInfo field)
        {
            var fieldKey = Utils.GetEntryKeyForField(field);
            var fieldValue = (string)field.GetValue(localizedSO);
            return TryCreateTableEntry(stringTable, fieldKey, fieldValue);
        }

        static bool TryCreateTableArrayEntry(LocalizedSO localizedSO, StringTable stringTable, FieldInfo field)
        {
            var fieldKey = Utils.GetEntryKeyForField(field);
            var fieldArray = (IEnumerable<string>)field.GetValue(localizedSO);
            var (i, atLeastOneProcessed) = (0, false);
            foreach (var value in fieldArray)
            {
                if (TryCreateTableEntry(stringTable, $"{fieldKey}_{i}", value))
                    atLeastOneProcessed = true;
                i++;
            }
            return atLeastOneProcessed;
        }

        static bool TryCreateTableEntry(StringTable stringTable, string fieldKey, string fieldValue)
        {
            var existingEntry = stringTable.GetEntry(fieldKey);
            if (existingEntry == null)
            {
                stringTable.AddEntry(fieldKey, fieldValue);
                return true;
            }
            existingEntry.Value = fieldValue;
            return true;
        }
    }
}
