using System.Collections.Generic;
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
      if (__instance != GameNetworkManager.Instance.localPlayerController) {
        return;
      }
      if (GameNetworkManager.Instance.localPlayerController.isPlayerDead) {
        return;
      }

      List<WalkieTalkie> walkieTalkiesInRange = new List<WalkieTalkie>();
      List<WalkieTalkie> walkieTalkiesOutOfRange = new List<WalkieTalkie>();
      for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
      {
        float distance = Vector3.Distance(WalkieTalkie.allWalkieTalkies[i].transform.position, __instance.transform.position);

        if (distance <= AudibleDistance)
        {
          if (WalkieTalkie.allWalkieTalkies[i].isBeingUsed) {
            walkieTalkiesInRange.Add(WalkieTalkie.allWalkieTalkies[i]);
          }
        } else {
          walkieTalkiesOutOfRange.Add(WalkieTalkie.allWalkieTalkies[i]);
        }
      }

      bool isAnyWalkieInRange = walkieTalkiesInRange.Count > 0;

      if (isAnyWalkieInRange != __instance.holdingWalkieTalkie) {
        ___holdingWalkieTalkie = isAnyWalkieInRange;
          
        for (int i = 0; i < walkieTalkiesOutOfRange.Count; i++)
        {
          walkieTalkiesInRange[i].thisAudio.Stop();
        }

        if (isAnyWalkieInRange) {
          StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }
      }
    }
  }
}