using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using Network;

namespace Oxide.Plugins
{
    [Info("Automated Workcart Notifications", "WhiteThunder", "0.1.0")]
    [Description("Notifies players via chat when Automated Workcarts stop nearby.")]
    internal class AutomatedWorkcartNotifications : CovalencePlugin
    {
        #region Fields

        [PluginReference]
        private Plugin AutomatedWorkcarts;

        private Configuration _pluginConfig;

        #endregion

        #region Hooks

        private void OnServerInitialized()
        {
            if (AutomatedWorkcarts == null)
            {
                LogError("AutomatedWorkcarts is not loaded, get it at http://umod.org.");
            }
        }

        #endregion

        #region Commands

        [Command("workcart.notify")]
        private void CommandWorkcartNotify(IPlayer player, string cmd, string[] args)
        {
            if (!player.IsServer)
                return;

            uint workcartId;
            if (args.Length < 3 || !uint.TryParse(args[0], out workcartId))
            {
                LogError($"Invalid syntax. Expected: {cmd} <workcart_id> <notification_name> <delay_seconds>");
                return;
            }

            var workcart = BaseNetworkable.serverEntities.Find(workcartId) as TrainEngine;
            if (workcart == null || workcart.IsDestroyed)
            {
                LogError($"No workcart found with ID: {workcartId}. Make sure you are using '$id' parameter.");
                return;
            }

            var chatConfigKey = args[1];

            ChatConfig chatConfig;
            if (!_pluginConfig.ChatNotifications.TryGetValue(chatConfigKey, out chatConfig))
            {
                LogError($"Invalid chat config: '{chatConfigKey}'");
                return;
            }

            float notificationDelay;
            if (!float.TryParse(args[2], out notificationDelay))
            {
                LogError($"Invalid delay: '{args[2]}'");
                return;
            }

            timer.Once(notificationDelay, () =>
            {
                if (workcart == null || workcart.IsDestroyed)
                    return;

                if (chatConfig.MaxSpeed != 0 && workcart.TrackSpeed > chatConfig.MaxSpeed)
                    return;

                var workcartPosition = workcart.transform.position;

                var networkGroup = Net.sv.visibility.GetGroup(workcartPosition);
                if (networkGroup == null)
                    return;

                var maxDistanceSquared = Math.Pow(chatConfig.MaxDistance, 2);

                foreach (var connection in networkGroup.subscribers)
                {
                    if (!connection.active)
                        continue;

                    var basePlayer = connection.player as BasePlayer;
                    if (basePlayer == null)
                        continue;

                    if (basePlayer.GetParentEntity() is TrainCar)
                        continue;

                    if (basePlayer.GetMounted() is TrainCar)
                        continue;

                    if (basePlayer.SqrDistance(workcartPosition) > maxDistanceSquared)
                        continue;

                    if (args.Length > 3)
                    {
                        ChatMessage(basePlayer, string.Join(",", SkipArgs(args, 3)));
                    }

                    var message = args.Length > 3
                        ? GetMessage(basePlayer.UserIDString, chatConfigKey, SkipArgs(args, 3))
                        : GetMessage(basePlayer.UserIDString, chatConfigKey);

                    ChatMessage(basePlayer, message, _pluginConfig.ChatSteamIdIcon);
                }
            });
        }

        #endregion

        #region Helper Methods

        private string[] SkipArgs(string[] args, int count)
        {
            var newArgs = new string[args.Length - count];
            for (var i = count; i < args.Length; i++)
            {
                newArgs[i - count] = args[i];
            }
            return newArgs;
        }

        private void ChatMessage(BasePlayer player, string message, ulong steamId = 0)
        {
            player.SendConsoleCommand("chat.add", 2, steamId, message);
        }

        #endregion

        #region Configuration

        private class ChatConfig
        {
            [JsonProperty("Max distance")]
            public float MaxDistance = 30;

            [JsonProperty("Max speed", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public float MaxSpeed;
        }

        private class Configuration : SerializableConfiguration
        {
            [JsonProperty("Chat SteamID icon")]
            public ulong ChatSteamIdIcon;

            [JsonProperty("Chat notifications")]
            public Dictionary<string, ChatConfig> ChatNotifications = new Dictionary<string, ChatConfig>
            {
                ["Arrived"] = new ChatConfig
                {
                    MaxDistance = 40,
                },
                ["DepartingSoon"] = new ChatConfig
                {
                    MaxDistance = 20,
                    MaxSpeed = 1,
                },
            };
        }

        private Configuration GetDefaultConfig() => new Configuration();

        #region Configuration Boilerplate

        private class SerializableConfiguration
        {
            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonHelper.Deserialize(ToJson()) as Dictionary<string, object>;
        }

        private static class JsonHelper
        {
            public static object Deserialize(string json) => ToObject(JToken.Parse(json));

            private static object ToObject(JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        return token.Children<JProperty>()
                                    .ToDictionary(prop => prop.Name,
                                                  prop => ToObject(prop.Value));

                    case JTokenType.Array:
                        return token.Select(ToObject).ToList();

                    default:
                        return ((JValue)token).Value;
                }
            }
        }

        private bool MaybeUpdateConfig(SerializableConfiguration config)
        {
            var currentWithDefaults = config.ToDictionary();
            var currentRaw = Config.ToDictionary(x => x.Key, x => x.Value);
            return MaybeUpdateConfigDict(currentWithDefaults, currentRaw);
        }

        private bool MaybeUpdateConfigDict(Dictionary<string, object> currentWithDefaults, Dictionary<string, object> currentRaw)
        {
            bool changed = false;

            foreach (var key in currentWithDefaults.Keys)
            {
                object currentRawValue;
                if (currentRaw.TryGetValue(key, out currentRawValue))
                {
                    var defaultDictValue = currentWithDefaults[key] as Dictionary<string, object>;
                    var currentDictValue = currentRawValue as Dictionary<string, object>;

                    if (defaultDictValue != null)
                    {
                        if (currentDictValue == null)
                        {
                            currentRaw[key] = currentWithDefaults[key];
                            changed = true;
                        }
                        else if (MaybeUpdateConfigDict(defaultDictValue, currentDictValue))
                            changed = true;
                    }
                }
                else
                {
                    currentRaw[key] = currentWithDefaults[key];
                    changed = true;
                }
            }

            return changed;
        }

        protected override void LoadDefaultConfig() => _pluginConfig = GetDefaultConfig();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _pluginConfig = Config.ReadObject<Configuration>();
                if (_pluginConfig == null)
                {
                    throw new JsonException();
                }

                if (MaybeUpdateConfig(_pluginConfig))
                {
                    LogWarning("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Log($"Configuration changes saved to {Name}.json");
            Config.WriteObject(_pluginConfig, true);
        }

        #endregion

        #endregion

        #region Localization

        private class LangEntry
        {
            public static List<LangEntry> AllLangEntries = new List<LangEntry>();

            public static readonly LangEntry Arrived = new LangEntry("Arrived", "All Aboard! A train has has arrived at the station.");
            public static readonly LangEntry DepartingSoon = new LangEntry("DepartingSoon", "All Aboard! The train will be departing soon.");

            public string Name;
            public string English;

            public LangEntry(string name, string english)
            {
                Name = name;
                English = english;

                AllLangEntries.Add(this);
            }
        }

        private string GetMessage(string playerId, string messageName) =>
            lang.GetMessage(messageName, this, playerId);

        private string GetMessage(string playerId, string messageName, params object[] args) =>
            string.Format(GetMessage(playerId, messageName), args);

        protected override void LoadDefaultMessages()
        {
            var englishLangKeys = new Dictionary<string, string>();

            foreach (var langEntry in LangEntry.AllLangEntries)
            {
                // Only add the default lang entries for valid config entries.
                if (_pluginConfig.ChatNotifications.ContainsKey(langEntry.Name))
                {
                    englishLangKeys[langEntry.Name] = langEntry.English;
                }
            }

            foreach (var chatConfigEntry in _pluginConfig.ChatNotifications)
            {
                // Only add placeholder lang entries for config entries that have no plugin defaults.
                englishLangKeys.TryAdd(chatConfigEntry.Key, chatConfigEntry.Key);
            }

            lang.RegisterMessages(englishLangKeys, this, "en");
        }

        #endregion
    }
}
