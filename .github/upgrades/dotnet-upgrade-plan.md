# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade src\QuickInfo\QuickInfo.csproj
4. Upgrade src\QuickInfoWeb\QuickInfoWeb.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                                   |
|:------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Newtonsoft.Json                     |   13.0.3        |  13.0.4     | Recommended for .NET 9.0                      |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### src\QuickInfo\QuickInfo.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net9.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 9.0*)

#### src\QuickInfoWeb\QuickInfoWeb.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - No NuGet package changes required.

Other changes:
  - None
