using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public float roundTime = 10f;

    UIManager uiMan;

    public float scoreSpeed = 2f;
    public int scoreValue;
    float scoreDisplay;

    private void Awake()
    {
        uiMan = FindObjectOfType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (roundTime > 0)
        {
            roundTime -= Time.deltaTime;
            if (roundTime <= 0)
            {
                roundTime = 0;
                WinCheck();
            }
        }

        uiMan.timeValue.text = $"{roundTime:0.0}s";

        scoreDisplay = Mathf.Lerp(scoreDisplay, scoreValue, scoreSpeed * Time.deltaTime);
        uiMan.scoreValue.text = $"{scoreDisplay:0}";
    }

    void WinCheck()
    {

    }
}
