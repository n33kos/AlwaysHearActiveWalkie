using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAlwaysHearWalkieMod.Patches;
using UnityEngine.Scripting;

namespace BepInEx5.PluginTemplate
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCAlwaysHearWalkieMod : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private const string modGUID = "suskitech.LCAlwaysHearActiveWalkie";
        private const string modName = "LC Always Hear Active Walkies";
        private const string modVersion = "1.4.3";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static LCAlwaysHearWalkieMod Instance;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            
            Log.LogInfo("\\ /");
            Log.LogInfo("/|\\");
            Log.LogInfo(" |----|");
            Log.LogInfo(" |[__]| Always Hear Active Walkies");
           Log.LogInfo($" |.  .| Version {modVersion} Loaded");
            Log.LogInfo(" |____|");

            harmony.PatchAll(typeof(LCAlwaysHearWalkieMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(WalkieTalkiePatch));
        }
    }
}
