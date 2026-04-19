using UnityEngine;

namespace DAT.Core.DesignPatterns
{
    /// <summary>
    /// Minimal, fast, safe MonoBehaviour singleton.
    /// Optional persistence across scenes via inspector toggle.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isShuttingDown;

        [Header("Singleton Settings")]
        [Tooltip("Giữ lại qua scene (DontDestroyOnLoad)")]
        [SerializeField] private bool persistAcrossScenes = true;

        /// <summary>
        /// True if an instance exists (without creating one).
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Global instance. Creates one if missing (unless application is quitting).
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isShuttingDown) return null;
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;

                var go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
                return;
            }

            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
