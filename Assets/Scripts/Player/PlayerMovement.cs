using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("Footsteps Audio")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips; // Acá vas a meter tus 5 audios
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float stepTimer;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 keyboardInput = GetMovementInput();
        float x = keyboardInput.x;
        float z = keyboardInput.y;



        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        bool isMoving = keyboardInput.magnitude > 0f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayRandomFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }

    }

    private Vector2 GetMovementInput()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
        }

        return new Vector2(x, z).normalized;
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || footstepAudioSource == null) return;

        footstepAudioSource.PlayOneShot(footstepClips[0]);
    }

}
