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

      for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
      {
        float distance = Vector3.Distance(WalkieTalkie.allWalkieTalkies[i].transform.position, __instance.transform.position);

        if (WalkieTalkie.allWalkieTalkies[i].isBeingUsed && distance <= AudibleDistance)
        {
          ___holdingWalkieTalkie = true;
          WalkieTalkie.allWalkieTalkies[i].thisAudio.Stop();
          StartOfRound.Instance.UpdatePlayerVoiceEffects();
          return;
        }
      }

      ___holdingWalkieTalkie = false;
    }
  }
}