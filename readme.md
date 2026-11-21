# Wooting Support for Space Engineers

This is a plugin for Space Engineers that allows analog control of the character, space ships, and rovers in singleplayer and multiplayer games. It also affects programmable blocks, where the values can be accessed through `IMyShipController.MoveIndicator` as normal but the values are much more analog.

The controls can be set as usual in the game options. The pitch, yaw and roll sensitivity can be adjusted in the plugin's configuration dialog, which opens from the plugin's entry in the Pulsar plugin list.

The plugin uses Harmony to patch the game code. That has a small impact on the game and should be less prone to breakage when new game versions are released.

## Requirements

- [Space Engineers](https://store.steampowered.com/app/244850/Space_Engineers/)
- [Pulsar](https://github.com/SpaceGT/Pulsar), the plugin loader. Both of its Space Engineers 1 executables are supported:
  - `Interim` runs the game on .NET 10 and needs the [.NET 10 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
  - `Legacy` runs the game on .NET Framework
- The [Wooting Analog SDK](https://github.com/WootingKb/wooting-analog-sdk/releases). On Windows, Wootility installs it for you.

## Installation

Pulsar compiles the plugin from this repository, so there is nothing to download by hand.

1. Clone this repository.
2. Start Pulsar with the `-sources` command line option.
3. Click the Sources button, add the cloned folder as a development folder and select `WootingPlugin.xml` as its data file.
4. Enable Wooting Plugin in the plugin list and restart the game.

Pulsar downloads the Wooting SDK wrapper from NuGet for the runtime the game was started on. This works the same way with `Interim` and `Legacy`.

## Building

Building is only needed for development, or to install the plugin as a prebuilt DLL.

You need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0). Then run:

```
dotnet build -c Release
```

This builds the plugin twice: for .NET 10 (`net10.0`, loaded by `Interim`) and for .NET Framework (`net48`, loaded by `Legacy`).

The game is located through Steam. If that fails, create a `Directory.Build.props.user` file next to `Directory.Build.props` and set the paths there. This file is ignored by Git:

```xml
<Project>
  <PropertyGroup>
    <!-- Folder containing SpaceEngineers.exe -->
    <Bin64>C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Bin64</Bin64>

    <!-- Optional: Pulsar's folder, to deploy the plugin after each build -->
    <Pulsar>C:\Path\To\Pulsar</Pulsar>
  </PropertyGroup>
</Project>
```

Setting the `SE_GAME_DIR` environment variable to the game's folder works as well.

If `Pulsar` is set, each build is copied into Pulsar's local plugin folder together with the Wooting SDK wrapper it needs:

| Build     | Deployed to                              |
|-----------|------------------------------------------|
| `net10.0` | `<Pulsar>/Interim/Local/WootingPlugin/`  |
| `net48`   | `<Pulsar>/Legacy/Local/WootingPlugin/`   |

The `Interim` executable shares the `Legacy` data folder until an `Interim` folder exists next to it. Copy the `Legacy` folder as `Interim` before deploying the .NET 10 build.

A deployed DLL shows up in Pulsar as a separate local plugin. Don't enable it together with the development folder.

## Keyboard Layouts

The game identifies keys by their Windows key codes, which depend on the keyboard layout, while the keyboard reports analog values per physical key. The plugin translates between the two with the layout the game window uses, so layouts such as Dvorak or AZERTY work, and it follows along when the layout is switched while the game runs. The layout in use is written to the game log.

## Known Issues

There is an issue with wheels and analog controls. The brakes and throttle are treated as digital (on/off), affecting the Wooting and other controllers alike. Another plugin, [SEAnalogWheels](https://github.com/Garbius/SEAnalogWheels), addresses this issue until Keen fixes it.

## Support

Please provide the game log file located in `%APPDATA%\SpaceEngineers\SpaceEngineers_xxxxxxxx_xxxxxxxxx.log` and Pulsar's `info.log`, as they often contain information crucial to solving your problem. If it looks more like a bug, please open an issue on the GitHub page or create a pull request if you can fix it on your own.
