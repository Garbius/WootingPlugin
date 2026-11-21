using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Sandbox;
using VRage.Utils;
using VRage.Plugins;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using WootingAnalogSDKNET;
using WootingPlugin.Gui;

// Pulsar compiles the plugin from these source files, without the project file,
// so the version has to be declared in code to show up in its plugin list.
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]

namespace WootingPlugin
{
    public class Plugin : IPlugin
    {
        public const string Name = "WootingPlugin";

        public static Plugin Instance { get; private set; }

        private bool sdkInitialised;

        // Called by Pulsar when the plugin is configured from its plugin list
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenPluginConfig());
        }

        public void Init(object gameInstance)
        {
            Instance = this;

            MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin: Patching methods");
            try
            {
                var harmony = new Harmony(Name);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: Caught {e} while patching methods.\nDisabling plugin");
                return;
            }

            try
            {
                InitSdk();
            }
            catch (SEHException)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "WootingPlugin: WootingAnalogSDK failed to initialize due to an internal error");
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException || e is BadImageFormatException)
            {
                // The native wrapper library is missing or does not fit this platform
                MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: Could not load the Wooting Analog SDK wrapper: {e.Message}");
            }
        }

        private void InitSdk()
        {
            var (numDev, result) = WootingAnalogSDK.Initialise();

            if (result != WootingAnalogResult.Ok)
            {
                MyLogExtensions.Error(MySandboxGame.Log, $"Failed to initialize WootingPlugin: {result}");
                return;
            }

            sdkInitialised = true;
            MyLogExtensions.Info(MySandboxGame.Log, $"WootingPlugin initialized successfully, {numDev} device(s)");

            // The game identifies keys by their Windows virtual key code. Without this mode
            // the SDK would read those codes as HID codes, which are entirely different keys.
            WootingAnalogResult modeResult = WootingAnalogSDK.SetKeycodeMode(KeycodeType.VirtualKey);
            if (modeResult != WootingAnalogResult.Ok)
            {
                MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: The Wooting Analog SDK could not switch to virtual key codes ({modeResult}), analog input stays disabled. Updating the SDK may help.");
                return;
            }

            if (numDev < 1)
            {
                DeviceFailed();
            }
            else
            {
                Patch_GetGameControlAnalogState.ReadAnalog = true;
            }
        }

        public void DeviceFailed()
        {
            MyLogExtensions.Error(MySandboxGame.Log, "WootingPlugin: No keyboard detected");
            // Never subscribe twice, this is called again whenever reading a key finds no device
            WootingAnalogSDK.DeviceEvent -= DeviceEvent;
            WootingAnalogSDK.DeviceEvent += DeviceEvent;
            Patch_GetGameControlAnalogState.ReadAnalog = false;
        }

        public void DeviceEvent(DeviceEventType type, DeviceInfo info)
        {
            if (type == DeviceEventType.Connected)
            {
                WootingAnalogSDK.DeviceEvent -= DeviceEvent;
                Patch_GetGameControlAnalogState.ReadAnalog = true;
                MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin: Keyboard detected");
            }
        }

        public void Update() { }

        public void Dispose()
        {
            Patch_GetGameControlAnalogState.ReadAnalog = false;

            if (sdkInitialised)
            {
                sdkInitialised = false;
                WootingAnalogSDK.UnInitialise();
            }

            Instance = null;
        }
    }
}
