using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Poné este script en un GameObject vacío de tu escena "MainMenu".
/// Conectá los botones del Canvas a los métodos públicos desde el Inspector
/// (OnClick() del botón → arrastrás este GameObject → elegís el método).
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Nombre EXACTO de la escena del museo (como figura en Build Settings)")]
    public string museumSceneName = "Museo";

    [Header("Panel de instrucciones (se prende/apaga con el botón)")]
    public GameObject instructionsPanel;

    [Header("Panel de curaduría / marco teórico (se prende/apaga con el botón)")]
    public GameObject curatorialPanel;

    void Start()
    {
        // Arrancan ocultos, por si te olvidaste de desactivarlos a mano en el editor
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (curatorialPanel != null) curatorialPanel.SetActive(false);
    }

    /// <summary>Conectar al botón "Start".</summary>
    public void OnStartButton()
    {
        SceneManager.LoadScene(museumSceneName);
    }

    /// <summary>Conectar al botón "Instrucciones" (lo abre).</summary>
    public void OnShowInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    /// <summary>Conectar al botón "Volver" dentro del panel de instrucciones.</summary>
    public void OnCloseInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    /// <summary>Conectar al botón "Curaduría" (lo abre).</summary>
    public void OnShowCuratorial()
    {
        if (curatorialPanel != null) curatorialPanel.SetActive(true);
    }

    /// <summary>Conectar al botón "Volver" dentro del panel de curaduría.</summary>
    public void OnCloseCuratorial()
    {
        if (curatorialPanel != null) curatorialPanel.SetActive(false);
    }

    /// <summary>Opcional: conectar a un botón "Salir" si querés esa opción.</summary>
    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}