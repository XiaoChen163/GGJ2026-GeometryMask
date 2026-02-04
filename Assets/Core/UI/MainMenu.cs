using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : BasePanel
{
    public GameObject UIStartGameButton;
    public GameObject UIGameRulesButton;
    public GameObject UIConfigButton;
    public GameObject UIQuitButton;
    public GameObject UIPersonalURL;

    protected override void Awake()
    {
        InitClick();
    }

    private void InitClick()
    {
        UIStartGameButton.GetComponent<Button>().onClick.AddListener(OnClickStartGameButton);
        UIGameRulesButton.GetComponent<Button>().onClick.AddListener(OnClickGameRulesButton);
        UIConfigButton.GetComponent<Button>().onClick.AddListener(OnClickConfigButton);
        UIQuitButton.GetComponent<Button>().onClick.AddListener(OnClickQuitButton);
        UIPersonalURL.GetComponent<Button>().onClick.AddListener(OnClickPersonalURL);
    }

    private void OnClickPersonalURL()
    {
        Application.OpenURL("https://space.bilibili.com/277039923");
    }

    private void OnClickStartGameButton()
    {
        GameManager.Instance.GameStart();
        UIManager.Instance.OpenPanel("InGameMenu");
        ClosePanel();
    } 

    private void OnClickGameRulesButton()
    {
        UIManager.Instance.OpenPanel("GameRuleMenu");
        ClosePanel();
    }

    private void OnClickConfigButton()
    {
        UIManager.Instance.OpenPanel("ConfigMenu");
        ClosePanel();
    }

    private void OnClickQuitButton()
    {
        Application.Quit();
    }
}


