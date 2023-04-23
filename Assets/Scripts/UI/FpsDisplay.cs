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

    void Start()
    {
        GameObject fpsObject = new GameObject("FpsText");
        fpsObject.transform.SetParent(transform);

        fpsText = fpsObject.AddComponent<Text>();
        fpsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fpsText.fontSize = fontSize;
        fpsText.color = textColor;
        fpsText.horizontalOverflow = HorizontalWrapMode.Overflow;
        fpsText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = fpsText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = -margin;
    }

    void Update()
    {
        frames++;
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= 1f)
        {
            int fps = Mathf.RoundToInt(frames / timeElapsed);
            fpsText.text = fps.ToString() + " FPS";
            frames = 0;
            timeElapsed = 0;
        }
    }
}
