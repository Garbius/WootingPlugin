using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Sandbox;
using VRage.Utils;
using VRage.Plugins;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using WootingAnalogSDKNET;
using WootingPlugin.Gui;

namespace WootingPlugin
{
    public class Plugin : IPlugin
    {
        public static Plugin Instance { get; private set; }
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenPluginConfig());
        }
        
        public void Init(object gameInstance = null)
        {
            Instance = this;

            MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin: Patching methods");
            try
            {
                var harmony = new Harmony("WootingPlugin");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: Caught {e}:{e.Message} while patching methods.\nDisabling plugin");
                return;
            }

            try
            {
                var (numDev, result) = WootingAnalogSDK.Initialise();

                if (result == WootingAnalogResult.Ok)
                {
                    MyLogExtensions.Info(MySandboxGame.Log, $"WootingPlugin initialized successfully, {numDev} device(s)");
                }
                else
                {
                    MyLogExtensions.Error(MySandboxGame.Log, $"Failed to initialize WootingPlugin: {result}");
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
            catch (SEHException)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "WootingPlugin: WootingAnalogSDK failed to initialize due to an internal error");
                return;
            }

            WootingAnalogSDK.SetKeycodeMode(KeycodeType.VirtualKey);
        }

        public void DeviceFailed()
        {
            MyLogExtensions.Error(MySandboxGame.Log, "WootingPlugin: No keyboard detected");
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
            WootingAnalogSDK.UnInitialise();
        }
    }
}
