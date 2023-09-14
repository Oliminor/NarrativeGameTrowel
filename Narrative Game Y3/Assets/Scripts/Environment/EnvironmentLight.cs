using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

enum LightSource
{
    EnvironmentLight,
    TableSpotLight
}

public class EnvironmentLight : MonoBehaviour
{
    public static EnvironmentLight instance;

    Light directionalLight;

    [SerializeField] private float lerpTime;

    [SerializeField] private EnvironmentSO defaultLight;

    [SerializeField] private EnvironmentSO changableScriptableObject;

    [SerializeField] private float intensity;

    [Range(0f, 360f)]
    [SerializeField] private float lightXDirection;

    [Range(0f, 360f)]
    [SerializeField] private float lightYDirection;

    [Range(1500, 20000)]
    [SerializeField] private float temperature;

    [SerializeField] private Color filterColor;

    private EnvironmentSO tempLight;

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (EnvironmentLight)");
        instance = this;

        changableScriptableObject = null;
    }

    void Start()
    {
        directionalLight = GetComponent<Light>();
    }

    private void OnValidate()
    {
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (!changableScriptableObject)
        {
            ApplyEnvironmentLight(defaultLight);
            tempLight = null;
            return;
        }

        LoadScritableObject();

        changableScriptableObject.Intensity = intensity;
        changableScriptableObject.LightXDirection = lightXDirection;
        changableScriptableObject.LightYDirection = lightYDirection;
        changableScriptableObject.Temperature = temperature;
        changableScriptableObject.FilterColor = filterColor;

#if UNITY_EDITOR
        EditorUtility.SetDirty(changableScriptableObject);
#endif

        ApplyEnvironmentLight(changableScriptableObject);
    }

    private void LoadScritableObject()
    {
        if (changableScriptableObject == tempLight) return;

        ApplyEnvironmentLight(changableScriptableObject);

        tempLight = changableScriptableObject;
    }

    private void ApplyEnvironmentLight(EnvironmentSO _light)
    {
        directionalLight = GetComponent<Light>();

        directionalLight.intensity = _light.Intensity;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(_light.LightXDirection, _light.LightYDirection, 0));
        directionalLight.colorTemperature = _light.Temperature;
        directionalLight.color = _light.FilterColor;

        intensity = _light.Intensity;
        lightXDirection = _light.LightXDirection;
        lightYDirection = _light.LightYDirection;
        temperature = _light.Temperature;
        filterColor = _light.FilterColor;
    }

    public void ChangeToDefaultIntensity()
    {
        StartCoroutine(ChangeLightIE(defaultLight));
    }

    public void ChangeLight(EnvironmentSO _light)
    {
        StartCoroutine(ChangeLightIE(_light));
    }

    IEnumerator ChangeLightIE(EnvironmentSO _light)
    {
        float lerp = 0;
        float startIntenstiy = directionalLight.intensity;
        float endIntensity = _light.Intensity;

        float startXDirection = directionalLight.transform.rotation.eulerAngles.x;
        float endXDirection = _light.LightXDirection;

        float startYDirection = directionalLight.transform.rotation.eulerAngles.y;
        float endYDirection = _light.LightYDirection;

        float startTemperature = directionalLight.colorTemperature;
        float endTemperature = _light.Temperature;

        Color startColor = directionalLight.color;
        Color endColor = _light.FilterColor;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            lerp += Time.deltaTime / lerpTime;
            directionalLight.intensity = Mathf.Lerp(startIntenstiy, endIntensity, lerp);
            directionalLight.transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(startXDirection, startYDirection, 0)), Quaternion.Euler(new Vector3(endXDirection, endYDirection, 0)), lerp);
            directionalLight.colorTemperature = Mathf.Lerp(startTemperature, endTemperature, lerp);
            directionalLight.color = Color.Lerp(startColor, endColor, lerp);
        }
    }
}
