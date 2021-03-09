using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ConfigurableAutosave
{
    [HarmonyPatch(typeof(Game))]
    public static class GamePatches
    {
        [HarmonyPatch("UpdateSaving")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchAutosave(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (ConfigurableAutosave.disableAutosave.Value)
            {
                codes[0] = new CodeInstruction(OpCodes.Ret);
            }

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 2].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 3].opcode == OpCodes.Ble_Un)
                {
                    codes[i + 2].operand = ConfigurableAutosave.autosavePeriod.Value;
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPatch("UpdateSleeping")]
        [HarmonyPrefix]
        public static void SaveOnSleep(Game __instance)
        {
            if (ZNet.instance.IsServer() && !(__instance.m_sleeping || EnvMan.instance.IsTimeSkipping()))
            {
                if ((EnvMan.instance.IsAfternoon() || EnvMan.instance.IsNight()) && __instance.EverybodyIsTryingToSleep())
                {
                    __instance.m_saveTimer = 0f;
                    __instance.SavePlayerProfile(false);
                    if (ZNet.instance)
                    {
                        ZNet.instance.Save(false);
                    }
                }
            }
        }
    }
}
