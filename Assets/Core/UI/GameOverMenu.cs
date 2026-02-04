using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : BasePanel
{
    public GameObject UITryAgainButton;

    protected override void Awake()
    {
        InitClick();
    }

    private void InitClick()
    {
        UITryAgainButton.GetComponent<Button>().onClick.AddListener(OnClickTryAgainButton);
    }

    private void OnClickTryAgainButton()
    {
        GameManager.Instance.GameReStart();
        ClosePanel();
    }
}
