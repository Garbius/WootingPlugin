# Wooting Support for Space Engineers

This is a plugin for Space Engineers that allows analog control of the character, space ships, and rovers in singleplayer and multiplayer games. It also affects programmable blocks, where the values can be accessed through `IMyShipController.MoveIndicator` as normal but the values are much more analog.

The controls can be set as usual in the game options.

Starting with v1.0, this project uses Harmony to patch the game code. That has a smaller impact on the game and should be less prone to breakage when new game versions are released.

## Known Issues

There is an issue in the Wooting SDK that affects the arrow keys. When testing with a Wooting Two (the one with Flaretech switches), binding any movement action to the arrow keys would look right in the settings menu but it would be bound to the numpad in game. It will hopefully be fixed in a future version of the Wooting SDK.

There is an issue with wheels and analog controls. The brakes and throttle are treated as digital (on/off), affecting the Wooting and other controllers alike. I have made another plugin, [SEAnalogWheels](https://github.com/Garbius/SEAnalogWheels), that addresses this issue until Keen fixes it.

## Installation

Install the custom game launcher from the [Github repository](https://github.com/sepluginloader/SpaceEngineersLauncher). Instructions are at the bottom of the page. It will automatically install [Plugin Loader](https://github.com/sepluginloader/PluginLoader). Start the game using the launcher and open the Plugins window from the main menu. Click the little plus sign in the bottom right corner of the Plugins column and find this plugin in the list and enable it. The game must be restarted before it can be used.

## Support

I can be found in the Wooting, KeenSWH, and Plugin Loader Discord servers. Please provide the log file located in `%APPDATA%\SpaceEngineers\SpaceEngineers_xxxxxxxx_xxxxxxxxx.log` as it often contains information crucial to solving your problem. If it looks more like a bug, please open an issue on the GitHub page or create a pull request if you can fix it on your own.

