using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Feedback visual temporal al guardar una pieza en la bitácora: el texto
/// se "escribe" letra por letra (efecto máquina de escribir / lápiz) y
/// después de un rato se desvanece solo.
///
/// Uso: desde donde sea que hoy procesás la tecla E para guardar en la
/// bitácora, llamá a journalToast.Show() (o Show("otro mensaje") si en
/// algún caso querés un texto distinto, ej. "Ya estaba en tu bitácora").
/// </summary>
public class JournalToast : MonoBehaviour
{
    [Header("Referencias UI (armar en un Canvas aparte, esquina de pantalla)")]
    [SerializeField] private CanvasGroup canvasGroup;   // en el panel raíz del toast
    [SerializeField] private TMP_Text label;

    [Tooltip("Opcional: un ícono (lápiz, check, etc.) que se mueve junto con el texto a medida que se 'escribe'. Podés dejarlo vacío.")]
    [SerializeField] private RectTransform movingIcon;

    [Header("Timing")]
    [SerializeField] private float charsPerSecond = 28f;
    [SerializeField] private float holdDuration = 1.4f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Sonido (opcional, para cuando sumes audio)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip writeSound;   // loop corto o ráfaga de "garabato", opcional
    [SerializeField] private AudioClip confirmSound; // un "ding" corto al terminar de escribir, opcional

    private Coroutine running;
    private string defaultMessage = "Escultura registrada";

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string message = null)
    {
        if (running != null) StopCoroutine(running);
        gameObject.SetActive(true);
        running = StartCoroutine(RunToast(string.IsNullOrEmpty(message) ? defaultMessage : message));
    }

    private IEnumerator RunToast(string message)
    {
        label.text = "";

        if (audioSource != null && writeSound != null)
            audioSource.PlayOneShot(writeSound);

        yield return Fade(0f, 1f, fadeDuration);

        float delayPerChar = 1f / Mathf.Max(1f, charsPerSecond);
        for (int i = 0; i < message.Length; i++)
        {
            label.text = message.Substring(0, i + 1);

            if (movingIcon != null)
            {
                Vector3 pos = movingIcon.localPosition;
                pos.x = label.preferredWidth;
                movingIcon.localPosition = pos;
            }

            yield return new WaitForSeconds(delayPerChar);
        }

        if (audioSource != null && confirmSound != null)
            audioSource.PlayOneShot(confirmSound);

        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeDuration);

        gameObject.SetActive(false);
        running = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, duration > 0f ? elapsed / duration : 1f);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}