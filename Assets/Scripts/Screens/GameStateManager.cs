using UnityEngine;

/// <summary>
/// Estado global del juego, en reemplazo de los booleanos sueltos
/// (isPaused, etc.) repartidos entre IntroScreenManager y PauseMenuManager.
///
/// No necesita configuración en el Inspector: se auto-crea la primera vez
/// que alguien pide GameStateManager.Instance, así que no hace falta
/// arrastrarlo a ningún GameObject ni armar nada a mano.
/// </summary>
public enum GameState { Intro, Playing, Paused }

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager instance;

    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameStateManager");
                instance = go.AddComponent<GameStateManager>();
            }
            return instance;
        }
    }

    // Arranca en Playing: así el juego funciona normalmente aunque
    // IntroScreenManager no esté en la escena (si lo agregás más adelante,
    // su propio Start() pasa el estado a Intro).
    public GameState CurrentState { get; private set; } = GameState.Playing;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
    }
}