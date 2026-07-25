using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GeneradorEscenario : EditorWindow
{
    private float tamanoEscenario = 50f;
    private float separacion = 2f;

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

        if (GUILayout.Button("Generar Escenario"))
        {
            Generar();
        }
    }

    private void Generar()
    {
        GameObject contenedor = new GameObject("Escenario_Generado");
        CrearSuelo(contenedor);

        string[] modelos = ObtenerModelos();
        if (modelos.Length == 0)
        {
            Debug.LogError("No hay modelos .obj en Assets/Models/Vegetacion");
            return;
        }

        int celdasPorLado = Mathf.FloorToInt(tamanoEscenario / separacion);
        int mitad = celdasPorLado / 2;
        float offset = (tamanoEscenario - celdasPorLado * separacion) / 2f;

        bool[,] esCamino = new bool[celdasPorLado, celdasPorLado];

        GenerarCaminosRamificados(esCamino, celdasPorLado);

        Random.InitState(System.DateTime.Now.Millisecond);
        int indiceModelo = 0;

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
                    piso.transform.localScale = new Vector3(separacion * 0.4f, 0.05f, separacion * 0.4f);
                    Renderer r = piso.GetComponent<Renderer>();
                    r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    r.sharedMaterial.color = new Color(0.3f, 0.25f, 0.2f);
                    Object.DestroyImmediate(piso.GetComponent<Collider>());
                }
                else
                {
                    string modelo = modelos[indiceModelo % modelos.Length];
                    indiceModelo++;
                    ColocarModelo(contenedor, modelo, new Vector3(posX, 0, posZ));
                }
            }
        }

        string rutaPrefab = "Assets/Prefabs/Entorno/EscenarioGenerado.prefab";
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Entorno");
        PrefabUtility.SaveAsPrefabAsset(contenedor, rutaPrefab);
        Debug.Log($"Escenario generado: {celdasPorLado}x{celdasPorLado} celdas, {indiceModelo} modelos colocados.");

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(rutaPrefab);
    }

    private void CrearSuelo(GameObject contenedor)
    {
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.SetParent(contenedor.transform);
        suelo.transform.localScale = Vector3.one * tamanoEscenario / 10f;
        suelo.transform.position = new Vector3(tamanoEscenario / 2f, -0.05f, tamanoEscenario / 2f);
        Renderer r = suelo.GetComponent<Renderer>();
        r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.sharedMaterial.color = new Color(0.12f, 0.1f, 0.07f);
    }

    private void GenerarCaminosRamificados(bool[,] esCamino, int n)
    {
        int centro = n / 2;
        Queue<Vector2Int> cola = new Queue<Vector2Int>();
        esCamino[centro, centro] = true;
        cola.Enqueue(new Vector2Int(centro, centro));

        int maxCaminos = Mathf.RoundToInt(n * n * 0.25f);
        int contador = 0;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (cola.Count > 0 && contador < maxCaminos)
        {
            Vector2Int actual = cola.Dequeue();

            for (int paso = 0; paso < Random.Range(2, 5); paso++)
            {
                Vector2Int dir = dirs[Random.Range(0, dirs.Length)];
                Vector2Int sig = actual + dir;

                if (sig.x < 0 || sig.x >= n || sig.y < 0 || sig.y >= n)
                    continue;
                if (esCamino[sig.x, sig.y])
                    continue;

                esCamino[sig.x, sig.y] = true;
                cola.Enqueue(sig);
                actual = sig;
                contador++;

                if (contador >= maxCaminos) break;
            }
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
        float escala = 1f;

        if (nombre.Contains("bamboo"))
            escala = 0.5f;
        else if (System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^\d+_[dst]$"))
            escala = 0.25f;

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
    }

    private string[] ObtenerModelos()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Models/Vegetacion" });
        List<string> modelos = new List<string>();

        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (ruta.EndsWith(".obj") || ruta.EndsWith(".fbx"))
                modelos.Add(ruta);
        }

        return modelos.ToArray();
    }
}
