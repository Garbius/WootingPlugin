using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRage;
using VRage.Input;
using VRage.ModAPI;
using VRage.Plugins;
using VRage.Utils;
using WootingAnalogSDKNET;

namespace Garbius.WootingPlugin
{
    public class WootingPlugin : IPlugin
    {
        public static WootingPlugin Static { get; set; }

        private bool m_initialized = false;

        private class WootingAxis : MyControllerHelper.IControl
        {
            private readonly MyKeys keycode;

            private float m_value = 0f;
            private bool m_pressed = false;
            private bool m_newpressed = false;
            private bool m_newreleased = false;

            public WootingAxis(MyKeys key)
            {
                keycode = key;
            }

            internal void Update()
            {
                (float value, WootingAnalogResult result) = WootingAnalogSDK.ReadAnalog((byte)keycode);
                if (result != WootingAnalogResult.Ok || !MyVRage.Platform.Window.IsActive)
                    value = 0f;

                m_value = value;

                bool pressed = value > 0f;
                m_newpressed = pressed && !m_pressed;
                m_newreleased = !pressed && m_pressed;
                m_pressed = pressed;
            }


            public Func<bool> Condition
            {
                get;
                private set;
            }

            public float AnalogValue() => m_value;
            public byte Code => (byte)keycode;
            public bool IsNewPressed() => m_newpressed;
            public bool IsNewPressedRepeating() => false;
            public bool IsPressed() => m_pressed;
            public bool IsNewReleased() => m_newreleased;
            public object ControlCode() => keycode.ToString().Substring(6);
        }

        private Dictionary<MyStringId, WootingAxis> m_bindings = new Dictionary<MyStringId, WootingAxis>(MyStringId.Comparer)
        {
            { MyControlsSpace.FORWARD, null },
            { MyControlsSpace.BACKWARD, null },
            { MyControlsSpace.STRAFE_LEFT, null },
            { MyControlsSpace.STRAFE_RIGHT, null },
            { MyControlsSpace.ROLL_LEFT, null },
            { MyControlsSpace.ROLL_RIGHT, null },
            { MyControlsSpace.JUMP, null },
            { MyControlsSpace.CROUCH, null },
        };

        public void Init(object gameInstance = null)
        {
            if (Static == null)
                Static = this;
            else
                return;

            Parallel.Start(BackgroundInitializer, InitCallback);

            IDictionary bindings = (IDictionary)typeof(MyControllerHelper).GetField("m_bindings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (bindings == null)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "Failed to get private bindings dictionary from MyControllerHelper");
                return;
            }

            PropertyInfo context = typeof(MyControllerHelper).GetNestedType("Context", BindingFlags.NonPublic | BindingFlags.Instance).GetProperty("Item");
            if (context == null)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "Failed to get private Context type from MyControllerHelper");
                return;
            }

            object cx_character = bindings[MySpaceBindingCreator.CX_CHARACTER];
            object cx_jetpack = bindings[MySpaceBindingCreator.CX_JETPACK];
            object cx_spaceship = bindings[MySpaceBindingCreator.CX_SPACESHIP];

            foreach (var key in m_bindings.Keys.ToList())
            {
                var control = MyInput.Static.GetGameControl(key);
                if (control == null)
                    continue;

                var axis = new WootingAxis(control.GetKeyboardControl());
                m_bindings[key] = axis;
                MyInput.Static.SetControlBlock(key, true);

                context.SetValue(cx_character, axis, new object[] { key });
                context.SetValue(cx_jetpack, axis, new object[] { key });
                context.SetValue(cx_spaceship, axis, new object[] { key });
            }
        }

        private void BackgroundInitializer()
        {
            WootingAnalogSDK.Initialise();

            if (!WootingAnalogSDK.IsInitialised)
                System.Threading.Thread.Sleep(500);

            if (!WootingAnalogSDK.IsInitialised)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "Failed to initialize WootingAnalogSDK");
                return;
            }
            WootingAnalogSDK.SetKeycodeMode(KeycodeType.VirtualKey);

            while (Sandbox.ModAPI.MyAPIGateway.Input == null)
                System.Threading.Thread.Sleep(10);
        }

        private void InitCallback()
        {
            if (!WootingAnalogSDK.IsInitialised)
                return;

            Sandbox.ModAPI.MyAPIGateway.Input = new WootingInput();

            m_initialized = true;
            MyLogExtensions.Info(MySandboxGame.Log, "WootingPlugin initialized successfully");
        }

        public bool IsNewGameControlPressed(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return m_bindings[controlEnum].IsNewPressed();
            }
            return false;
        }

        public bool IsGameControlPressed(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return m_bindings[controlEnum].IsPressed();
            }
            return false;
        }

        public bool IsNewGameControlReleased(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return m_bindings[controlEnum].IsNewReleased();
            }
            return false;
        }

        public bool IsGameControlReleased(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_bindings[controlEnum].IsPressed();
            }
            return false;
        }

        public float GetGameControlAnalogState(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return m_bindings[controlEnum].AnalogValue();
            }
            return 0f;
        }

        public void Update()
        {
            if (!m_initialized)
                return;

            foreach (WootingAxis control in m_bindings.Values)
            {
                control.Update();
            }
        }

        public void Dispose()
        {
            WootingAnalogSDK.UnInitialise();
            Static = null;
        }
    }

    public class WootingInput : VRage.ModAPI.IMyInput
    {
        bool VRage.ModAPI.IMyInput.IsNewGameControlPressed(MyStringId controlEnum)
        {
            return MyInput.Static.IsNewGameControlPressed(controlEnum) || WootingPlugin.Static.IsNewGameControlPressed(controlEnum);
        }

        bool VRage.ModAPI.IMyInput.IsGameControlPressed(MyStringId controlEnum)
        {
            return MyInput.Static.IsGameControlPressed(controlEnum) || WootingPlugin.Static.IsGameControlPressed(controlEnum);
        }

        bool VRage.ModAPI.IMyInput.IsNewGameControlReleased(MyStringId controlEnum)
        {
            return MyInput.Static.IsNewGameControlReleased(controlEnum) || WootingPlugin.Static.IsNewGameControlReleased(controlEnum);
        }

        bool VRage.ModAPI.IMyInput.IsGameControlReleased(MyStringId controlEnum)
        {
            return MyInput.Static.IsGameControlReleased(controlEnum) || WootingPlugin.Static.IsGameControlReleased(controlEnum);
        }

        float VRage.ModAPI.IMyInput.GetGameControlAnalogState(MyStringId controlEnum)
        {
            return Math.Max(MyInput.Static.GetGameControlAnalogState(controlEnum), WootingPlugin.Static.GetGameControlAnalogState(controlEnum));
        }


        // Big block of MyVRageInput redirects below. Nothing to see here
        event Action<bool> VRage.ModAPI.IMyInput.JoystickConnected
        {
            add { MyInput.Static.JoystickConnected += value; }
            remove { MyInput.Static.JoystickConnected -= value; }
        }
        string VRage.ModAPI.IMyInput.JoystickInstanceName => MyInput.Static.JoystickInstanceName;
        VRage.Collections.ListReader<char> VRage.ModAPI.IMyInput.TextInput => MyInput.Static.TextInput;
        List<string> VRage.ModAPI.IMyInput.EnumerateJoystickNames() => MyInput.Static.EnumerateJoystickNames();
        bool VRage.ModAPI.IMyInput.IsAnyKeyPress() => MyInput.Static.IsAnyKeyPress();
        bool VRage.ModAPI.IMyInput.IsAnyMousePressed() => MyInput.Static.IsAnyMousePressed();
        bool VRage.ModAPI.IMyInput.IsAnyNewMousePressed() => MyInput.Static.IsAnyNewMousePressed();
        bool VRage.ModAPI.IMyInput.IsAnyShiftKeyPressed() => MyInput.Static.IsAnyShiftKeyPressed();
        bool VRage.ModAPI.IMyInput.IsAnyAltKeyPressed() => MyInput.Static.IsAnyAltKeyPressed();
        bool VRage.ModAPI.IMyInput.IsAnyCtrlKeyPressed() => MyInput.Static.IsAnyCtrlKeyPressed();
        void VRage.ModAPI.IMyInput.GetPressedKeys(List<MyKeys> keys) => MyInput.Static.GetPressedKeys(keys);
        bool VRage.ModAPI.IMyInput.IsKeyPress(MyKeys key) => MyInput.Static.IsKeyPress(key);
        bool VRage.ModAPI.IMyInput.WasKeyPress(MyKeys key) => MyInput.Static.WasKeyPress(key);
        bool VRage.ModAPI.IMyInput.IsNewKeyPressed(MyKeys key) => MyInput.Static.IsNewKeyPressed(key);
        bool VRage.ModAPI.IMyInput.IsNewKeyReleased(MyKeys key) => MyInput.Static.IsNewKeyReleased(key);
        bool VRage.ModAPI.IMyInput.IsMousePressed(MyMouseButtonsEnum button) => MyInput.Static.IsMousePressed(button);
        bool VRage.ModAPI.IMyInput.IsMouseReleased(MyMouseButtonsEnum button) => MyInput.Static.IsMouseReleased(button);
        bool VRage.ModAPI.IMyInput.IsNewMousePressed(MyMouseButtonsEnum button) => MyInput.Static.IsNewMousePressed(button);
        bool VRage.ModAPI.IMyInput.IsNewLeftMousePressed() => MyInput.Static.IsNewLeftMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewLeftMouseReleased() => MyInput.Static.IsNewLeftMouseReleased();
        bool VRage.ModAPI.IMyInput.IsLeftMousePressed() => MyInput.Static.IsLeftMousePressed();
        bool VRage.ModAPI.IMyInput.IsLeftMouseReleased() => MyInput.Static.IsLeftMouseReleased();
        bool VRage.ModAPI.IMyInput.IsRightMousePressed() => MyInput.Static.IsRightMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewRightMousePressed() => MyInput.Static.IsNewRightMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewRightMouseReleased() => MyInput.Static.IsNewRightMouseReleased();
        bool VRage.ModAPI.IMyInput.WasRightMousePressed() => MyInput.Static.WasRightMousePressed();
        bool VRage.ModAPI.IMyInput.WasRightMouseReleased() => MyInput.Static.WasRightMouseReleased();
        bool VRage.ModAPI.IMyInput.IsMiddleMousePressed() => MyInput.Static.IsMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewMiddleMousePressed() => MyInput.Static.IsNewMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewMiddleMouseReleased() => MyInput.Static.IsNewMiddleMouseReleased();
        bool VRage.ModAPI.IMyInput.WasMiddleMousePressed() => MyInput.Static.WasMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.WasMiddleMouseReleased() => MyInput.Static.WasMiddleMouseReleased();
        bool VRage.ModAPI.IMyInput.IsXButton1MousePressed() => MyInput.Static.IsXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton1MousePressed() => MyInput.Static.IsNewXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton1MouseReleased() => MyInput.Static.IsNewXButton1MouseReleased();
        bool VRage.ModAPI.IMyInput.WasXButton1MousePressed() => MyInput.Static.WasXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.WasXButton1MouseReleased() => MyInput.Static.WasXButton1MouseReleased();
        bool VRage.ModAPI.IMyInput.IsXButton2MousePressed() => MyInput.Static.IsXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton2MousePressed() => MyInput.Static.IsNewXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton2MouseReleased() => MyInput.Static.IsNewXButton2MouseReleased();
        bool VRage.ModAPI.IMyInput.WasXButton2MousePressed() => MyInput.Static.WasXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.WasXButton2MouseReleased() => MyInput.Static.WasXButton2MouseReleased();
        bool VRage.ModAPI.IMyInput.IsJoystickButtonPressed(MyJoystickButtonsEnum button) => MyInput.Static.IsJoystickButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => MyInput.Static.IsJoystickButtonNewPressed(button);
        bool VRage.ModAPI.IMyInput.IsNewJoystickButtonReleased(MyJoystickButtonsEnum button) => MyInput.Static.IsNewJoystickButtonReleased(button);
        float VRage.ModAPI.IMyInput.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => MyInput.Static.GetJoystickAxisStateForGameplay(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisPressed(MyJoystickAxesEnum axis) => MyInput.Static.IsJoystickAxisPressed(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => MyInput.Static.IsJoystickAxisNewPressed(axis);
        bool VRage.ModAPI.IMyInput.IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => MyInput.Static.IsNewJoystickAxisReleased(axis);
        bool VRage.ModAPI.IMyInput.IsAnyMouseOrJoystickPressed() => MyInput.Static.IsAnyMouseOrJoystickPressed();
        bool VRage.ModAPI.IMyInput.IsAnyNewMouseOrJoystickPressed() => MyInput.Static.IsAnyNewMouseOrJoystickPressed();
        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonPressed() => MyInput.Static.IsNewPrimaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonPressed() => MyInput.Static.IsNewSecondaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonReleased() => MyInput.Static.IsNewPrimaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonReleased() => MyInput.Static.IsNewSecondaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsPrimaryButtonReleased() => MyInput.Static.IsPrimaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsSecondaryButtonReleased() => MyInput.Static.IsSecondaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsPrimaryButtonPressed() => MyInput.Static.IsPrimaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsSecondaryButtonPressed() => MyInput.Static.IsSecondaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewButtonPressed(MySharedButtonsEnum button) => MyInput.Static.IsNewButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsButtonPressed(MySharedButtonsEnum button) => MyInput.Static.IsButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsNewButtonReleased(MySharedButtonsEnum button) => MyInput.Static.IsNewButtonReleased(button);
        bool VRage.ModAPI.IMyInput.IsButtonReleased(MySharedButtonsEnum button) => MyInput.Static.IsButtonReleased(button);
        int VRage.ModAPI.IMyInput.MouseScrollWheelValue() => MyInput.Static.MouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.PreviousMouseScrollWheelValue() => MyInput.Static.PreviousMouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.DeltaMouseScrollWheelValue() => MyInput.Static.DeltaMouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.GetMouseXForGamePlay() => MyInput.Static.GetMouseXForGamePlay();
        int VRage.ModAPI.IMyInput.GetMouseYForGamePlay() => MyInput.Static.GetMouseYForGamePlay();
        int VRage.ModAPI.IMyInput.GetMouseX() => MyInput.Static.GetMouseX();
        int VRage.ModAPI.IMyInput.GetMouseY() => MyInput.Static.GetMouseY();
        bool VRage.ModAPI.IMyInput.GetMouseXInversion() => MyInput.Static.GetMouseXInversion();
        bool VRage.ModAPI.IMyInput.GetMouseYInversion() => MyInput.Static.GetMouseYInversion();
        float VRage.ModAPI.IMyInput.GetMouseSensitivity() => MyInput.Static.GetMouseSensitivity();
        VRageMath.Vector2 VRage.ModAPI.IMyInput.GetMousePosition() => MyInput.Static.GetMousePosition();
        VRageMath.Vector2 VRage.ModAPI.IMyInput.GetMouseAreaSize() => MyInput.Static.GetMouseAreaSize();
        bool VRage.ModAPI.IMyInput.IsKeyValid(MyKeys key) => MyInput.Static.IsKeyValid(key);
        bool VRage.ModAPI.IMyInput.IsKeyDigit(MyKeys key) => MyInput.Static.IsKeyDigit(key);
        bool VRage.ModAPI.IMyInput.IsMouseButtonValid(MyMouseButtonsEnum button) => MyInput.Static.IsMouseButtonValid(button);
        bool VRage.ModAPI.IMyInput.IsJoystickButtonValid(MyJoystickButtonsEnum button) => MyInput.Static.IsJoystickButtonValid(button);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisValid(MyJoystickAxesEnum axis) => MyInput.Static.IsJoystickAxisValid(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickConnected() => MyInput.Static.IsJoystickConnected();
        bool VRage.ModAPI.IMyInput.JoystickAsMouse => MyInput.Static.JoystickAsMouse;
        bool VRage.ModAPI.IMyInput.IsJoystickLastUsed => MyInput.Static.IsJoystickLastUsed;
        IMyControl VRage.ModAPI.IMyInput.GetControl(MyKeys key) => (IMyControl)MyInput.Static.GetControl(key);
        IMyControl VRage.ModAPI.IMyInput.GetControl(MyMouseButtonsEnum button) => (IMyControl)MyInput.Static.GetControl(button);
        void VRage.ModAPI.IMyInput.GetListOfPressedKeys(List<MyKeys> keys) => MyInput.Static.GetListOfPressedKeys(keys);
        void VRage.ModAPI.IMyInput.GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result) => MyInput.Static.GetListOfPressedMouseButtons(result);
        IMyControl VRage.ModAPI.IMyInput.GetGameControl(MyStringId controlEnum) => (IMyControl)MyInput.Static.GetGameControl(controlEnum);
        string VRage.ModAPI.IMyInput.GetKeyName(MyKeys key) => MyInput.Static.GetKeyName(key);
        string VRage.ModAPI.IMyInput.GetName(MyMouseButtonsEnum mouseButton) => MyInput.Static.GetName(mouseButton);
        string VRage.ModAPI.IMyInput.GetName(MyJoystickButtonsEnum joystickButton) => MyInput.Static.GetName(joystickButton);
        string VRage.ModAPI.IMyInput.GetName(MyJoystickAxesEnum joystickAxis) => MyInput.Static.GetName(joystickAxis);
        string VRage.ModAPI.IMyInput.GetUnassignedName() => MyInput.Static.GetUnassignedName();
    }
}
