using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches
{
  [HarmonyPatch(typeof(PlayerControllerB))]
  internal class PlayerControllerBPatch
  {
    private static float AudibleDistance = 15f;


    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void alwaysHearWalkieTalkiesPatch(ref bool ___holdingWalkieTalkie, ref PlayerControllerB __instance)
    {
      if (__instance == null) return;

      
      bool isNearActiveWalkieTalkie = false;

      for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
      {
        float distance = Vector3.Distance(WalkieTalkie.allWalkieTalkies[i].transform.position, __instance.transform.position);

        if (WalkieTalkie.allWalkieTalkies[i].isBeingUsed && distance <= AudibleDistance)
        { 
          isNearActiveWalkieTalkie = true;
          break;
        }
      }

      ___holdingWalkieTalkie = isNearActiveWalkieTalkie;
    }
  }
}