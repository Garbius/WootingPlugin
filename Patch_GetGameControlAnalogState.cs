using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Sandbox;
using Sandbox.Game;
using VRage.Input;
using VRage.Utils;
using WootingAnalogSDKNET;

namespace WootingPlugin
{
    /// <summary>
    /// Changed to patch MyVrageInput.GetGameControlAnalogState instead of MyControl.GetAnalogState (thanksKeen)
    /// </summary>
    [HarmonyPatch(typeof(MyVRageInput), "GetGameControlAnalogState")]
    internal static class Patch_GetGameControlAnalogState
    {
        public static bool ReadAnalog { get; set; }
        private static FieldInfo hasFocusField = typeof(MySandboxGame).GetField("hasFocus", BindingFlags.NonPublic | BindingFlags.Instance);

        private static WootingPluginSettings Settings => WootingPluginSettings.I;
        
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
        
        // Some default sensitivity multpliers (looking at you, roll) make less sense once you have analogue control over that axis
        private static readonly Dictionary<MyStringId, Func<float>> m_sensitivityMultipliers = new Dictionary<MyStringId, Func<float>>()
        {
            { MyControlsSpace.ROTATION_DOWN, () => Settings?.PitchSensitivityMultiplier ?? 1f },
            { MyControlsSpace.ROTATION_UP, () => Settings?.PitchSensitivityMultiplier ?? 1f },
    
            { MyControlsSpace.ROTATION_LEFT, () => Settings?.YawSensitivityMultiplier ?? 1f },
            { MyControlsSpace.ROTATION_RIGHT, () => Settings?.YawSensitivityMultiplier ?? 1f },
    
            { MyControlsSpace.ROLL_LEFT, () => Settings?.RollSensitivityMultiplier ?? 1f },
            { MyControlsSpace.ROLL_RIGHT, () => Settings?.RollSensitivityMultiplier ?? 1f },
        };
        static float Postfix(float returnValue, MyStringId controlId)
        {
            if (!ReadAnalog) return returnValue;
            if (!m_analogControls.Contains(controlId)) return returnValue;
            if (MySandboxGame.Static.PauseInput) return returnValue;
            
            // Check if the game has focus
            if (hasFocusField != null && MySandboxGame.Static != null)
            {
                bool hasFocus = (bool)hasFocusField.GetValue(MySandboxGame.Static);
                if (!hasFocus) return returnValue;
            }

 
            MyControl control = MyInput.Static.GetGameControl(controlId);
            if (control == null) return returnValue;

            float key1Val = GetAnalogValue(control.GetKeyboardControl());
            float key2Val = GetAnalogValue(control.GetSecondKeyboardControl());
            float analogVal = key1Val > key2Val ? key1Val : key2Val;

            if (m_sensitivityMultipliers.TryGetValue(controlId, out var multiplier)) analogVal *= multiplier();
            
            
            // Use whichever is greater: the Wooting analog value or the original game value.
            // This way analog always works, and non-Wooting inputs (gamepad, mouse) still function.
            return (analogVal != 0f) ? analogVal : returnValue;
        }

        private static float GetAnalogValue(MyKeys key)
        {
            if (key != MyKeys.None)
            {
                var (value, result) = WootingAnalogSDK.ReadAnalog((ushort)key);

                if (result != WootingAnalogResult.Ok)
                {
                    if (result == WootingAnalogResult.NoDevices)
                    {
                        Plugin.Instance.DeviceFailed();
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