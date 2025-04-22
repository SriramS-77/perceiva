using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SpeakOnClick : MonoBehaviour
{
    Button btn;
    TextMeshProUGUI label;
    string InitialText = "Button Clicked ";

    void Awake()
    {
        btn = GetComponent<Button>();
        label = GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (label != null)
            TTSManager.Instance.Speak(InitialText + label.text);
    }
}
