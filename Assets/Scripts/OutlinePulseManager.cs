using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Met à jour tous les OutlineController actifs en un seul Update.
/// Placer ce script sur n'importe quel GameObject persistant (ex: GameManager).
/// </summary>
public class OutlinePulseManager : MonoBehaviour
{
    private static OutlinePulseManager _instance;
    private static readonly List<OutlineController> _controllers = new List<OutlineController>();

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this);
    }

    public static void Register(OutlineController c)
    {
        if (!_controllers.Contains(c))
            _controllers.Add(c);
    }

    public static void Unregister(OutlineController c)
    {
        _controllers.Remove(c);
    }



    void Update()
    {
        float t = Time.time;
        for (int i = _controllers.Count - 1; i >= 0; i--)
        {
            if (_controllers[i] == null) { _controllers.RemoveAt(i); continue; }
            _controllers[i].Tick(t);
        }
    }
}