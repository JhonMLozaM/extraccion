using UnityEngine;
using UnityEngine.Events;

public class GameCostManager : MonoBehaviour
{
    public static GameCostManager Instancia { get; private set; }

    [Header("Variables del Jugador")]
    [SerializeField] private float vidaActual = 100f;
    [SerializeField] private float poder = 50f;
    [SerializeField] private float recursos = 30f;

    [Header("Límites")]
    [SerializeField] private float vidaMax = 100f;
    [SerializeField] private float poderMax = 100f;
    [SerializeField] private float recursosMax = 100f;

    [Header("Eventos")]
    public UnityEvent<float> OnVidaCambiada;
    public UnityEvent<float> OnPoderCambiado;
    public UnityEvent<float> OnRecursosCambiados;
    public UnityEvent<float> OnEfectoNegativoAplicado;

    public float VidaActual => vidaActual;
    public float Poder => poder;
    public float Recursos => recursos;

    public float VidaNormalizada => vidaActual / vidaMax;
    public float PoderNormalizada => poder / poderMax;
    public float RecursosNormalizada => recursos / recursosMax;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AplicarCosto(int costVida, float efectoNegativo)
    {
        float vidaAnterior = vidaActual;

        vidaActual = Mathf.Clamp(vidaActual - costVida, 0, vidaMax);
        poder = Mathf.Clamp(poder - efectoNegativo, 0, poderMax);
        recursos = Mathf.Clamp(recursos - efectoNegativo, 0, recursosMax);

        OnVidaCambiada?.Invoke(vidaActual);
        OnPoderCambiado?.Invoke(poder);
        OnRecursosCambiados?.Invoke(recursos);

        float perdida = vidaAnterior - vidaActual;
        if (perdida > 0)
        {
            OnEfectoNegativoAplicado?.Invoke(perdida / vidaMax);
        }

        if (vidaActual <= 0)
        {
            Debug.Log("Game Over: El jugador ha muerto pagando el costo.");
        }
    }

    public void AplicarBeneficio(float beneficioVida, float beneficioPoder, float beneficioRecursos)
    {
        vidaActual = Mathf.Clamp(vidaActual + beneficioVida, 0, vidaMax);
        poder = Mathf.Clamp(poder + beneficioPoder, 0, poderMax);
        recursos = Mathf.Clamp(recursos + beneficioRecursos, 0, recursosMax);

        OnVidaCambiada?.Invoke(vidaActual);
        OnPoderCambiado?.Invoke(poder);
        OnRecursosCambiados?.Invoke(recursos);
    }
}
