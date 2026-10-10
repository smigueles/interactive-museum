using UnityEngine;

/// <summary>
/// Poné este script en un BoxCollider (Is Trigger = true) dentro de la sala actual,
/// por ejemplo pegado a una de las paredes o en el lugar del vano que cerraste.
/// Al cruzar, hace fade a negro y teletransporta al jugador a la otra sala.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TeleportTrigger : MonoBehaviour
{
    [Header("A dónde teletransporta")]
    public Transform destinationSpawnPoint; // objeto vacío ubicado en la entrada de la otra sala

    [Header("Mensaje opcional de transición (va al Debug.Log por ahora)")]
    [TextArea(2, 3)] public string transitionMessage;

    [Header("Evitar que el trigger se dispare en loop si el spawn point queda muy pegado a otro trigger")]
    public float cooldownSeconds = 1f;
    private float lastTeleportTime = -999f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastTeleportTime < cooldownSeconds) return;

        Debug.Log("Anda");

        lastTeleportTime = Time.time;

        // Guardamos referencias necesarias ANTES de que empiece el fade
        CharacterController cc = other.GetComponent<CharacterController>();
        Transform playerTransform = other.transform;

        if (!string.IsNullOrEmpty(transitionMessage))
            Debug.Log($"[Teleport] {transitionMessage}");

        if (FadeScreenController.Instance != null)
        {
            FadeScreenController.Instance.FadeAndExecute(() => TeleportPlayer(playerTransform, cc));
        }
        else
        {
            // Si no hay FadeScreenController en la escena, teletransporta igual sin fade
            TeleportPlayer(playerTransform, cc);
        }
    }

    private void TeleportPlayer(Transform playerTransform, CharacterController cc)
    {
        if (destinationSpawnPoint == null)
        {
            Debug.LogWarning("TeleportTrigger: falta asignar el destinationSpawnPoint.");
            return;
        }

        // Si el Player usa CharacterController, hay que desactivarlo antes de mover
        // el Transform directamente, porque CharacterController pisa la posición
        // manual si está activo (es lo que suele romper teletransportes a mitad de frame).
        if (cc != null) cc.enabled = false;

        playerTransform.position = destinationSpawnPoint.position;
        playerTransform.rotation = destinationSpawnPoint.rotation;

        if (cc != null) cc.enabled = true;
    }
}