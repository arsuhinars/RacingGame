using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Rendering.Universal;

public class CarConfig : MonoBehaviour
{
    public enum CarClass
    {
        A, B, C, S
    }

    private const string CarClassAKey = "classA";
    private const string CarClassBKey = "classB";
    private const string CarClassCKey = "classC";
    private const string CarClassSKey = "classS";

    public float Opacity
    {
        get => _opacity;
        set
        {
            _opacity = value;
            foreach (var renderer in meshRenderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetFloat("_Alpha", value);
                }
                renderer.materials = materials;
            }

            if (shadowDecal != null)
            {
                shadowDecal.fadeFactor = value;
            }
        }
    }
    public Color BodyColor
    {
        get => _color;
        set
        {
            _color = value;
            var materials = bodyMesh.materials;
            materials[bodyMaterialIndex].SetColor("_BaseColor", _color);

            bodyMesh.materials = materials;
        }
    }

    public LocalizedString carName;
    public CarClass classType;
    [Header("Characteristics")]
    public float maxSpeed;
    public float gasAcceleration;
    public float frictionDeceleration;
    public float breaksDeceleration;
    public float maxTurnAngle;
    public float angleSmoothTime = 0.4f;
    [Header("Model")]
    public DecalProjector shadowDecal;
    public MeshRenderer bodyMesh;
    public int bodyMaterialIndex;
    public float wheelRadius;
    public Transform wheelFR;
    public Transform wheelFL;
    public Transform wheelBR;
    public Transform wheelBL;
    [Header("Audio")]
    public AudioSource engineAudio;
    public float engineMinPitch = 0.5f;
    public float engienMaxPitch = 1.5f;

    private string localizedCarName = "";
    private float _opacity = 1f;
    private Color _color = Color.white;
    private MeshRenderer[] meshRenderers;
    
    private static readonly Dictionary<CarClass, string> localizedClassNames = new();

    public string GetLocalizedName()
    {
        if (localizedCarName.Length == 0)
            localizedCarName = carName.GetLocalizedString();

        return localizedCarName;
    }

    public static string GetCarClassName(CarClass carClass)
    {
        if (!localizedClassNames.ContainsKey(carClass))
        {
            localizedClassNames.Add(carClass, LocalizationHelper.GetLocalizedString(carClass switch
            {
                CarClass.A => CarClassAKey,
                CarClass.B => CarClassBKey,
                CarClass.C => CarClassCKey,
                CarClass.S => CarClassSKey,
                _ => ""
            }));
        }

        return localizedClassNames[carClass];
    }

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private void Start()
    {
        Opacity = _opacity;
        BodyColor = _color;
    }
}
