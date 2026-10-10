using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla un panel negro fullscreen con CanvasGroup para hacer fade in/out.
/// Poné este script en un Canvas con un Image negra que ocupe toda la pantalla,
/// con un componente CanvasGroup en el mismo objeto que la Image.
/// Es un singleton: un solo FadeScreenController por escena.
/// </summary>
public class FadeScreenController : MonoBehaviour
{
    public static FadeScreenController Instance { get; private set; }

    [Header("Referencia al CanvasGroup de la imagen negra fullscreen")]
    public CanvasGroup fadeCanvasGroup;

    [Header("Duración del fade (segundos)")]
    public float fadeDuration = 0.4f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Hace fade a negro, ejecuta la acción (ej: mover al jugador), y vuelve a aclarar.
    /// Es el método que vas a llamar desde el trigger de teleport.
    /// </summary>
    public void FadeAndExecute(Action duringBlackScreen)
    {
        StartCoroutine(FadeRoutine(duringBlackScreen));
    }

    private IEnumerator FadeRoutine(Action duringBlackScreen)
    {
        yield return StartCoroutine(Fade(0f, 1f)); // a negro

        duringBlackScreen?.Invoke(); // acá se teletransporta al jugador, pantalla ya está negra

        yield return new WaitForSeconds(0.1f); // pequeño respiro para que no se sienta instantáneo
        yield return StartCoroutine(Fade(1f, 0f)); // vuelve a aclarar
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true; // bloquea clicks/interacción mientras está en negro
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
        fadeCanvasGroup.blocksRaycasts = (to >= 1f); // solo bloquea si quedó en negro total
    }
}