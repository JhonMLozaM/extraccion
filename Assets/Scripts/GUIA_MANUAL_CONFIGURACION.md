# GUÍA MANUAL — Configuración en el Editor

Sigue estos pasos en orden.

---

## 1. Instalar Paquetes

**Window > Package Manager:**
- `com.unity.xr.openxr` — Add package by name
- `com.unity.xr.management` — Add package by name (si no está)

---

## 2. Configurar XR

**Edit > Project Settings > XR Plug-in Management:**
- Pestaña **Windows** → marcar **OpenXR**
- Pestaña **Linux** → marcar **OpenXR**
- En **OpenXR** (dentro de la misma sección) → agregar **Interaction Profiles** según tu dispositivo
- Dejar el resto por defecto

---

## 3. Crear el Player

| Paso | Acción |
|------|--------|
| 1 | Jerarquía → clic derecho → **Create Empty** → nombre **Player** |
| 2 | Con Player seleccionado: **Add Component** → `CharacterController` |
| 3 | En CharacterController: Height = `1.8`, Radius = `0.3`, Center = `(0, 0.9, 0)` |
| 4 | **Add Component** → `MobileVRCapsuleController` |
| 5 | En el script: `Usar XR` = ✅ (marcado) |

---

## 4. Configurar VR Camera

| Paso | Acción |
|------|--------|
| 1 | Dentro de Player → clic derecho → **Camera** → nombre **VR Camera** |
| 2 | Resetear posición: **(0, 1.6, 0)** (altura de ojos) |
| 3 | **Clear Flags**: Solid Color, **Background**: Negro |
| 4 | **Field of View**: `80` |
| 5 | Eliminar **Audio Listener** (o dejarlo, no afecta) |

---

## 5. Configurar Post-Processing (URP)

### 5.1 Volume en la Cámara

| Paso | Acción |
|------|--------|
| 1 | Con VR Camera seleccionada → **Add Component** → `Volume` |
| 2 | Marcar **Global** |
| 3 | En Profile → clic **New** → nombre `VR_PostProfile` |
| 4 | Clic **Add Override** y agregar estos 3: |

### 5.2 Overrides necesarios

| Override | Propiedades a dejar por defecto |
|----------|-------------------------------|
| ✅ **Vignette** | Intensity: `0`, Smoothness: `0.4` |
| ✅ **Chromatic Aberration** | Intensity: `0` |
| ✅ **Color Adjustments** | Saturation: `0`, Post Exposure: `0` |

### 5.3 Script de Feedback

| Paso | Acción |
|------|--------|
| 1 | **Add Component** → `CostVisualFeedback` |
| 2 | Arrastrar el mismo Volume al campo **Volumen Global** |

### 5.4 Cámara

| Paso | Acción |
|------|--------|
| 1 | En la cámara: **Render Type** = `Base` |
| 2 | **Post Processing** = ✅ (marcado) |

---

## 6. Crear GameManager

| Paso | Acción |
|------|--------|
| 1 | Jerarquía → Create Empty → nombre **GameManager** |
| 2 | **Add Component** → `GameCostManager` |
| 3 | Valores recomendados: Vida=`100`, Poder=`50`, Recursos=`30` |

---

## 7. Crear Objetos Interactivos

Agrega modelos 3D a la escena (cubos, cilindros, o los assets que tengas).
Para cada objeto, haz lo siguiente:

### 7.1 Palanca

| Componente | Valor |
|------------|-------|
| Transform | Posición cerca de entrada de la cueva |
| Mesh Filter / Mesh | Cubo alargado o el modelo que tengas |
| **Add Component** → `InteractableObject` |
| Nombre Objeto | `"Palanca"` |
| Tipo Costo | `Vida` |
| Costo Vida | `20` |
| Efecto Negativo | `15` |
| Beneficio Iluminación | `2` |
| Abre Salida | (desmarcado) |
| **Add Component** → `Light` (desactivado, marcar ✅) |

### 7.2 Antorcha

| Componente | Valor |
|------------|-------|
| **Add Component** → `InteractableObject` |
| Nombre Objeto | `"Antorcha"` |
| Tipo Costo | `CampoDeVision` |
| Costo Vida | `10` |
| Efecto Negativo | `25` |
| Beneficio Iluminación | `5` |
| Abre Salida | (desmarcado) |
| **Add Component** → `Light` (desactivado) |

### 7.3 Cristal de Salida

| Componente | Valor |
|------------|-------|
| **Add Component** → `InteractableObject` |
| Nombre Objeto | `"Cristal de Salida"` |
| Tipo Costo | `DistorsionVisual` |
| Costo Vida | `40` |
| Efecto Negativo | `30` |
| Beneficio Iluminación | `8` |
| Abre Salida | ✅ (marcado) |
| **Add Component** → `Light` (desactivado) |

---

## 8. Tag del Player

| Paso | Acción |
|------|--------|
| 1 | Seleccionar Player |
| 2 | Inspector → Tag → **Add Tag** → crear tag `Player` |
| 3 | Asignar tag `Player` al GameObject Player |

---

## 9. Iluminación de la Cueva

| Objeto | Acción |
|--------|--------|
| Directional Light | **Eliminar** o Intensity = `0` |
| **Window > Rendering > Lighting** | Ambient Intensity = `0.05` |
| Opcional | Agregar luces puntuales débiles para orientación mínima |

---

## 10. Verificar Escena

Revisa en el Inspector que el Player tenga los 3 componentes principales:
- ✅ `CharacterController`
- ✅ `MobileVRCapsuleController`
- ✅ Hijo `VR Camera` con `Camera`, `Volume`, `CostVisualFeedback`

Y en la jerarquía:
```
SampleScene
├── GameManager           ← GameCostManager
├── Player                ← CharacterController + MobileVRCapsuleController
│   └── VR Camera         ← Camera + Volume + CostVisualFeedback
├── Palanca               ← InteractableObject + Light (desactivado)
├── Antorcha              ← InteractableObject + Light (desactivado)
├── Cristal               ← InteractableObject + Light (desactivado)
└── EscenarioCueva        ← Mesh + Colliders
```

---

## 11. Probar en PC

Presiona **Play**. Deberías poder:
- Mirar alrededor con el **mouse** (clic derecho + arrastrar)
- Moverte hacia adelante con **clic izquierdo**
- Apuntar a un objeto interactivo → la retícula se posa sobre él
- Mantener la mirada **2 segundos** → se activa y ves el efecto visual

---

## 12. Resumen de costos por objeto

| Objeto | Costo Vida | Efecto Negativo | Beneficio Luz | Abre Salida |
|--------|-----------|----------------|--------------|-------------|
| Palanca | 20 | 15 | 2 | ✗ |
| Antorcha | 10 | 25 | 5 | ✗ |
| Cristal | 40 | 30 | 8 | ✓ |

El jugador empieza con **100 de vida**. Si activa todo: **100 - 70 = 30 de vida restante**.

El orden de activación es clave — el efecto visual (viñeta, aberración, grises) se intensifica con cada costo pagado.
