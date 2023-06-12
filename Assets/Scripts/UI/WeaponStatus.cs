using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponStatus : MonoBehaviour
{

    private TextMeshProUGUI heatStatus;
    // Start is called before the first frame update
    void Start()
    {
        heatStatus = gameObject.GetComponent<TextMeshProUGUI>();
        heatStatus.text = "0/100";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStatus(int currentHeat, int heatMax) {
        if (currentHeat >= heatMax) {
            heatStatus.color = Color.red;
            heatStatus.text = "OVERHEAT!";
        } else {
            heatStatus.color = Color.white;
            heatStatus.text = currentHeat + "/" + heatMax;
        }
    }
}
