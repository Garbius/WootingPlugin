using Sandbox;
using System.Runtime.InteropServices;
using VRage.Input;
using VRage.Plugins;
using VRage.Utils;
using WootingAnalogSDKNET;

namespace Garbius.WootingPlugin
{
    public class WootingPlugin : IPlugin
    {
        private IMyInput m_input;

        public void Init(object gameInstance = null)
        {
            MyLogExtensions.Info(MySandboxGame.Log, "Initializing Wooting SDK");
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
            }
            catch (SEHException)
            {
                MyLogExtensions.Error(MySandboxGame.Log, "WootingAnalogSDK failed to initialize due to an internal error");
                return;
            }

            MyLogExtensions.Info(MySandboxGame.Log, "Setting key code mode");
            WootingAnalogSDK.SetKeycodeMode(KeycodeType.VirtualKey);

            MyLogExtensions.Info(MySandboxGame.Log, "Replacing MyInput.Static with WootingInput");
            var vrage = MyInput.Static;
            MyInput.UnloadData();
            MyInput.Initialize(m_input ??= new WootingInput(vrage as MyVRageInput));
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            WootingAnalogSDK.UnInitialise();
        }
    }
}
