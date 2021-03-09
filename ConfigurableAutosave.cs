using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;

namespace ConfigurableAutosave
{
    [BepInPlugin("com.github.johndowson.ConfigurableAutosave", "ConfigurableAutosave", "1.1.0")]
    public class ConfigurableAutosave : BaseUnityPlugin
    {

        public static ConfigEntry<float> autosavePeriod;
        public static ConfigEntry<bool> disableAutosave;

        private static readonly Harmony harmony = new(typeof(ConfigurableAutosave).GetCustomAttributes(typeof(BepInPlugin), false)
            .Cast<BepInPlugin>()
            .First()
            .GUID);
#pragma warning disable IDE0051 // Remove unused private members
        private void Awake()
        {
            autosavePeriod = Config.Bind("General", "AutosaveInterval", 1200f, "Time between autosaves in seconds");
            disableAutosave = Config.Bind("General", "DisableAutosave", false, "Disable autosave");
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
