using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YouWonMenu : BasePanel
{
    public GameObject UITryAgainButton;
    public GameObject UITimeTakenHolder;

    private GameObject player;


    protected override void Awake()
    {
        InitClick();
    }

    private void InitClick()
    {
        UITryAgainButton.GetComponent<Button>().onClick.AddListener(OnClickTryAgainButton);
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void OnClickTryAgainButton()
    {
        GameManager.Instance.GameReStart();
        ClosePanel();
    }

    public void SetTimeTaken(float timer)
    {
        UITimeTakenHolder.GetComponent<TMP_Text>().text = FormatTimer(timer);
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
