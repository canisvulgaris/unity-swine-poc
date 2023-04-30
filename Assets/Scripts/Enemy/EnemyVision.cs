using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float enemyStartingHealth = 100f;
    public float damageScale = 1f;
    public GameObject enemyView;
    public GameObject enemyModel;
    public Material viewFireMaterial;
    public Material viewAlertMaterial;
    public Material viewNetralMaterial;
    public Material enemyDeadMaterial;
    public float viewDistanceDefault = 10f;
    public float viewAngleDefault = 45f;
    public float viewDistanceAlert = 70f;
    public float viewAngleAlert = 270f;
    public LayerMask obstacleMask;
    public LayerMask targetMask;

    public float rotationSpeed = 2.0f;
    public float fireRate = 0.5f;
    public float firingDuration = 1.2f;
    public float pauseBetweenFiring = 0.5f;
    public float timeUntilNeutral = 30;

    private float timeSinceSeenPlayer = 0;
    private bool playerDetected = false;
    private bool enemyAlert = false;
    private bool enemyIsFiring = false;

    private bool enemyAlive = true;

    private float viewDistance;
    private float viewAngle;



    // Start is called before the first frame update
    void Start()
    {
        viewDistance = viewDistanceDefault;
        viewAngle = viewAngleDefault;
        enemyAlive = true;
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate()
    {
        if (enemyAlive && enemyStartingHealth <= 0)
        {
            enemyAlive = false;
            enemyModel.GetComponent<Renderer>().material = enemyDeadMaterial;
        }
        
        if (enemyAlive)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
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
        timeSinceSeenPlayer = 0;
        enemyAlert = true;
        viewDistance = viewDistanceAlert;
        viewAngle = viewAngleAlert;
    }

    public void TurnToFacePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        CanSeeTarget(player.transform);
    }

    public void NeutralState()
    {
        enemyAlert = false;
        enemyView.GetComponent<Renderer>().material = viewNetralMaterial;
        viewDistance = viewDistanceDefault;
        viewAngle = viewAngleDefault;

    }

    private void PlayerDetected() {
        // Debug.Log("Player detected!");
        playerDetected = true;

        enemyView.GetComponent<Renderer>().material = viewFireMaterial;
    }

    private void PlayerLost() {
        // Debug.Log("Player lost!");
        playerDetected = false;

        enemyView.GetComponent<Renderer>().material = viewAlertMaterial;
    }

    private bool CanSeeTarget(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleBetweenEnemyAndTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleBetweenEnemyAndTarget <= viewAngle / 2)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= viewDistance)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToTarget, out hit, viewDistance, obstacleMask | targetMask))
                {
                    if (hit.transform == target)
                    {
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
            if (timeSinceLastFire >= fireRate && enemyAlive)
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
        if (coll.gameObject.tag == "Projectile")
        {
            AlertState();
            Debug.Log("enemy damaged " + coll.relativeVelocity.magnitude);
            enemyStartingHealth -= coll.relativeVelocity.magnitude * damageScale;
        }
    }
}
