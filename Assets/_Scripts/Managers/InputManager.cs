using System;
using UnityEngine;
using UnityEngine.InputSystem;
using AsyncOperator.Singleton;

// To be sure this class will run first on Unity's execution order
[DefaultExecutionOrder( -10 )]
public sealed class InputManager : Singleton<InputManager> {
    private InputControls inputControls;

    public bool Enabled { get; set; } = true;

    public static event Action<Vector2> OnTouchStartPosition;
    public static event Action<Vector2> OnTouchCancelPosition;

    protected override void Awake() {
        base.Awake();   // Run ancestor Awake
        inputControls = new();  // Create instance from InputControls object which is generated from InputActions.asset
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
        if ( Enabled ) {
            OnTouchStartPosition?.Invoke( inputControls.Swipe.TouchPosition.ReadValue<Vector2>() );
        }
    }

    private void OnTouchCancelled( InputAction.CallbackContext ctx ) {
        if ( Enabled ) {
            OnTouchCancelPosition?.Invoke( inputControls.Swipe.TouchPosition.ReadValue<Vector2>() );
        }
    }
}