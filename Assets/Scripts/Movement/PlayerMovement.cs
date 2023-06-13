using System.Collections;
using System.Collections.Generic;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class PlayerMovement : MonoBehaviour, RagdollProcessor.IRagdollAnimatorReceiver
    {
        //UI
        public GameObject console;

        //Player Stuff
        public float playerStartingHealth = 100f;
        public float damageScale = 1f;
        public Material playerDeadMaterial;

        public GameObject playerModel;
        public GameObject playerRagdollParent;
        public float moveSpeed = 13.0f;
        public float jumpForce = 5.0f;

        public float groundedModifier = 1.0f;
        public LayerMask groundLayer;

        private Rigidbody rb;
        
        public enum PlayerState
        {
            Alive,
            Ragdoll,
            Dead
        }

        public PlayerState currentPlayerState = PlayerState.Alive;

        private bool activelyDiving = false;
        private bool activelyPostDiving = false;

        private float horizontalInput;
        private float verticalInput;
        private Vector3 lastPosition;
        private Animator animator;

        // rag doll
        [FPD_Header("Ragdoll")]
        public GameObject playerRagdollObject;

        public bool ragdollEnabled = true;
        public RagdollAnimator ragdoll;
        public bool autoGetUp = true;
        public float powerMul = 5f;
        [Range(0f, 0.65f)] public float impactDuration = 0.4f;
        [Range(0f, 1f)] public float fadeMusclesTo = 0.01f;
        [Range(0f, 1.25f)] public float fadeMusclesDuration = 0.75f;
        public LayerMask snapToGroundLayer = 1 << 0;

        public string triggerBlendOnTagged = "Projectile";
        public float restoreCulldown = 0.4f;
        float blendInTimer = 0f;

        [FPD_Header("Debugging")]

        public bool playerInvincible = false;
        public string testPlayAnimOnRagdoll = "";
        public RagdollProcessor.EGetUpType canGetUp = RagdollProcessor.EGetUpType.None;
        public Vector3 limbsVelocity;
        private Vector3 limbsAngularVelocity;
        public float limbsVelocityMagn;
        private float limbsAngularVelocityMagn;

        float toGetUpElapsed = 0f;

        void Start()
        {
            lastPosition = transform.position;
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();

            RecursiveFindChild(playerRagdollParent.transform, "B_Head");

        }

        void RecursiveFindChild(Transform parentTransform, string TargetName)
        {
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform childTransform = parentTransform.GetChild(i);
                // Debug.Log(childTransform.name);
                if (childTransform.name == TargetName)
                {                    
                    // Debug.Log("Found " + TargetName);
                    playerRagdollObject = childTransform.gameObject;
                    return;
                }
                else {
                    RecursiveFindChild(childTransform, TargetName);
                }
                
            }
            // Debug.Log("Could not find " + TargetName);
            return;
        }

        void Update()
        {
            // isGrounded = Physics.CheckSphere(transform.position, groundedModifier, groundLayer, QueryTriggerInteraction.Ignore);

            // Input.GetButtonDown("Jump")
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftShift)) && currentPlayerState == PlayerState.Alive)
            {
                PlayerRagDoll(40, playerRagdollObject, true);
            }

            //rag doll related
            if (ragdollEnabled && autoGetUp)
            {
                if (ragdoll.Parameters.FreeFallRagdoll)
                    if (canGetUp != RagdollProcessor.EGetUpType.None)
                    {
                        if (limbsAngularVelocityMagn < 1f)
                            if (limbsVelocityMagn < 0.1f)
                            {
                                toGetUpElapsed += Time.deltaTime;
                                if (toGetUpElapsed > 0.5f)
                                {
                                    toGetUpElapsed = 0f;
                                    TriggerGetUp();
                                }
                            }
                    }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayerRagDoll(100);
            }

            canGetUp = ragdoll.Parameters.User_CanGetUp(null, false);
            limbsVelocity = ragdoll.Parameters.User_GetSpineLimbsVelocity();
            limbsAngularVelocity = ragdoll.Parameters.User_GetSpineLimbsAngularVelocity();
            limbsVelocityMagn = limbsVelocity.magnitude;
            limbsAngularVelocityMagn = limbsAngularVelocity.magnitude;

            if (ragdoll.Parameters.FreeFallRagdoll == false && currentPlayerState == PlayerState.Ragdoll && autoGetUp) {
                currentPlayerState = PlayerState.Alive;
            }

            if (currentPlayerState == PlayerState.Alive) {
                blendInTimer -= Time.deltaTime;

                if (blendInTimer > 0f)
                {
                    float blend = ragdoll.Parameters.RagdolledBlend;
                    ragdoll.Parameters.RagdolledBlend = Mathf.MoveTowards(blend, 1f, Time.deltaTime * 15f);
                }
                else
                {
                    float blend = ragdoll.Parameters.RagdolledBlend;
                    ragdoll.Parameters.RagdolledBlend = Mathf.MoveTowards(blend, 0f, Time.deltaTime * 5f);
                }
            }

            
        }

        void PlayerRagDoll(float power, GameObject hitObject = null, bool reverse = false)
        {
            if (currentPlayerState == PlayerState.Alive) {
                currentPlayerState = PlayerState.Ragdoll;
            }
            
            ragdoll.StopAllCoroutines();
            ragdoll.Parameters.SafetyResetAfterCouroutinesStop();
            ragdoll.User_SetAllKinematic(false);
            ragdoll.User_EnableFreeRagdoll();
            ragdoll.User_SwitchAnimator(null, false, 0.15f);

            // ragdoll.User_SetLimbImpact(hit.rigidbody, ray.direction.normalized * powerMul, impactDuration);
            if (hitObject) {
                Vector3 objectRotation = -hitObject.transform.forward;
                if (reverse) {
                    objectRotation = hitObject.transform.up;
                }
                ragdoll.User_SetLimbImpact(hitObject.GetComponent<Rigidbody>(), objectRotation * power, impactDuration);
            }            

            if (fadeMusclesTo < 1f) {
                ragdoll.User_FadeMuscles(fadeMusclesTo, fadeMusclesDuration);
            }

            if (testPlayAnimOnRagdoll != "") {
                ragdoll.ObjectWithAnimator.GetComponent<Animator>().CrossFadeInFixedTime(testPlayAnimOnRagdoll, 0.15f);
            }            
        }

        void FixedUpdate()
        {
            if (currentPlayerState == PlayerState.Alive)
            {
                MovePlayer();
            }
            else
            {
                rb.velocity = Vector3.zero;
                transform.position = new Vector3(playerRagdollObject.transform.position.x, transform.position.y, playerRagdollObject.transform.position.z);
            }
        }
        void MovePlayer()
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


            if (currentSpeed > 0.1f)
            {
                animator.SetBool("Moving", true);
                animator.SetFloat("Speed", currentSpeed);
            }
            else
            {
                animator.SetBool("Moving", false);
                animator.SetFloat("Speed", 0);
            }

            rb.velocity = new Vector3(horizontalInput * moveSpeedModifier, rb.velocity.y + moveHeightModifier, verticalInput * moveSpeedModifier);
        }

        void OnCollisionEnter(Collision coll)
        {
            if (coll.gameObject.tag == "Projectile")
            {                

                if (playerInvincible == false) {
                    playerStartingHealth -= coll.relativeVelocity.magnitude * damageScale;
                    Debug.Log("player damaged " + coll.relativeVelocity.magnitude);
                }
                
                if (playerStartingHealth <= 0)
                {
                    PlayerDead();                    
                }
            }
        }        

        public void RagdAnim_OnCollisionEnterEvent(RagdollProcessor.RagdollCollisionHelper c)
        {
            if (c.LatestEnterCollision.collider.CompareTag(triggerBlendOnTagged) && c.LatestEnterCollision.relativeVelocity.magnitude > 20f)
            {
                Debug.Log("Hit Limb " + c.LimbID + " with power " + c.LatestEnterCollision.relativeVelocity.magnitude);
                ragdoll = c.ParentRagdollAnimator;
                blendInTimer = restoreCulldown;
                PlayerRagDoll(c.LatestEnterCollision.relativeVelocity.magnitude, c.gameObject);
            }
        }

        public void RagdAnim_OnCollisionExitEvent(RagdollProcessor.RagdollCollisionHelper c)
        {
        }

        public void PlayerDead()
        {
            currentPlayerState = PlayerState.Dead;
            PlayerRagDoll(10);
            autoGetUp = false;            
            // playerModel.GetComponent<Renderer>().material = playerDeadMaterial;
            console.SendMessage("ShowFail"); //Send Damage message to hit object
        }

        public void TriggerGetUp()
        {
            ragdoll.transform.rotation = ragdoll.Parameters.User_GetMappedRotationHipsToHead(Vector3.up);
            ragdoll.User_SwitchAnimator(null, true);
            ragdoll.User_GetUpStackV2(0f, 0.8f, 0.7f);
            ragdoll.User_ForceRagdollToAnimatorFor(0.5f, 0.5f); // (if using blend on collision) Force non-ragdoll for 0.5 sec and restore transition in 0.5 sec
                                                                // TryPlayGetupAnimation();
        }

    }

}