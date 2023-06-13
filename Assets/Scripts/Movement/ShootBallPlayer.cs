using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBallPlayer : MonoBehaviour
{

    public GameObject weaponStatusUI;
    public GameObject ballPrefab;

    private float ballOffset = 1f;

    private float minRotation = 0f;
    private float maxRotation = 10f;

    private float fireRate = 10f;
    private int burstRate = 5;

    public float heatMax = 100f;
    public float currentHeat = 0f;
    public float heatPerShot = 6f;
    public float heatSinkRate = 10f;

    private float currentRotation = 1f;

    private bool isFiring = false;
    private float nextFireTime = 0f;
    private int burstCount = 0;

    // Start is called before the first frame update
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // Shoot();
            isFiring = true;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
        {
            // Shoot();
            isFiring = false;
            burstCount = 0;
        }

        if (isFiring && burstCount <= burstRate && Time.time > nextFireTime)
        {
            Shoot();
            burstCount++;
        }
        if (currentHeat > 0)
        {
            currentHeat = currentHeat - Time.deltaTime * heatSinkRate;
            weaponStatusUI.GetComponent<WeaponStatus>().UpdateStatus((int)currentHeat, (int)heatMax);
        }
    }

    void Shoot()
    {
        if (currentHeat < heatMax)
        {
            float RandomX = Random.Range(-1 * currentRotation, currentRotation) - burstCount * 2;
            float RandomY = Random.Range(-1 * currentRotation, currentRotation);

            Quaternion randomRotation = Quaternion.Euler(RandomX, RandomY, 0f);
            // Quaternion.Euler(Random.Range(minRotation * 3.0f, maxRotation * 2.0f), Random.Range(minRotation, maxRotation), 0f);

            Quaternion newRotation = transform.rotation * randomRotation;
            GameObject ball = Instantiate(ballPrefab, transform.position + transform.forward, newRotation);

            nextFireTime = Time.time + 1f / fireRate;
            currentHeat = currentHeat + heatPerShot;
        }
    }
}
