using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using AsyncOperator.Helpers;

namespace Core {
    public sealed class Swiper : MonoBehaviour {
        public enum DirectionType {
            None,
            Right,
            Left,
            Up,
            Down
        }

        private static readonly ReadOnlyDictionary<DirectionType, Vector2> _PossibleDirectionsDict = new ReadOnlyDictionary<DirectionType, Vector2>( new Dictionary<DirectionType, Vector2> {
            { DirectionType.Right, Vector2.right },
            { DirectionType.Left, Vector2.left },
            { DirectionType.Up, Vector2.up },
            { DirectionType.Down, Vector2.down }
        } );

        /// <summary>
        /// We use this value when comparing the vectors in readonly collections with the vector calculated based on data we got from InputManager,
        /// Ex: so let's pick a vector inside collection (Vector2.right) and let's assume the calculated vector is (0.3f, 0.7f) so then we check the angle between these vector and if the angle we got is bigger than this value
        /// we skip that vector which is not the one we are looking for
        /// </summary>
        [Tooltip( "Angle in degrees" )]
        [Range( 10f, 40f ), SerializeField] private float angleThreshold;

        private Vector2 touchStartPosition;
        private Vector2 touchEndPosition;

        public event Action<Vector2, DirectionType> OnSwipeCalculated;

        private void OnEnable() {
            InputManager.OnTouchStartPosition += InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition += InputManager_OnTouchCancelPosition;
        }

        private void OnDisable() {
            InputManager.OnTouchStartPosition -= InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition -= InputManager_OnTouchCancelPosition;
        }

        private void InputManager_OnTouchStartPosition( Vector2 touchStartPosition ) {
            // Camera.main is safe no longer generate garbage
            this.touchStartPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchStartPosition );
        }

        private void InputManager_OnTouchCancelPosition( Vector2 touchEndPosition ) {
            // Camera.main is safe no longer generate garbage
            this.touchEndPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchEndPosition );
            CalculateSwipeDirection();
        }

        private void CalculateSwipeDirection() {
#if UNITY_EDITOR
            // To visualize swipe in scene view ~dont forget to enable gizmos
            Debug.DrawLine( touchStartPosition, touchEndPosition, Color.blue, 10f );
#endif

            Vector2 swipeDirection = ( touchEndPosition - touchStartPosition ).normalized;
            float minAngle = float.PositiveInfinity;
            DirectionType swipeDirectionType = DirectionType.None;

            // Try find direction based upon player's input
            foreach ( KeyValuePair<DirectionType, Vector2> kvp in _PossibleDirectionsDict ) {
                float angleBetween = Vector2.Angle( swipeDirection, kvp.Value );
                if ( angleBetween <= angleThreshold && angleBetween < minAngle ) {
                    minAngle = angleBetween;    // Update minAngle
                    swipeDirectionType = kvp.Key;   // Update swipeDirection
                }
            }

            if ( swipeDirectionType != DirectionType.None ) {
                OnSwipeCalculated( touchStartPosition, swipeDirectionType );
            }
        }
    }
}