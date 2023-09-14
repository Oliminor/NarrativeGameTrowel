using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum LampTarget
{
    UP = 0,
    DIORAMA = 1
}

public class Lamp : MonoBehaviour
{
    public static Lamp instance;

    [SerializeField] MultiAimConstraint constraint;

    int currentIndex = 0;

    float lerp = 1;

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (Lamp)");
        instance = this;
    }

    private void Update()
    {
        lerp = Mathf.Clamp01(lerp);

        lerp += Time.deltaTime;

        var sources = constraint.data.sourceObjects;

        sources.SetWeight(currentIndex, Mathf.Lerp(0, 1, lerp));

        for (int i = 0; i < constraint.data.sourceObjects.Count; i++)
        {
            if (i == currentIndex) continue;
            sources.SetWeight(i, Mathf.Lerp(1, 0, lerp));
        }

        constraint.data.sourceObjects = sources;
    }

    public void ChangeLampTarget(LampTarget _target)
    {
        currentIndex = (int)_target;
        lerp = 0;
    }
}
