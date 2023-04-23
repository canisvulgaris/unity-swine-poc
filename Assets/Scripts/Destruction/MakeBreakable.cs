using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBreakable : MonoBehaviour
{
    public float MinImpactToBreak = 10.0f;

    public bool BridgeMesh = false;
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
        if (!InitialBreak && other.gameObject != this.gameObject)
        {
            // Debug.Log("Collision " + gameObject.name + " with " + other.gameObject.name);
            if (other.attachedRigidbody)
            {
                // Get the Rigidbody component from the other collider
                Rigidbody otherRigidbody = other.attachedRigidbody;

                // Calculate the relative velocity
                if (gameObject.transform.parent == null || gameObject.transform.parent.GetComponent<Rigidbody>() == null)
                {
                    // if parent rigidbody has been destroyed, you can exit
                    return;
                }

                Vector3 relativeVelocity = otherRigidbody.velocity - gameObject.transform.parent.GetComponent<Rigidbody>().velocity;

                // Print the relative velocity
                // Debug.Log("Collision " + gameObject.name + " with " + other.gameObject.name + " at " + relativeVelocity.magnitude);
                // Debug.Log("Relative Velocity Magnitude: " + relativeVelocity.magnitude);

                if (relativeVelocity.magnitude > MinImpactToBreak)
                {
                    if (!InitialBreak)
                    {
                        InitialBreak = true;
                        if (BridgeMesh)
                        {
                            Destroy(transform.parent.GetComponent<Rigidbody>());
                            foreach (Transform child in transform.parent.transform) 
                            {
                                // print("Foreach loop: " + child);
                                Destroy(child.gameObject.GetComponent<BoxCollider>());
                                child.gameObject.AddComponent<Rigidbody>();
                                child.gameObject.GetComponent<GK.BreakableSurface>().enabled = true;                          
                            }
                            gameObject.transform.parent = null;
                            Vector3 pnt = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                            // Debug.Log("Break " + gameObject.name + " Point of impact: " + pnt);
                            gameObject.GetComponent<GK.BreakableSurface>().Init();
                            gameObject.GetComponent<GK.BreakableSurface>().Break((Vector2)transform.InverseTransformPoint(pnt));
                        }
                        else 
                        {
                            gameObject.transform.parent = null;
                            Destroy(gameObject.GetComponent<BoxCollider>());
                            gameObject.AddComponent<Rigidbody>();
                            gameObject.GetComponent<GK.BreakableSurface>().enabled = true;                          
                            Vector3 pnt = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                            // Debug.Log("Break " + gameObject.name + " Point of impact: " + pnt);
                            gameObject.GetComponent<GK.BreakableSurface>().Init();
                            gameObject.GetComponent<GK.BreakableSurface>().Break((Vector2)transform.InverseTransformPoint(pnt));
                        }
                    }
                }
            }
        }
    }
}
