using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace SimpleLocalizedSO.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedDataString))]
    public class LocalizedDataStringDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var propField = new PropertyField(property);
            container.Add(propField);

            if (property.propertyType != SerializedPropertyType.String)
            {
                Debug.LogError("Can only use the LocalizedDataString attribute on members of type string, string[] or List<string>");
                return container;
            }

            var localSO = property.serializedObject.targetObject as LocalizedSO;
            var localizedDataStringAttribute = attribute as LocalizedDataString;

            container.Add(GetLocalTextField(localSO, property, localizedDataStringAttribute));
            if (localizedDataStringAttribute.addEntryButton)
                container.Add(GetAddButton(localSO, property, localizedDataStringAttribute));

            return container;
        }

        TextField GetLocalTextField(LocalizedSO so, SerializedProperty property, LocalizedDataString attribute)
        {
            var locale = LocalizationSettings.SelectedLocale;
            return new()
            {
                label = $"„¤„Ÿ„Ÿ Local value ({locale.Identifier.Code})",
                value = GetLocalValue(so, locale, property, attribute),
                isReadOnly = true,
            };
        }

        string GetLocalValue(
            LocalizedSO so,
            Locale locale,
            SerializedProperty property,
            LocalizedDataString attribute)
        {
            var table = LocalizationSettings.StringDatabase.GetTable(so.localTableCollectionName, locale);
            if (table == null)
                return $"ERR: table {so.localTableCollectionName} not found";

            var entryKey = GetEntryKey(property, attribute);
            var entry = table.GetEntry(entryKey);
            if (entry == null)
                return $"ERR: entry {entryKey} not found in table {table.name}";
            return entry.Value;
        }

        /// <summary>
        /// if this changes then MyLocLoader.GetEntryKeyForField(..) also needs to change<br />
        /// property format: myPropertyArray<br />
        /// table entry: myPropertyArray_0, myPropertyArray_1, ...
        /// </summary>
        string GetEntryKey(SerializedProperty property, LocalizedDataString attribute)
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

        Button GetAddButton(LocalizedSO so, SerializedProperty property, LocalizedDataString attribute)
        {
            var entryKey = GetEntryKey(property, attribute);
            return new(() => HandleAddButton(so, entryKey, property))
            {
                text = $"Add entry {entryKey} to table collection {so.localTableCollectionName}"
            };
        }

        void HandleAddButton(LocalizedSO so, string entryKey, SerializedProperty property)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(new(so.localIdCode));
            var table = LocalizationSettings.StringDatabase.GetTable(so.localTableCollectionName, locale);
            if (table == null)
            {
                Debug.LogError($"Create a table collection with name {so.localTableCollectionName} first");
                return;
            }

            table.AddEntry(entryKey, property.stringValue);
            Debug.Log($"Created {entryKey} in {table.name} with value {property.stringValue}");
        }
    }
}
