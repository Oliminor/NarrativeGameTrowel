using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global outline is just to make easier to setup every outline at the same time instead of one by one
/// </summary>
public class GlobalOutlineManager : MonoBehaviour
{
    public static GlobalOutlineManager instance;

    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;
    [SerializeField] private Color outlineColor;
    [SerializeField] private Outline.Mode outlineMode;

    [SerializeField] bool resetOutlineAtStart;

    Outline[] outlines;

    public float GetOutlineWIdth() { return outlineWidth; }
    public Color GetOutlineColor() { return outlineColor; }
    public Outline.Mode GetOutlineMode() { return outlineMode; }

    private void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (GlobalOutlineManager)");
        instance = this;
    }

    void Start()
    {
        outlines = FindObjectsOfType<Outline>();

        UpdateOutlienIfChanges();

        if (resetOutlineAtStart) SetAllOutlineWidth(0);
    }

    void OnValidate()
    {
        if (outlines == null) return;

        UpdateOutlienIfChanges();
    }

    /// <summary>
    /// Updates the outline if something changed on the inspector mode (Only works play mode)
    /// </summary>
    void UpdateOutlienIfChanges()
    {
        SetAllOutlineWidth(outlineWidth);
        SetAllOutlineColor(outlineColor);
        SetAllOutlineMode(outlineMode);
    }

    /// <summary>
    /// Changes the Outline width (thicknes)
    /// </summary>
    void SetAllOutlineWidth(float _width)
    {
        foreach (var item in outlines)
        {
            item.SetOutlineWidth(_width);
        }
    }

    /// <summary>
    ///  Changes the outline Color
    /// </summary>
    void SetAllOutlineColor(Color _color)
    {
        foreach (var item in outlines)
        {
            item.SetOutlineColor(_color);
        }
    }

    /// <summary>
    /// Changes the outline Mode
    /// </summary>
    void SetAllOutlineMode(Outline.Mode _mode)
    {
        foreach (var item in outlines)
        {
            item.SetOutlineMode(_mode);
        }
    }
}
