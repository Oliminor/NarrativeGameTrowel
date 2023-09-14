using UnityEngine;


[CreateAssetMenu(fileName = "EnvironmentLight", menuName = "ScriptableObjects/EnvironmentLight", order = 2)]

public class EnvironmentSO : ScriptableObject
{
    [SerializeField] private float intensity = 0.5f;

    [Range(0f, 360f)]
    [SerializeField] private float lightXDirection = 50;

    [Range(0f, 360f)]
    [SerializeField] private float lightYDirection = 0;

    [Range(1500, 20000)]
    [SerializeField] private float temperature = 6587;

    [SerializeField] private Color filterColor = Color.white;

    public float Intensity { get { return intensity; } set { intensity = value; } }
    public float LightXDirection { get { return lightXDirection; }  set { lightXDirection = value; } }
    public float LightYDirection { get { return lightYDirection; }  set { lightYDirection = value; } }
    public float Temperature { get { return temperature; }  set { temperature = value; } }
    public Color FilterColor { get { return filterColor; } set { filterColor = value; } }
}
