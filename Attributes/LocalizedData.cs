using UnityEngine;

namespace SimpleLocalizedStrings
{
    public class LocalizedData : PropertyAttribute
    {
        public readonly bool isArray;
        public readonly bool addEntryButton;

        public LocalizedData(bool isArray = false, bool addEntryButton = false)
        {
            this.isArray = isArray;
            this.addEntryButton = addEntryButton;
        }
    }
}
