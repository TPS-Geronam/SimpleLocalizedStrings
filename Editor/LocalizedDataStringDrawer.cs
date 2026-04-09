using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace SimpleLocalizedStrings.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedData))]
    public class LocalizedDataStringDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var propField = new PropertyField(property);
            container.Add(propField);

            if (property.propertyType != SerializedPropertyType.String)
            {
                Debug.LogError("Can only use the LocalizedData attribute on members of type string, string[] or List<string>");
                return container;
            }

            var localSO = property.serializedObject.targetObject as LocalizedSO;
            var localizedDataStringAttribute = attribute as LocalizedData;

            container.Add(GetLocalTextField(localSO, property, localizedDataStringAttribute));
            if (localizedDataStringAttribute.addEntryButton)
                container.Add(GetAddButton(localSO, property, localizedDataStringAttribute));

            return container;
        }

        TextField GetLocalTextField(LocalizedSO so, SerializedProperty property, LocalizedData attribute)
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
            LocalizedData attribute)
        {
            var table = LocalizationSettings.StringDatabase.GetTable(so.localStringTableCollectionName, locale);
            if (table == null)
                return $"ERR: table {so.localStringTableCollectionName} not found";

            var entryKey = Utils.GetEntryKeyForSerializedProperty(property, attribute);
            var entry = table.GetEntry(entryKey);
            if (entry == null)
                return $"ERR: entry {entryKey} not found in table {table.name}";
            return entry.Value;
        }

        Button GetAddButton(LocalizedSO so, SerializedProperty property, LocalizedData attribute)
        {
            var entryKey = Utils.GetEntryKeyForSerializedProperty(property, attribute);
            return new(() => HandleAddButton(so, entryKey, property))
            {
                text = $"Add entry {entryKey} to table collection {so.localStringTableCollectionName}"
            };
        }

        void HandleAddButton(LocalizedSO so, string entryKey, SerializedProperty property)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(new(so.localIdCode));

            var stringTable = LocalizationSettings.StringDatabase.GetTable(so.localStringTableCollectionName, locale);

            if (isString && stringTable == null)
            {
                Debug.LogError($"Create a table collection with name {so.localStringTableCollectionName} first");
                return;
            }

            stringTable.AddEntry(entryKey, property.stringValue);
            Debug.Log($"Created {entryKey} in {stringTable.name} with value {property.stringValue}");
        }
    }
}
