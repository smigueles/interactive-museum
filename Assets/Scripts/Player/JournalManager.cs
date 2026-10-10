using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Guarda las esculturas que el jugador "coleccionó" apretando E al mirarlas,
/// y las muestra como lista en un panel al apretar Tab.
/// Es un Singleton simple: un solo JournalManager por escena.
/// </summary>
public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

    [Header("UI de la bitácora")]
    public GameObject journalPanel;       // panel que se prende/apaga con Tab
    public TextMeshProUGUI journalContent; // texto único donde listamos todo lo guardado
    public KeyCode toggleKey = KeyCode.Tab;

    private readonly List<string> savedNames = new List<string>();
    private readonly Dictionary<string, string> entries = new Dictionary<string, string>();

    void Awake()
    {
        // Singleton básico: si ya hay uno, este se destruye (evita duplicados entre salas)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (journalPanel != null) journalPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            bool nowActive = !journalPanel.activeSelf;
            journalPanel.SetActive(nowActive);
            Cursor.lockState = nowActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = nowActive;
            if (nowActive) RefreshUI();
        }
    }

    /// <summary>
    /// Agrega una pieza a la bitácora. Si ya estaba guardada, no la duplica.
    /// </summary>
    public void AddEntry(string sculptureName, string tooltipText)
    {
        if (entries.ContainsKey(sculptureName)) return; // ya la tenía

        entries[sculptureName] = tooltipText;
        savedNames.Add(sculptureName);

        // feedback simple en consola; si querés, acá podés disparar un sonido o un popup de "¡guardado!"
        Debug.Log($"Bitácora: se guardó '{sculptureName}' ({savedNames.Count} piezas en total)");
    }

    private void RefreshUI()
    {
        if (journalContent == null) return;

        if (savedNames.Count == 0)
        {
            journalContent.text = "Todavía no guardaste ninguna pieza.\nMirá una estatua y apretá E.";
            return;
        }

        string result = "";
        foreach (string name in savedNames)
        {
            result += $"{entries[name]}\n\n";
        }
        journalContent.text = result;
    }

    public int EntryCount => savedNames.Count;
}