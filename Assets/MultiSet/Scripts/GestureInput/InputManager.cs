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

    Vector2 lastTouchPos;
    Vector2 swipeStartPos;
    private readonly float minSwipeDist = 50f;       // pixels
    private readonly float edgeMargin = 50f;        // ignore touches near edges

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
        // record latest position wherever press is held
        touchControls.Touch.TouchPosition.started += ctx =>
            lastTouchPos = ctx.ReadValue<Vector2>();

        touchControls.Touch.TouchPosition.performed += ctx =>
                    lastTouchPos = ctx.ReadValue<Vector2>();

        // record start on press-down
        touchControls.Touch.TouchPress.performed += ctx =>
            OnTouchDown();

        // touchControls.Touch.TouchPress.started += ctx => Debug.LogError($"Touch Press, Started!!!");

        // handle swipe on press-up
        touchControls.Touch.TouchPress.canceled += ctx =>
            OnTouchUp();

/*        // when the touch ends, compute the delta
        touchControls.Touch.TouchPosition.canceled += OnTouchEnded;*/   // Error

        _tripleTapCallback = ctx => voiceNav.OnTripleTap();
        touchControls.Touch.TripleTapInput.performed += _tripleTapCallback;

        // _hold3sCallback = ctx => voiceNav.StartVoiceRecognition();
        // touchControls.Touch.PressAndHoldInput.performed += _hold3sCallback;
    }

    private void OnDisable()
    {
        touchControls.Touch.TouchPosition.performed -= ctx =>
            lastTouchPos = ctx.ReadValue<Vector2>();
        
        touchControls.Touch.TouchPress.performed -= ctx => OnTouchDown();

        // touchControls.Touch.TouchPress.started -= ctx => Debug.LogError($"Touch Press, Started!!!");

        touchControls.Touch.TouchPress.canceled -= ctx => OnTouchUp();

        touchControls.Touch.TripleTapInput.performed -= _tripleTapCallback;
        // touchControls.Touch.PressAndHoldInput.performed -= _ => voiceNav.StartVoiceRecognition();
        
        touchControls.Touch.Disable();
        touchControls.Dispose();
    }

    void OnTouchDown()
    {
        Debug.Log($"[TouchDown] Touch detected at some postion"); // {lastTouchPos.ToString()}");
        Debug.Log($"[TouchDown] {lastTouchPos.ToString()}");
        // only record if not too close to edges
        if (lastTouchPos.x < edgeMargin || lastTouchPos.x > Screen.width - edgeMargin ||
            lastTouchPos.y < edgeMargin || lastTouchPos.y > Screen.height - edgeMargin)
            return;

        swipeStartPos = lastTouchPos;
        Debug.Log($"[TouchDown] Assigned swipeStartPos value: {swipeStartPos.ToString()}");
    }

    void OnTouchUp()
    {
        Vector2 delta = lastTouchPos - swipeStartPos;
        Debug.Log($"[TouchUp] {swipeStartPos.ToString()}, {lastTouchPos.ToString()}");
        Debug.Log($"[TouchUp] Swipe of distance {delta.ToString()} detected");

        // ignore tiny moves or bogus 0,0 events
        if (swipeStartPos == Vector2.zero && lastTouchPos == Vector2.zero)
            return;
        if (delta.magnitude < minSwipeDist)
            return;

        // vertical swipe?
        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            if (delta.y < 0) voiceNav.OnSwipeDown();
            else voiceNav.OnSwipeUp();
        }
        // horizontal swipe?
        else
        {
            if (delta.x > 0) voiceNav.OnSwipeRight();
            else voiceNav.OnSwipeLeft();
        }
    }

    /*void OnTouchStarted(InputAction.CallbackContext ctx)
    {
        swipeStartPos = ctx.ReadValue<Vector2>();
        Debug.LogError($"Swipe started -> {swipeStartPos.ToString()}");
    }

    void OnTouchEnded(InputAction.CallbackContext ctx)
    {
        Vector2 endPos = ctx.ReadValue<Vector2>();
        Debug.LogError($"Swipe ended -> {endPos.ToString()}");
        Vector2 delta = endPos - swipeStartPos;

        // vertical swipe?
        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > minSwipeDist)
        {
            if (delta.y < 0)
                voiceNav.OnSwipeDown();
            else
                voiceNav.OnSwipeUp();
        }
        // horizontal swipe?
        else if (Mathf.Abs(delta.x) > minSwipeDist)
        {
            if (delta.x > 0)
                voiceNav.OnSwipeRight();
            // (you could also support swipe-left if desired)
        }
    }*/


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
