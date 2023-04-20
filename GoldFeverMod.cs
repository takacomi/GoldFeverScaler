using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.UI;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace GoldFeverScaler
{
    public static class ModInfo
    {
        public const string Name = "Gold Fever Scaler";
        public const string Description = "Sets the Gold Fever Scaling to 1";
        public const string Author = "Takacomic";
        public const string Company = "Disappointment";
        public const string Version = "1.0.0";
        public const string Download = "";
    }
    public class ConfigData
    {
        public bool Enabled { get; set; }
    }

    public class GoldFeverMod : MelonMod
    {
        static readonly string configFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs", "com", "takacomic");
        static readonly string filePath = Path.Combine(configFolder, "GoldScale.json");

        static readonly string enabledKey = "Enabled";
        static bool enabled;

        static void UpdateDebug(bool value) => UpdateEnabled(value);
        static bool scaleSettingAdded = false;
        static System.Action<bool> scaleSettingChanged = UpdateDebug;

        public override void OnInitializeMelon()
        {
            ValidateConfig();
        }

        [HarmonyPatch(typeof(GoldFeverUIManager), "OnEnable")]
        public class GoldFeverUIManagerOnEnable_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(GoldFeverUIManager __instance)
            {
                if (!enabled) return true;
                __instance.IntroTween();
                return false;
            }
        }
        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.BuildGameplayPage))]
        static class PatchBuildGameplayPage
        {
            static void Postfix(OptionsController __instance)
            {
                if (!scaleSettingAdded) __instance.AddTickBox("GoldFeverScale", enabled, scaleSettingChanged, false);
                scaleSettingAdded = true;
            }
        }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.AddVisibleJoysticks))]
        static class PatchAddVisibleJoysticks { static void Postfix() => scaleSettingAdded = false; }
        private static void UpdateEnabled(bool value)
        {
            ModifyConfigValue(enabledKey, value);
            enabled = value;
        }

        private static void ValidateConfig()
        {
            try
            {
                if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
                if (!File.Exists(filePath)) File.WriteAllText(filePath, JsonConvert.SerializeObject(new ConfigData { }, Formatting.Indented));

                LoadConfig();
            }
            catch (System.Exception ex) { MelonLogger.Msg($"Error: {ex}"); }
        }

        private static void ModifyConfigValue<T>(string key, T value)
        {
            string file = File.ReadAllText(filePath);
            JObject json = JObject.Parse(file);

            if (!json.ContainsKey(key)) json.Add(key, JToken.FromObject(value));
            else
            {
                System.Type type = typeof(T);
                JToken newValue = JToken.FromObject(value);

                if (type == typeof(string)) json[key] = newValue.ToString();
                else if (type == typeof(int)) json[key] = newValue.ToObject<int>();
                else if (type == typeof(bool)) json[key] = newValue.ToObject<bool>();
                else { MelonLogger.Msg($"Unsupported type '{type.FullName}'"); return; }
            }

            string finalJson = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(filePath, finalJson);
        }

        private static void LoadConfig() => enabled = JObject.Parse(File.ReadAllText(filePath) ?? "{}").Value<bool>(enabledKey);
    }
}
