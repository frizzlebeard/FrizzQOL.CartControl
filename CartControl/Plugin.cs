using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace CartControl
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.frizzqol.cartcontrol";
        public const string PluginName = "FrizzQOL Cart Control";
        public const string PluginVersion = "0.4.3";

        internal static Plugin Instance { get; private set; }

        internal static ConfigEntry<bool> Invincible { get; private set; }

        internal static ConfigEntry<bool> HammerDeconstruct { get; private set; }

        private void Awake()
        {
            Instance = this;
            Invincible = Config.Bind("General", "Invincible", true, "On by default. Carts take no damage and do not wear down, so they cannot break. Set this to false to use normal cart breaking.");
            HammerDeconstruct = Config.Bind("General", "HammerDeconstruct", true, "On by default. Hammer middle-click empties a cart onto the ground, then removes the cart and returns its materials.");
            CartCatalog.Initialize(Config);
            Config.SettingChanged += (_, __) =>
            {
                CartApplier.ApplyAllLoaded();
                CartDeconstruct.ApplyAllLoaded(FeatureOn());
            };
            Harmony harmony = new Harmony(PluginGuid);
            try
            {
                harmony.PatchAll();
                Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Harmony patch failed: {ex.Message}");
            }
        }

        internal static bool FeatureOn()
        {
            return HammerDeconstruct != null && HammerDeconstruct.Value;
        }

        internal static void LogInfo(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogInfo(message);
            }
        }

        internal static void LogPull(Vagon cart, float empty, float cargoPull, float pullMass)
        {
            if (Instance == null || cart == null)
            {
                return;
            }

            string key = cart.GetInstanceID() + ":" + empty.ToString("0.##") + ":" + cargoPull.ToString("0.##") + ":" + pullMass.ToString("0.##");
            if (!_pullLogs.Add(key))
            {
                return;
            }

            Instance.Logger.LogInfo(CartCatalog.PrefabName(cart.gameObject) + " pull mass " + pullMass.ToString("0.##") + " (empty " + empty.ToString("0.##") + ", cargo factor " + cargoPull.ToString("0.##") + ").");
        }

        private static readonly HashSet<string> _pullLogs = new HashSet<string>();

        internal static void LogError(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogError(message);
            }
        }

        internal static void LogWarning(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogWarning(message);
            }
        }
    }
}
