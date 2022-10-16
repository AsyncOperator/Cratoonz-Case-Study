using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Grid = Core.Grid<Tile>;
using AsyncOperator.Helpers;

namespace Core {
    public sealed class Swiper : MonoBehaviour {
        private enum DirectionType {
            None,
            Right,
            Left,
            Up,
            Down
        }

        private static readonly ReadOnlyDictionary<DirectionType, Vector2> _DirectionsDict = new ReadOnlyDictionary<DirectionType, Vector2>( new Dictionary<DirectionType, Vector2> {
            { DirectionType.Right, Vector2.right },
            { DirectionType.Left, Vector2.left },
            { DirectionType.Up, Vector2.up },
            { DirectionType.Down, Vector2.down }
        } );

        [Range( 10f, 40f ), SerializeField] private float angleThreshold;

        public Grid Grid { get; set; }
        private Tile selectedTile;

        private Vector2 touchStartPosition;
        private Vector2 touchEndPosition;

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
                selectedTile = hit.transform.TryGetComponent( out Tile tile ) ? tile : null; 
            }
            else {
                selectedTile = null;
            }
        }

        private void InputManager_OnTouchCancelPosition( Vector2 touchEndPosition ) {
            if ( selectedTile == null || selectedTile.Drop == null ) {
                return;
            }

            this.touchEndPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchEndPosition );
            CalculateSwipeDirection();
        }

        private void CalculateSwipeDirection() {
#if UNITY_EDITOR
            Debug.DrawLine( touchStartPosition, touchEndPosition, Color.blue, 10f );
#endif
            Vector2 direction = ( touchEndPosition - touchStartPosition ).normalized;

            float minAngle = float.PositiveInfinity;
            DirectionType swipeDirectionType = DirectionType.None;

            foreach ( KeyValuePair<DirectionType, Vector2> kvp in _DirectionsDict ) {
                float angleBetween = Vector2.Angle( direction, kvp.Value );
                if ( angleBetween <= angleThreshold && angleBetween < minAngle ) {
                    minAngle = angleBetween;
                    swipeDirectionType = kvp.Key;
                }
            }

            // Swap

            if ( swipeDirectionType != DirectionType.None ) {
                Tile targetTile = null;

                int x, y;
                Grid.GetXY( selectedTile.transform.position, out x, out y );

                switch ( swipeDirectionType ) {
                    case DirectionType.Right:
                        targetTile = Grid.RightNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Drop selectedTileDrop = selectedTile.Drop;
                                Drop targetTileDrop = targetTile.Drop;

                                selectedTileDrop.transform.SetParent( targetTile.transform, false );
                                targetTileDrop.transform.SetParent( selectedTile.transform, false );

                                selectedTile.Drop = targetTileDrop;
                                targetTile.Drop = selectedTileDrop;
                            }
                        }
                        break;
                    case DirectionType.Left:
                        targetTile = Grid.LeftNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Drop selectedTileDrop = selectedTile.Drop;
                                Drop targetTileDrop = targetTile.Drop;

                                selectedTileDrop.transform.SetParent( targetTile.transform, false );
                                targetTileDrop.transform.SetParent( selectedTile.transform, false );

                                selectedTile.Drop = targetTileDrop;
                                targetTile.Drop = selectedTileDrop;
                            }
                        }
                        break;
                    case DirectionType.Up:
                        targetTile = Grid.UpNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Drop selectedTileDrop = selectedTile.Drop;
                                Drop targetTileDrop = targetTile.Drop;

                                selectedTileDrop.transform.SetParent( targetTile.transform, false );
                                targetTileDrop.transform.SetParent( selectedTile.transform, false );

                                selectedTile.Drop = targetTileDrop;
                                targetTile.Drop = selectedTileDrop;
                            }
                        }
                        break;
                    case DirectionType.Down:
                        targetTile = Grid.DownNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Drop selectedTileDrop = selectedTile.Drop;
                                Drop targetTileDrop = targetTile.Drop;

                                selectedTileDrop.transform.SetParent( targetTile.transform, false );
                                targetTileDrop.transform.SetParent( selectedTile.transform, false );

                                selectedTile.Drop = targetTileDrop;
                                targetTile.Drop = selectedTileDrop;
                            }
                        }
                        break;
                }
            }
        }
    }
}