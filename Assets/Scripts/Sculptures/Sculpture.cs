using UnityEngine;

/// <summary>
/// Clase base abstracta para todas las esculturas del museo.
/// Contiene los datos comunes a cualquier era (nombre, tooltip base)
/// y el contrato que el sistema de raycast/tooltip espera.
/// Cada era hereda esto y agrega SU PROPIO scope de datos.
/// </summary>
public abstract class Sculpture : MonoBehaviour
{
    [Header("Datos generales (todas las esculturas)")]
    [SerializeField] protected string sculptureName;

    [SerializeField, TextArea(2, 5)]
    protected string baseTooltip;

    [Header("Highlight (resaltado al mirar)")]
    [SerializeField] private Renderer[] renderersToHighlight; // arrastrá los MeshRenderer de la estatua acá
    [SerializeField] private Color highlightColor = new Color(0.98f, 0.96f, 0.9f); // casi blanco, apenas cálido

    [Tooltip("Qué tan lejos llega el highlight en su punto máximo (0 = nada, 1 = color completo). Bajo = sutil.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxIntensity = 0.35f;

    [Header("Contorno (funciona para cualquier material, incluido GLB)")]
    [Tooltip("Factor de escala del contorno respecto al mesh original. 1.005-1.01 para un borde casi imperceptible.")]
    [SerializeField] private float outlineScale = 1.008f;

    [Tooltip("Duración del fade al prender/apagar el highlight, en segundos.")]
    [SerializeField] private float transitionDuration = 0.18f;

    private Color[] originalBaseColors;
    private bool highlightCached = false;
    private static readonly int BaseColorURP = Shader.PropertyToID("_BaseColor");        // URP/Lit
    private static readonly int ColorBuiltin = Shader.PropertyToID("_Color");            // Standard/Built-in
    private static readonly int BaseColorFactorGltf = Shader.PropertyToID("_BaseColorFactor"); // glTFast (shader propio, sin convertir a URP/Lit)

    // Contorno universal: una copia del mesh, agrandada, con un material simple
    // que no depende del shader original. Esto es lo que garantiza el highlight
    // en los GLB (y en cualquier pieza futura, sea del shader que sea).
    private GameObject[] outlineObjects;
    private Material outlineMaterial;
    private Coroutine highlightCoroutine;
    private float currentBlend = 0f; // 0 = sin highlight, 1 = highlight completo

    public string SculptureName => sculptureName;

    protected virtual void Awake()
    {
        // Si no arrastraste nada a mano en el Inspector, buscamos automáticamente
        // todos los Renderer en los hijos del objeto (típico en modelos importados
        // donde el mesh real vive en un child, no en el GameObject raíz).
        // Chequeo robusto: array nulo, vacío, O con todos sus elementos en null
        // (esto último pasa si quedó un slot vacío cargado a mano en el Inspector).
        bool needsAutoFill = renderersToHighlight == null || renderersToHighlight.Length == 0;

        if (!needsAutoFill)
        {
            bool allNull = true;
            foreach (var r in renderersToHighlight)
            {
                if (r != null) { allNull = false; break; }
            }
            needsAutoFill = allNull;
        }

        if (needsAutoFill)
        {
            // El "true" incluye objetos inactivos en la búsqueda (por defecto los ignora).
            renderersToHighlight = GetComponentsInChildren<Renderer>(true);
        }

        BuildOutlines();
    }

    // Crea, por cada renderer, un GameObject hijo con una copia del mesh,
    // levemente agrandado, usando Unlit/Color (shader built-in de Unity,
    // siempre disponible, sin depender de texturas ni del material original).
    // Arranca desactivado; SetHighlight lo prende/apaga.
    private void BuildOutlines()
    {
        if (renderersToHighlight == null || renderersToHighlight.Length == 0) return;

        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null)
        {
            Debug.LogWarning($"{sculptureName}: no se encontró el shader Unlit/Color, el contorno de highlight no va a funcionar.");
            return;
        }

        outlineMaterial = new Material(unlitShader);
        outlineMaterial.color = highlightColor;

        outlineObjects = new GameObject[renderersToHighlight.Length];

        for (int i = 0; i < renderersToHighlight.Length; i++)
        {
            Renderer r = renderersToHighlight[i];
            if (r == null) continue;

            Mesh mesh = GetMeshFrom(r);
            if (mesh == null) continue; // renderer sin mesh (raro, pero por si acaso)

            GameObject outline = new GameObject(r.name + "_Outline");
            outline.transform.SetParent(r.transform, worldPositionStays: false);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = Vector3.one; // arranca igual al mesh original; el fade lo agranda

            MeshFilter mf = outline.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = outline.AddComponent<MeshRenderer>();
            mr.sharedMaterial = outlineMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            outline.SetActive(false);
            outlineObjects[i] = outline;
        }
    }

    private Mesh GetMeshFrom(Renderer r)
    {
        if (r is SkinnedMeshRenderer skinned) return skinned.sharedMesh;

        MeshFilter mf = r.GetComponent<MeshFilter>();
        return mf != null ? mf.sharedMesh : null;
    }

    /// <summary>
    /// Prende o apaga el resaltado visual, con un fade suave (no es un on/off de golpe).
    /// Usa DOS mecanismos en paralelo:
    /// 1) Tiñe el color base del material original si reconoce la propiedad del shader
    ///    (funciona en las piezas viejas con Standard/URP).
    /// 2) Un contorno duplicado con un shader propio (Unlit/Color), que funciona siempre,
    ///    sin importar qué shader traiga el material del GLB. Crece/decrece de tamaño
    ///    durante la transición en vez de aparecer de golpe.
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (renderersToHighlight == null || renderersToHighlight.Length == 0) return;

        if (!highlightCached)
        {
            originalBaseColors = new Color[renderersToHighlight.Length];
            for (int i = 0; i < renderersToHighlight.Length; i++)
            {
                if (renderersToHighlight[i] == null) continue;
                Material mat = renderersToHighlight[i].material;
                originalBaseColors[i] = GetBaseColor(mat);
            }
            highlightCached = true;
        }

        if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
        highlightCoroutine = StartCoroutine(AnimateHighlight(active));
    }

    private System.Collections.IEnumerator AnimateHighlight(bool active)
    {
        float targetBlend = active ? 1f : 0f;
        float startBlend = currentBlend;

        // Si vamos a prender, el contorno tiene que estar activo YA para que se vea crecer
        // desde escala 1 (invisible, mismo tamaño que el mesh original) hasta outlineScale.
        if (active) SetOutlinesActive(true);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionDuration > 0f ? elapsed / transitionDuration : 1f;
            currentBlend = Mathf.Lerp(startBlend, targetBlend, t);
            ApplyBlend(currentBlend);
            yield return null;
        }

        currentBlend = targetBlend;
        ApplyBlend(currentBlend);

        // Si apagamos, recién ahora que el blend llegó a 0 desactivamos los GameObjects
        // del contorno (si los dejáramos activos sin escala, no se nota, pero así ahorramos
        // el costo de render de algo invisible).
        if (!active) SetOutlinesActive(false);

        highlightCoroutine = null;
    }

    private void ApplyBlend(float blend)
    {
        // maxIntensity frena el fade antes de llegar al color/escala completos,
        // así el highlight queda sutil incluso en su punto más alto.
        float effectiveBlend = blend * maxIntensity;

        for (int i = 0; i < renderersToHighlight.Length; i++)
        {
            if (renderersToHighlight[i] == null) continue;

            // Mecanismo 1: tinte del material original (best-effort), interpolado.
            Material mat = renderersToHighlight[i].material;
            Color targetColor = Color.Lerp(originalBaseColors[i], highlightColor, effectiveBlend);
            SetBaseColor(mat, targetColor);

            // Mecanismo 2: contorno universal, escala interpolada entre 1 (invisible,
            // mismo tamaño que el original) y outlineScale (borde visible).
            if (outlineObjects != null && outlineObjects[i] != null)
            {
                float scale = Mathf.Lerp(1f, outlineScale, effectiveBlend);
                outlineObjects[i].transform.localScale = Vector3.one * scale;
            }
        }
    }

    private void SetOutlinesActive(bool active)
    {
        if (outlineObjects == null) return;
        foreach (var outline in outlineObjects)
        {
            if (outline != null) outline.SetActive(active);
        }
    }

    // Detecta si el shader usa _BaseColor (URP), _Color (Built-in/Standard) o
    // _BaseColorFactor (shader propio de glTFast en GLB no convertidos a URP/Lit) y lee el valor actual.
    private Color GetBaseColor(Material mat)
    {
        if (mat.HasProperty(BaseColorURP)) return mat.GetColor(BaseColorURP);
        if (mat.HasProperty(ColorBuiltin)) return mat.GetColor(ColorBuiltin);
        if (mat.HasProperty(BaseColorFactorGltf)) return mat.GetColor(BaseColorFactorGltf);
        return Color.white; // fallback si el shader no tiene ninguna de las tres (caso raro)
    }

    private void SetBaseColor(Material mat, Color color)
    {
        if (mat.HasProperty(BaseColorURP)) mat.SetColor(BaseColorURP, color);
        else if (mat.HasProperty(ColorBuiltin)) mat.SetColor(ColorBuiltin, color);
        else if (mat.HasProperty(BaseColorFactorGltf)) mat.SetColor(BaseColorFactorGltf, color);
        // Si no matchea ninguna, no pasa nada: el contorno (mecanismo 2) se encarga igual.
    }

    /// <summary>
    /// Texto que se muestra en el tooltip al hacer raycast sobre la pieza.
    /// Las subclases lo sobreescriben para agregar su información específica,
    /// pero siempre pueden reusar el formato base con base.GetTooltip().
    /// </summary>
    public virtual string GetTooltip()
    {
        return $"<b>{sculptureName}</b>\n{baseTooltip}";
    }
}