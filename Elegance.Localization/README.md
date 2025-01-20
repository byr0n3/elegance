# Elegance.Localization

A Source Generator for working with localized text.

## Usage

1. Create your localization file(s) like this (make sure to suffix the filename with `.lang.json`):

```json5
// In this example, the file is called: app.en.lang.json
{
  "key": "value",
  "yes": "Yes",
  "no": "No",
  // By using '{}', you can pass arguments when getting the localized value.
  // The number between the brackets indicates the index of the argument to use.
  "greeting": "Hello {0}!",
  "date": "Today is: {0}.",
  // You can add a specific format to use like this:
  "year": "The current year is: {0:Y}"
}
```

2. Add the project as a dependency and add your localization files as `<AdditionalFiles />`:

```xml

<Project>
	…
	<ItemGroup>
		<AdditionalFiles Include="localization/*.lang.json"/>
	</ItemGroup>

	<ItemGroup>
		<!-- Make sure to replace `0.2.0` with the latest version. -->
		<PackageReference Include="Elegance.Enums" Version="0.2.0" OutputItemType="Analyzer"
						  ReferenceOutputAssembly="false"/>
	</ItemGroup>
	…
</Project>
```

3. Based on your localization file name, you can now use your localization data like this:

```csharp
// Assuming the localization file is called `app.en.lang.json`,
// you can use the data in this file by calling the static class `AppLocalization`.

var yes = AppLocalization.Get("yes"); // "Yes"
var no = AppLocalization.Get("no"); // "No"
var greeting = AppLocalization.Get("greeting", "John"); // "Hello John!"
// By default, `CultureInfo.InvariantCulture` will be used to format arguments.
var date = AppLocalization.Get("date", System.DateTime.Now); // Today is: 01/20/2025 19:33:33.
// By passing a `CultureInfo` instance as the first argument, you can change what CultureInfo will be used. 
var dutchDate = AppLocalization.Get(Elegance.Utilities.Culture.NL, "date", System.DateTime.Now); // Today is: 20-01-2025 19:33:33.
// This localization value has a custom format.
var year = AppLocalization.Get("year", System.DateTime.Now); // The current year is: 2025 January
```

## Planned

- [ ] Support for multiple languages
