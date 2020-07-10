using System;
using System.Linq;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Input;
using VRage.Input.Keyboard;
using VRage.Utils;
using VRage.Serialization;
using VRageMath;
using VRage.ModAPI;
using System.Text;
using Sandbox.Game;

namespace Garbius.WootingPlugin
{
    public class WootingInput : VRage.Input.IMyInput, VRage.ModAPI.IMyInput
    {
        private MyVRageInput m_VRageInput;

        private List<MyKeys> m_keys = new List<MyKeys>();
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


        public WootingInput(MyVRageInput VRageInput)
        {
            m_VRageInput = VRageInput;

            foreach (var controlEnum in m_bindings.Keys.ToList())
            {
                var control = VRageInput.GetGameControl(controlEnum);
                if (control == null)
                    continue;

                var axis = new WootingAxis(control.GetKeyboardControl());
                m_bindings[controlEnum] = axis;
            }
        }

        public bool Update(bool gameFocused)
        {
            if (gameFocused)
                foreach (WootingAxis control in m_bindings.Values)
                {
                    control.Update();
                }

            return m_VRageInput.Update(gameFocused);
        }

        public void UpdateStates()
        {
            foreach (WootingAxis control in m_bindings.Values)
            {
                control.Update();
            }
            m_VRageInput.UpdateStates();
        }

        public bool IsNewGameControlPressed(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_VRageInput.IsControlBlocked(controlEnum) && m_bindings[controlEnum].IsNewPressed();
            }
            return m_VRageInput.IsNewGameControlPressed(controlEnum);
        }

        public bool IsGameControlPressed(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_VRageInput.IsControlBlocked(controlEnum) && m_bindings[controlEnum].IsPressed();
            }
            return m_VRageInput.IsGameControlPressed(controlEnum);
        }

        public bool IsNewGameControlReleased(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_VRageInput.IsControlBlocked(controlEnum) && m_bindings[controlEnum].IsNewReleased();
            }
            return m_VRageInput.IsNewGameControlReleased(controlEnum);
        }

        public bool IsGameControlReleased(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_VRageInput.IsControlBlocked(controlEnum) && !m_bindings[controlEnum].IsPressed();
            }
            return m_VRageInput.IsGameControlReleased(controlEnum);
        }

        public float GetGameControlAnalogState(MyStringId controlEnum)
        {
            if (m_bindings.ContainsKey(controlEnum))
            {
                return !m_VRageInput.IsControlBlocked(controlEnum) ? m_bindings[controlEnum].AnalogValue() : 0f;
            }
            return m_VRageInput.GetGameControlAnalogState(controlEnum);
        }

        public void SaveControls(
            SerializableDictionary<string, string> controlsGeneral,
            SerializableDictionary<string, SerializableDictionary<string, string>> controlsButtons)
        {
            foreach (var controlEnum in m_bindings.Keys)
            {
                var control = m_VRageInput.GetGameControl(controlEnum);
                if (control == null)
                    continue;

                m_bindings[controlEnum].Code = (byte)control.GetKeyboardControl();
            }

            m_VRageInput.SaveControls(controlsGeneral, controlsButtons);
        }

        // Big block of MyVRageInput redirects below. Nothing to see here
        #region VRage.Input.IMyInput
        public string JoystickInstanceName { get; set; }
        public void SearchForJoystick() => m_VRageInput.SearchForJoystick();
        public void LoadData(
          SerializableDictionary<string, string> controlsGeneral,
          SerializableDictionary<string, SerializableDictionary<string, string>> controlsButtons) =>
            m_VRageInput.LoadData(controlsGeneral, controlsButtons);
        public bool LoadControls(
          SerializableDictionary<string, string> controlsGeneral,
          SerializableDictionary<string, SerializableDictionary<string, string>> controlsButtons) =>
            m_VRageInput.LoadControls(controlsGeneral, controlsButtons);
        public void LoadContent() => m_VRageInput.LoadContent();
        public ListReader<char> TextInput => m_VRageInput.TextInput;
        public bool OverrideUpdate
        {
            get { return m_VRageInput.OverrideUpdate; }
            set { m_VRageInput.OverrideUpdate = value; }
        }
        public MyMouseState ActualMouseState => m_VRageInput.ActualMouseState;
        public MyJoystickState ActualJoystickState => m_VRageInput.ActualJoystickState;
        public void UnloadData() => m_VRageInput.UnloadData();
        public List<string> EnumerateJoystickNames() => m_VRageInput.EnumerateJoystickNames();
        public void UpdateJoystickChanged() => m_VRageInput.UpdateJoystickChanged();
        public void UpdateStatesFromPlayback(
          MyKeyboardState currentKeyboard,
          MyKeyboardState previousKeyboard,
          MyMouseState currentMouse,
          MyMouseState previousMouse,
          MyJoystickState currentJoystick,
          MyJoystickState previousJoystick,
          int x,
          int y,
          List<char> keyboardSnapshotText) =>
            m_VRageInput.UpdateStatesFromPlayback(
              currentKeyboard,
              previousKeyboard,
              currentMouse,
              previousMouse,
              currentJoystick,
              previousJoystick,
              x,
              y,
              keyboardSnapshotText);
        public void ClearStates() => m_VRageInput.ClearStates();
        public void SetControlBlock(MyStringId controlEnum, bool block = false) => m_VRageInput.SetControlBlock(controlEnum, block);
        public bool IsControlBlocked(MyStringId controlEnum) => m_VRageInput.IsControlBlocked(controlEnum);
        public void ClearBlacklist() => m_VRageInput.ClearBlacklist();
        public bool IsAnyKeyPress() => m_VRageInput.IsAnyKeyPress();
        public bool IsAnyMousePressed() => m_VRageInput.IsAnyMousePressed();
        public bool IsAnyNewMousePressed() => m_VRageInput.IsAnyNewMousePressed();
        public bool IsAnyShiftKeyPressed() => m_VRageInput.IsAnyShiftKeyPressed();
        public bool IsAnyAltKeyPressed() => m_VRageInput.IsAnyAltKeyPressed();
        public bool IsAnyCtrlKeyPressed() => m_VRageInput.IsAnyCtrlKeyPressed();
        public void GetPressedKeys(List<MyKeys> keys) => m_VRageInput.GetPressedKeys(keys);
        public bool IsKeyPress(MyKeys key) => m_VRageInput.IsKeyPress(key);
        public bool WasKeyPress(MyKeys key) => m_VRageInput.WasKeyPress(key);
        public bool IsNewKeyPressed(MyKeys key) => m_VRageInput.IsNewKeyPressed(key);
        public bool IsNewKeyReleased(MyKeys key) => m_VRageInput.IsNewKeyReleased(key);
        public bool IsMousePressed(MyMouseButtonsEnum button) => m_VRageInput.IsMousePressed(button);
        public bool IsMouseReleased(MyMouseButtonsEnum button) => m_VRageInput.IsMouseReleased(button);
        public bool IsNewMousePressed(MyMouseButtonsEnum button) => m_VRageInput.IsNewMousePressed(button);
        public bool IsNewLeftMousePressed() => m_VRageInput.IsNewLeftMousePressed();
        public bool IsNewLeftMouseReleased() => m_VRageInput.IsNewLeftMouseReleased();
        public bool IsLeftMousePressed() => m_VRageInput.IsLeftMousePressed();
        public bool IsLeftMouseReleased() => m_VRageInput.IsLeftMouseReleased();
        public bool IsRightMousePressed() => m_VRageInput.IsRightMousePressed();
        public bool IsNewRightMousePressed() => m_VRageInput.IsNewRightMousePressed();
        public bool IsNewRightMouseReleased() => m_VRageInput.IsNewRightMouseReleased();
        public bool WasRightMousePressed() => m_VRageInput.WasRightMousePressed();
        public bool WasRightMouseReleased() => m_VRageInput.WasRightMouseReleased();
        public bool IsMiddleMousePressed() => m_VRageInput.IsMiddleMousePressed();
        public bool IsNewMiddleMousePressed() => m_VRageInput.IsNewMiddleMousePressed();
        public bool IsNewMiddleMouseReleased() => m_VRageInput.IsNewMiddleMouseReleased();
        public bool WasMiddleMousePressed() => m_VRageInput.WasMiddleMousePressed();
        public bool WasMiddleMouseReleased() => m_VRageInput.WasMiddleMouseReleased();
        public bool IsXButton1MousePressed() => m_VRageInput.IsXButton1MousePressed();
        public bool IsNewXButton1MousePressed() => m_VRageInput.IsNewXButton1MousePressed();
        public bool IsNewXButton1MouseReleased() => m_VRageInput.IsNewXButton1MouseReleased();
        public bool WasXButton1MousePressed() => m_VRageInput.WasXButton1MousePressed();
        public bool WasXButton1MouseReleased() => m_VRageInput.WasXButton1MouseReleased();
        public bool IsXButton2MousePressed() => m_VRageInput.IsXButton2MousePressed();
        public bool IsNewXButton2MousePressed() => m_VRageInput.IsNewXButton2MousePressed();
        public bool IsNewXButton2MouseReleased() => m_VRageInput.IsNewXButton2MouseReleased();
        public bool WasXButton2MousePressed() => m_VRageInput.WasXButton2MousePressed();
        public bool WasXButton2MouseReleased() => m_VRageInput.WasXButton2MouseReleased();
        public bool IsJoystickButtonPressed(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonPressed(button);
        public bool IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonNewPressed(button);
        public bool IsJoystickButtonNewReleased(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonNewReleased(button);
        public float GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => m_VRageInput.GetJoystickAxisStateForGameplay(axis);
        public Vector3 GetJoystickPositionForGameplay(RequestedJoystickAxis requestedAxis = RequestedJoystickAxis.All) => m_VRageInput.GetJoystickPositionForGameplay(requestedAxis);
        public Vector3 GetJoystickRotationForGameplay(RequestedJoystickAxis requestedAxis = RequestedJoystickAxis.All) => m_VRageInput.GetJoystickRotationForGameplay(requestedAxis);
        public MyControl GetControl(MyKeys key) => m_VRageInput.GetControl(key);
        public bool IsJoystickAxisPressed(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisPressed(axis);
        public bool IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisNewPressed(axis);
        public bool IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => m_VRageInput.IsNewJoystickAxisReleased(axis);
        public float GetJoystickSensitivity() => m_VRageInput.GetJoystickSensitivity();
        public void SetJoystickSensitivity(float newSensitivity) => m_VRageInput.SetJoystickSensitivity(newSensitivity);
        public float GetJoystickExponent() => m_VRageInput.GetJoystickExponent();
        public void SetJoystickExponent(float newExponent) => m_VRageInput.SetJoystickExponent(newExponent);
        public float GetJoystickDeadzone() => m_VRageInput.GetJoystickDeadzone();
        public void SetJoystickDeadzone(float newDeadzone) => m_VRageInput.SetJoystickDeadzone(newDeadzone);
        public bool IsAnyMouseOrJoystickPressed() => m_VRageInput.IsAnyMouseOrJoystickPressed();
        public bool IsAnyNewMouseOrJoystickPressed() => m_VRageInput.IsAnyNewMouseOrJoystickPressed();
        public bool IsNewPrimaryButtonPressed() => m_VRageInput.IsNewPrimaryButtonPressed();
        public bool IsNewSecondaryButtonPressed() => m_VRageInput.IsNewSecondaryButtonPressed();
        public bool IsNewPrimaryButtonReleased() => m_VRageInput.IsNewPrimaryButtonReleased();
        public bool IsNewSecondaryButtonReleased() => m_VRageInput.IsNewSecondaryButtonReleased();
        public bool IsPrimaryButtonReleased() => m_VRageInput.IsPrimaryButtonReleased();
        public bool IsSecondaryButtonReleased() => m_VRageInput.IsSecondaryButtonReleased();
        public bool IsPrimaryButtonPressed() => m_VRageInput.IsPrimaryButtonPressed();
        public bool IsSecondaryButtonPressed() => m_VRageInput.IsSecondaryButtonPressed();
        public bool IsNewButtonPressed(MySharedButtonsEnum button) => m_VRageInput.IsNewButtonPressed(button);
        public bool IsButtonPressed(MySharedButtonsEnum button) => m_VRageInput.IsButtonPressed(button);
        public bool IsNewButtonReleased(MySharedButtonsEnum button) => m_VRageInput.IsNewButtonReleased(button);
        public bool IsButtonReleased(MySharedButtonsEnum button) => m_VRageInput.IsButtonReleased(button);
        public int MouseScrollWheelValue() => m_VRageInput.MouseScrollWheelValue();
        public int PreviousMouseScrollWheelValue() => m_VRageInput.PreviousMouseScrollWheelValue();
        public int DeltaMouseScrollWheelValue() => m_VRageInput.DeltaMouseScrollWheelValue();
        public bool IsEnabled() => m_VRageInput.IsEnabled();
        public void EnableInput(bool enable) => m_VRageInput.EnableInput(enable);
        public int GetMouseXForGamePlay() => m_VRageInput.GetMouseXForGamePlay();
        public int GetMouseYForGamePlay() => m_VRageInput.GetMouseYForGamePlay();
        public float GetMouseXForGamePlayF() => m_VRageInput.GetMouseXForGamePlayF();
        public float GetMouseYForGamePlayF() => m_VRageInput.GetMouseYForGamePlayF();
        public int GetMouseX() => m_VRageInput.GetMouseX();
        public int GetMouseY() => m_VRageInput.GetMouseY();
        public bool GetMouseXInversion() => m_VRageInput.GetMouseXInversion();
        public bool GetMouseYInversion() => m_VRageInput.GetMouseYInversion();
        public bool GetMouseScrollBlockSelectionInversion() => m_VRageInput.GetMouseScrollBlockSelectionInversion();
        public void SetMouseXInversion(bool inverted) => m_VRageInput.SetMouseXInversion(inverted);
        public void SetMouseYInversion(bool inverted) => m_VRageInput.SetMouseYInversion(inverted);
        public void SetMouseScrollBlockSelectionInversion(bool inverted) => m_VRageInput.SetMouseScrollBlockSelectionInversion(inverted);
        public bool GetJoystickYInversionCharacter() => m_VRageInput.GetJoystickYInversionCharacter();
        public void SetJoystickYInversionCharacter(bool inverted) => m_VRageInput.SetJoystickYInversionCharacter(inverted);
        public bool GetJoystickYInversionVehicle() => m_VRageInput.GetJoystickYInversionVehicle();
        public void SetJoystickYInversionVehicle(bool inverted) => m_VRageInput.SetJoystickYInversionVehicle(inverted);
        public float GetMouseSensitivity() => m_VRageInput.GetMouseSensitivity();
        public void SetMouseSensitivity(float sensitivity) => m_VRageInput.SetMouseSensitivity(sensitivity);
        public Vector2 GetMousePosition() => m_VRageInput.GetMousePosition();
        public Vector2 GetMouseAreaSize() => m_VRageInput.GetMouseAreaSize();
        public void SetMousePosition(int x, int y) => m_VRageInput.SetMousePosition(x, y);
        public bool IsKeyValid(MyKeys key) => m_VRageInput.IsKeyValid(key);
        public bool IsKeyDigit(MyKeys key) => m_VRageInput.IsKeyDigit(key);
        public bool IsMouseButtonValid(MyMouseButtonsEnum button) => m_VRageInput.IsMouseButtonValid(button);
        public bool IsJoystickButtonValid(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonValid(button);
        public bool IsJoystickAxisValid(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisValid(axis);
        public bool IsJoystickConnected() => m_VRageInput.IsJoystickConnected();
        public bool JoystickAsMouse
        {
            get { return m_VRageInput.JoystickAsMouse; }
            set { m_VRageInput.JoystickAsMouse = value; }
        }
        public bool IsJoystickLastUsed
        {
            get { return m_VRageInput.IsJoystickLastUsed; }
            set { m_VRageInput.IsJoystickLastUsed = value; }
        }
        public event Action<bool> JoystickConnected
        {
            add { m_VRageInput.JoystickConnected += value; }
            remove { m_VRageInput.JoystickConnected -= value; }
        }
        public MyControl GetControl(MyMouseButtonsEnum button) => m_VRageInput.GetControl(button);
        public void GetListOfPressedKeys(List<MyKeys> keys) => m_VRageInput.GetListOfPressedKeys(keys);
        public void GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result) => m_VRageInput.GetListOfPressedMouseButtons(result);
        public DictionaryValuesReader<MyStringId, MyControl> GetGameControlsList() => m_VRageInput.GetGameControlsList();
        public void TakeSnapshot() => m_VRageInput.TakeSnapshot();
        public void RevertChanges() => m_VRageInput.RevertChanges();
        public MyControl GetGameControl(MyStringId controlEnum) => m_VRageInput.GetGameControl(controlEnum);
        public void RevertToDefaultControls() => m_VRageInput.RevertToDefaultControls();
        public void AddDefaultControl(MyStringId stringId, MyControl control) => m_VRageInput.AddDefaultControl(stringId, control);
        public bool ENABLE_DEVELOPER_KEYS => m_VRageInput.ENABLE_DEVELOPER_KEYS;
        public bool IsDirectInputInitialized => m_VRageInput.IsDirectInputInitialized;
        public string GetKeyName(MyKeys key) => m_VRageInput.GetKeyName(key);
        public string GetName(MyMouseButtonsEnum mouseButton) => m_VRageInput.GetName(mouseButton);
        public string GetName(MyJoystickButtonsEnum joystickButton) => m_VRageInput.GetName(joystickButton);
        public string GetName(MyJoystickAxesEnum joystickAxis) => m_VRageInput.GetName(joystickAxis);
        public string GetUnassignedName() => m_VRageInput.GetUnassignedName();
        public bool IsGamepadKeyRightPressed() => m_VRageInput.IsGamepadKeyRightPressed();
        public bool IsGamepadKeyLeftPressed() => m_VRageInput.IsGamepadKeyLeftPressed();
        public bool IsNewGamepadKeyDownPressed() => m_VRageInput.IsNewGamepadKeyDownPressed();
        public bool IsNewGamepadKeyUpPressed() => m_VRageInput.IsNewGamepadKeyUpPressed();
        public void GetActualJoystickState(StringBuilder text) => m_VRageInput.GetActualJoystickState(text);
        public bool IsNewGameControlJoystickOnlyPressed(MyStringId controlId) => m_VRageInput.IsNewGameControlJoystickOnlyPressed(controlId);
        public void DeviceChangeCallback() => m_VRageInput.DeviceChangeCallback();
        public void NegateEscapePress() => m_VRageInput.NegateEscapePress();
        #endregion

        #region VRage.ModAPI.IMyInput
        string VRage.ModAPI.IMyInput.JoystickInstanceName => m_VRageInput.JoystickInstanceName;
        ListReader<char> VRage.ModAPI.IMyInput.TextInput => m_VRageInput.TextInput;
        List<string> VRage.ModAPI.IMyInput.EnumerateJoystickNames() => m_VRageInput.EnumerateJoystickNames();
        bool VRage.ModAPI.IMyInput.IsAnyKeyPress() => m_VRageInput.IsAnyKeyPress();
        bool VRage.ModAPI.IMyInput.IsAnyMousePressed() => m_VRageInput.IsAnyMousePressed();
        bool VRage.ModAPI.IMyInput.IsAnyNewMousePressed() => m_VRageInput.IsAnyNewMousePressed();
        bool VRage.ModAPI.IMyInput.IsAnyShiftKeyPressed() => m_VRageInput.IsAnyShiftKeyPressed();
        bool VRage.ModAPI.IMyInput.IsAnyAltKeyPressed() => m_VRageInput.IsAnyAltKeyPressed();
        bool VRage.ModAPI.IMyInput.IsAnyCtrlKeyPressed() => m_VRageInput.IsAnyCtrlKeyPressed();
        void VRage.ModAPI.IMyInput.GetPressedKeys(List<MyKeys> keys) => m_VRageInput.GetPressedKeys(keys);
        bool VRage.ModAPI.IMyInput.IsKeyPress(MyKeys key) => m_VRageInput.IsKeyPress(key);
        bool VRage.ModAPI.IMyInput.WasKeyPress(MyKeys key) => m_VRageInput.WasKeyPress(key);
        bool VRage.ModAPI.IMyInput.IsNewKeyPressed(MyKeys key) => m_VRageInput.IsNewKeyPressed(key);
        bool VRage.ModAPI.IMyInput.IsNewKeyReleased(MyKeys key) => m_VRageInput.IsNewKeyReleased(key);
        bool VRage.ModAPI.IMyInput.IsMousePressed(MyMouseButtonsEnum button) => m_VRageInput.IsMousePressed(button);
        bool VRage.ModAPI.IMyInput.IsMouseReleased(MyMouseButtonsEnum button) => m_VRageInput.IsMouseReleased(button);
        bool VRage.ModAPI.IMyInput.IsNewMousePressed(MyMouseButtonsEnum button) => m_VRageInput.IsNewMousePressed(button);
        bool VRage.ModAPI.IMyInput.IsNewLeftMousePressed() => m_VRageInput.IsNewLeftMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewLeftMouseReleased() => m_VRageInput.IsNewLeftMouseReleased();
        bool VRage.ModAPI.IMyInput.IsLeftMousePressed() => m_VRageInput.IsLeftMousePressed();
        bool VRage.ModAPI.IMyInput.IsLeftMouseReleased() => m_VRageInput.IsLeftMouseReleased();
        bool VRage.ModAPI.IMyInput.IsRightMousePressed() => m_VRageInput.IsRightMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewRightMousePressed() => m_VRageInput.IsNewRightMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewRightMouseReleased() => m_VRageInput.IsNewRightMouseReleased();
        bool VRage.ModAPI.IMyInput.WasRightMousePressed() => m_VRageInput.WasRightMousePressed();
        bool VRage.ModAPI.IMyInput.WasRightMouseReleased() => m_VRageInput.WasRightMouseReleased();
        bool VRage.ModAPI.IMyInput.IsMiddleMousePressed() => m_VRageInput.IsMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewMiddleMousePressed() => m_VRageInput.IsNewMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.IsNewMiddleMouseReleased() => m_VRageInput.IsNewMiddleMouseReleased();
        bool VRage.ModAPI.IMyInput.WasMiddleMousePressed() => m_VRageInput.WasMiddleMousePressed();
        bool VRage.ModAPI.IMyInput.WasMiddleMouseReleased() => m_VRageInput.WasMiddleMouseReleased();
        bool VRage.ModAPI.IMyInput.IsXButton1MousePressed() => m_VRageInput.IsXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton1MousePressed() => m_VRageInput.IsNewXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton1MouseReleased() => m_VRageInput.IsNewXButton1MouseReleased();
        bool VRage.ModAPI.IMyInput.WasXButton1MousePressed() => m_VRageInput.WasXButton1MousePressed();
        bool VRage.ModAPI.IMyInput.WasXButton1MouseReleased() => m_VRageInput.WasXButton1MouseReleased();
        bool VRage.ModAPI.IMyInput.IsXButton2MousePressed() => m_VRageInput.IsXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton2MousePressed() => m_VRageInput.IsNewXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.IsNewXButton2MouseReleased() => m_VRageInput.IsNewXButton2MouseReleased();
        bool VRage.ModAPI.IMyInput.WasXButton2MousePressed() => m_VRageInput.WasXButton2MousePressed();
        bool VRage.ModAPI.IMyInput.WasXButton2MouseReleased() => m_VRageInput.WasXButton2MouseReleased();
        bool VRage.ModAPI.IMyInput.IsJoystickButtonPressed(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonNewPressed(button);
        bool VRage.ModAPI.IMyInput.IsNewJoystickButtonReleased(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonNewReleased(button);
        float VRage.ModAPI.IMyInput.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => m_VRageInput.GetJoystickAxisStateForGameplay(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisPressed(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisPressed(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisNewPressed(axis);
        bool VRage.ModAPI.IMyInput.IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => m_VRageInput.IsNewJoystickAxisReleased(axis);
        bool VRage.ModAPI.IMyInput.IsAnyMouseOrJoystickPressed() => m_VRageInput.IsAnyMouseOrJoystickPressed();
        bool VRage.ModAPI.IMyInput.IsAnyNewMouseOrJoystickPressed() => m_VRageInput.IsAnyNewMouseOrJoystickPressed();
        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonPressed() => m_VRageInput.IsNewPrimaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonPressed() => m_VRageInput.IsNewSecondaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonReleased() => m_VRageInput.IsNewPrimaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonReleased() => m_VRageInput.IsNewSecondaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsPrimaryButtonReleased() => m_VRageInput.IsPrimaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsSecondaryButtonReleased() => m_VRageInput.IsSecondaryButtonReleased();
        bool VRage.ModAPI.IMyInput.IsPrimaryButtonPressed() => m_VRageInput.IsPrimaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsSecondaryButtonPressed() => m_VRageInput.IsSecondaryButtonPressed();
        bool VRage.ModAPI.IMyInput.IsNewButtonPressed(MySharedButtonsEnum button) => m_VRageInput.IsNewButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsButtonPressed(MySharedButtonsEnum button) => m_VRageInput.IsButtonPressed(button);
        bool VRage.ModAPI.IMyInput.IsNewButtonReleased(MySharedButtonsEnum button) => m_VRageInput.IsNewButtonReleased(button);
        bool VRage.ModAPI.IMyInput.IsButtonReleased(MySharedButtonsEnum button) => m_VRageInput.IsButtonReleased(button);
        int VRage.ModAPI.IMyInput.MouseScrollWheelValue() => m_VRageInput.MouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.PreviousMouseScrollWheelValue() => m_VRageInput.PreviousMouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.DeltaMouseScrollWheelValue() => m_VRageInput.DeltaMouseScrollWheelValue();
        int VRage.ModAPI.IMyInput.GetMouseXForGamePlay() => m_VRageInput.GetMouseXForGamePlay();
        int VRage.ModAPI.IMyInput.GetMouseYForGamePlay() => m_VRageInput.GetMouseYForGamePlay();
        int VRage.ModAPI.IMyInput.GetMouseX() => m_VRageInput.GetMouseX();
        int VRage.ModAPI.IMyInput.GetMouseY() => m_VRageInput.GetMouseY();
        bool VRage.ModAPI.IMyInput.GetMouseXInversion() => m_VRageInput.GetMouseXInversion();
        bool VRage.ModAPI.IMyInput.GetMouseYInversion() => m_VRageInput.GetMouseYInversion();
        float VRage.ModAPI.IMyInput.GetMouseSensitivity() => m_VRageInput.GetMouseSensitivity();
        Vector2 VRage.ModAPI.IMyInput.GetMousePosition() => m_VRageInput.GetMousePosition();
        Vector2 VRage.ModAPI.IMyInput.GetMouseAreaSize() => m_VRageInput.GetMouseAreaSize();
        bool VRage.ModAPI.IMyInput.IsNewGameControlPressed(MyStringId controlEnum) => IsNewGameControlPressed(controlEnum); // We're using this
        bool VRage.ModAPI.IMyInput.IsGameControlPressed(MyStringId controlEnum) => IsGameControlPressed(controlEnum); // ... and this ...
        bool VRage.ModAPI.IMyInput.IsNewGameControlReleased(MyStringId controlEnum) => IsNewGameControlReleased(controlEnum); // ... and this ...
        float VRage.ModAPI.IMyInput.GetGameControlAnalogState(MyStringId controlEnum) => GetGameControlAnalogState(controlEnum); // ... and this ...
        bool VRage.ModAPI.IMyInput.IsGameControlReleased(MyStringId controlEnum) => IsGameControlReleased(controlEnum); // ... and also this
        bool VRage.ModAPI.IMyInput.IsKeyValid(MyKeys key) => m_VRageInput.IsKeyValid(key);
        bool VRage.ModAPI.IMyInput.IsKeyDigit(MyKeys key) => m_VRageInput.IsKeyDigit(key);
        bool VRage.ModAPI.IMyInput.IsMouseButtonValid(MyMouseButtonsEnum button) => m_VRageInput.IsMouseButtonValid(button);
        bool VRage.ModAPI.IMyInput.IsJoystickButtonValid(MyJoystickButtonsEnum button) => m_VRageInput.IsJoystickButtonValid(button);
        bool VRage.ModAPI.IMyInput.IsJoystickAxisValid(MyJoystickAxesEnum axis) => m_VRageInput.IsJoystickAxisValid(axis);
        bool VRage.ModAPI.IMyInput.IsJoystickConnected() => m_VRageInput.IsJoystickConnected();
        bool VRage.ModAPI.IMyInput.JoystickAsMouse => m_VRageInput.JoystickAsMouse;
        bool VRage.ModAPI.IMyInput.IsJoystickLastUsed => m_VRageInput.IsJoystickLastUsed;
        event Action<bool> VRage.ModAPI.IMyInput.JoystickConnected
        {
            add { m_VRageInput.JoystickConnected += value; }
            remove { m_VRageInput.JoystickConnected -= value; }
        }
        IMyControl VRage.ModAPI.IMyInput.GetControl(MyKeys key) => m_VRageInput.GetControl(key);
        IMyControl VRage.ModAPI.IMyInput.GetControl(MyMouseButtonsEnum button) => m_VRageInput.GetControl(button);
        void VRage.ModAPI.IMyInput.GetListOfPressedKeys(List<MyKeys> keys) => m_VRageInput.GetListOfPressedKeys(keys);
        void VRage.ModAPI.IMyInput.GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result) => m_VRageInput.GetListOfPressedMouseButtons(result);
        IMyControl VRage.ModAPI.IMyInput.GetGameControl(MyStringId controlEnum) => m_VRageInput.GetGameControl(controlEnum);
        string VRage.ModAPI.IMyInput.GetKeyName(MyKeys key) => m_VRageInput.GetKeyName(key);
        string VRage.ModAPI.IMyInput.GetName(MyMouseButtonsEnum mouseButton) => m_VRageInput.GetName(mouseButton);
        string VRage.ModAPI.IMyInput.GetName(MyJoystickButtonsEnum joystickButton) => m_VRageInput.GetName(joystickButton);
        string VRage.ModAPI.IMyInput.GetName(MyJoystickAxesEnum joystickAxis) => m_VRageInput.GetName(joystickAxis);
        string VRage.ModAPI.IMyInput.GetUnassignedName() => m_VRageInput.GetUnassignedName();
        #endregion
    }
}