namespace Ascension
{
    /// <summary>Global time-scale knobs that are not the engine's Time.timeScale (e.g. Chronomancer enemy slow).</summary>
    public static class GlobalTime
    {
        public static float EnemyTimeScale = 1f;
        public static void Reset() => EnemyTimeScale = 1f;
    }
}
