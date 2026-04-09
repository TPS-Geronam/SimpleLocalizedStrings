using System.Collections.Generic;
using UnityEngine;

namespace SimpleLocalizedStrings.Example
{
    public class MyLocLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("The localized data scriptable objects." +
            " Would have been loaded by some kind of content loader before Start.")]
        List<LocalizedSO> localizedSOs = new();

        public void Load() => _ = LocalizationLoader.ProcessLocalizedSOsAsync(localizedSOs);
    }
}
