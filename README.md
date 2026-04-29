# SimpleLocalizedStrings

A simple system for importing localized string data from ScriptableObjects into localization tables for Unity3D. For setups where `LocalizedString` or `LocalizedAsset` are not practical to use, e.g. SOs created by importing data from external resources. Uses a `[LocalizedData]` attribute to map string fields to localization tables, with support for enumerables. If in the editor, new table collections will be instantiated.

Last tested on Unity 6.4

<img width="518" height="500" alt="sls" src="https://github.com/user-attachments/assets/4c9e7d4e-109a-4220-b20b-fe6bfcd90a9d" />

[drawio](https://github.com/TPS-Geronam/SimpleLocalizedStrings/blob/main/sls.drawio)

## Dependencies
- [Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.5) 
- [UniTask](https://github.com/cysharp/UniTask)

## Usage

1. (Optional in Editor) Create your string table collection. Use the name of this collection as its key.

2. Create your localized ScriptableObject: holds localized string data. Configure the SO inside the inspector. Specify locale code and table collection key.

```C#
[CreateAssetMenu(menuName = "SimpleLocalizedSO/Example/LocalizedSO", fileName = "MyLocalizedSO.asset")]
public class MyLocalizedSO : LocalizedSO {
	// simple localized string
	[LocalizedData] public string myData;
	
	// set isArray true if enumerable
	[LocalizedData(isArray: true)]
	public string[] stringDataArray = new string[0];
	
	// set addEntryButton true to show a button in the inspector for this field
	// button will execute import on this specific field
	// for enumerables, a button will appear for each entry
	[LocalizedData(isArray: true, addEntryButton: true)]
	public List<string> stringData = new();
}
```

3. Load SOs into tables inside your collection

```C#
List<LocalizedSO> localizedSOs = ...
// load them all
await LocalizationLoader.ProcessLocalizedSOsAsync(localizedSOs)
// load one, this will silently overwrite previous values
await LocalizationLoader.ProcessLocalizedSOAsync(localizedSOs.First())
```

## Thoughts
- if you can, use LocalizedString and LocalizedAsset instead
- nice for importing localizations while in the editor
	- it may not make sense to populate tables runtime
	- especially not in Update, as reflection is involved
	- table collections are created only while in the editor
- Localization package uses Addressables, makes sense to make localization SOs also Addressable assets
- once the SOs have been loaded into the tables, query the tables directly instead of getting text from the SOs

## Example Scene
- import button
	- calls localization loader
	- reimports all LocalizedData strings
	- writes SO's field values into localization table by SO's table collection key and locale code
- label displays a localized string from a table
- 2 SOs with some string data inside: english, french
- 1 string table collection: my_loc
	- 2 tables: my_loc_en, my_loc_fr
