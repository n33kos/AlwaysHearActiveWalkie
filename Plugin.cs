using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAlwaysHearWalkieMod.Patches;

namespace BepInEx5.PluginTemplate
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCAlwaysHearWalkieMod : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private const string modGUID = "n33kos.LCAlwaysHearWalkie";
        private const string modName = "LC Always Hear Walkie";
        private const string modVersion = "1.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static LCAlwaysHearWalkieMod Instance;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            harmony.PatchAll(typeof(LCAlwaysHearWalkieMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
