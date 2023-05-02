using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsDisplay : MonoBehaviour
{
    public int fontSize = 24;
    public Color textColor = Color.white;
    public Vector2 margin = new Vector2(10, 10);

    private Text fpsText;
    private int frames;
    private float timeElapsed;

    private float count;
    
    private IEnumerator Start()
    {
        GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // private void OnGUI()
    // {
    //     GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(count));
    // }

    private void OnGUI()
    {
        DrawFPS();

    }

    private void DrawFPS() {
        Rect location = new Rect(5, 5, 85, 25);
        string text = $"FPS: {Mathf.Round(count)}";
        Texture black = Texture2D.linearGrayTexture;
        GUI.DrawTexture(location, black, ScaleMode.StretchToFill);
        GUI.color = Color.black;
        GUI.skin.label.fontSize = 18;
        GUI.Label(location, text);
    }

}
