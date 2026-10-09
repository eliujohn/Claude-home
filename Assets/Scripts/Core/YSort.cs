using UnityEngine;

namespace Ascension
{
    /// <summary>Fake-isometric depth: renderers sort by world Y so tall things overlap correctly.</summary>
    public class YSort : MonoBehaviour
    {
        SpriteRenderer[] renderers;
        int[] baseOrders;

        void Start()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            baseOrders = new int[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseOrders[i] = renderers[i].sortingOrder;
        }

        void LateUpdate()
        {
            int y = -Mathf.RoundToInt(transform.position.y * 100f);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].sortingOrder = y + baseOrders[i];
        }
    }
}
