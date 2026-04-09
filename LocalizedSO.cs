using UnityEngine;
using UnityEngine.Localization.Settings;
using static SimpleLocalizedStrings.Utils;

namespace SimpleLocalizedStrings
{
    public abstract class LocalizedSO : ScriptableObject
    {
        [Tooltip("The locale identifier code of this localized SO.")]
        public string localIdCode;
        [Tooltip("The main string localization table collection key this scriptable object will use.")]
        public string localStringTableCollectionName;

        void Awake()
        {
            if (string.IsNullOrEmpty(localIdCode))
                localIdCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            if (string.IsNullOrEmpty(localStringTableCollectionName))
                localStringTableCollectionName = FormatName(name, NameFormat.LowerSnake);
        }

        void OnValidate()
        {
            if (string.IsNullOrEmpty(localIdCode))
                Debug.LogError($"localIdCode of {name} is empty or null");
            if (string.IsNullOrEmpty(localStringTableCollectionName))
                Debug.LogError($"localStringTableCollectionName of {name} is empty or null");
        }
    }
}
