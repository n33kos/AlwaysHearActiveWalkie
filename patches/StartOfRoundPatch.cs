using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch("UpdatePlayerVoiceEffects")]
    [HarmonyPrefix]
    public static bool updatePlayerVoiceEffectsPatch(
        StartOfRound __instance,
        ref float ___updatePlayerVoiceInterval,
        ref PlayerControllerB[] ___allPlayerScripts
        )
    {
        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
        {
            return false;
        }
        ___updatePlayerVoiceInterval = 2f;
        PlayerControllerB playerControllerB = ((!GameNetworkManager.Instance.localPlayerController.isPlayerDead || !(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null)) ? GameNetworkManager.Instance.localPlayerController : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript);
        for (int i = 0; i < ___allPlayerScripts.Length; i++)
        {
            PlayerControllerB playerControllerB2 = ___allPlayerScripts[i];
            if ((!playerControllerB2.isPlayerControlled && !playerControllerB2.isPlayerDead) || playerControllerB2 == GameNetworkManager.Instance.localPlayerController)
            {
                continue;
            }
            if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatIngameSettings._playerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
            {
                __instance.RefreshPlayerVoicePlaybackObjects();
                if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                {
                    Debug.Log($"Was not able to access voice chat object for player #{i}; {playerControllerB2.voicePlayerState == null}; {playerControllerB2.currentVoiceChatAudioSource == null}");
                    continue;
                }
            }
            AudioSource currentVoiceChatAudioSource = ___allPlayerScripts[i].currentVoiceChatAudioSource;
            bool flag = playerControllerB2.speakingToWalkieTalkie && playerControllerB.holdingWalkieTalkie && playerControllerB2 != playerControllerB;
            if (playerControllerB2.isPlayerDead)
            {
                currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().enabled = false;
                currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = false;
                currentVoiceChatAudioSource.panStereo = 0f;
                SoundManager.Instance.playerVoicePitchTargets[playerControllerB2.playerClientId] = 1f;
                SoundManager.Instance.SetPlayerPitch(1f, (int)playerControllerB2.playerClientId);
                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    currentVoiceChatAudioSource.spatialBlend = 0f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                    playerControllerB2.voicePlayerState.Volume = 1f;
                }
                else
                {
                    currentVoiceChatAudioSource.spatialBlend = 1f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                    playerControllerB2.voicePlayerState.Volume = 0f;
                }
                continue;
            }
            AudioLowPassFilter component = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
            OccludeAudio component2 = currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
            component.enabled = true;
            component2.overridingLowPass = flag || ___allPlayerScripts[i].voiceMuffledByEnemy;
            currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = flag;
            if (!flag)
            {
                currentVoiceChatAudioSource.spatialBlend = 1f;
                playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                currentVoiceChatAudioSource.bypassListenerEffects = false;
                currentVoiceChatAudioSource.bypassEffects = false;
                currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                component.lowpassResonanceQ = 1f;
            }
            else
            {
                currentVoiceChatAudioSource.spatialBlend = 0f;
                playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    currentVoiceChatAudioSource.panStereo = 0f;
                    currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                    currentVoiceChatAudioSource.bypassListenerEffects = false;
                    currentVoiceChatAudioSource.bypassEffects = false;
                }
                else
                {
                    if (playerControllerB.currentlyHeldObjectServer is WalkieTalkie) {
                        currentVoiceChatAudioSource.panStereo = 0.4f;
                        Debug.Log("Playing through right ear");
                    } 
                    else {
                        currentVoiceChatAudioSource.panStereo = 0f;
                        Debug.Log("Playing normally");
                    }
                    currentVoiceChatAudioSource.bypassListenerEffects = false;
                    currentVoiceChatAudioSource.bypassEffects = false;
                    currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                }
                component2.lowPassOverride = 4000f;
                component.lowpassResonanceQ = 3f;
            }
            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
            {
                playerControllerB2.voicePlayerState.Volume = 0.8f;
            }
            else
            {
                playerControllerB2.voicePlayerState.Volume = 1f;
            }
        }

        return false; // Skip the original method
    }
}

