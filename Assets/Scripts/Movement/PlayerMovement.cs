using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject console;
    public float playerStartingHealth = 100f;
    public float damageScale = 1f;
    public Material playerDeadMaterial;

    public GameObject playerModel;
    public float moveSpeed = 13.0f;
    public float jumpForce = 5.0f;

    public float groundedModifier = 1.0f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    public bool isGrounded;

    public float diveLeapForce = 20.0f;
    public float diveLeapHeight = 0.1f;

    public float diveTime = 0.2f;
    public float divePostTime = 0.4f;

    private bool activelyDiving = false;
    private bool activelyPostDiving = false;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 lastPosition;
    private Animator animator;

    void Start()
    {
        lastPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundedModifier, groundLayer, QueryTriggerInteraction.Ignore);

        // Input.GetButtonDown("Jump")
        if (
            isGrounded &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftShift))            
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

            lastPosition = transform.position;
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            Vector2 direction2d = new Vector2(horizontalInput, verticalInput);
            float angleInDegrees = transform.eulerAngles.y;
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);

            float newX = cosTheta * direction2d.x - sinTheta * direction2d.y;
            float newY = sinTheta * direction2d.x + cosTheta * direction2d.y;

            // Apply the rotation
            Vector2 rotatedPoint2D = new Vector2(newX, newY);
            // Debug.Log("direction2d " + direction2d + " - rotatedPoint2D " + rotatedPoint2D + " - angle " + angleInDegrees);

            animator.SetFloat("MoveX", rotatedPoint2D.x);
            animator.SetFloat("MoveY", rotatedPoint2D.y);
        }

        float currentSpeed = Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput);


        if (currentSpeed > 0.1f) {
            animator.SetBool("Moving", true);
            animator.SetFloat("Speed", currentSpeed);
        }
        else {
            animator.SetBool("Moving", false);
            animator.SetFloat("Speed", 0);
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

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "Projectile")
        {
            Debug.Log("player damaged " + coll.relativeVelocity.magnitude);
            playerStartingHealth -= coll.relativeVelocity.magnitude * damageScale;
            if (playerStartingHealth <= 0)
            {
                PlayerDead();
            }
        }
    }

    public void PlayerDead() {
        // playerModel.GetComponent<Renderer>().material = playerDeadMaterial;
        console.SendMessage("ShowFail"); //Send Damage message to hit object
    }

}
