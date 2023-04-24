using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 13.0f;
    public float jumpForce = 5.0f;

    public float groundedModifier = 1.0f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    public bool isGrounded;

    public GameObject PlayerStand;
    public GameObject PlayerDive;

    public float diveLeapForce = 20.0f;
    public float diveLeapHeight = 0.1f;

    public float diveTime = 0.2f;
    public float divePostTime = 0.4f;

    private bool activelyDiving = false;
    private bool activelyPostDiving = false;

    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundedModifier, groundLayer, QueryTriggerInteraction.Ignore);

        if (
            // Input.GetButtonDown("Jump")
            Input.GetKeyDown(KeyCode.E)
            // && isGrounded
            )
        {
            //add dive modifier
            if (activelyDiving == false || activelyPostDiving == false)
            {
                StartCoroutine(DiveCoroutine());
            }
        }
    }

    void FixedUpdate()
    {
        float moveSpeedModifier = moveSpeed;
        float moveHeightModifier = 0.0f;

        if (!activelyDiving || !activelyPostDiving)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }
        
        if (activelyDiving == true)
        {
            moveSpeedModifier = diveLeapForce;
            moveHeightModifier = diveLeapHeight;
        }

        if (activelyPostDiving == true)
        {
            moveSpeedModifier = diveLeapForce * 0.1f;
        }

        rb.velocity = new Vector3(horizontalInput * moveSpeedModifier, rb.velocity.y + moveHeightModifier, verticalInput * moveSpeedModifier);
    }

    IEnumerator DiveCoroutine()
    {
        activelyDiving = true;
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(diveTime);
        activelyDiving = false;

        activelyPostDiving = true;
        yield return new WaitForSeconds(divePostTime);
        activelyPostDiving = false;
    }


}
