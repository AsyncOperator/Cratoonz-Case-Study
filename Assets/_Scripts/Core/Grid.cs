using UnityEngine;

namespace Core {
    public sealed class Grid<T> {
        private int width;
        private int height;
        private float tileSize;
        private Vector3 origin;

        private T[,] tileArr;

        public Grid( int width, int height, float tileSize = 1f, Vector3 origin = default ) {
            this.width = width;
            this.height = height;
            this.tileSize = tileSize;
            this.origin = origin;

            tileArr = new T[ width, height ];

            for ( int x = 0 ; x < tileArr.GetLength( 0 ) ; x++ ) {
                for ( int y = 0 ; y < tileArr.GetLength( 1 ) ; y++ ) {
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x, y + 1 ), Color.red, Mathf.Infinity );
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x + 1, y ), Color.red, Mathf.Infinity );
                }
            }

            Debug.DrawLine( GetWorldPosition( 0, height ), GetWorldPosition( width, height ), Color.red, Mathf.Infinity );
            Debug.DrawLine( GetWorldPosition( width, 0 ), GetWorldPosition( width, height ), Color.red, Mathf.Infinity );
        }

        public Vector3 GetWorldPosition( int x, int y ) => new Vector3( x, y ) * tileSize + origin;

        public void GetXY( Vector3 worldPosition, out int x, out int y ) {
            x = Mathf.FloorToInt( ( worldPosition - origin ).x / tileSize );
            y = Mathf.FloorToInt( ( worldPosition - origin ).y / tileSize );
        }

        public void SetValue( int x, int y, T value ) {
            if ( x >= 0 && y >= 0 && x < width && y < height ) {
                tileArr[ x, y ] = value;
            }
        }

        public void SetValue( Vector3 worldPosition, T value ) {
            GetXY( worldPosition, out int x, out int y );
            SetValue( x, y, value );
        }

        public T GetValue( int x, int y ) {
            if ( x >= 0 && y >= 0 && x < width && y < height ) {
                return tileArr[ x, y ];
            }
            else {
                return default( T );
            }
        }

        public T GetValue( Vector3 worldPosition ) {
            GetXY( worldPosition, out int x, out int y );
            return GetValue( x, y );
        }

        public T RightNeighbour( int x, int y ) {
            if ( x >= 0 && y >= 0 && x < width - 1 && y < height ) {
                return tileArr[ x + 1, y ];
            }
            else {
                return default( T );
            }
        }

        public T LeftNeighbour( int x, int y ) {
            if ( x >= 1 && y >= 0 && x < width && y < height ) {
                return tileArr[ x - 1, y ];
            }
            else {
                return default( T );
            }
        }

        public T UpNeighbour( int x, int y ) {
            if ( x >= 0 && y >= 0 && x < width && y < height - 1 ) {
                return tileArr[ x, y + 1 ];
            }
            else {
                return default( T );
            }
        }

        public T DownNeighbour( int x, int y ) {
            if ( x >= 0 && y >= 1 && x < width && y < height ) {
                return tileArr[ x, y - 1 ];
            }
            else {
                return default( T );
            }
        }
    }
}