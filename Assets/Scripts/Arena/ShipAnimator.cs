using UnityEngine;

namespace AsobiDemo
{
    public class ShipAnimator : MonoBehaviour
    {
        public Sprite[] frames; // 12 frames: 4 rows x 3 cols
        public float frameRate = 8f;

        SpriteRenderer _sr;
        int _row; // 0=down, 1=left, 2=right, 3=up
        float _frameTimer;
        int _frameCol;
        Vector3 _lastPos;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _lastPos = transform.position;
        }

        void Update()
        {
            var delta = transform.position - _lastPos;
            _lastPos = transform.position;

            if (delta.sqrMagnitude > 0.0001f)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    _row = delta.x < 0 ? 1 : 2;
                else
                    _row = delta.y < 0 ? 0 : 3;

                _frameTimer += Time.deltaTime;
                if (_frameTimer >= 1f / frameRate)
                {
                    _frameTimer = 0;
                    _frameCol = (_frameCol + 1) % 3;
                }
            }
            else
            {
                _frameCol = 1; // idle center frame
            }

            int idx = _row * 3 + _frameCol;
            if (frames != null && idx < frames.Length)
                _sr.sprite = frames[idx];
        }
    }
}
