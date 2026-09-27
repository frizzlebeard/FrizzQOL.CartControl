using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CartControl
{
    internal static class CartCatalog
    {
        internal sealed class CartConfig
        {
            public int VanillaWidth;
            public int VanillaHeight;
            public float VanillaCartWeight;
            public float VanillaCargoPull;
            public ConfigEntry<int> Width;
            public ConfigEntry<int> Height;
            public ConfigEntry<float> CartWeight;
            public ConfigEntry<float> CargoWeight;
            public ConfigEntry<float> CargoPull;
        }

        private static ConfigFile _config;
        private static readonly Dictionary<string, CartConfig> Entries = new Dictionary<string, CartConfig>();
        private static readonly Dictionary<Inventory, string> Inventories = new Dictionary<Inventory, string>();

        public static void Initialize(ConfigFile config)
        {
            _config = config;
        }

        public static void DiscoverFromScene(ZNetScene scene)
        {
            if (_config == null || scene == null)
            {
                return;
            }

            List<GameObject> prefabs = AccessTools.Field(typeof(ZNetScene), "m_prefabs")?.GetValue(scene) as List<GameObject>;
            if (prefabs == null)
            {
                return;
            }

            int added = 0;
            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null || prefab.GetComponent<Vagon>() == null)
                {
                    continue;
                }

                if (Bind(prefab))
                {
                    added++;
                }
            }

            if (added > 0)
            {
                Plugin.LogInfo("Bound config for " + added + " carts.");
            }
        }

        public static CartConfig BindFromVagon(Vagon cart)
        {
            if (cart == null)
            {
                return null;
            }

            Bind(cart.gameObject);
            return Get(PrefabName(cart.gameObject));
        }

        public static CartConfig Get(string prefabName)
        {
            CartConfig config;
            return prefabName != null && Entries.TryGetValue(prefabName, out config) ? config : null;
        }

        public static void RegisterInventory(Inventory inventory, string prefabName)
        {
            if (inventory == null || string.IsNullOrEmpty(prefabName))
            {
                return;
            }

            Inventories[inventory] = prefabName;
        }

        public static CartConfig GetForInventory(Inventory inventory)
        {
            string name;
            return inventory != null && Inventories.TryGetValue(inventory, out name) ? Get(name) : null;
        }

        public static string PrefabName(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            string name = obj.name;
            int clone = name.IndexOf("(Clone)");
            return clone >= 0 ? name.Substring(0, clone).Trim() : name;
        }

        private static bool Bind(GameObject cartObj)
        {
            string name = PrefabName(cartObj);
            if (string.IsNullOrEmpty(name) || _config == null || Entries.ContainsKey(name))
            {
                return false;
            }

            Vagon cart = cartObj.GetComponent<Vagon>();
            Container chest = cart != null ? cart.m_container : null;
            if (chest == null)
            {
                chest = cartObj.GetComponentInChildren<Container>(true);
            }

            int width = chest != null ? ReadInt(chest, "m_width", 6) : 6;
            int height = chest != null ? ReadInt(chest, "m_height", 3) : 3;
            float mass = cart != null ? cart.m_baseMass : 20f;
            float pull = cart != null ? cart.m_itemWeightMassFactor : 1f;

            Entries[name] = new CartConfig
            {
                VanillaWidth = width,
                VanillaHeight = height,
                VanillaCartWeight = mass,
                VanillaCargoPull = pull,
                Width = _config.Bind(name, "Width", width, "Storage columns. 1 to 8. 0 or less keeps the cart's normal width."),
                Height = _config.Bind(name, "Height", height, "Storage rows. 0 or less keeps the cart's normal height."),
                CartWeight = _config.Bind(name, "CartWeight", mass, "Pull weight of the empty cart. 0 is weightless. Below 0 keeps the normal empty weight."),
                CargoWeight = _config.Bind(name, "CargoWeight", 0f, "Most item weight this cart will hold. 0 or less means no storage limit. This does not change how heavy the cart feels to pull."),
                CargoPull = _config.Bind(name, "CargoPull", 0.25f, "How much cargo weight counts while pulling. 1 is normal Valheim. 0.25 makes a full cart about one quarter as heavy and easier to steer. 0 means cargo adds no pull weight. Below 0 keeps the cart's normal pull.")
            };
            return true;
        }

        private static int ReadInt(object instance, string fieldName, int fallback)
        {
            FieldInfo field = AccessTools.Field(instance.GetType(), fieldName);
            object value = field != null ? field.GetValue(instance) : null;
            return value is int number ? number : fallback;
        }
    }
}
