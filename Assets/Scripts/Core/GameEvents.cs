using System;

namespace Ascension
{
    /// <summary>Static event bus. Systems publish here; SponsorManager and UI subscribe. No direct cross references.</summary>
    public static class GameEvents
    {
        public static event Action<Enemy> EnemyKilled;
        public static event Action<RoomController> RoomEntered;
        public static event Action<RoomController> RoomCleared;
        public static event Action<DamageInfo> PlayerDied;

        public static void RaiseEnemyKilled(Enemy e) => EnemyKilled?.Invoke(e);
        public static void RaiseRoomEntered(RoomController r) => RoomEntered?.Invoke(r);
        public static void RaiseRoomCleared(RoomController r) => RoomCleared?.Invoke(r);
        public static void RaisePlayerDied(DamageInfo d) => PlayerDied?.Invoke(d);
    }
}
