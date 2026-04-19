using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DAT.Core.DesignPatterns;

namespace DAT.Core
{
    public enum Orientation
    {
        Portrait,
        Landscape
    }

    [AddComponentMenu("DAT SDK/Core/ResponsiveManager")]
    [DisallowMultipleComponent]
    public class ResponsiveManager : Singleton<ResponsiveManager>
    {
        [Header("Current Orientation (ReadOnly)")]
        [SerializeField] private Orientation currentOrientation = Orientation.Portrait;
        public Orientation CurrentOrientation => currentOrientation;

        [Header("Events")]
        public UnityEvent OnPortrait;
        public UnityEvent OnLandscape;
        public UnityEvent<Orientation> OnOrientationChanged;

        [Header("Auto Toggle Objects")]
        [Tooltip("Những GameObject này sẽ được bật khi ở chế độ Portrait và tắt khi ở Landscape")]
        [SerializeField] private List<GameObject> portraitOnlyObjects = new List<GameObject>();

        [Tooltip("Những GameObject này sẽ được bật khi ở chế độ Landscape và tắt khi ở Portrait")]
        [SerializeField] private List<GameObject> landscapeOnlyObjects = new List<GameObject>();

        [Header("Canvas Scaler (Optional)")]
        [Tooltip("Nếu được set, CanvasScaler sẽ đổi resolution theo hướng màn hình")]
        [SerializeField] private CanvasScaler targetCanvasScaler;
        [Tooltip("Tự động tìm CanvasScaler gần nhất nếu để trống")]
        [SerializeField] private bool autoFindCanvasScaler = true;

        [Tooltip("Reference Resolution khi Portrait (width x height)")]
        [SerializeField] private Vector2 portraitResolution = new Vector2(1080, 1920);

        [Tooltip("Reference Resolution khi Landscape (width x height)")]
        [SerializeField] private Vector2 landscapeResolution = new Vector2(1920, 1080);

        [Header("Detection Settings")]
        [Tooltip("Sử dụng Screen.orientation/DeviceOrientation nếu có, fallback về tỉ lệ màn hình")]
        [SerializeField] private bool useDeviceOrientation = false;
        [Tooltip("Ngưỡng thay đổi tỉ lệ để coi như đã đổi hướng")] 
        [SerializeField] private float aspectChangeThreshold = 0.01f;

        private float lastAspectRatio = -1;

        void Start()
        {
            // Ensure singleton initialization happens early
            // (Awake() in base already sets persistence if enabled)
            if (targetCanvasScaler == null && autoFindCanvasScaler)
            {
                targetCanvasScaler = GetComponentInParent<CanvasScaler>();
            }
            EvaluateOrientation(forceInvoke: true);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        void LateUpdate() // đảm bảo Canvas đã layout xong
        {
            EvaluateOrientation();
        }

        private void EvaluateOrientation(bool forceInvoke = false)
        {
            float aspect = (float)Screen.width / Screen.height;

            if (!forceInvoke && Mathf.Abs(aspect - lastAspectRatio) < aspectChangeThreshold)
                return;

            lastAspectRatio = aspect;

            Orientation newOrientation = DetermineOrientation(aspect);

            if (newOrientation != currentOrientation || forceInvoke)
            {
                currentOrientation = newOrientation;


                ToggleObjectsByOrientation(currentOrientation);
                UpdateCanvasScaler(currentOrientation);

                OnOrientationChanged?.Invoke(currentOrientation);
                if (currentOrientation == Orientation.Landscape)
                    OnLandscape?.Invoke();
                else
                    OnPortrait?.Invoke();
            }
        }

        private Orientation DetermineOrientation(float aspect)
        {
            if (useDeviceOrientation)
            {
                // Try Screen.orientation first
                switch (Screen.orientation)
                {
                    case ScreenOrientation.LandscapeLeft:
                    case ScreenOrientation.LandscapeRight:
                        return Orientation.Landscape;
                    case ScreenOrientation.Portrait:
                    case ScreenOrientation.PortraitUpsideDown:
                        return Orientation.Portrait;
                }

                // Fallback to Input.deviceOrientation
                var dev = Input.deviceOrientation;
                if (dev == DeviceOrientation.LandscapeLeft || dev == DeviceOrientation.LandscapeRight)
                    return Orientation.Landscape;
                if (dev == DeviceOrientation.Portrait || dev == DeviceOrientation.PortraitUpsideDown)
                    return Orientation.Portrait;
            }

            // Fallback to aspect ratio
            return aspect >= 1f ? Orientation.Landscape : Orientation.Portrait;
        }

        private void ToggleObjectsByOrientation(Orientation orientation)
        {
            bool isPortrait = orientation == Orientation.Portrait;

            foreach (var obj in portraitOnlyObjects)
            {
                if (obj != null && obj.activeSelf != isPortrait)
                    obj.SetActive(isPortrait);
            }

            foreach (var obj in landscapeOnlyObjects)
            {
                if (obj != null && obj.activeSelf != !isPortrait)
                    obj.SetActive(!isPortrait);
            }
        }

        private void UpdateCanvasScaler(Orientation orientation)
        {
            if (targetCanvasScaler == null) return;

            targetCanvasScaler.referenceResolution =
                orientation == Orientation.Landscape ? landscapeResolution : portraitResolution;

        }

        /// <summary>
        /// Gọi thủ công khi cần cập nhật ngay (ví dụ thay đổi cấu hình trong runtime).
        /// </summary>
        public void ForceEvaluate()
        {
            EvaluateOrientation(forceInvoke: true);
        }
    }
}
