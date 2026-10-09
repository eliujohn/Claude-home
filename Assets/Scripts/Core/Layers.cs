using UnityEngine;

namespace Ascension
{
    /// <summary>Layer indices used without needing named layers in TagManager.</summary>
    public static class Layers
    {
        public const int Obstacle = 6;
        public const int Player = 8;
        public const int Enemy = 9;
        public const int Projectile = 10;

        public static int ObstacleMask => 1 << Obstacle;

        public static void ConfigurePhysics()
        {
            Physics2D.gravity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(Player, Enemy, true);       // Hades-style: bodies overlap, hits are explicit
            Physics2D.IgnoreLayerCollision(Projectile, Projectile, true);
            Physics2D.IgnoreLayerCollision(Projectile, Obstacle, false);
        }
    }
}
