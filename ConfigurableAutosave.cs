using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ConfigurableAutosave
{
    [BepInPlugin("com.github.johndowson.ConfigurableAutosave", "ConfigurableAutosave", "1.0.0")]
    public class ConfigurableAutosave : BaseUnityPlugin
    {

        public static ConfigEntry<float> autosavePeriod;

        private static readonly Harmony harmony = new(typeof(ConfigurableAutosave).GetCustomAttributes(typeof(BepInPlugin), false)
            .Cast<BepInPlugin>()
            .First()
            .GUID);
#pragma warning disable IDE0051 // Remove unused private members
        private void Awake()
        {
            autosavePeriod = Config.Bind("General", "AutosaveInterval", 1200f, "Time between autosaves in seconds");
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Game), "UpdateSaving")]
        public static class Autosave_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_0 &&
                        codes[i + 1].opcode == OpCodes.Ldfld &&
                        codes[i + 2].opcode == OpCodes.Ldc_R4 &&
                        codes[i + 3].opcode == OpCodes.Ble_Un)
                    {
                        codes[i + 2].operand = autosavePeriod.Value;
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
