using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Sandbox;
using Sandbox.Game;
using VRage.Input;
using VRage.Utils;
using VRage.Plugins;
using HarmonyLib;
using WootingAnalogSDKNET;

namespace WootingPlugin
{
    public class WootingPlugin : IPlugin
    {
        public static WootingPlugin Instance { get; private set; }

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
                    MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin initialized successfully");
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
                    Patch.ReadAnalog = true;
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
            Patch.ReadAnalog = false;
        }

        public void DeviceEvent(DeviceEventType type, DeviceInfo info)
        {
            if (type == DeviceEventType.Connected)
            {
                WootingAnalogSDK.DeviceEvent -= DeviceEvent;
                Patch.ReadAnalog = true;
                MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin: Keyboard detected");
            }
        }

        public void Update() { }

        public void Dispose()
        {
            WootingAnalogSDK.UnInitialise();
        }
    }

    [HarmonyPatch(typeof(MyControl), "GetAnalogState")]
    internal static class Patch
    {
        public static bool ReadAnalog { set; get; }

        private static readonly MyStringId[] m_analogControls = new MyStringId[]
        {
            MyControlsSpace.FORWARD,
            MyControlsSpace.BACKWARD,
            MyControlsSpace.STRAFE_LEFT,
            MyControlsSpace.STRAFE_RIGHT,
            MyControlsSpace.ROLL_LEFT,
            MyControlsSpace.ROLL_RIGHT,
            MyControlsSpace.JUMP,
            MyControlsSpace.CROUCH,
            MyControlsSpace.ROTATION_DOWN,
            MyControlsSpace.ROTATION_LEFT,
            MyControlsSpace.ROTATION_RIGHT,
            MyControlsSpace.ROTATION_UP,
        };

        private static float Postfix(float returnValue, ref MyControl __instance)
        {
            if (ReadAnalog && m_analogControls.Contains(__instance.GetGameControlEnum()))
            {
                float key1Val = GetAnalogValue(__instance.GetKeyboardControl());
                float key2Val = GetAnalogValue(__instance.GetSecondKeyboardControl());

                return key1Val > key2Val ? key1Val : key2Val;
            }
            return returnValue;
        }

        private static float GetAnalogValue(MyKeys key)
        {
            if (key != MyKeys.None)
            {
                var (value, result) = WootingAnalogSDK.ReadAnalog((byte)key);

                if (result != WootingAnalogResult.Ok)
                {
                    MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: Failed to read key {key} from WootingAnalogSDK: {result}");
                    if (result == WootingAnalogResult.NoDevices)
                    {
                        WootingPlugin.Instance.DeviceFailed();
                    }
                }
                else
                {
                    return value;
                }
            }
            return 0f;
        }
    }
}
