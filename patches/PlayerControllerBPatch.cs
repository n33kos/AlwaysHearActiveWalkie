using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches
{
  [HarmonyPatch(typeof(PlayerControllerB))]
  internal class PlayerControllerBPatch
  {
    private static float AudibleDistance = 15f;

    // Adds a postfix to the PlayerIsHearingOthersThroughWalkieTalkie method which returns true if the
    // player is within range of a walkie talkie that is being used.
    [HarmonyPatch("PlayerIsHearingOthersThroughWalkieTalkie")]
    [HarmonyPostfix]
    static bool alwaysHearWalkieTalkiesPatch(bool result, ref PlayerControllerB __instance)
    {
      if (result == true) return true;

      for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
      {
        float distance = Vector3.Distance(WalkieTalkie.allWalkieTalkies[i].transform.position, __instance.transform.position);

        if (
          WalkieTalkie.allWalkieTalkies[i].isBeingUsed &&
          distance <= AudibleDistance
        )
        {
          return true;
        }
      }

      return false;
    }
  }
}