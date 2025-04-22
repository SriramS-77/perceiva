using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private TouchControls touchControls;
    // Reference your other component(s) in the Inspector
    [SerializeField] VoiceNavigationController voiceNav;
    private Action<InputAction.CallbackContext> _tripleTapCallback;
    private Action<InputAction.CallbackContext> _hold3sCallback;

    //public static InputManager Instance { get; private set; }
    //private bool doubleTap = false;
    //private bool tripleTap = false;

    private void Awake()
    {
        touchControls = new TouchControls();
        touchControls.Touch.Enable();
    }

    private void OnEnable()
    {
        _tripleTapCallback = ctx => voiceNav.OnTripleTap();
        touchControls.Touch.TripleTapInput.performed += _tripleTapCallback;

        _hold3sCallback = ctx => voiceNav.StartVoiceRecognition();
        touchControls.Touch.PressAndHoldInput.performed += _hold3sCallback;
    }

    private void OnDisable()
    {
        touchControls.Touch.TripleTapInput.performed -= _tripleTapCallback;
        touchControls.Touch.PressAndHoldInput.performed -= _hold3sCallback;
        touchControls.Touch.Disable();
        touchControls.Dispose();
    }

/*    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // touchControls.Touch.DoubleTapInput.WasCompletedThisFrame += ctx => onTripleTap();
        return;
    }

    public bool DoubleClick() { return doubleTap; }
    public bool TripleClick() { return tripleTap; }

    // Update is called once per frame
    void Update()
    {
        doubleTap = touchControls.Touch.DoubleTapInput.WasCompletedThisFrame();
        tripleTap = touchControls.Touch.TripleTapInput.WasCompletedThisFrame();

        //touchControls.Touch.DoubleTapInput.performed

        if (doubleTap)
        {
            Debug.Log("[GestureInput] Double tap input!");
        }
        if (tripleTap)
        {
            Debug.Log("[GestureInput] Triple tap input!");
        }
    }*/
}
