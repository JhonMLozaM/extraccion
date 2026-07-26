using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GeneradorEscenario : EditorWindow
{
    private float tamanoEscenario = 50f;
    private float separacion = 2f;
    private bool soloModelosGrandes = true;
    private float probabilidadDetalle = 0.15f;

    [MenuItem("Herramientas/Generar Escenario 50x50")]
    private static void Init()
    {
        GeneradorEscenario window = GetWindow<GeneradorEscenario>();
        window.titleContent = new GUIContent("Generar Escenario");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuración del Escenario", EditorStyles.boldLabel);
        tamanoEscenario = EditorGUILayout.FloatField("Tamaño Escenario", tamanoEscenario);
        separacion = EditorGUILayout.FloatField("Separación", separacion);
        soloModelosGrandes = EditorGUILayout.Toggle("Solo modelos principales (sin detalle)", soloModelosGrandes);
        if (!soloModelosGrandes)
        {
            probabilidadDetalle = EditorGUILayout.Slider("Probabilidad detalle (NN_d/s/t)", probabilidadDetalle, 0.05f, 0.5f);
        }

        if (GUILayout.Button("Generar Escenario"))
        {
            Generar();
        }
    }

    private void Generar()
    {
        GameObject contenedor = new GameObject("Escenario_Generado");
        CrearSuelo(contenedor);

        string[] modelosGrandes, modelosMedianos, modelosDetalle;
        ObtenerModelosSeparados(out modelosGrandes, out modelosMedianos, out modelosDetalle);
        if (modelosGrandes.Length == 0 && modelosMedianos.Length == 0)
        {
            Debug.LogError("No hay modelos en Assets/Models/Vegetacion");
            return;
        }

        int celdasPorLado = Mathf.FloorToInt(tamanoEscenario / separacion);
        int mitad = celdasPorLado / 2;
        float offset = (tamanoEscenario - celdasPorLado * separacion) / 2f;

        bool[,] esCamino = new bool[celdasPorLado, celdasPorLado];

        GenerarCaminosRamificados(esCamino, celdasPorLado);

        Random.InitState(System.DateTime.Now.Millisecond);
        int contGrandes = 0;
        int contMedianos = 0;
        int contDetalle = 0;

        for (int x = 0; x < celdasPorLado; x++)
        {
            for (int z = 0; z < celdasPorLado; z++)
            {
                float posX = x * separacion + offset;
                float posZ = z * separacion + offset;

                if (esCamino[x, z])
                {
                    GameObject piso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    piso.name = $"Camino_{x}_{z}";
                    piso.transform.SetParent(contenedor.transform);
                    piso.transform.position = new Vector3(posX, 0.01f, posZ);
                    piso.transform.localScale = new Vector3(separacion * 2.0f, 0.05f, separacion * 2.0f);
                    Renderer r = piso.GetComponent<Renderer>();
                    Material mat = ObtenerMaterialSuelo();
                    if (mat != null)
                        r.sharedMaterial = mat;
                    else
                    {
                        r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        r.sharedMaterial.color = new Color(0.3f, 0.25f, 0.2f);
                    }
                    Object.DestroyImmediate(piso.GetComponent<Collider>());
                }
                else
                {
                    string modelo;
                    bool adyacente = EsAdyacenteACamino(esCamino, x, z, celdasPorLado);

                    if (adyacente && modelosMedianos.Length > 0)
                    {
                        modelo = modelosMedianos[Random.Range(0, modelosMedianos.Length)];
                        contMedianos++;
                    }
                    else if (!soloModelosGrandes && modelosDetalle.Length > 0 && Random.value < probabilidadDetalle)
                    {
                        modelo = modelosDetalle[Random.Range(0, modelosDetalle.Length)];
                        contDetalle++;
                    }
                    else if (modelosGrandes.Length > 0)
                    {
                        modelo = modelosGrandes[Random.Range(0, modelosGrandes.Length)];
                        contGrandes++;
                    }
                    else
                    {
                        modelo = modelosMedianos[Random.Range(0, modelosMedianos.Length)];
                        contMedianos++;
                    }
                    ColocarModelo(contenedor, modelo, new Vector3(posX, 0, posZ));
                }
            }
        }

        string rutaPrefab = "Assets/Prefabs/Entorno/EscenarioGenerado.prefab";
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Entorno");
        PrefabUtility.SaveAsPrefabAsset(contenedor, rutaPrefab);
        Debug.Log($"Escenario generado: {celdasPorLado}x{celdasPorLado} celdas. Grandes:{contGrandes} Med medianos:{contMedianos} Detalle:{contDetalle}");

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(rutaPrefab);
    }

    private Material _materialSuelo;

    private Material ObtenerMaterialSuelo()
    {
        if (_materialSuelo != null)
            return _materialSuelo;

        Texture2D texSuelo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Vegetacion/ground.jpeg");
        if (texSuelo != null)
            _materialSuelo = ObtenerOCrearMaterial(texSuelo, null);
        return _materialSuelo;
    }

    private void CrearSuelo(GameObject contenedor)
    {
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.SetParent(contenedor.transform);
        suelo.transform.localScale = Vector3.one * tamanoEscenario / 10f;
        suelo.transform.position = new Vector3(tamanoEscenario / 2f, -0.05f, tamanoEscenario / 2f);
        Renderer r = suelo.GetComponent<Renderer>();
        Material mat = ObtenerMaterialSuelo();
        if (mat != null)
            r.sharedMaterial = mat;
        else
        {
            r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.sharedMaterial.color = new Color(0.12f, 0.1f, 0.07f);
        }
    }

    private void GenerarCaminosRamificados(bool[,] esCamino, int n)
    {
        if (n < 3) return;
        int centro = n / 2;

        // Camino de entrada: desde el borde izquierdo (x=0) hasta el centro
        for (int x = 0; x <= centro; x++)
            esCamino[x, centro] = true;

        // Rama superior: centro → esquina superior derecha (x=n-1, z=0)
        for (int x = centro, z = centro; x < n - 1 || z > 0; )
        {
            if (x < n - 1) x++;
            if (z > 0) z--;
            esCamino[x, z] = true;
        }

        // Rama inferior: centro → esquina inferior derecha (x=n-1, z=n-1)
        for (int x = centro, z = centro; x < n - 1 || z < n - 1; )
        {
            if (x < n - 1) x++;
            if (z < n - 1) z++;
            esCamino[x, z] = true;
        }
    }

    private void ColocarModelo(GameObject contenedor, string rutaModelo, Vector3 posicion)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rutaModelo);
        GameObject instancia;

        if (prefab != null)
        {
            instancia = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instancia == null)
                instancia = Object.Instantiate(prefab);
        }
        else
        {
            instancia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instancia.name = "Sustituto";
            float h = Random.Range(0.5f, 2f);
            instancia.transform.localScale = new Vector3(0.3f, h, 0.3f);
            Renderer r = instancia.GetComponent<Renderer>();
            r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.sharedMaterial.color = new Color(Random.value * 0.3f, 0.5f + Random.value * 0.3f, Random.value * 0.2f);
        }

        string nombre = System.IO.Path.GetFileNameWithoutExtension(rutaModelo).ToLower();
        AsignarTexturaPorNombre(instancia, nombre);
        float escala = 1f;

        if (nombre.Contains("bamboo") || (nombre.Length <= 2 && RegexNumerico.IsMatch(nombre)))
            escala = 0.3f;
        else if (RegexDetalle.IsMatch(nombre))
            escala = 0.1f;

        instancia.transform.SetParent(contenedor.transform);
        instancia.transform.position = posicion;
        instancia.transform.localScale = Vector3.one * escala;
        instancia.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        if (instancia.GetComponent<Collider>() == null)
        {
            MeshFilter mf = instancia.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider mc = instancia.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;
            }
            else
            {
                instancia.AddComponent<BoxCollider>();
            }
        }
        else
        {
            Collider c = instancia.GetComponent<Collider>();
            if (c is MeshCollider mc)
                mc.convex = true;
        }

        Rigidbody rb = instancia.GetComponent<Rigidbody>();
        if (rb == null)
            rb = instancia.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private static readonly Regex RegexDetalle = new Regex(@"^\d+_[dst]$", RegexOptions.Compiled);
    private static readonly Regex RegexNumerico = new Regex(@"^\d+$", RegexOptions.Compiled);

    private bool EsAdyacenteACamino(bool[,] esCamino, int x, int z, int n)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                int nx = x + dx;
                int nz = z + dz;
                if (nx >= 0 && nx < n && nz >= 0 && nz < n && esCamino[nx, nz])
                    return true;
            }
        }
        return false;
    }

    private void ObtenerModelosSeparados(out string[] grandes, out string[] medianos, out string[] detalle)
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Models/Vegetacion" });
        List<string> listaGrandes = new List<string>();
        List<string> listaMedianos = new List<string>();
        List<string> listaDetalle = new List<string>();

        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (!ruta.EndsWith(".obj") && !ruta.EndsWith(".fbx"))
                continue;

            string nombre = System.IO.Path.GetFileNameWithoutExtension(ruta).ToLower();
            if (RegexDetalle.IsMatch(nombre))
                listaDetalle.Add(ruta);
            else if (nombre.Contains("bamboo") || RegexNumerico.IsMatch(nombre))
                listaMedianos.Add(ruta);
            else
                listaGrandes.Add(ruta);
        }

        grandes = listaGrandes.ToArray();
        medianos = listaMedianos.ToArray();
        detalle = listaDetalle.ToArray();

        Debug.Log($"Modelos: {grandes.Length} grandes, {medianos.Length} medianos, {detalle.Length} detalle");
    }

    private void AsignarTexturaPorNombre(GameObject instancia, string nombreModelo)
    {
        string rutaDiffuse = null;

        if (RegexDetalle.IsMatch(nombreModelo))
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Models/Vegetacion" });
            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(ruta).ToLower() == "grass_diffuse")
                {
                    rutaDiffuse = ruta;
                    break;
                }
            }
        }
        else
        {
            rutaDiffuse = BuscarTexturaTGA(nombreModelo, "diffuse");
        }

        if (rutaDiffuse == null) return;

        Texture2D texDiffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaDiffuse);
        if (texDiffuse == null) return;

        string rutaNormal = BuscarTexturaTGA(nombreModelo, "normal");
        Texture2D texNormal = null;
        if (rutaNormal != null)
            texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaNormal);

        Material materialGuardado = ObtenerOCrearMaterial(texDiffuse, texNormal);

        Renderer[] renderers = instancia.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.sharedMaterial = materialGuardado;
        }
    }

    private static Material ObtenerOCrearMaterial(Texture2D texDiffuse, Texture2D texNormal)
    {
        string nombreMaterial = texDiffuse.name;
        string rutaCarpeta = "Assets/Materials/Generated";
        string rutaMaterial = rutaCarpeta + "/" + nombreMaterial + ".mat";

        System.IO.Directory.CreateDirectory(rutaCarpeta);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(rutaMaterial);
        if (mat != null)
            return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.mainTexture = texDiffuse;
        mat.color = Color.white;

        if (texNormal != null)
        {
            mat.SetTexture("_BumpMap", texNormal);
            mat.EnableKeyword("_NORMALMAP");
        }

        AssetDatabase.CreateAsset(mat, rutaMaterial);
        return mat;
    }

    private string BuscarTexturaTGA(string nombreModelo, string sufijo)
    {
        string nombreBase = nombreModelo;
        Match match = Regex.Match(nombreModelo, @"^(\d+)_[dst]$");
        if (match.Success)
            nombreBase = match.Groups[1].Value;

        string[] sufijos = sufijo == "diffuse"
            ? new string[] { "_" + sufijo, "-" + sufijo, "_dffuse" }
            : new string[] { "_" + sufijo, "-" + sufijo };

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Models/Vegetacion" });
        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            string nombreTGA = System.IO.Path.GetFileNameWithoutExtension(ruta).ToLower();

            foreach (string s in sufijos)
            {
                if (!nombreTGA.EndsWith(s)) continue;

                string nombreSinSufijo = nombreTGA.Substring(0, nombreTGA.Length - s.Length);

                if (nombreSinSufijo == nombreBase)
                    return ruta;

                if (RegexNumerico.IsMatch(nombreBase))
                {
                    string[] partes = nombreSinSufijo.Split('-');
                    if (System.Array.IndexOf(partes, nombreBase) >= 0)
                        return ruta;
                }
            }
        }
        return null;
    }
}
