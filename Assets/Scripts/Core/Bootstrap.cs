using UnityEngine;

namespace Ascension
{
    /// <summary>Spawns the game into whatever scene is open, so no scene authoring is required.</summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;
            new GameObject("Ascension").AddComponent<GameManager>();
        }
    }
}
