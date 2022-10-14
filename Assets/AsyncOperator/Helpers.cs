using UnityEngine;

namespace AsyncOperator.Helpers {
    public static class Helpers {
        public static Vector2 ConvertPixelCoordinateToWorldPosition( Camera camera, Vector2 pixelCoordinate ) => camera.ScreenToWorldPoint( pixelCoordinate );
    }
}