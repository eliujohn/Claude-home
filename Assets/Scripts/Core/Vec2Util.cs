using UnityEngine;

namespace Ascension
{
    public static class Vec2Util
    {
        public static Vector2 FromAngle(float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }

        public static float Angle(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        /// <summary>Rotate unit vector 'from' toward 'to' by at most maxRadians.</summary>
        public static Vector2 RotateToward(Vector2 from, Vector2 to, float maxRadians)
        {
            if (from.sqrMagnitude < 1e-6f) return to;
            float a = Mathf.Atan2(from.y, from.x);
            float b = Mathf.Atan2(to.y, to.x);
            float d = Mathf.DeltaAngle(a * Mathf.Rad2Deg, b * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float step = Mathf.Clamp(d, -maxRadians, maxRadians);
            float n = a + step;
            return new Vector2(Mathf.Cos(n), Mathf.Sin(n));
        }
    }
}
