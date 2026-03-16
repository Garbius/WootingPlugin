using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Sandbox;
using VRage.FileSystem;
using VRage.Input;
using VRage.Utils;
using VRageMath;


namespace WootingPlugin
{
    public class WootingPluginSettings
    {
        private const string fileName = "WootingPluginSettings.xml";
        private static string FilePath => Path.Combine(MyFileSystem.UserDataPath, "Storage", fileName);

        private static WootingPluginSettings _i;
        public static WootingPluginSettings I
        {
            get
            {
                if (_i == null) Load();
                return _i;
            }
            private set => _i = value;
        }

        public float PitchSensitivityMultiplier { get; set; } = 2f;
        public float YawSensitivityMultiplier { get; set; } = 2f;
        public float RollSensitivityMultiplier { get; set; } = 3f;
        
        public WootingPluginSettings()
        {
            _i = this;
        }
        
        
        
        public static void Load()
        {
            string file = FilePath;
            if (File.Exists(file))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(WootingPluginSettings));
                    using (XmlReader xml = XmlReader.Create(file))
                    { _i = (WootingPluginSettings)serializer.Deserialize(xml);
                    }
                }
                catch (Exception e)
                {
                    MyLogExtensions.Error(MySandboxGame.Log, $"Failed to load WootingPluginSettings from {file}: {e}");
                }
            }
            if (_i == null)
            {
                _i = new WootingPluginSettings();
                MyLogExtensions.Info(MySandboxGame.Log, $"WootingPluginSettings: No config file, using defaults");
                _i.Save();
            }
            
        }

        public void Save()
        {
            try
            {
                string file = FilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                XmlSerializer serializer = new XmlSerializer(typeof(WootingPluginSettings));
                using (StreamWriter stream = File.CreateText(file))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (Exception e)
            {
                MyLogExtensions.Error(MySandboxGame.Log, $"Failed to save WootingPluginSettings to {FilePath}: {e}");
            }
            MyLogExtensions.Info(MySandboxGame.Log, $"WootingPluginSettings saved to {FilePath}");
        }
    }
}