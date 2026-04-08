using UnityEngine;
using UnityEngine.Localization.Settings;
using static SimpleLocalizedSO.Utils;

namespace SimpleLocalizedSO
{
    public abstract class LocalizedSO : ScriptableObject
    {
        [Tooltip("The locale identifier code of this localized SO.")]
        public string localIdCode;
        [Tooltip("The main localization table collection key this scriptable object will use.")]
        public string localTableCollectionName;

        void Awake()
        {
            if (string.IsNullOrEmpty(localIdCode))
                localIdCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            if (string.IsNullOrEmpty(localTableCollectionName))
                localTableCollectionName = FormatName(name, NameFormat.LowerSnake);
        }

        void OnValidate()
        {
            if (string.IsNullOrEmpty(localIdCode))
                Debug.LogError($"localIdCode of {name} is empty or null");
            if (string.IsNullOrEmpty(localTableCollectionName))
                Debug.LogError($"localTableCollectionName of {name} is empty or null");
        }
    }
}
