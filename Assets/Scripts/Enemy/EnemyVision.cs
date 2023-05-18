using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public GameObject console;
    public float enemyStartingHealth = 100f;
    public float damageScale = 1f;
    public GameObject enemyView;
    public GameObject enemyModel;
    public GameObject enemyLight;
    public Material viewFireMaterial;
    public Material viewAlertMaterial;
    public Material viewNetralMaterial;
    public Material enemyDeadMaterial;
    public float viewDistanceDefault = 10f;
    public float viewAngleDefault = 45f;
    public float viewDistanceAlert = 70f;
    public float viewAngleAlert = 270f;
    public LayerMask ignoreMask;

    public float rotationSpeed = 2.0f;
    public float fireRate = 0.5f;
    public float firingDuration = 1.2f;
    public float pauseBetweenFiring = 0.5f;
    public float timeUntilNeutral = 30;

    private float timeSinceSeenPlayer = 0;
    private bool playerDetected = false;
    private bool playerSearch = false;
    private bool enemyAlert = false;
    private bool enemyIsFiring = false;

    private bool enemyAlive = true;

    private float viewDistance;
    private float viewAngle;
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform lastKnownPlayerPosition;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        enemyLight.GetComponent<Light>().enabled = false;
        viewDistance = viewDistanceDefault;
        viewAngle = viewAngleDefault;
        enemyAlive = true;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate()
    {
        if (enemyAlive && enemyStartingHealth <= 0)
        {
            enemyAlive = false;
            agent.isStopped = true;
            enemyModel.GetComponent<Renderer>().material = enemyDeadMaterial;
            enemyView.GetComponent<Renderer>().material = enemyDeadMaterial;
            console.SendMessage("ReduceEnemyCount");
        }
        
        if (enemyAlive)
        {
            if (player != null)
            {
                if (enemyAlert == true)
                {
                    timeSinceSeenPlayer += Time.deltaTime;
                    if (timeSinceSeenPlayer >= timeUntilNeutral)
                    {
                        NeutralState();
                    }
                }
                else
                {
                    enemyView.GetComponent<Renderer>().material = viewNetralMaterial;
                }

                // if enemy can see player                
                if (CanSeeTarget(player.transform))
                {
                    Debug.Log("Enemy sees player!");
                    Debug.DrawLine(transform.position, player.transform.position, Color.yellow, 5.0f);
                    lastKnownPlayerPosition = player.transform;
                    AlertState();
                    if (enemyIsFiring == false)
                    {
                        enemyIsFiring = true;
                        StartCoroutine(RotateAndFire(player.transform));
                    }

                    if (playerDetected == false)
                    {
                        PlayerDetected();
                    }
                }
                else
                {
                    if (playerDetected == true)
                    {
                        PlayerLost();
                    }
                }
            }
        }
    }

    public void AlertState()
    {
        if (enemyAlive)
        {
            timeSinceSeenPlayer = 0;
            enemyAlert = true;
            viewDistance = viewDistanceAlert;
            viewAngle = viewAngleAlert;
            enemyView.GetComponent<Renderer>().material = viewAlertMaterial;
            enemyLight.GetComponent<Light>().enabled = true;
        }
    }

    public void HeardSomething(Transform target)
    {
        // Debug.Log("Heard something!");        
        if (!CanSeeTarget(player.transform) && playerSearch == false && playerDetected == false && enemyAlive == true)
        {
            AlertState();
            agent.SetDestination(target.position);
        }
    }

    public void TurnToFacePlayer()
    {
        if (enemyAlive)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            CanSeeTarget(player.transform);
        }
    }

    public void NeutralState()
    {
        if (enemyAlive) {
            enemyAlert = false;
            playerSearch = false;
            enemyView.GetComponent<Renderer>().material = viewNetralMaterial;
            viewDistance = viewDistanceDefault;
            viewAngle = viewAngleDefault;
            enemyLight.GetComponent<Light>().enabled = false;
        }
    }

    private void PlayerDetected() {
        if (enemyAlive)
        {
            // Debug.Log("Player detected!");
            playerDetected = true;
            playerSearch = false;

            enemyView.GetComponent<Renderer>().material = viewFireMaterial;
        }
    }

    private void PlayerLost() {
        if (enemyAlive)
        {
            // Debug.Log("Player lost!");
            playerDetected = false;
            playerSearch = true;
            enemyView.GetComponent<Renderer>().material = viewAlertMaterial;
            agent.SetDestination(lastKnownPlayerPosition.position);
        }
    }

    private bool CanSeeTarget(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        float angleBetweenEnemyAndTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleBetweenEnemyAndTarget <= viewAngle / 2)
        {
            // Debug.Log("CanSeeTarget - found in angle - angleBetweenEnemyAndTarget " + angleBetweenEnemyAndTarget + " - viewAngle " + viewAngle/2);
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            // Debug.DrawLine(transform.position, directionToTarget, Color.green, 1.0f);

            if (distanceToTarget <= viewDistance)
            {
                // Debug.Log("CanSeeTarget - found in dist - distanceToTarget " + distanceToTarget + " - viewDistance " + viewDistance);
                RaycastHit hit;
                // Debug.DrawLine(transform.position, directionToTarget * viewDistance, Color.yellow, 1.0f);
                
                if (Physics.Raycast(transform.position, directionToTarget, out hit, viewDistance, ~ignoreMask))
                {                    
                    Debug.Log("raycast " + hit.transform.name);
                    if (hit.transform.name == "PlayerHolder")
                    {
                        // Debug.DrawLine(transform.position, directionToTarget * viewDistance, Color.red, 1.0f);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private IEnumerator RotateAndFire(Transform target)
    {
        // randomize variables a bit
        float firingDurationInstance = firingDuration + Random.Range(firingDuration * 0.8f, firingDuration * 1.2f);
        float pauseBetweenFiringInstance = pauseBetweenFiring + Random.Range(pauseBetweenFiring * 0.8f, pauseBetweenFiring * 1.2f);

        float elapsedTime = 0f;
        float timeSinceLastFire = 0f;

        while (elapsedTime < firingDurationInstance)
        {
            // Rotate towards the player
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Fire at the player
            if (timeSinceLastFire >= fireRate && enemyAlive == true)
            {
                Fire();
                timeSinceLastFire = 0f;
            }

            elapsedTime += Time.deltaTime;
            timeSinceLastFire += Time.deltaTime;
            yield return null;
            // yield return new WaitForSeconds(diveTime);
        }
        
        yield return new WaitForSeconds(pauseBetweenFiringInstance);
        // reset material to latest alert state
        if (playerDetected == true)
        {
            PlayerDetected();
        }
        else {
            PlayerLost();
        }
        enemyIsFiring = false;
    }

    private void Fire()
    {
        enemyView.GetComponent<ShootBallEnemy>().Shoot();
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "ProjectilePlayer" && enemyAlive == true)
        {
            AlertState();
            Debug.Log("enemy damaged " + coll.relativeVelocity.magnitude);
            enemyStartingHealth -= coll.relativeVelocity.magnitude * damageScale;
        }
    }
}
