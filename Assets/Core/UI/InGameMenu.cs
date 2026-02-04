using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameMenu : BasePanel
{
    public GameObject UIScoreHolder;
    public GameObject UIHpHolder;
    public GameObject UITimerHolder;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        if(player != null)
        {
            var pc = player.GetComponent<PlayerController>();

            pc.OnHpChanged += RefreshHp;
            pc.OnScoreChanged += RefreshScore;

            RefreshHp(pc.Hp);
            RefreshScore(pc.Score);
        }
    }

    private void Update()
    {
        RefreshTimer(player.GetComponent<PlayerController>().timer);
    }

    public void RefreshScore(int score)
    {
        UIScoreHolder.GetComponent<TMP_Text>().text = score.ToString();
    }

    public void RefreshHp(int hp)
    {
        UIHpHolder.GetComponent<TMP_Text>().text = hp.ToString();
    }

    public void RefreshTimer(float timer)
    {
        UITimerHolder.GetComponent<TMP_Text>().text = FormatTimer(timer);
    }

    private string FormatTimer(float totalSeconds)
    {  
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        float fractionalPart = totalSeconds - Mathf.Floor(totalSeconds);
        int sixtiethsOfSecond = Mathf.FloorToInt(fractionalPart * 60f);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, sixtiethsOfSecond);
    }
}
