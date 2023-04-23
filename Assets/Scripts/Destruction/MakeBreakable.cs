using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBreakable : MonoBehaviour
{
    public float MinImpactToBreak = 6.0f;
    private bool InitialBreak = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other) {        
        if (other.gameObject != this.gameObject)
        {
            // Debug.Log("Collision " + gameObject.name + " with " + other.gameObject.name);
            if (other.attachedRigidbody)
            {
                // Get the Rigidbody component from the other collider
                Rigidbody otherRigidbody = other.attachedRigidbody;

                // Calculate the relative velocity
                Vector3 relativeVelocity = otherRigidbody.velocity - gameObject.transform.parent.GetComponent<Rigidbody>().velocity;

                // Print the relative velocity
                Debug.Log("Collision " + gameObject.name + " with " + other.gameObject.name + " at " + relativeVelocity.magnitude);
                // Debug.Log("Relative Velocity Magnitude: " + relativeVelocity.magnitude);

                if (relativeVelocity.magnitude > MinImpactToBreak)
                {
                    if (!InitialBreak)
                    {
                        Destroy(gameObject.GetComponent<BoxCollider>());
                        gameObject.AddComponent<Rigidbody>();
                        gameObject.GetComponent<GK.BreakableSurface>().enabled = true;
                        // var pnt = other.contacts[0].point;
                        // Break((Vector2)transform.InverseTransformPoint(pnt));
                        InitialBreak = true;
                    }
                }
            }
        }
    }
}
