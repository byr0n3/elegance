# Elegance.Icons

Converts `.svg` files into `Razor renderable components` using source generation.

## Usage

- Place your icons into a folder in your consuming project. (like `/icons`)
- Add your folder as `additional files` and add the source generator:

```xml
<ItemGroup>
	<AdditionalFiles Include="icons/*.svg"/>

	<PackageReference Include="Elegance.Icons" Version="0.1.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false"/>
</ItemGroup>
```

- Use the icons in your `.razor` files:

```razor
@using Elegance.Icons;

<main>
    @(Icons.Search)
</main>
```
