using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Game.Chat;
using Newtonsoft.Json;

namespace XIVLauncher.Settings
{
    static class DalamudSettings
    {
        public enum ChannelType
        {
            Guild,
            User
        }

        [Serializable]
        public class ChannelConfiguration
        {
            public ChannelType Type { get; set; }

            public ulong GuildId { get; set; }
            public ulong ChannelId { get; set; }
        }

        [Serializable]
        public class ChatTypeConfiguration
        {
            public XivChatType ChatType { get; set; }

            public ChannelConfiguration Channel { get; set; }
            public int Color { get; set; }
        }

        [Serializable]
        public class DiscordFeatureConfiguration
        {
            public string Token { get; set; }

            public bool CheckForDuplicateMessages { get; set; } = false;
            public int ChatDelayMs { get; set; } = 1000;

            public bool DisableEmbeds { get; set; } = false;

            public ulong OwnerUserId { get; set; } = 123830058426040321;

            public List<ChatTypeConfiguration> ChatTypeConfigurations { get; set; } = new List<ChatTypeConfiguration>();

            public ChannelConfiguration CfNotificationChannel { get; set; }
            public ChannelConfiguration CfPreferredRoleChannel { get; set; }
            public ChannelConfiguration RetainerNotificationChannel { get; set; }
        }

        [Serializable]
        public class DalamudConfiguration
        {
            public DiscordFeatureConfiguration DiscordFeatureConfig { get; set; } = new DiscordFeatureConfiguration();

            public bool OptOutMbCollection { get; set; } = false;

            public List<string> BadWords { get; set; } = new List<string>();

            public bool DutyFinderTaskbarFlash { get; set; } = true;
            public bool DutyFinderChatMessage { get; set; } = false;

            public string LanguageOverride { get; set; } = "zh";

            public string LastVersion { get; set; } = "5.1.1.1";

            public XivChatType GeneralChatType { get; set; } = XivChatType.Debug;

            public bool DoPluginTest { get; set; } = true;
            public bool DoDalamudTest { get; set; } = false;

            public float GlobalUiScale { get; set; } = 1.0f;
            public bool ToggleUiHide { get; set; } = true;
            public bool ToggleUiHideDuringCutscenes { get; set; } = true;
            public bool ToggleUiHideDuringGpose { get; set; } = true;

            [JsonIgnore]
            public string ConfigPath;
        }

        public static string configPath = Path.Combine(Paths.RoamingPath, "dalamudConfig.json");

        public static DalamudConfiguration GetSettings()
        {
            if (File.Exists(configPath))
                return JsonConvert.DeserializeObject<DalamudConfiguration>(File.ReadAllText(configPath));
            else {

                var config= new DalamudConfiguration();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config,Formatting.Indented));
                return config;
            }

        }
    }
}
