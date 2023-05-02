using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConsoleMessages : MonoBehaviour
{
    private bool failState;
    public int totalEnemyCount;
    
    private TextMeshProUGUI consoleText;

    // Start is called before the first frame update
    void Start()
    {
        failState = false;
        consoleText = gameObject.GetComponent<TextMeshProUGUI>();
        ShowDefault();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDefault() {
        consoleText.text = "Find and destroy all enemy bots!";
    }

    public void ShowSuccess() {
        if (!failState) {
            consoleText.color = Color.green;
            consoleText.text = "Success!\nPress R to restart.";
        }
    }

    public void ReduceEnemyCount() {
        totalEnemyCount -= 1;
        if (totalEnemyCount <= 0) {
            ShowSuccess();
        }
    }

    public void ShowFail() {
        consoleText.color = Color.red;
        consoleText.text = "You are dead.\nPress R to restart.";
        failState = true;
    }
}
