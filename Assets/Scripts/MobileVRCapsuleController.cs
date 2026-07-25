using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class MobileVRCapsuleController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 0.05f;
    [SerializeField] private float sensibilidadMouse = 2f;

    [Header("Reticula de Gaze")]
    [SerializeField] private GameObject reticulaPrefab;
    [SerializeField] private float distanciaReticula = 10f;
    [SerializeField] private float tiempoGazeActivacion = 2f;

    [Header("Opciones VR")]
    [SerializeField] private bool usarXR = true;

    private CharacterController characterController;
    private Camera camaraVR;
    private Transform reticulaTransform;
    private GameObject reticulaInstancia;

    private Vector2 rotacionActual;
    private bool gyroHabilitado;
    private UnityEngine.Gyroscope giroscopio;
    private InteractableObject objetivoGazeActual;
    private bool xrActivo;

    private UnityEngine.XR.InputDevice xrDispositivo;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camaraVR = GetComponentInChildren<Camera>();

        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
        if (AttitudeSensor.current != null)
            InputSystem.EnableDevice(AttitudeSensor.current);
    }

    private void Start()
    {
        ConfigurarModoVR();
        ConfigurarGiroscopio();
        CrearReticula();

        if (!xrActivo)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ConfigurarModoVR()
    {
        xrActivo = usarXR && XRSettings.enabled;

        if (xrActivo)
        {
            xrDispositivo = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.Head);
            camaraVR.transform.localPosition = Vector3.zero;
            camaraVR.transform.localRotation = Quaternion.identity;
        }
    }

    private void ConfigurarGiroscopio()
    {
        giroscopio = Input.gyro;
        if (giroscopio != null)
        {
            giroscopio.enabled = true;
            gyroHabilitado = SystemInfo.supportsGyroscope;
        }

        if (!gyroHabilitado && !xrActivo)
            Debug.LogWarning("Sin giroscopio ni XR. Usa el mouse en el Editor.");
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
            ret.transform.localScale = Vector3.one * 0.02f;
            ret.name = "ReticulaGaze";
            ret.transform.SetParent(camaraVR.transform);
            ret.transform.localPosition = new Vector3(0, 0, distanciaReticula);
            Destroy(ret.GetComponent<Collider>());
            reticulaTransform = ret.transform;
        }
    }

    private void Update()
    {
        if (!xrActivo)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        RotarConVRoGiroscopio();
        MoverHaciaAdelante();
        DetectarGazeInteraccion();
    }

    private void RotarConVRoGiroscopio()
    {
        if (xrActivo)
        {
            if (xrDispositivo.isValid &&
                xrDispositivo.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rot))
            {
                transform.rotation = rot;
            }
        }
        else if (gyroHabilitado && giroscopio != null)
        {
            Quaternion correccion = new Quaternion(0, 0, 1, 0);
            Quaternion rotacionGiro = giroscopio.attitude * correccion;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionGiro, Time.deltaTime * 2f);
        }
        else
        {
            if (Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue() * sensibilidadMouse * 0.01f;
                rotacionActual.x += delta.x;
                rotacionActual.y = Mathf.Clamp(rotacionActual.y - delta.y, -90f, 90f);
            }
            transform.localRotation = Quaternion.Euler(rotacionActual.y, rotacionActual.x, 0);
        }
    }

    private void MoverHaciaAdelante()
    {
        Vector3 adelante = camaraVR.transform.forward;
        Vector3 derecha = camaraVR.transform.right;
        adelante.y = 0;
        derecha.y = 0;
        adelante.Normalize();
        derecha.Normalize();

        Vector3 movimiento = Vector3.zero;

        if (xrActivo)
        {
            UnityEngine.XR.InputDevice controller = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (controller.isValid &&
                controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 joystick))
            {
                movimiento = adelante * joystick.y + derecha * joystick.x;
            }
        }
        else
        {
            Vector2 inputTeclado = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) inputTeclado.y += 1;
                if (Keyboard.current.sKey.isPressed) inputTeclado.y -= 1;
                if (Keyboard.current.aKey.isPressed) inputTeclado.x -= 1;
                if (Keyboard.current.dKey.isPressed) inputTeclado.x += 1;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                Vector2 delta = Touchscreen.current.primaryTouch.delta.ReadValue();
                if (delta.magnitude > 10f)
                    inputTeclado = delta.normalized;
            }

            movimiento = adelante * inputTeclado.y + derecha * inputTeclado.x;
            if (movimiento.magnitude > 1f)
                movimiento.Normalize();
        }

        characterController.Move(movimiento * velocidadMovimiento);
    }

    private void DetectarGazeInteraccion()
    {
        InteractableObject interactivo = null;
        Vector3 puntoGolpe = Vector3.zero;
        bool botonPresionado = false;

        if (Physics.Raycast(camaraVR.transform.position, camaraVR.transform.forward, out RaycastHit hit, distanciaReticula))
        {
            interactivo = hit.collider.GetComponent<InteractableObject>();
            puntoGolpe = hit.point;
        }

        if (xrActivo)
        {
            UnityEngine.XR.InputDevice controller = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (controller.isValid)
                botonPresionado = controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool trig) && trig;
        }

        if (interactivo != null)
        {
            if (objetivoGazeActual != null && objetivoGazeActual != interactivo)
                objetivoGazeActual.SalirGaze();

            objetivoGazeActual = interactivo;

            if (botonPresionado)
                objetivoGazeActual.ActivarInstantaneo();
            else
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
