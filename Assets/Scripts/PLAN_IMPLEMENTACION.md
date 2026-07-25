# PLAN DE IMPLEMENTACIÓN - "Todo Tiene Un Costo" VR

## 1. ARQUITECTURA GENERAL

```
Escena Principal (SampleScene)
│
├── GameManager (GameObject vacío)
│   └── GameCostManager.cs  ← Singleton, DontDestroyOnLoad
│
├── Player (GameObject)
│   ├── CharacterController
│   ├── MobileVRCapsuleController.cs
│   └── Camera (hijo)
│       ├── Reticula (hijo, esfera/gameobject)
│       └── CostVisualFeedback.cs + Volume (post-procesado URP)
│
├── Cueva (Escenario 3D)
│   ├── Palanca → InteractableObject.cs (costo: vida, beneficio: iluminación)
│   ├── Antorcha → InteractableObject.cs (costo: visión, beneficio: luz)
│   └── Cristal  → InteractableObject.cs (costo: distorsión, beneficio: abre salida)
│
└── ZonaSalida (trigger al final del mapa)
```

## 2. ORDEN DE EJECUCIÓN EN LA ESCENA

1. **GameCostManager** se inicializa primero (Awake → Singleton)
2. **MobileVRCapsuleController** configura giroscopio y retícula (Start)
3. **CostVisualFeedback** se suscribe a cambios de vida (Start)
4. **InteractableObject** en cada objeto, espera gaze (2s)

## 3. FLUJO DE INTERACCIÓN

```
Jugador mira objeto → Reticula se posiciona sobre él
    → timer 0s → 2s (material cambia de color)
    → se activa → InteractableObject.AplicarCosto()
        → GameCostManager.AplicarCosto(costoVida, efectoNegativo)
            → CostVisualFeedback.Update() reacciona a valores bajos
                → más viñeta, aberración cromática, escala de grises
```

## 4. SCRIPTS GENERADOS (Assets/Scripts/)

| Script | Función |
|--------|---------|
| `MobileVRCapsuleController.cs` | Rotación con giroscopio + movimiento touch + raycast gaze |
| `GameCostManager.cs` | Singleton: gestiona vida, poder, recursos y costos |
| `InteractableObject.cs` | Detección gaze 2s → aplica costo + beneficio en cadena |
| `CostVisualFeedback.cs` | Post-procesado URP: viñeta, aberración, saturación |

## 5. PAQUETES XR A INSTALAR

Unity → Window → Package Manager:
1. `Google Cardboard XR Plugin` (com.google.xr.cardboard)
2. `XR Plugin Management` (com.unity.xr.management)
3. En Project Settings → XR Plug-in Management → Android → habilitar "Cardboard"

## 6. CONFIGURACIÓN DE BUILD (Android)

- Build Settings → Switch Platform → Android
- Player Settings → Other Settings:
  - Graphics APIs: Vulkan (o OpenGL ES 3.0 si hay problemas)
  - Multithreaded Rendering: ✓
  - Minimum API Level: Android 8.0 (API 26)
- Resolution and Presentation:
  - Default Orientation: Landscape Left
- XR Settings:
  - Virtual Reality Supported → ✓
  - Cardboard SDK → agregado automáticamente

## 7. PRUEBAS EN EDITOR

Usar clic derecho + mouse para simular giroscopio cuando no hay gyro.
