# Wooting support for Space Engineers

This is a plugin for Space Engineers that allows analog control of the character, space ships, and rovers in singleplayer and multiplayer games. It also affects programmable blocks, where the values can be accessed through `IMyShipController.MoveIndicator` as normal but the values are much more analog.

The controls can be set as usual in the game options.


## Installation

* Download the plugin from the [Releases](https://github.com/Garbius/WootingPlugin/releases) section or build it from source
* Drop the dll files into your `common\SpaceEngineers\Bin64` folder
  * `WootingPlugin.dll`
  * `WootingAnalogSDK.NET.dll`
  * `wooting_analog_wrapper.dll`
* Add `-plugin WootingPlugin.dll` to your Space Engineers launch options, which can be found by opening Space Engineers properties in your Steam Library and hitting the _"SET LAUNCH OPTIONS..."_ button


## Support

I can be found on the Wooting and KeenSWH Discord servers. Please provide the log file located in `%APPDATA%\SpaceEngineers\SpaceEngineers_xxxxxxxx_xxxxxxxxx.log` as it often contains information crucial to solving your problem. If it looks more like a bug, please open an issue on the GitHub page or create a pull request if you can fix it on your own.


## Build it yourself

To build this project, you need to reference the following assemblies from your `common\SpaceEngineers\Bin64` folder:
* `Sandbox.Common`
* `Sandbox.Game`
* `VRage`
* `VRage.Input`
* `VRage.Library`
* `VRage.Math`

You will also need [WootingAnalogSDK.NET](https://www.nuget.org/packages/WootingAnalogSDK.NET/) in your project because that's what this is all about.
