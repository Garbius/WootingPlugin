# Wooting support for Space Engineers

This is a plugin for Space Engineers that allows analog control of the character, space ships, and rovers in singleplayer and multiplayer games. It requires the latest keyboard firmware, which can be installed with the [beta version of wootility](https://s3.eu-west-2.amazonaws.com/wooting-update/wootility-win-beta/wootility-beta+Setup+3.4.3-beta.exe). Back up your profiles before installing.

The controls can be set as usual in the game options. Due to the way this is implemented (read "laziness"), you must restart your game after changing controls. There may also be problems when playing scenarios that block/unblock player movements. These issues may be fixed in the future.

## Installation

* Download the plugin from the [Releases](https://github.com/Garbius/WootingPlugin/releases) section or build it from source
* Drop the dll files into your `common\SpaceEngineers\Bin64` folder
  * `WootingPlugin.dll`
  * `WootingAnalogSDK.NET.dll`
  * `wooting_analog_wrapper.dll`
* Add `-plugin WootingPlugin.dll` to your Space Engineers launch options, which can be found by opening Space Engineers properties in your Steam Library and hitting the _"SET LAUNCH OPTIONS..."_ button

## Dependencies

To build this project, you need to reference the following assemblies from your `common\SpaceEngineers\Bin64` folder:
* `Sandbox.Common`
* `Sandbox.Game`
* `VRage`
* `VRage.Input`
* `VRage.Library`
* `VRage.Math`

The repository includes two binary libraries under `lib/`. They were compiled from their respective official WootingKb git repos.
* `WootingAnalogSDK.NET.dll` from [wooting-analog-sdk](https://github.com/WootingKb/wooting-analog-sdk)
* `wooting_analog_wrapper.dll` from [wooting-analog-wrappers](https://github.com/WootingKb/wooting-analog-wrappers)
