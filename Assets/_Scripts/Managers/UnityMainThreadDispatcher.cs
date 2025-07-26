using UnityEngine;
using System;
using System.Collections.Concurrent;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if (_instance == null)
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        _actions.Enqueue(action);
    }

    private void Update()
    {
        // Processa todas as ações enfileiradas no thread principal
        while (_actions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }
}