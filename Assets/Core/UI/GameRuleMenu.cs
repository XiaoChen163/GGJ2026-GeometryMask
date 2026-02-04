using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRuleMenu : BasePanel
{
    public GameObject UIBackButton;

    protected override void Awake()
    {
        InitClick();
    }

    private void InitClick()
    {
        UIBackButton.GetComponent<Button>().onClick.AddListener(OnClickBackButton);
    }

    private void OnClickBackButton()
    {
        UIManager.Instance.OpenPanel("MainMenu");
        ClosePanel();
    }
}
