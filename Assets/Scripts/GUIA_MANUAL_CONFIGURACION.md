# GUÍA MANUAL - Configuración de Escena VR

## PASO 1: Configurar el Player (Jugador)

1. Crear GameObject vacío → nombre "Player"
2. Añadir componente: `CharacterController`
   - Height: 1.8, Radius: 0.3
3. Añadir componente: `MobileVRCapsuleController`
   - Velocidad Movimiento: 2
   - Sensibilidad Giroscopio: 1
   - Reticula Prefab: (dejar vacío, se genera automática)
   - Distancia Reticula: 5

4. Crear hijo: Camera → nombre "VR Camera"
   - Reset position (0, 1.6, 0) → altura de ojos
   - Clear Flags: Solid Color, Background: Negro
   - Field of View: 80

## PASO 2: Configurar Post-Processing URP

1. En la Camera VR del Player:
   - Añadir componente: `Volume`
   - **IMPORTANTE**: Marcar "Global" en el Volume
   - Crear nuevo Profile: clic New → "VRPostProfile"
   - Añadir Overrides:
     - ✅ Vignette → Intensity: 0, Smoothness: 0.4
     - ✅ Chromatic Aberration → Intensity: 0
     - ✅ Color Adjustments → Saturation: 0

2. Añadir componente: `CostVisualFeedback`
   - Volumen Global: arrastrar el mismo Volume

3. En la Camera, habilitar:
   - Render Type: Base
   - Post Processing: ✓

## PASO 3: Configurar GameCostManager

1. Crear GameObject vacío → nombre "GameManager"
2. Añadir componente: `GameCostManager`
   - Vida Actual: 100
   - Poder: 50
   - Recursos: 30

## PASO 4: Crear Objetos Interactivos en la Cueva

Para CADA objeto (Palanca, Antorcha, Cristal):

### A. Palanca (cerca de entrada)
- Mesh:立方体 (alargada como palanca)
- Añadir: `InteractableObject`
  - Nombre Objeto: "Palanca"
  - Tipo Costo: Vida
  - Costo Vida: 20
  - Efecto Negativo: 15
  - Beneficio Iluminación: 2 (enciende luz tenue)
  - Abre Salida: false
- Añadir: Light (punto de luz, desactivado por defecto)

### B. Antorcha (mitad de la cueva)
- Mesh: Cilindro + esfera
- Añadir: `InteractableObject`
  - Nombre Objeto: "Antorcha"
  - Tipo Costo: CampoDeVision
  - Costo Vida: 10
  - Efecto Negativo: 25
  - Beneficio Iluminación: 5
  - Abre Salida: false
- Añadir: Light (punto de luz, desactivado, color anaranjado)

### C. Cristal (final, abre salida)
- Mesh: forma de cristal/gema
- Añadir: `InteractableObject`
  - Nombre Objeto: "Cristal de Salida"
  - Tipo Costo: DistorsionVisual
  - Costo Vida: 40
  - Efecto Negativo: 30
  - Beneficio Iluminación: 8
  - Abre Salida: true
- Añadir: Light (punto de luz brillante, desactivado, color cyan)

## PASO 5: Configurar Iluminación de la Cueva

- Directional Light → Intensity: 0 (cueva oscura)
- Ambient Mode: Static
- Ambient Intensity: 0.05 (mínima visibilidad)
- Agregar luces puntuales débiles para dar orientación mínima

## PASO 6: Tags y Layers

- Tag para Player: "Player"

## PASO 7: Build Android

1. File → Build Settings → Android → Switch Platform
2. Player Settings:
   - Package Name: com.jam.todotieneuncosto
   - Minimum API Level: 26
3. XR Plugin Management:
   - Android → Cardboard ✓
4. Build & Run

---

## VALORES RECOMENDADOS POR OBJETO (Inspector)

| Objeto | Costo Vida | Efecto Negativo | Beneficio Luz | Abre Salida |
|--------|-----------|----------------|--------------|-------------|
| Palanca | 20 | 15 | 2 | false |
| Antorcha | 10 | 25 | 5 | false |
| Cristal | 40 | 30 | 8 | true |

El jugador empieza con 100 de vida. Si activa todo, pierde 70 de vida total.
¡La decisión estratégica está en qué orden activarlos!
