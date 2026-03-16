using System;
using Sandbox;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Utils;
using VRageMath;
using WootingPlugin;

namespace WootingPlugin.Gui
{
    public class MyGuiScreenPluginConfig : MyGuiScreenBase
    {
        private const float Space = 0.01f;
        private MyGuiControlParent _contentPanel;
        
        private WootingPluginSettings Settings => WootingPluginSettings.I;

        public MyGuiScreenPluginConfig() : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.6f, 0.8f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            EnabledBackgroundFade = true;
            CloseButtonEnabled = true;
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenModConfig";
        }

        public override void LoadContent()
        {
            base.LoadContent();
            WootingPluginSettings.Load();
            RecreateControls(true);
        }
        
        protected override void OnClosed()
        {
            Settings.Save();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            
            _contentPanel = new MyGuiControlParent()
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = Vector2.Zero,
                Size = new Vector2(0.51f, 0.6f),
            };

            var scrollPanel = new MyGuiControlScrollablePanel(_contentPanel)
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                Position = new Vector2(0f, 0.00f),
                Size = new Vector2(0.55f, 0.6f),
                ScrollbarVEnabled = true,
                CanFocusChildren = true,
                ScrolledAreaPadding = new MyGuiBorderThickness(0.005f),
            };
            Controls.Add(scrollPanel);
            
            
            
            MyGuiControlLabel caption = AddCaption("Wooting Plugin Settings");
            Vector2 pos = new Vector2(_contentPanel.Size.X / 2 - Space * 2, -_contentPanel.Size.Y / 2);
            pos.Y += Space;
            
            pos = AddTextbox(pos, Settings.PitchSensitivityMultiplier.ToString(), "Pitch Sensitivity Multiplier", 0.1m, 10, tb => SetPitchSensitivityMultiplier(tb));
            pos = AddTextbox(pos, Settings.YawSensitivityMultiplier.ToString(), "Yaw Sensitivity Multiplier", 0.1m, 10, tb => SetYawSensitivityMultiplier(tb));
            pos = AddTextbox(pos, Settings.RollSensitivityMultiplier.ToString(), "Roll Sensitivity Multiplier", 0.1m, 10, tb => SetRollSensitivityMultiplier(tb));
            
            
            Vector2 closeButtonPos = new Vector2(0, (m_size.Value.Y / 2) - Space);
            MyGuiControlButton closeButton = new MyGuiControlButton(closeButtonPos, text: MyTexts.Get(MyCommonTexts.Close), originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, onButtonClick: OnCloseClicked);
            Controls.Add(closeButton);
        }
        
        
        private void AddCaption(MyGuiControlBase control, string caption, bool offsetWidth = false)
        {

            var pos = new Vector2(-_contentPanel.Size.X / 2 + Space * 2, control.PositionY);
            
            _contentPanel.Controls.Add(new MyGuiControlLabel(pos, text: caption, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP));
        }
        
        private Vector2 AddTextbox(Vector2 pos, string text, string label, decimal minValue, decimal maxValue, Action<MyGuiControlTextbox> onChanged)
        {
            var textbox = new MyGuiControlTextbox(pos, text, 5, type: MyGuiControlTextboxType.DigitsOnly, minNumericValue: minValue, maxNumericValue: maxValue);
            textbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            textbox.TextChanged += onChanged;
            textbox.Size = new Vector2(0.1f, textbox.Size.Y);
            _contentPanel.Controls.Add(textbox);
            AddCaption(textbox, label, true);
            return pos + new Vector2(0, textbox.Size.Y + Space);
        }
        
        
        
        // These could be condensed into one function. As is they violate DRY. But really it's an unnecessary abstraction until - and if - we add more controls.
        private void SetPitchSensitivityMultiplier(MyGuiControlTextbox tb)
        {
            if (!float.TryParse(tb.Text, out var multiplier)) return;
            
            Settings.PitchSensitivityMultiplier = multiplier;
        }
        
        private void SetYawSensitivityMultiplier(MyGuiControlTextbox tb)
        {
            if (!float.TryParse(tb.Text, out var multiplier)) return;
            Settings.YawSensitivityMultiplier = multiplier;
        }
        
        private void SetRollSensitivityMultiplier(MyGuiControlTextbox tb)
        {
            if (!float.TryParse(tb.Text, out var multiplier)) return;
            Settings.RollSensitivityMultiplier = multiplier;
        }
        
        private void OnCloseClicked(MyGuiControlButton btn)
        {
            CloseScreen();
        }
    }
}