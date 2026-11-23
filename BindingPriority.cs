using System.Collections.Generic;
using System.Reflection;
using Sandbox;
using VRage.Input;
using VRage.ModAPI;
using VRage.Utils;

namespace WootingPlugin
{
    /// <summary>
    /// Decides whether a key bound to a control counts for that control right now, the
    /// same way the game does for a pressed key.
    ///
    /// A key can be bound more than once: W to move forward and Alt+W to rotate a block.
    /// While Alt is held the game gives the key to the binding with the modifier and the
    /// plain one stays off. Which combinations do that depends only on the key bindings,
    /// so Shift+W keeps moving as long as nothing is bound to Shift+W.
    ///
    /// The game applies these rules to keys that are pressed. The analog value is read
    /// before a key reaches its actuation point, so the rules are applied here to keys
    /// that are merely touched.
    /// </summary>
    internal static class BindingPriority
    {
        // For every key the controls bound to it, the ones that win first
        private static readonly FieldInfo priorityMapField = typeof(MyVRageInput).GetField("m_keysPriorityMap", BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool priorityMapMissing;

        /// <summary>
        /// Returns true if the primary or secondary key of the control may drive it:
        /// the modifiers of its own binding are held and no other binding of the same
        /// key takes precedence.
        /// </summary>
        public static bool IsKeyUsable(MyVRageInput input, MyControl control, bool primaryKey)
        {
            MyKeys key = primaryKey ? control.GetKeyboardControl() : control.GetSecondKeyboardControl();
            if (key == MyKeys.None)
                return false;

            MyKeyboardModifiers modifiers = primaryKey ? control.GetKeyboardModifier() : control.GetSecondKeyboardModifier();
            if (modifiers != MyKeyboardModifiers.None && !AreModifiersHeld(control, key, modifiers))
                return false;

            Dictionary<MyKeys, List<MyControl>> priorityMap = GetPriorityMap(input);
            if (priorityMap == null)
            {
                // The game's own check only sees a key once it is fully pressed
                return !input.IsPriorityKeyPressed(control, primaryKey);
            }

            return !priorityMap.TryGetValue(key, out List<MyControl> controls) || !IsOverridden(controls, control, key);
        }

        /// <summary>
        /// Mirrors MyVRageInput.IsPriorityKeyPressed, without asking for the key to be
        /// pressed. The list holds the controls bound to the key in the game's order,
        /// only the ones in front of the control can take the key from it.
        /// </summary>
        internal static bool IsOverridden(List<MyControl> controls, MyControl control, MyKeys key)
        {
            foreach (MyControl other in controls)
            {
                if (other == control)
                    break;

                if (!other.IsEnabled)
                    continue;

                bool active = other.GetKeyboardControl() == key
                    && other.GetKeyboardModifier() != MyKeyboardModifiers.None
                    && AreModifiersHeld(other, key, other.GetKeyboardModifier());

                if (!active && other.GetSecondKeyboardControl() == key && other.GetSecondKeyboardModifier() != MyKeyboardModifiers.None)
                    active = AreModifiersHeld(other, key, other.GetSecondKeyboardModifier());

                if (active)
                    return true;
            }

            return false;
        }

        // The game ignores the modifiers of a binding whose key is a modifier key itself
        private static bool AreModifiersHeld(MyControl control, MyKeys key, MyKeyboardModifiers modifiers)
        {
            return IsModifierKey(key) || control.IsModifierPressed(modifiers);
        }

        private static bool IsModifierKey(MyKeys key)
        {
            switch (key)
            {
                case MyKeys.Shift:
                case MyKeys.LeftShift:
                case MyKeys.RightShift:
                case MyKeys.Control:
                case MyKeys.LeftControl:
                case MyKeys.RightControl:
                case MyKeys.Alt:
                case MyKeys.LeftAlt:
                case MyKeys.RightAlt:
                    return true;
                default:
                    return false;
            }
        }

        private static Dictionary<MyKeys, List<MyControl>> GetPriorityMap(MyVRageInput input)
        {
            if (priorityMapMissing)
                return null;

            if (priorityMapField?.GetValue(input) is Dictionary<MyKeys, List<MyControl>> priorityMap)
                return priorityMap;

            priorityMapMissing = true;
            MyLogExtensions.Error(MySandboxGame.Log, "WootingPlugin: Cannot read the game's key binding priorities. A key that is also bound with a modifier keeps its analog value until it is fully pressed.");
            return null;
        }
    }
}
