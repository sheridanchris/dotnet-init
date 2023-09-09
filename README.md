# dotnet-init
[![NuGet](https://buildstats.info/nuget/dotnet-init-tool)](https://www.nuget.org/packages/dotnet-init-tool)

A dotnet tool for quickly creating a C# or F# solution and project.

### Installation
You can install this as a dotnet tool by using
```
dotnet tool install --global dotnet-init-tool
```

### Usage
```
Description:
  Initializes a new .NET solution

Usage:
  init [options]

Options:
  --version                     Show version information
  -?, -h, --help                Show help and usage information
  -lang, --language <language>  The language of the project you'd like to create (c#, 
                                f#).
  -o, --output <output>         The root directory of the solution and project(s) you'd 
                                like to create.
  --name <name>                 The name of the solution and project you'd like to 
                                create.
  --template <template>         The dotnet template of the project you'd like to 
                                create.
  --includeTests                Include a test project.
  --includeFormatter            Include a formatting tool (fantomas or csharpier).
```
