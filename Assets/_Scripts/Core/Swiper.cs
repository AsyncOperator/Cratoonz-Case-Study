using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Board;
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
        [SerializeField] private Matcher matcher;

        private Tile selectedTile;
        private Vector2 touchStartPosition;
        private Vector2 touchEndPosition;

        private Grid grid;

        private void OnEnable() {
            FindObjectOfType<BoardCreator>().OnBoardCreated += ( g ) => grid = g;

            InputManager.OnTouchStartPosition += InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition += InputManager_OnTouchCancelPosition;
        }

        private void OnDisable() {
            FindObjectOfType<BoardCreator>().OnBoardCreated -= ( g ) => grid = g;

            InputManager.OnTouchStartPosition -= InputManager_OnTouchStartPosition;
            InputManager.OnTouchCancelPosition -= InputManager_OnTouchCancelPosition;
        }

        private void InputManager_OnTouchStartPosition( Vector2 touchStartPosition ) {
            this.touchStartPosition = Helpers.ConvertPixelCoordinateToWorldPosition( Camera.main, touchStartPosition );

            RaycastHit2D hit = Physics2D.Raycast( this.touchStartPosition, Vector2.zero );

            // If ray hit something then try get tile component from the hit object, otherwise set selectedTile field to null
            if ( hit.transform != null ) {
                selectedTile = hit.transform.TryGetComponent( out Tile tile ) ? tile : null;
            }
            // If ray did not hit anything
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
            // To visualize swipe in scene view ~dont forget to enable gizmos
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
                grid.GetXY( selectedTile.transform.position, out x, out y );

                switch ( swipeDirectionType ) {
                    case DirectionType.Right:
                        targetTile = grid.RightNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Swap( targetTile );
                            }
                        }
                        break;
                    case DirectionType.Left:
                        targetTile = grid.LeftNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Swap( targetTile );
                            }
                        }
                        break;
                    case DirectionType.Up:
                        targetTile = grid.UpNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Swap( targetTile );
                            }
                        }
                        break;
                    case DirectionType.Down:
                        targetTile = grid.DownNeighbour( x, y );
                        if ( targetTile != null ) {
                            if ( targetTile.Drop != null ) {
                                Swap( targetTile );
                            }
                        }
                        break;
                }
            }

            void Swap( Tile targetTile ) {
                Drop selectedTileDrop = selectedTile.Drop;
                Drop targetTileDrop = targetTile.Drop;

                selectedTileDrop.transform.SetParent( targetTile.transform, false );
                targetTileDrop.transform.SetParent( selectedTile.transform, false );

                selectedTile.Drop = targetTileDrop;
                targetTile.Drop = selectedTileDrop;

                Vector2Int firstRowColumn = grid.GetXY( selectedTile.transform.position );
                Vector2Int secondRowColumn = grid.GetXY( targetTile.transform.position );

                matcher.Match( firstRowColumn, secondRowColumn );
            }
        }
    }
}