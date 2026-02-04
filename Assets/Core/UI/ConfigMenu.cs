using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ConfigMenu : BasePanel
{
    public GameObject UIDifficultyDropdown;
    public GameObject UIBackButton;

    protected override void Awake()
    {
        InitClick();
    }

    private void InitClick()
    {
        UIBackButton.GetComponent<Button>().onClick.AddListener(OnClickBackButton);
        UIDifficultyDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnValueChangedDifficultyDropdown);
        
    }

    //public float _DetectionRange;
    //public float _MoveSpeed;
    //public float _SpotTimeMin;
    //public float _SpotTimeMax;
    //public float _BulletSpeed;

    private void OnValueChangedDifficultyDropdown(int arg0)
    {
        switch (arg0)
        {
            case 0:
                Debug.Log("Current Difficulty: Easy");
                Debug.Log("DetectionRange: 5");
                Debug.Log("MoveSpeed: 3");
                Debug.Log("SpotTimeMin: 0.75");
                Debug.Log("SpotTimeMax: 3");
                Debug.Log("BulletSpeed: 10");
                Debug.Log("FireInterval: 1.2");
                GameManager.Instance.ChangeAiValue(5f, 3f, 0.75f, 3f, 10f, 1.2f);
                break;
            case 1:
                Debug.Log("Current Difficulty: Normal");
                Debug.Log("DetectionRange: 6");
                Debug.Log("MoveSpeed: 3");
                Debug.Log("SpotTimeMin: 0.5");
                Debug.Log("SpotTimeMax: 1.5");
                Debug.Log("BulletSpeed: 20");
                Debug.Log("FireInterval: 1");
                GameManager.Instance.ChangeAiValue(6f, 3f, 0.5f, 1.5f, 20f, 1f);
                break;
            case 2:
                Debug.Log("Current Difficulty: Hard");
                Debug.Log("DetectionRange: 7");
                Debug.Log("MoveSpeed: 3");
                Debug.Log("SpotTimeMin: 0.1");
                Debug.Log("SpotTimeMax: 1");
                Debug.Log("BulletSpeed: 25");
                Debug.Log("FireInterval: 0.8");
                GameManager.Instance.ChangeAiValue(7f, 3f, 0.1f, 1f, 25f, 0.8f);
                break;
        }
    }

    private void OnClickBackButton()
    {
        UIManager.Instance.OpenPanel("MainMenu");
        ClosePanel();
    }
}
