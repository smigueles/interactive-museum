using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crosshair simple en el centro de pantalla. Poné este script en un GameObject
/// con el componente Image (el puntito/cruz), colgado DIRECTO del Canvas principal
/// full-screen (no de un panel chico, mismo tema que tuvimos con el fade).
///
/// Es un singleton: PlayerInteraction lo llama solo via CrosshairController.Instance,
/// SIN que tengas que arrastrar ninguna referencia a mano en el Inspector de
/// PlayerInteraction. Lo único que tenés que hacer es:
///   1) Crear la Image del crosshair (ver pasos de armado).
///   2) Poner este script en ese mismo GameObject (o uno vacío) y arrastrar
///      la Image al campo "Crosshair Image" de ESTE componente.
///   3) Listo. PlayerInteraction ya está actualizado para llamarlo solo.
/// </summary>
public class CrosshairController : MonoBehaviour
{
    public static CrosshairController Instance { get; private set; }

    [Header("Referencia a la Image del crosshair (el punto/cruz)")]
    public Image crosshairImage;

    [Header("Colores")]
    public Color idleColor = Color.white;
    public Color activeColor = new Color(1f, 0.85f, 0.4f); // dorado, igual al highlight de Sculpture

    [Header("Tamaño (opcional: crece un poco cuando hay algo interactuable)")]
    public bool growWhenActive = true;
    public float idleScale = 1f;
    public float activeScale = 1.4f;

    private RectTransform rect;

    void Awake()
    {
        // Singleton simple. Si por error hay dos CrosshairController en la escena,
        // el segundo se destruye solo (y avisa por consola) para no pisarse.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Ya existe un CrosshairController en la escena. Destruyendo el duplicado.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (crosshairImage != null)
        {
            rect = crosshairImage.rectTransform;
            crosshairImage.color = idleColor;
        }
        else
        {
            Debug.LogWarning("CrosshairController: no asignaste 'Crosshair Image' en el Inspector. No va a hacer nada.");
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Llamado automáticamente por PlayerInteraction. No hace falta llamarlo a mano.
    /// </summary>
    public void SetActive(bool isLookingAtInteractable)
    {
        if (crosshairImage == null) return;

        crosshairImage.color = isLookingAtInteractable ? activeColor : idleColor;

        if (growWhenActive && rect != null)
        {
            float scale = isLookingAtInteractable ? activeScale : idleScale;
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}