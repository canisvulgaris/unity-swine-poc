using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  
 public class BallisticsMechanic : MonoBehaviour 
 {
    public float bullet_firing_velocity,gravity,bullet_damage;
    public GameObject bulletDebris;

    private int numDebris = 5;
    private float debrisSpeed = 1f;
    private float minDebrisAngle = -10f;
    private float maxDebrisAngle = 10f;

    private Vector3 bullet_velocity = new Vector3 (0, 0, 0); //the vector that contains bullet's current speed
    private Vector3 last_position = new Vector3(0,0,0), 
                    current_position = new Vector3 (0,0,0),
                    original_position = new Vector3 (0,0,0);

     private Vector3[] debrisPosition = new [] { new Vector3(0f,0.2f,0f), new Vector3(0.2f,0f,0f), new Vector3(0f,0f,0.2f) };

     // Use this for initialization
     void Start () 
     {
         bullet_velocity = transform.forward * bullet_firing_velocity; //bullet velocity = forward vector * bullet firing velocity
  
         /* linecasting */
         current_position = transform.position;
         original_position = transform.position;
        //  DestroyObject (gameObject, 3); //destroy bullet after 3 sec's
     }
     
     void FixedUpdate () 
     {
        gameObject.GetComponent<Rigidbody>().velocity = bullet_velocity; //make the bullet's velocity equal to current velocity value
        bullet_velocity.y -= gravity; //decrease the y axis of bullet velocity by amount of gravity

        /* linecasting */
        RaycastHit hit;
        last_position = current_position;
        current_position = transform.position;

        int mask = 1 << 6;
        mask = ~mask;

        if (Physics.Linecast(last_position, current_position, out hit, mask))
        {
            hit.transform.SendMessage("AddDamage", bullet_damage, SendMessageOptions.DontRequireReceiver); //Send Damage message to hit object
            Debug.DrawLine (original_position, current_position, Color.red, 0.2f);
            Destroy(gameObject);
        }            
     }

     void OnCollisionEnter(Collision coll)
     {
        SpawnDebris(coll);
     }

     void SpawnDebris(Collision coll) {
        for (int i = 0; i < Random.Range(1, numDebris); i++) {
            GameObject debris = Instantiate(bulletDebris, transform.position + transform.forward * -1, Quaternion.identity);
            Rigidbody debrisRigidbody = debris.GetComponent<Rigidbody>();

            // Apply a random direction to the debris
            Vector3 randomDirection = Quaternion.Euler(Random.Range(minDebrisAngle, maxDebrisAngle), Random.Range(minDebrisAngle, maxDebrisAngle), 0f) * Vector3.back;
            debrisRigidbody.velocity = randomDirection * debrisSpeed;


            // Apply the same force to the debris as the bullet collision
            debrisRigidbody.AddForce(coll.relativeVelocity * 0.01f, ForceMode.Impulse);
        }
        
     }
 }