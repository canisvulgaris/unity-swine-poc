using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  
 public class BallisticsMechanic : MonoBehaviour 
 {
     public float bullet_firing_velocity,gravity,bullet_damage; //variables
     private Vector3 bullet_velocity = new Vector3 (0, 0, 0); //the vector that contains bullet's current speed
     private Vector3 last_position = new Vector3(0,0,0), current_position = new Vector3 (0,0,0);
  
     // Use this for initialization
     void Start () 
     {
         bullet_velocity = transform.forward * bullet_firing_velocity; //bullet velocity = forward vector * bullet firing velocity
  
         /* linecasting */
         current_position = transform.position;
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
             Destroy(gameObject);
         }
         Debug.DrawLine (last_position, current_position, Color.red);
     }

     void OnCollisionEnter(Collision collision)
     {
        Destroy(gameObject);
     }
 }