using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches
{
  [HarmonyPatch(typeof(PlayerControllerB))]
  internal class PlayerControllerBPatch
  {
    private static float AudibleDistance = 20f;
    private static float throttleInterval = 0.4f;
    private static float throttle = 0f;
    private static float AverageDistanceToHeldWalkie = 2f;
    private static float WalkieRecordingRange = 20f;
    private static float PlayerToPlayerSpatialHearingRange = 20f;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void alwaysHearWalkieTalkiesPatch(ref bool ___holdingWalkieTalkie, ref PlayerControllerB __instance)
    {
      // Throttle calls to reduce performance impact
      throttle += Time.deltaTime;
      if (throttle < throttleInterval) {
        return;
      }
      throttle = 0f;

      // Early returns
      if (__instance == null) {
        return;
      }
      if (GameNetworkManager.Instance == null)
      {
        return;
      }
      if (__instance != GameNetworkManager.Instance.localPlayerController) {
        return;
      }
      if (GameNetworkManager.Instance.localPlayerController == null)
      {
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

      // If the player is going in or out of range of an active walkie
      if (isAnyWalkieInRange != __instance.holdingWalkieTalkie) {
        // Set the holdingWalkieTalkie bool to true if the player is within range of an active walkie
        ___holdingWalkieTalkie = isAnyWalkieInRange;
        
        // Immediately stop audio from walkies that are out of range
        for (int i = 0; i < walkieTalkiesOutOfRange.Count; i++)
        {
          if (i < walkieTalkiesInRange.Count) {
            walkieTalkiesOutOfRange[i].thisAudio.Stop();
          }
        }
      }

      // Return early if we are out of range of all active walkies
      if (!isAnyWalkieInRange) {
        return;
      }

      // Get the local player controller (or the spectated player controller) as the listener
      PlayerControllerB localOrSpectatedPlayerController = (
        !GameNetworkManager.Instance.localPlayerController.isPlayerDead
        || !(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null)
      )
        ? GameNetworkManager.Instance.localPlayerController
        : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;

      for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
      {
        // Return if the player is not controlled by a player or is dead or if its the local player controller
        if (
          (!StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && !StartOfRound.Instance.allPlayerScripts[i].isPlayerDead)
          || StartOfRound.Instance.allPlayerScripts[i] == GameNetworkManager.Instance.localPlayerController
          || StartOfRound.Instance.allPlayerScripts[i].isPlayerDead
        )
        {
          continue;
        }

        PlayerControllerB otherPlayerController = StartOfRound.Instance.allPlayerScripts[i];

        // In PlayerControllerBPatch we set the holdingWalkieTalkie bool to true if the player is within range of an active walkie instead of actively holding one
        bool isOtherPlayerNearActiveWalkie = otherPlayerController.holdingWalkieTalkie;

        if (isOtherPlayerNearActiveWalkie)
        {
          float distanceLocalPlayerToOtherPlayer = Vector3.Distance(localOrSpectatedPlayerController.transform.position, otherPlayerController.transform.position);
          float distanceOtherPlayerToClosestWalkie = float.MaxValue;
          float distanceLocalPlayerToClosestWalkie = float.MaxValue;

          for (int j = 0; j < WalkieTalkie.allWalkieTalkies.Count; j++)
          {
            // If walkie talkie is not being used skip it.
            if (!WalkieTalkie.allWalkieTalkies[j].isBeingUsed)
            {
              continue;
            }

            // Get the distance from the local player to the closest active walkie
            float distanceLocalToWalkie = Vector3.Distance(WalkieTalkie.allWalkieTalkies[j].target.transform.position, localOrSpectatedPlayerController.transform.position);
            if (distanceLocalToWalkie < distanceLocalPlayerToClosestWalkie)
            {
              distanceLocalPlayerToClosestWalkie = distanceLocalToWalkie;
            }


            // Only if walkie is being spoken into, get the distance from the other player to the closest active walkie
            if (!WalkieTalkie.allWalkieTalkies[j].speakingIntoWalkieTalkie)
            {
              float distanceOtherToWalkie = Vector3.Distance(WalkieTalkie.allWalkieTalkies[j].transform.position, otherPlayerController.transform.position);
              if (distanceOtherToWalkie < distanceOtherPlayerToClosestWalkie)
              {
                distanceOtherPlayerToClosestWalkie = distanceOtherToWalkie;
              }
            }
          }

          // Derive the volume for the other player based on the distance of both players to their closest active walkie
          float playerVolumeByWalkieTalkieDistance = Mathf.Min(
            1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, distanceOtherPlayerToClosestWalkie),
            1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, distanceLocalPlayerToClosestWalkie)
          );

          // Derive the volume for the other player based on the distance of the local player to the other player
          float playerVolumeBySpatialDistance = 1f - Mathf.InverseLerp(0f, PlayerToPlayerSpatialHearingRange, distanceLocalPlayerToOtherPlayer);

          // Set the volume of the other player to the louder of the two volumes
          otherPlayerController.voicePlayerState.Volume = Mathf.Max(playerVolumeByWalkieTalkieDistance, playerVolumeBySpatialDistance);
        }
      }
    }
  }
}