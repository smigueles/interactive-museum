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
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.4f); // dorado suave

    private Color[] originalBaseColors;
    private bool highlightCached = false;
    private static readonly int BaseColorURP = Shader.PropertyToID("_BaseColor"); // URP/Lit
    private static readonly int ColorBuiltin = Shader.PropertyToID("_Color");     // Standard/Built-in

    public string SculptureName => sculptureName;

    protected virtual void Awake()
    {
        // Si no arrastraste nada a mano en el Inspector, buscamos automáticamente
        // todos los Renderer en los hijos del objeto (típico en modelos importados
        // donde el mesh real vive en un child, no en el GameObject raíz).
        if (renderersToHighlight == null || renderersToHighlight.Length == 0)
        {
            renderersToHighlight = GetComponentsInChildren<Renderer>();
        }
    }

    /// <summary>
    /// Prende o apaga el resaltado visual, tiñendo el color base del material
    /// (compatible con cualquier shader: URP/Lit usa _BaseColor, Standard usa _Color).
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

        for (int i = 0; i < renderersToHighlight.Length; i++)
        {

            if (renderersToHighlight[i] == null) continue;

            Material mat = renderersToHighlight[i].material;

            Color targetColor = active ? highlightColor : originalBaseColors[i];
            SetBaseColor(mat, targetColor);
        }
    }

    // Detecta si el shader usa _BaseColor (URP) o _Color (Built-in/Standard) y lee el valor actual.
    private Color GetBaseColor(Material mat)
    {
        if (mat.HasProperty(BaseColorURP)) return mat.GetColor(BaseColorURP);
        if (mat.HasProperty(ColorBuiltin)) return mat.GetColor(ColorBuiltin);
        return Color.white; // fallback si el shader no tiene ninguna de las dos (caso raro)
    }

    private void SetBaseColor(Material mat, Color color)
    {
        if (mat.HasProperty(BaseColorURP)) mat.SetColor(BaseColorURP, color);
        else if (mat.HasProperty(ColorBuiltin)) mat.SetColor(ColorBuiltin, color);
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