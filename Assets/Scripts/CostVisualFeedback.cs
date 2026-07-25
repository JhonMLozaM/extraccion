using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CostVisualFeedback : MonoBehaviour
{
    [Header("Referencias de Post-Procesado URP")]
    [SerializeField] private Volume volumenGlobal;

    [Header("Configuración de Efectos")]
    [SerializeField] private float intensidadMaxVigneta = 0.6f;
    [SerializeField] private float intensidadMaxAberracion = 1f;
    [SerializeField] private float saturacionMinima = -100f;

    private Vignette vigneta;
    private ChromaticAberration aberracionCromatica;
    private ColorAdjustments ajustesColor;

    private void Start()
    {
        if (volumenGlobal == null)
            volumenGlobal = GetComponent<Volume>();

        if (volumenGlobal == null)
        {
            Debug.LogError("CostVisualFeedback: No hay Volume asignado. Asigna el Volume Global de URP.");
            enabled = false;
            return;
        }

        volumenGlobal.profile.TryGet(out vigneta);
        volumenGlobal.profile.TryGet(out aberracionCromatica);
        volumenGlobal.profile.TryGet(out ajustesColor);
    }

    private void Update()
    {
        if (GameCostManager.Instancia == null) return;

        float vidaNorm = GameCostManager.Instancia.VidaNormalizada;
        float poderNorm = GameCostManager.Instancia.PoderNormalizada;
        float recursosNorm = GameCostManager.Instancia.RecursosNormalizada;

        float nivelCosto = 1f - Mathf.Min(vidaNorm, Mathf.Min(poderNorm, recursosNorm));

        if (vigneta != null)
        {
            vigneta.intensity.Override(Mathf.Lerp(0f, intensidadMaxVigneta, nivelCosto));
            vigneta.smoothness.Override(Mathf.Lerp(0.3f, 0.8f, nivelCosto));
        }

        if (aberracionCromatica != null)
        {
            aberracionCromatica.intensity.Override(Mathf.Lerp(0f, intensidadMaxAberracion, nivelCosto));
        }

        if (ajustesColor != null)
        {
            float saturacion = Mathf.Lerp(0f, saturacionMinima, nivelCosto);
            ajustesColor.saturation.Override(saturacion);
        }
    }
}
