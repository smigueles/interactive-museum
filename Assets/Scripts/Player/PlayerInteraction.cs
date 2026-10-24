using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 7f;
    public Camera playerCamera;

    [Header("Tecla para mostrar el tooltip (mantener apretada)")]
    public KeyCode interactKey = KeyCode.F;

    [Header("UI de tooltip (referenciá tu Text/TextMeshPro acá)")]
    public GameObject tooltipPanel;   // el panel/fondo del tooltip, se activa/desactiva
    public TMPro.TextMeshProUGUI tooltipText; 
    private Sculpture currentSculpture; // guarda a qué escultura estamos mirando ahora
    [SerializeField] private JournalToast journalToast;

    void Update()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Buscamos el componente Sculpture (o cualquier subclase: Egyptian/Greek/Renaissance)
            // GetComponentInParent: busca en el objeto con el collider Y hacia arriba en la jerarquía,
            // así funciona aunque el collider esté en un hijo y el script en el contenedor padre.
            Sculpture sculpture = hit.collider.GetComponentInParent<Sculpture>();


            if (sculpture != null)
            {
                // Si es una escultura distinta a la que ya estábamos mirando:
                // apagamos highlight/tooltip de la anterior, prendemos highlight de la nueva.
                // El tooltip NO se muestra automático: solo el highlight es pasivo.
                if (sculpture != currentSculpture)
                {
                    if (currentSculpture != null) currentSculpture.SetHighlight(false);

                    currentSculpture = sculpture;
                    currentSculpture.SetHighlight(true);
                    HideTooltip(); // por si quedó abierto de la pieza anterior

                    // Crosshair: avisa que hay algo interactuable. No hace falta
                    // asignar nada a mano, CrosshairController es un singleton.
                    if (CrosshairController.Instance != null) CrosshairController.Instance.SetActive(true);
                }

                // Tooltip: se muestra SOLO mientras mantenés F apretado mirando la pieza
                if (Input.GetKey(interactKey))
                {
                    ShowTooltip(sculpture.GetTooltip());
                }
                else
                {
                    HideTooltip();
                }

                // Guardar en la bitácora con E (solo una vez por pieza, Journal se encarga de eso)
                if (Input.GetKeyDown(KeyCode.E) && JournalManager.Instance != null)
                {
                    JournalManager.Instance.AddEntry(sculpture.SculptureName, sculpture.GetTooltip());
                    journalToast.Show();
                }

                return; 
            }
        }

       
        if (currentSculpture != null)
        {
            currentSculpture.SetHighlight(false);
            currentSculpture = null;
            HideTooltip();

            if (CrosshairController.Instance != null) CrosshairController.Instance.SetActive(false);
        }
    }

    void ShowTooltip(string text)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (tooltipText != null) tooltipText.text = text;
    }

    void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}