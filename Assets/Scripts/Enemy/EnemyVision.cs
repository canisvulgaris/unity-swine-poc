using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public bool playerDetected = false;
    private bool enemyIsFiring = false;
    public GameObject enemyView;
    public Material viewFireMaterial;
    public Material viewAlertMaterial;
    public Material viewNetralMaterial;
    public float viewDistance = 10f;
    public float viewAngle = 45f;
    public LayerMask obstacleMask;
    public LayerMask targetMask;

    public float rotationSpeed = 2f;
    public GameObject bulletPrefab;
    public float fireRate = 0.5f;

    public float firingDuration = 1.2f;
    public float pauseBetweenFiring = 0.5f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Debug.Log(player.gameObject.name);
            if (CanSeeTarget(player.transform))
            {
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
            else {
                if (playerDetected == true)
                {
                    PlayerLost();
                }
            }
        }
    }

    private void PlayerDetected() {
        Debug.Log("Player detected!");
        playerDetected = true;

        enemyView.GetComponent<Renderer>().material = viewAlertMaterial;
    }

    private void PlayerLost() {
        Debug.Log("Player lost!");
        playerDetected = false;

        enemyView.GetComponent<Renderer>().material = viewNetralMaterial;
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
            if (timeSinceLastFire >= fireRate)
            {
                enemyView.GetComponent<Renderer>().material = viewFireMaterial;
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
}
