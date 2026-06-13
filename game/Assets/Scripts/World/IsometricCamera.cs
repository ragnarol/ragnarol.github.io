using UnityEngine;
using UnityEngine.InputSystem;

namespace FadingSuns.World
{
    /// <summary>
    /// Smooth-follow camera for isometric view.
    /// Follows the party leader; supports zoom and edge-pan.
    /// Unity project must use Isometric Z-as-Y tilemap for correct depth sorting.
    /// </summary>
    public class IsometricCamera : MonoBehaviour
    {
        [Header("Follow Target")]
        public Transform target;
        public float followSpeed = 8f;
        public Vector3 offset = new(0, 2f, -10f);

        [Header("Zoom")]
        public Camera cam;
        public float zoomSpeed = 2f;
        public float minZoom = 3f;
        public float maxZoom = 10f;

        [Header("Edge Pan")]
        public bool enableEdgePan = true;
        public float edgePanSpeed = 5f;
        public float edgePanThreshold = 20f; // pixels from screen edge

        [Header("Bounds")]
        public bool useBounds = false;
        public Bounds worldBounds;

        void Awake()
        {
            if (cam == null) cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (target != null)
                FollowTarget();

            if (enableEdgePan)
                HandleEdgePan();

            HandleZoom();

            if (useBounds)
                ClampToBounds();
        }

        private void FollowTarget()
        {
            var desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        }

        private void HandleEdgePan()
        {
            var mouse = Mouse.current.position.ReadValue();
            var pan = Vector3.zero;

            if (mouse.x < edgePanThreshold) pan.x -= edgePanSpeed * Time.deltaTime;
            if (mouse.x > Screen.width - edgePanThreshold) pan.x += edgePanSpeed * Time.deltaTime;
            if (mouse.y < edgePanThreshold) pan.y -= edgePanSpeed * Time.deltaTime;
            if (mouse.y > Screen.height - edgePanThreshold) pan.y += edgePanSpeed * Time.deltaTime;

            transform.position += pan;
        }

        private void HandleZoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0)) return;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
        }

        private void ClampToBounds()
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, worldBounds.min.x, worldBounds.max.x);
            pos.y = Mathf.Clamp(pos.y, worldBounds.min.y, worldBounds.max.y);
            transform.position = pos;
        }

        public void SnapToTarget()
        {
            if (target != null)
                transform.position = target.position + offset;
        }
    }
}
