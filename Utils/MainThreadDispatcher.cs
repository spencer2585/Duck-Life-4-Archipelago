using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckLife4Archipelago.Utils;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private readonly Queue<Action> _queue = new Queue<Action>();

    public static MainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            GameObject obj = new GameObject("MainThreadDispatcher");
            _instance = obj.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
        }
    }

    private void Update()
    {
        while (true)
        {
            Action action;
            lock (_queue)
            {
                if (_queue.Count == 0) break;
                action = _queue.Dequeue();
            }
            action();
        }
    }
}