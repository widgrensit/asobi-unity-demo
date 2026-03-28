using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace AsobiDemo
{
    public class UnityMainThread : MonoBehaviour
    {
        static readonly ConcurrentQueue<Action> _queue = new();
        static UnityMainThread _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (_instance != null) return;
            var go = new GameObject("UnityMainThread");
            _instance = go.AddComponent<UnityMainThread>();
            DontDestroyOnLoad(go);
        }

        public static void Enqueue(Action action) => _queue.Enqueue(action);

        void Update()
        {
            while (_queue.TryDequeue(out var action))
                action?.Invoke();
        }
    }
}
