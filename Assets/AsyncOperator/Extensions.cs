using UnityEngine;

namespace AsyncOperator.Extensions {
    public static class Extensions {
        public static void GetComponent<T>( this GameObject gameObject, ref T component ) where T : class {
            if ( component == null ) {
                component = gameObject.GetComponent<T>();
            }
        }
    }
}