using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

public class VoiceNavigationController : MonoBehaviour
{
    [Tooltip("Seconds allowed between taps to count triple?tap")]
    public float tapThreshold = 0.5f;

    int tapCount = 0;
    float lastTapTime = 0f;

    bool waitingForVoice = false;
    POI[] lastOptions;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    DictationRecognizer dictation;
#endif

    void Update()
    {
        if (waitingForVoice)
        {
            // in voice?listening mode, ignore taps
            return;
        }
/*
        // detect screen tap (works for mouse click too)
        if (WasScreenTapped())
        {   
            var now = Time.time;
            if (now - lastTapTime <= tapThreshold)
                tapCount++;
            else
                tapCount = 1;

            float dt = now - lastTapTime;
            Debug.Log($"[VoiceNav] Tap #{tapCount} (dt={dt:F2}s)");
            lastTapTime = now;

            if (tapCount == 3)
            {
                Debug.Log("[VoiceNav] Triple tap detected!");
                OnTripleTap();
                tapCount = 0;
            }
        }*/

/*        if (InputManager.Instance.DoubleClick())
        {
            Debug.Log("[VoiceNav] Double tap detected!");
            OnTripleTap();
        }*/
    }

    bool WasScreenTapped()
    {
        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            return true;
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            return true;
        return false;
    }   


    public void OnTripleTap()
    {
        // 1) Gather reachable POIs (all POIs for simplicity)
        lastOptions = NavigationController.instance.augmentedSpace.GetPOIs();
        Debug.Log($"[VoiceNav] {lastOptions.Length} options found");

        // 2) Speak them out
        for (int i = 0; i < lastOptions.Length; i++)
        {
            var name = lastOptions[i].poiName;
            Debug.Log($"[VoiceNav] Speaking Option {i}: {name}");
            TTSManager.Instance.Speak($"Option {i}: {name}");
        }

        // 3) Prepare to listen on next tap - StartCoroutine(WaitForNextTapThenListen());
    }

/*    IEnumerator WaitForNextTapThenListen()
    {
        // Wait until user taps once more
        do { yield return null; }
        while (!WasScreenTapped());

        // Prompt
        TTSManager.Instance.Speak("Give your selection");

        // Activate voice listener
        waitingForVoice = true;
        StartVoiceRecognition();
    }*/

    public void StartVoiceRecognition()
    {
        // Prompt
        TTSManager.Instance.Speak("Give your selection");

        // Activate voice listener
        waitingForVoice = true;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        dictation = new DictationRecognizer();
        dictation.InitialSilenceTimeoutSeconds = 5;
        dictation.AutoSilenceTimeoutSeconds = 2;
        dictation.DictationResult += OnDictationResult;
        dictation.DictationHypothesis += _ => { /* you can show UI hint */ };
        dictation.DictationComplete += status => CleanupDictation();
        dictation.DictationError += (err, h) => CleanupDictation();
        dictation.Start();
#else
        // On Android/iOS you must plug in platform?specific speech recognition.
        // e.g. launch an Android Intent for RecognizerIntent and implement
        // OnActivityResult in a custom plugin that calls back into Unity.
        Debug.LogError("Voice recognition not implemented on this platform yet.");
        waitingForVoice = false;
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        CleanupDictation();
        ProcessVoiceCommand(text);
    }
#endif

    void CleanupDictation()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (dictation != null)
        {
            dictation.DictationResult -= OnDictationResult;
            dictation.DictationHypothesis -= _ => { };
            dictation.DictationComplete -= status => CleanupDictation();
            dictation.DictationError -= (err, h) => CleanupDictation();
            dictation.Stop();
            dictation.Dispose();
            dictation = null;
        }
#endif
        waitingForVoice = false;
    }

    void ProcessVoiceCommand(string spoken)
    {
        // Try to parse an integer out of what the user said
        if (int.TryParse(spoken.Trim(), out var choice)
            && lastOptions != null
            && choice >= 0
            && choice < lastOptions.Length)
        {
            var poi = lastOptions[choice];
            TTSManager.Instance.Speak($"Starting navigation to {poi.poiName}");
            NavigationController.instance.SetPOIForNavigation(poi);
        }
        else
        {
            TTSManager.Instance.Speak("Sorry, I didn't understand. Try triple?tap again.");
        }
    }
}
