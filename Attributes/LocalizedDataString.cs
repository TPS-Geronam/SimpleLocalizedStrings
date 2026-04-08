using UnityEngine;

namespace SimpleLocalizedSO
{
    public class LocalizedDataString : PropertyAttribute
    {
        public readonly bool isArray;
        public readonly bool addEntryButton;

        public LocalizedDataString(bool isArray = false, bool addEntryButton = false)
        {
            this.isArray = isArray;
            this.addEntryButton = addEntryButton;
        }
    }
}
