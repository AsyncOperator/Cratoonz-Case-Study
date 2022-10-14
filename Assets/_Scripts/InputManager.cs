using System;
using UnityEngine;
using UnityEngine.InputSystem;
using AsyncOperator.Singleton;

public sealed class InputManager : Singleton<InputManager> {
    private InputControls inputControls;

    public static event Action<Vector2> OnTouchStartPosition;
    public static event Action<Vector2> OnTouchCancelPosition;

    protected override void Awake() {
        base.Awake();
        inputControls = new();
    }

    private void OnEnable() {
        inputControls.Swipe.OnTouch.started += OnTouchStarted;
        inputControls.Swipe.OnTouch.canceled += OnTouchCancelled;

        inputControls.Enable();
    }

    private void OnDisable() {
        inputControls.Swipe.OnTouch.started -= OnTouchStarted;
        inputControls.Swipe.OnTouch.canceled -= OnTouchCancelled;

        inputControls.Disable();
    }

    private void OnTouchStarted( InputAction.CallbackContext ctx ) {
        OnTouchStartPosition?.Invoke( inputControls.Swipe.TouchPosition.ReadValue<Vector2>() );
    }

    private void OnTouchCancelled( InputAction.CallbackContext ctx ) {
        OnTouchCancelPosition?.Invoke( inputControls.Swipe.TouchPosition.ReadValue<Vector2>() );
    }
}