using UnityEngine;

namespace AsobiDemo
{
    public class ArenaBounds : MonoBehaviour
    {
        void Start()
        {
            var w = GameConfig.ArenaWidth / GameConfig.PixelsPerUnit;
            var h = GameConfig.ArenaHeight / GameConfig.PixelsPerUnit;

            // Draw arena boundary using LineRenderer
            var lr = gameObject.AddComponent<LineRenderer>();
            lr.positionCount = 5;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = NavalTheme.Secondary * 0.6f;
            lr.endColor = NavalTheme.Secondary * 0.6f;

            lr.SetPositions(new Vector3[]
            {
                new(0, 0, 0),
                new(w, 0, 0),
                new(w, h, 0),
                new(0, h, 0),
                new(0, 0, 0)
            });
        }
    }
}
