using UnityEngine;
using System.Collections;
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
        [SerializeField] private DropMover dropMover;

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

            // If ray hit something
            if ( hit.transform != null ) {
                // Try get Tile component from hit gameObject
                if ( hit.transform.TryGetComponent( out Tile tile ) ) {
                    // Then check whether the tile contains some drop inside then it is a valid tile
                    selectedTile = tile.Drop?.DropData != null ? tile : null;
                }
                else {
                    selectedTile = null;
                }
            }
            // If ray did not hit anything ~means player clicking somewhere outside of the grid boundaries
            else {
                selectedTile = null;
            }
        }

        private void InputManager_OnTouchCancelPosition( Vector2 touchEndPosition ) {
            if ( selectedTile == null ) {
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

                Vector2Int selectedTileXY = grid.GetXY( selectedTile.transform.position );

                switch ( swipeDirectionType ) {
                    case DirectionType.Right:
                        targetTile = grid.RightNeighbour( selectedTileXY.x, selectedTileXY.y );
                        break;
                    case DirectionType.Left:
                        targetTile = grid.LeftNeighbour( selectedTileXY.x, selectedTileXY.y );
                        break;
                    case DirectionType.Up:
                        targetTile = grid.UpNeighbour( selectedTileXY.x, selectedTileXY.y );
                        break;
                    case DirectionType.Down:
                        targetTile = grid.DownNeighbour( selectedTileXY.x, selectedTileXY.y );
                        break;
                }
                if ( targetTile?.Drop?.DropData != null ) {
                    StartCoroutine( Swap( targetTile ) );
                }
            }

            IEnumerator Swap( Tile targetTile ) {
                // Wait for tween finished to call matcher
                yield return dropMover.SwapTiles( selectedTile, targetTile );

                Vector2Int firstRowColumn = grid.GetXY( selectedTile.transform.position );
                Vector2Int secondRowColumn = grid.GetXY( targetTile.transform.position );

                matcher.Match( firstRowColumn, secondRowColumn );
            }
        }
    }
}