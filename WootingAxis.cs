using System;
using VRage.Input;
using WootingAnalogSDKNET;

namespace Garbius.WootingPlugin
{
    public class WootingAxis : IMyControllerControl
    {
        public float AnalogValue() => m_value;
        public byte Code { get; internal set; }
        public bool IsNewPressed() => m_newpressed;
        public bool IsNewPressedRepeating() => false;
        public bool IsPressed() => m_pressed;
        public bool IsNewReleased() => m_newreleased;
        public object ControlCode() => Code.ToString().Substring(6);

        private float m_value = 0f;
        private bool m_pressed = false;
        private bool m_newpressed = false;
        private bool m_newreleased = false;

        public WootingAxis(MyKeys key)
        {
            Code = (byte)key;
        }

        internal void Update()
        {
            var (value, result) = WootingAnalogSDK.ReadAnalog(Code);
            if (result != WootingAnalogResult.Ok) // || !MyVRage.Platform.Windows.Window.IsActive)
                                                  //if (result != WootingAnalogResult.Ok || !MyVRage.Platform.Window.IsActive)
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
    }
}