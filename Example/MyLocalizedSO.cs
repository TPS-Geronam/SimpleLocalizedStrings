using System.Collections.Generic;
using UnityEngine;

namespace SimpleLocalizedSO.Example
{
    [CreateAssetMenu(menuName = "SimpleLocalizedSO/Example/LocalizedSO", fileName = "MyLocalizedSO.asset")]
    public class MyLocalizedSO : LocalizedSO
    {
        [LocalizedData(isArray: true, addEntryButton: true)]
        public List<string> stringData = new();
        [LocalizedData(isArray: true)]
        public string[] stringDataArray = new string[0];
        [LocalizedData]
        public string myData;
    }
}
