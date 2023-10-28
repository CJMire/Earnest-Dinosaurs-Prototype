using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;

    [Header("----- Player Stats -----")]
    [SerializeField] float playerSpeed;
    [SerializeField] float playerJumpHeight;
    [SerializeField] int playerJumpMax;
    [SerializeField] float gravityStrength;

    private Vector3 move;
    private Vector3 playerVelocity;
    private bool playerIsGrounded;
    private int jumpTimes;
    private bool isShooting;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerIsGrounded = characterController.isGrounded;
        if(playerIsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpTimes = 0;
        }

        move = Input.GetAxis("Horizontal") * transform.right +
               Input.GetAxis("Vertical") * transform.forward;
        characterController.Move(move * Time.deltaTime * playerSpeed);

        if(Input.GetButtonDown("Jump") && jumpTimes < playerJumpMax)
        {
            playerVelocity.y = playerJumpHeight;
            jumpTimes++;
        }
        playerVelocity.y += gravityStrength * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}
