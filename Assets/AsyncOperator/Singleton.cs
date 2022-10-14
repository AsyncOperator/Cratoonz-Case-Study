using UnityEngine;

namespace AsyncOperator.Singleton {
    [HelpURL( "https://youtu.be/tE1qH8OxO2Y?t=234" )]
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour {
        public static T Instance { get; private set; }

        protected virtual void Awake() => Instance = this as T;

        protected virtual void OnApplicationQuit() {
            Instance = null;
            Destroy( gameObject );
        }
    }

    [HelpURL( "https://youtu.be/tE1qH8OxO2Y?t=234" )]
    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour {
        protected override void Awake() {
            if ( Instance != null ) {
                Destroy( gameObject );
                return;
            }
            else
                base.Awake();
        }
    }

    [HelpURL( "https://youtu.be/tE1qH8OxO2Y?t=234" )]
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour {
        protected override void Awake() {
            base.Awake();
            DontDestroyOnLoad( gameObject );
        }
    }
}