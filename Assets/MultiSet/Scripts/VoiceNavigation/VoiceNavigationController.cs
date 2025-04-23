using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

public class VoiceNavigationController : MonoBehaviour
{
    [Tooltip("Seconds allowed between taps to count triple?tap")]
    public float tapThreshold = 0.5f;

    public int speechRequestCode = 1001;   // Expose a request code for speech-to-text

    bool waitingForVoice = false;
    POI[] lastOptions;
    private int currentIndex = 0;

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


    public void InitializeCurrentPOI ()
    {
        currentIndex = 0;
    }

    public void OnTripleTap()
    {
        // 1) Gather all POIs
        var all = NavigationController.instance.augmentedSpace.GetPOIs();                    // :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
        // 2) Compute distance, filter unreachable (< 0), sort by ascending distance
        var reachable = all
            .Select(poi => new {
                poi,
                dist = PathEstimationUtils.instance.EstimateDistanceToPosition(poi)         // :contentReference[oaicite:2]{index=2}&#8203;:contentReference[oaicite:3]{index=3}
            })
            .Where(x => x.dist >= 0)                                                       // keep only reachable
            .OrderBy(x => x.dist)                                                          // closest first
            .ToArray();

        // 3) Store the POIs in lastOptions
        lastOptions = reachable.Select(x => x.poi).ToArray();

        // lastOptions = NavigationController.instance.augmentedSpace.GetPOIs();
        Debug.Log($"[VoiceNav] {lastOptions.Length} reachable options found");

        currentIndex = 0;
        SpeakCurrent();
    }

    void SpeakCurrent()
    {
        if (lastOptions == null || lastOptions.Length == 0) return;
        var poi = lastOptions[currentIndex];
        // fetch and round distance
        float rawDist = PathEstimationUtils.instance.EstimateDistanceToPosition(poi);          // :contentReference[oaicite:4]{index=4}&#8203;:contentReference[oaicite:5]{index=5}
        int metersDist = Mathf.Max(0, Mathf.RoundToInt(rawDist));

        TTSManager.Instance.Speak(
            $"Option {currentIndex}: {poi.poiName}, {metersDist} meters away",
            false
        );
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
/*#elif UNITY_ANDROID && !UNITY_EDITOR
        // Build a RecognizerIntent
        var intentClass = new AndroidJavaClass("android.speech.RecognizerIntent");
        string action = intentClass.GetStatic<string>("ACTION_RECOGNIZE_SPEECH");
        var intent = new AndroidJavaObject("android.content.Intent", action);

        // Free?form language model
        intent.Call<AndroidJavaObject>("putExtra",
            intentClass.GetStatic<string>("EXTRA_LANGUAGE_MODEL"),
            intentClass.GetStatic<string>("LANGUAGE_MODEL_FREE_FORM"));

        // Optional prompt
        intent.Call<AndroidJavaObject>("putExtra",
            intentClass.GetStatic<string>("EXTRA_PROMPT"),
            "Give your selection");

        // Launch the Intent
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("startActivityForResult", intent, speechRequestCode);*/
#else
        Debug.LogError("Voice recognition only supported on Android devices or in Editor.");
        waitingForVoice = false;
#endif
    }


    // Must match the GameObject name and method name in UnitySendMessage below
    public void OnVoiceCommand(string spoken)
    {
        waitingForVoice = false;
        ProcessVoiceCommand(spoken);
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

    public void OnSwipeDown()
    {
        if (lastOptions == null) return;
        currentIndex = (currentIndex + 1) % lastOptions.Length;
        SpeakCurrent();
    }

    public void OnSwipeUp()
    {
        if (lastOptions == null) return;
        currentIndex = (currentIndex - 1 + lastOptions.Length)
                       % lastOptions.Length;
        SpeakCurrent();
    }

    public void OnSwipeRight()
    {
        if (lastOptions == null) return;
        var poi = lastOptions[currentIndex];
        // TTSManager.Instance.Speak($"Starting navigation to {poi.poiName}");
        NavigationController.instance.SetPOIForNavigation(poi);
    }

    public void OnSwipeLeft()
    {
        NavigationController.instance.StopNavigation();
    }
}
