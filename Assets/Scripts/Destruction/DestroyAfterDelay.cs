using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float minDelay = 0.5f;
    public float maxDelay = 2f;

    private void Start()
    {
        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        Destroy(gameObject);
    }
}
