using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance { get; private set; }
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject tts;
    const int QUEUE_FLUSH = 0;
    const int QUEUE_ADD   = 1;
#endif
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTTS();
        }
        else Destroy(gameObject);
    }
    void InitializeTTS()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using(var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var act = unity.GetStatic<AndroidJavaObject>("currentActivity");
            tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", act, null);
        }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        // nothing to init
#endif
    }
    public void Speak(string text, bool blocking = true)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int blockingParam = blocking ? QUEUE_ADD : QUEUE_FLUSH;
        tts.Call<int>(
            "speak",
            text,
            blockingParam,   // <-- queue instead of flush     
            null,           // no extra params
            null           // Unique ID ---> helpful if you implement a progress listener ---> string utteranceId = Guid.NewGuid().ToString();
        );
// #elif UNITY_STANDALONE_WIN || UNITY_EDITOR
//         using (var synth = new System.Speech.Synthesis.SpeechSynthesizer())
//             synth.SpeakAsync(text);
#else
        Debug.LogWarning("TTS not supported here");
#endif
    }
}
