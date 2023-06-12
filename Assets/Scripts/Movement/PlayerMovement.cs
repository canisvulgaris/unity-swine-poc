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
        private GameObject playerRagdollParent;
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
        public GameObject PlayerRagdollObject;
        public GameObject PlayerDiveObject;
        public bool RagdollEnabled = true;
        public RagdollAnimator ragdoll;
        public bool AutoGetUp = true;
        public float PowerMul = 5f;
        [Range(0f, 0.65f)] public float ImpactDuration = 0.4f;
        [Range(0f, 1f)] public float FadeMusclesTo = 0.01f;
        [Range(0f, 1.25f)] public float FadeMusclesDuration = 0.75f;
        public LayerMask snapToGroundLayer = 1 << 0;

        public string TriggerBlendOnTagged = "Projectile";
        public float RestoreCulldown = 0.4f;
        float blendInTimer = 0f;

        [FPD_Header("Debugging")]

        public bool PlayerInvincible = false;
        public string TestPlayAnimOnRagdoll = "";
        public RagdollProcessor.EGetUpType CanGetUp = RagdollProcessor.EGetUpType.None;
        public Vector3 LimbsVelocity;
        private Vector3 LimbsAngularVelocity;
        public float LimbsVelocityMagn;
        private float LimbsAngularVelocityMagn;

        float toGetUpElapsed = 0f;

        void Start()
        {
            lastPosition = transform.position;
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            PlayerRagdollObject = GameObject.Find("B_Pelvis"); // centering object around ragdoll pelvis
            playerRagdollParent = GameObject.Find("PlayerHolder-Ragdoll-SkeletonOrigin");
            PlayerDiveObject = playerRagdollParent.transform.Find("B_Pelvis").gameObject;
            // StartCoroutine(FindDiveObject());
        }

        // IEnumerator FindDiveObject()
        // {
        //     yield return new WaitForSeconds(5);

        //     // PlayerDiveObject = playerRagdollParent.transform.Find("PlayerHolder-Ragdoll-SkeletonOrigin").gameObject;
        //     string childName = "B_Head";
        //     Transform parentTransform = playerRagdollParent.transform;

        //     for (int i = 0; i < parentTransform.childCount; i++)
        //     {
        //         Transform childTransform = parentTransform.GetChild(i);
        //         if (childTransform.name == childName)
        //         {
        //             Debug.Log("Found a child with the name: " + childTransform.name);
        //             break;
        //         }
        //     }
        // }

        void Update()
        {
            // isGrounded = Physics.CheckSphere(transform.position, groundedModifier, groundLayer, QueryTriggerInteraction.Ignore);

            // Input.GetButtonDown("Jump")
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftShift)) && currentPlayerState == PlayerState.Alive)
            {
                PlayerRagDoll(40, PlayerDiveObject, true);
            }

            //rag doll related
            if (RagdollEnabled && AutoGetUp)
            {
                if (ragdoll.Parameters.FreeFallRagdoll)
                    if (CanGetUp != RagdollProcessor.EGetUpType.None)
                    {
                        if (LimbsAngularVelocityMagn < 1f)
                            if (LimbsVelocityMagn < 0.1f)
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

            CanGetUp = ragdoll.Parameters.User_CanGetUp(null, false);
            LimbsVelocity = ragdoll.Parameters.User_GetSpineLimbsVelocity();
            LimbsAngularVelocity = ragdoll.Parameters.User_GetSpineLimbsAngularVelocity();
            LimbsVelocityMagn = LimbsVelocity.magnitude;
            LimbsAngularVelocityMagn = LimbsAngularVelocity.magnitude;

            if (ragdoll.Parameters.FreeFallRagdoll == false && currentPlayerState == PlayerState.Ragdoll && AutoGetUp) {
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

            // ragdoll.User_SetLimbImpact(hit.rigidbody, ray.direction.normalized * PowerMul, ImpactDuration);
            if (hitObject) {
                Vector3 objectRotation = -hitObject.transform.forward;
                if (reverse) {
                    objectRotation = hitObject.transform.up;
                }
                ragdoll.User_SetLimbImpact(hitObject.GetComponent<Rigidbody>(), objectRotation * power, ImpactDuration);
            }            

            if (FadeMusclesTo < 1f) {
                ragdoll.User_FadeMuscles(FadeMusclesTo, FadeMusclesDuration);
            }

            if (TestPlayAnimOnRagdoll != "") {
                ragdoll.ObjectWithAnimator.GetComponent<Animator>().CrossFadeInFixedTime(TestPlayAnimOnRagdoll, 0.15f);
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
                transform.position = new Vector3(PlayerRagdollObject.transform.position.x, transform.position.y, PlayerRagdollObject.transform.position.z);
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

                if (PlayerInvincible == false) {
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
            if (c.LatestEnterCollision.collider.CompareTag(TriggerBlendOnTagged) && c.LatestEnterCollision.relativeVelocity.magnitude > 20f)
            {
                Debug.Log("Hit Limb " + c.LimbID + " with power " + c.LatestEnterCollision.relativeVelocity.magnitude);
                ragdoll = c.ParentRagdollAnimator;
                blendInTimer = RestoreCulldown;
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
            AutoGetUp = false;            
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