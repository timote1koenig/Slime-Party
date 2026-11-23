
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"Spawn Player - OwnerClientId: {OwnerClientId} | " +
            $"LocalClientId: {NetworkManager.Singleton.LocalClientId} | " +
            $"IsOwner: {IsOwner}"
        );
    }

    private void Update()
    {
        // Seul le propriétaire contrôle son personnage.
        if (!IsOwner)
            return;

        if (Keyboard.current == null)
        {
            Debug.LogWarning("Aucun clavier détecté !");
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            Debug.Log($"W reçu par le joueur {OwnerClientId}");
        }

        if (Keyboard.current == null)
            return;

        float x = 0f;
        float z = 0f;

        // Déplacements WASD et flèches directionnelles.
        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            x += 1f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            z += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            z -= 1f;

        Vector3 movement = new Vector3(x, 0f, z).normalized;

        controller.Move(movement * speed * Time.deltaTime);
    }
}
