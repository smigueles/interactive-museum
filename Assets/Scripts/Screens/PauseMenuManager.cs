using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
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
    public KeyCode pauseKey = KeyCode.Q;

    [Header("Referencias a bloquear mientras está en pausa")]
    public MonoBehaviour playerMovementScript; 
    public MonoBehaviour mouseLookScript;      
    public PlayerInteraction playerInteraction;

    [Header("Escena del menú principal (para 'Volver al menú', opcional)")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Intro) return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (GameStateManager.Instance.CurrentState == GameState.Paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        GameStateManager.Instance.SetState(GameState.Paused);

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
        GameStateManager.Instance.SetState(GameState.Playing);

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