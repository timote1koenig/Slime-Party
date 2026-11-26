
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class HorizontalController : NetworkBehaviour
{
    [SerializeField] private float speed = 7f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Keyboard.current == null)
            return;

        float x = 0f;

        if (Keyboard.current.aKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            x += 1f;

        Vector3 movement = new Vector3(x, 0f, 0f);

        controller.Move(movement * speed * Time.deltaTime);
    }
}
