using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MobileVRCapsuleController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 2f;
    [SerializeField] private float sensibilidadGiroscopio = 1f;

    [Header("Reticula de Gaze")]
    [SerializeField] private GameObject reticulaPrefab;
    [SerializeField] private float distanciaReticula = 5f;
    [SerializeField] private float tiempoGazeActivacion = 2f;

    private CharacterController characterController;
    private Camera camaraVR;
    private Transform reticulaTransform;
    private GameObject reticulaInstancia;

    private Vector2 rotacionActual;
    private bool gyroHabilitado;
    private Gyroscope giroscopio;
    private InteractableObject objetivoGazeActual;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camaraVR = GetComponentInChildren<Camera>();

        InputSystem.EnableDevice(Accelerometer.current);
        InputSystem.EnableDevice(AttitudeSensor.current);
    }

    private void Start()
    {
        ConfigurarGiroscopio();
        CrearReticula();

        Input.gyro.enabled = true;
        gyroHabilitado = SystemInfo.supportsGyroscope;

        if (!gyroHabilitado)
            Debug.LogWarning("Este dispositivo no soporta giroscopio. Usa el mouse en el Editor.");
    }

    private void ConfigurarGiroscopio()
    {
        giroscopio = Input.gyro;
        if (giroscopio != null)
        {
            giroscopio.enabled = true;
            gyroHabilitado = true;
        }
    }

    private void CrearReticula()
    {
        if (reticulaPrefab != null)
        {
            reticulaInstancia = Instantiate(reticulaPrefab, camaraVR.transform);
            reticulaInstancia.transform.localPosition = new Vector3(0, 0, distanciaReticula);
            reticulaInstancia.transform.localRotation = Quaternion.identity;
            reticulaTransform = reticulaInstancia.transform;
        }
        else
        {
            GameObject ret = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ret.transform.localScale = Vector3.one * 0.05f;
            ret.name = "ReticulaGaze";
            ret.transform.SetParent(camaraVR.transform);
            ret.transform.localPosition = new Vector3(0, 0, distanciaReticula);
            Destroy(ret.GetComponent<Collider>());
            reticulaTransform = ret.transform;
        }
    }

    private void Update()
    {
        RotarConGiroscopio();
        MoverHaciaAdelante();
        DetectarGazeInteraccion();
    }

    private void RotarConGiroscopio()
    {
        Vector3 rotacion = Vector3.zero;

        if (gyroHabilitado && giroscopio != null)
        {
            Quaternion correccion = new Quaternion(0, 0, 1, 0);
            Quaternion rotacionGiro = giroscopio.attitude * correccion;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionGiro, sensibilidadGiroscopio * Time.deltaTime);
        }
        else
        {
            float mouseX = Input.GetAxis("Mouse X") * sensibilidadGiroscopio;
            float mouseY = Input.GetAxis("Mouse Y") * sensibilidadGiroscopio;
            rotacionActual.x += mouseX;
            rotacionActual.y = Mathf.Clamp(rotacionActual.y - mouseY, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotacionActual.y, rotacionActual.x, 0);
        }
    }

    private void MoverHaciaAdelante()
    {
        Vector3 direccion = camaraVR.transform.forward;
        direccion.y = 0;
        direccion.Normalize();

        Vector2 inputMovimiento = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 delta = Touchscreen.current.primaryTouch.delta.ReadValue();
            if (delta.magnitude > 10f)
            {
                inputMovimiento = delta.normalized;
            }
        }

        if (Input.GetMouseButton(0) && !gyroHabilitado)
            inputMovimiento = Vector2.up;

        if (Mathf.Abs(inputMovimiento.magnitude) > 0.1f)
        {
            characterController.SimpleMove(direccion * velocidadMovimiento);
        }
    }

    private void DetectarGazeInteraccion()
    {
        InteractableObject interactivo = null;
        Vector3 puntoGolpe = Vector3.zero;

        if (Physics.Raycast(camaraVR.transform.position, camaraVR.transform.forward, out RaycastHit hit, distanciaReticula))
        {
            interactivo = hit.collider.GetComponent<InteractableObject>();
            puntoGolpe = hit.point;
        }

        if (interactivo != null)
        {
            if (objetivoGazeActual != null && objetivoGazeActual != interactivo)
                objetivoGazeActual.SalirGaze();

            objetivoGazeActual = interactivo;
            objetivoGazeActual.ManteniendoGaze(tiempoGazeActivacion);

            if (reticulaTransform != null)
                reticulaTransform.position = puntoGolpe;
        }
        else
        {
            if (objetivoGazeActual != null)
            {
                objetivoGazeActual.SalirGaze();
                objetivoGazeActual = null;
            }

            if (reticulaTransform != null)
                reticulaTransform.localPosition = new Vector3(0, 0, distanciaReticula);
        }
    }
}
