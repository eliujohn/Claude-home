using UnityEngine;

namespace Ascension
{
    /// <summary>Smooth follow camera with room-bounds clamp and trauma-style shake.</summary>
    public class CameraRig : MonoBehaviour
    {
        static float shake;
        static Rect bounds = new Rect(-16f, -9f, 32f, 18f);
        Camera cam;
        Vector3 smoothed;

        public static void Shake(float amount) => shake = Mathf.Max(shake, amount);
        public static void SetBounds(Rect r) => bounds = r;

        void Awake()
        {
            cam = GetComponent<Camera>();
            smoothed = new Vector3(0, 0, -10f);
        }

        void LateUpdate()
        {
            var p = Player.Current;
            if (p == null) return;
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            float minX = bounds.xMin - 1f + halfW, maxX = bounds.xMax + 1f - halfW;
            float minY = bounds.yMin - 1f + halfH, maxY = bounds.yMax + 1f - halfH;
            Vector2 target = p.Position;
            target.x = minX > maxX ? bounds.center.x : Mathf.Clamp(target.x, minX, maxX);
            target.y = minY > maxY ? bounds.center.y : Mathf.Clamp(target.y, minY, maxY);

            smoothed = Vector3.Lerp(smoothed, new Vector3(target.x, target.y, -10f), 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
            Vector2 jitter = shake > 0.001f ? Random.insideUnitCircle * shake : Vector2.zero;
            transform.position = smoothed + (Vector3)jitter;
            shake = Mathf.MoveTowards(shake, 0f, Time.unscaledDeltaTime * 1.2f);
        }
    }
}
