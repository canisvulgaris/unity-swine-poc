using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBallPlayer : MonoBehaviour
{

    public GameObject ballPrefab;
    public float ballSpeed = 20f;

    // Start is called before the first frame update
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        GameObject ball = Instantiate(ballPrefab, transform.position, transform.rotation);
        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.velocity = transform.forward * ballSpeed;
        
    }
}
