using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
#endif
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SimpleLocalizedStrings
{
    public static class LocalizationLoader
    {
        static readonly string STRING_TABLE_DEFAULT_ASSET_LOCATION = "Assets";

        /// <summary>
        /// process given scriptable objects with LocalizedData fields<br />
        /// will write field values to table collection by given locale code and table key
        /// </summary>
        /// <param name="localizedSOs">enumerable of scriptable objects with LocalizedData fields</param>
        /// <returns>awaitable unitask</returns>
        public static async UniTask ProcessLocalizedSOsAsync(IEnumerable<LocalizedSO> localizedSOs)
        {
            foreach (var so in localizedSOs)
                await ProcessLocalizedSOAsync(so);
        }

        /// <summary>
        /// process given scriptable object with LocalizedData fields<br />
        /// will write field values to table collection by given locale code and table key
        /// </summary>
        /// <param name="localizedSO">scriptable object with LocalizedData fields</param>
        /// <returns>awaitable unitask</returns>
        public static async UniTask ProcessLocalizedSOAsync(LocalizedSO localizedSO)
        {
            var locale = Utils.GetOrCreateLocale(localizedSO);
            var stringTable = await GetOrCreateTable(localizedSO, locale);
            if (TryProcessFields(localizedSO, stringTable))
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(stringTable);
                EditorUtility.SetDirty(stringTable.SharedData);
#endif
                Debug.Log($"{stringTable.name} imported from {localizedSO.name}");
            }
        }

        static async UniTask<StringTable> GetOrCreateTable(LocalizedSO localizedSO, Locale locale)
        {
#if UNITY_EDITOR
            var collection = LocalizationEditorSettings.GetStringTableCollection(localizedSO.localStringTableCollectionName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    localizedSO.localStringTableCollectionName,
                    STRING_TABLE_DEFAULT_ASSET_LOCATION);
            }
#endif
            return await GetTableAsync(localizedSO, locale);
        }

        static async UniTask<StringTable> GetTableAsync(LocalizedSO localizedSO, Locale locale) =>
            await LocalizationSettings.StringDatabase.GetTableAsync(localizedSO.localStringTableCollectionName, locale);

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
