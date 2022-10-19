using UnityEngine;

namespace Core {
    public sealed class Grid<T> {
        private int row;
        public int Row => row;

        private int column;
        public int Column => column;

        private float tileSize;
        public float TileSize => tileSize;

        private Vector3 origin;

        private T[,] tileArr;

        public Grid( int r, int c, float tileSize = 1f, Vector3 origin = default ) {
            row = r;
            column = c;
            this.tileSize = tileSize;
            this.origin = origin;

            tileArr = new T[ row, column ];

#if UNITY_EDITOR
            for ( int x = 0 ; x < tileArr.GetLength( 0 ) ; x++ ) {
                for ( int y = 0 ; y < tileArr.GetLength( 1 ) ; y++ ) {
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x, y + 1 ), Color.red, Mathf.Infinity );
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x + 1, y ), Color.red, Mathf.Infinity );
                }
            }

            Debug.DrawLine( GetWorldPosition( 0, column ), GetWorldPosition( row, column ), Color.red, Mathf.Infinity );
            Debug.DrawLine( GetWorldPosition( row, 0 ), GetWorldPosition( row, column ), Color.red, Mathf.Infinity );
#endif
        }

        public Vector3 GetWorldPosition( int r, int c ) => new Vector3( c, r ) * tileSize + origin;

        public void GetXY( Vector3 worldPosition, out int r, out int c ) {
            r = Mathf.FloorToInt( ( worldPosition - origin ).y / tileSize );
            c = Mathf.FloorToInt( ( worldPosition - origin ).x / tileSize );
        }

        public Vector2Int GetXY( Vector3 worldPosition ) {
            int r = Mathf.FloorToInt( ( worldPosition - origin ).y / tileSize );
            int c = Mathf.FloorToInt( ( worldPosition - origin ).x / tileSize );
            return new Vector2Int( r, c );
        }

        public void SetValue( int r, int c, T value ) {
            if ( r >= 0 && c >= 0 && r < row && c < column ) {
                tileArr[ r, c ] = value;
            }
        }

        public void SetValue( Vector3 worldPosition, T value ) {
            GetXY( worldPosition, out int r, out int c );
            SetValue( r, c, value );
        }

        public T GetValue( int r, int c ) {
            if ( r >= 0 && c >= 0 && r < row && c < column ) {
                return tileArr[ r, c ];
            }
            else {
                return default( T );
            }
        }

        public T GetValue( Vector3 worldPosition ) {
            GetXY( worldPosition, out int r, out int c );
            return GetValue( r, c );
        }

        public T RightNeighbour( int r, int c ) {
            if ( r >= 0 && c >= 0 && r < row && c < column - 1 ) {
                return tileArr[ r, c + 1 ];
            }
            else {
                return default( T );
            }
        }

        public T LeftNeighbour( int r, int c ) {
            if ( r >= 0 && c >= 1 && r < row && c < column ) {
                return tileArr[ r, c - 1 ];
            }
            else {
                return default( T );
            }
        }

        public T UpNeighbour( int x, int y ) {
            if ( x >= 0 && y >= 0 && x < row - 1 && y < column ) {
                return tileArr[ x + 1, y ];
            }
            else {
                return default( T );
            }
        }

        public T DownNeighbour( int x, int y ) {
            if ( x >= 1 && y >= 0 && x < row && y < column ) {
                return tileArr[ x - 1, y ];
            }
            else {
                return default( T );
            }
        }
    }
}