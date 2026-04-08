using System.Collections.Generic;
using UnityEngine;

namespace SimpleLocalizedSO.Example
{
    [CreateAssetMenu(menuName = "SimpleLocalizedSO/Example/LocalizedSO", fileName = "MyLocalizedSO.asset")]
    public class MyLocalizedSO : LocalizedSO
    {
        [LocalizedDataString(isArray: true, addEntryButton: true)]
        public List<string> stringData = new();
        [LocalizedDataString(isArray: true)]
        public string[] stringDataArray = new string[0];
        [LocalizedDataString]
        public string myData;
    }
}
