using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú de pausa. Poné este script en un GameObject vacío de la escena del museo.
/// ESC abre/cierra el panel. Mientras está abierto, pausa el tiempo del juego
/// (Time.timeScale = 0) y bloquea el movimiento/interacción, igual que hace
/// IntroScreenManager al principio.
///
/// Conectá los botones del panel a estos métodos públicos desde el Inspector
/// (OnClick() del botón → arrastrás este GameObject → elegís el método):
///   - Botón "Reanudar"  → OnResumeButton()
///   - Botón "Salir"     → OnQuitButton()
///   - Botón "Menú principal" (opcional) → OnReturnToMainMenu()
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject pausePanel;

    [Header("Tecla para pausar/reanudar")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Referencias a bloquear mientras está en pausa")]
    public MonoBehaviour playerMovementScript; // tu script de movimiento FPS (PlayerMovement)
    public MonoBehaviour mouseLookScript;      // la rotación de cámara (MouseLook), es un script APARTE
    public PlayerInteraction playerInteraction;

    [Header("Escena del menú principal (para 'Volver al menú', opcional)")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);

        Time.timeScale = 0f; // congela el juego (física, animaciones, etc.)

        SetPlayerControlsEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Conectar al botón "Reanudar".</summary>
    public void OnResumeButton() => Resume();

    public void Resume()
    {
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;

        SetPlayerControlsEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Conectar al botón "Salir" del panel de pausa.</summary>
    public void OnQuitButton()
    {
        // Importante: restaurar timeScale antes de salir. Si el build se usa para
        // testear algo más después (poco probable en un quit, pero por prolijidad)
        // o si Application.Quit no corta instantáneo en algunas plataformas.
        Time.timeScale = 1f;

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>Conectar a un botón opcional "Volver al menú principal".</summary>
    public void OnReturnToMainMenu()
    {
        Time.timeScale = 1f; // Si no resetéas esto, la escena del menú arranca "congelada"
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (playerMovementScript != null) playerMovementScript.enabled = enabled;
        if (mouseLookScript != null) mouseLookScript.enabled = enabled;
        if (playerInteraction != null) playerInteraction.enabled = enabled;
    }
}