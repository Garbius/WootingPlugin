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
        static float Postfix(float returnValue, MyVRageInput __instance, MyStringId controlId)
        {
            if (!ReadAnalog) return returnValue;
            if (!IsAnalogControl(controlId)) return returnValue;

            MySandboxGame game = MySandboxGame.Static;
            if (game == null || game.PauseInput) return returnValue;

            // Check if the game has focus
            if (hasFocusField != null)
            {
                bool hasFocus = (bool)hasFocusField.GetValue(game);
                if (!hasFocus) return returnValue;
            }

            // The game returns 0 for a control that it or a mod has switched off
            if (__instance.IsControlBlocked(controlId)) return returnValue;

            MyControl control = __instance.GetGameControl(controlId);
            if (control == null || !control.IsEnabled) return returnValue;

            float key1Val = GetAnalogValue(__instance, control, primaryKey: true);
            float key2Val = GetAnalogValue(__instance, control, primaryKey: false);
            float analogVal = key1Val > key2Val ? key1Val : key2Val;

            if (m_sensitivityMultipliers.TryGetValue(controlId, out var multiplier)) analogVal *= multiplier();


            // Use whichever is greater: the Wooting analog value or the original game value.
            // This way analog always works, and non-Wooting inputs (gamepad, mouse) still function.
            return (analogVal != 0f) ? analogVal : returnValue;
        }

        private static float GetAnalogValue(MyVRageInput input, MyControl control, bool primaryKey)
        {
            float value = GetAnalogValue(primaryKey ? control.GetKeyboardControl() : control.GetSecondKeyboardControl());
            if (value == 0f) return 0f;

            // The key may belong to another binding at the moment: with Alt+W bound to
            // rotating a block, W does not move forward while Alt is held
            return BindingPriority.IsKeyUsable(input, control, primaryKey) ? value : 0f;
        }

        // A plain loop on purpose: array.Contains() resolves to different extension methods
        // depending on the compiler version and the runtime the plugin is compiled for.
        private static bool IsAnalogControl(MyStringId controlId)
        {
            foreach (MyStringId analogControl in m_analogControls)
            {
                if (analogControl == controlId) return true;
            }
            return false;
        }

        private static float GetAnalogValue(MyKeys key)
        {
            // The game knows keys by their layout dependent virtual key code,
            // the keyboard reports analog values per physical key
            ushort code = key != MyKeys.None ? KeyTranslator.GetHidCode(key) : (ushort)0;

            if (code != 0)
            {
                var (value, result) = WootingAnalogSDK.ReadAnalog(code);

                if (result != WootingAnalogResult.Ok)
                {
                    if (result == WootingAnalogResult.NoDevices)
                    {
                        Plugin.Instance?.DeviceFailed();
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
