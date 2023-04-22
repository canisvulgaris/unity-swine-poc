using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructReplace : MonoBehaviour
{
    public GameObject parentOuter;
    public GameObject parentDetail;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Projectile")
        {
            Debug.Log("Player or Projectile has entered the trigger");
            parentDetail.SetActive(true);
            parentOuter.SetActive(false);
        }
        else {
            Debug.Log("Something has entered the trigger");
        }
    }
}
