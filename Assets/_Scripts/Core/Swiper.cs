using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AsyncOperator.Helpers;

namespace Core {
    public sealed class Swiper : MonoBehaviour {
        [Range( 10f, 40f ), SerializeField] private float angleThreshold;

        private Vector2 touchStartPosition;
        private Vector2 touchEndPosition;

        private static readonly IList<Vector2> _Directions = new ReadOnlyCollection<Vector2>( new Vector2[] {
            Vector2.right,
            Vector2.left,
            Vector2.up,
            Vector2.down
        } );

        private void OnEnable() {
            InputManager.OnTouchStartPosition += InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition += InputManager_OnTouchCancelPosition;
        }

        private void OnDisable() {
            InputManager.OnTouchStartPosition -= InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition -= InputManager_OnTouchCancelPosition;
        }

        private void InputManager_OnTouchStartPosition( Vector2 touchStartPosition ) {
            this.touchStartPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchStartPosition );

            RaycastHit2D hit = Physics2D.Raycast( this.touchStartPosition, Vector2.zero );
            if ( hit.transform != null ) {
                Debug.Log( $"{hit.transform.name}", hit.transform.gameObject );
            }
        }

        private void InputManager_OnTouchCancelPosition( Vector2 touchEndPosition ) {
            this.touchEndPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchEndPosition );
            DebugSwipe();
        }

        private void DebugSwipe() {
            Debug.DrawLine( touchStartPosition, touchEndPosition, Color.blue, 10f );

            Vector2 direction = ( touchEndPosition - touchStartPosition ).normalized;

            float minAngle = float.PositiveInfinity;
            Vector2 swipeDirection = Vector2.zero;

            foreach ( Vector2 dir in _Directions ) {
                float angleBetween = Vector2.Angle( direction, dir );
                if ( angleBetween <= angleThreshold && angleBetween < minAngle ) {
                    minAngle = angleBetween;
                    swipeDirection = dir;
                }
            }

            Debug.Log( $"Swipe direction => {swipeDirection}" );
        }
    }
}