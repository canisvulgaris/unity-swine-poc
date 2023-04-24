using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBallEnemy : MonoBehaviour
{

    public GameObject ballPrefab;

    public float ballOffset = 0.5f;

    public float minRotation = -2f;
    public float maxRotation = 2f;

    // Start is called before the first frame update
    private void Update()
    {
        
    }

    void Shoot()
    {
        float RandomX = Random.Range(minRotation, maxRotation);
        float RandomY = Random.Range(minRotation, maxRotation);

        Quaternion randomRotation = Quaternion.Euler(Random.Range(minRotation * 3.0f, maxRotation * 2.0f), Random.Range(minRotation, maxRotation), 0f);

        Quaternion newRotation = transform.rotation * randomRotation;
        GameObject ball = Instantiate(ballPrefab, transform.position + transform.forward * ballOffset, newRotation);        
    }
}
