using UnityEngine;

namespace AsobiDemo
{
    public class Crosshair : MonoBehaviour
    {
        Camera _cam;

        void Start()
        {
            _cam = Camera.main;
            Cursor.visible = false;
        }

        void Update()
        {
            var mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }

        void OnDestroy()
        {
            Cursor.visible = true;
        }
    }
}
