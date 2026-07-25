using UnityEngine;
using UnityEngine.Events;

public enum TipoCosto
{
    Vida,
    CampoDeVision,
    DistorsionVisual
}

public class InteractableObject : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    [SerializeField] private string nombreObjeto = "Objeto Interactivo";
    [SerializeField] private TipoCosto tipoCosto = TipoCosto.Vida;
    [SerializeField] private int costoVida = 20;
    [SerializeField] private float efectoNegativo = 15f;

    [Header("Beneficio al Activar")]
    [SerializeField] private float beneficioIluminacion = 0f;
    [SerializeField] private bool abreSalida = false;

    [Header("Feedback")]
    [SerializeField] private Material materialResaltado;
    [SerializeField] private AudioClip sonidoActivacion;
    [SerializeField] private UnityEvent OnActivado;

    private Material materialOriginal;
    private Renderer rendererObjeto;
    private AudioSource audioSource;
    private float tiempoGazeAcumulado = 0f;
    private bool activado = false;

    private void Awake()
    {
        rendererObjeto = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (rendererObjeto != null)
            materialOriginal = rendererObjeto.material;
    }

    public void ManteniendoGaze(float tiempoRequerido)
    {
        if (activado) return;

        tiempoGazeAcumulado += Time.deltaTime;

        float progreso = Mathf.Clamp01(tiempoGazeAcumulado / tiempoRequerido);

        if (materialResaltado != null && rendererObjeto != null)
        {
            rendererObjeto.material = materialResaltado;
        }

        if (tiempoGazeAcumulado >= tiempoRequerido)
        {
            Activar();
        }
    }

    public void SalirGaze()
    {
        if (activado) return;

        tiempoGazeAcumulado = 0f;
        if (rendererObjeto != null && materialOriginal != null)
            rendererObjeto.material = materialOriginal;
    }

    private void Activar()
    {
        if (activado) return;
        activado = true;

        Debug.Log($"Activado: {nombreObjeto}");

        GameCostManager costo = GameCostManager.Instancia;
        if (costo != null)
        {
            costo.AplicarCosto(costoVida, efectoNegativo);

            if (beneficioIluminacion > 0)
            {
                Light luz = GetComponentInChildren<Light>();
                if (luz != null)
                {
                    luz.intensity = beneficioIluminacion;
                    luz.enabled = true;
                }
                RenderSettings.ambientIntensity = Mathf.Clamp01(beneficioIluminacion / 10f);
            }

            if (abreSalida)
            {
                Debug.Log($"¡{nombreObjeto} ha abierto la salida!");
            }
        }

        if (sonidoActivacion != null && audioSource != null)
            audioSource.PlayOneShot(sonidoActivacion);

        OnActivado?.Invoke();

        if (rendererObjeto != null)
        {
            Color c = rendererObjeto.material.color;
            rendererObjeto.material.color = new Color(c.r, c.g, c.b, 0.5f);
        }

        enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<MobileVRCapsuleController>() != null)
        {
            SalirGaze();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
