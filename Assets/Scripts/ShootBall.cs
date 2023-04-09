using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    public GameObject ballPrefab;
    public float ballSpeed = 20f;
    public Camera mainCamera;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject ball = Instantiate(ballPrefab, mainCamera.transform.position, mainCamera.transform.rotation);
        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.velocity = mainCamera.transform.forward * ballSpeed;
    }
}
