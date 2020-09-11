using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    #region Debug Info Variables

    [Header("Debug Info")]
    public GameObject debugPanel;
    [HideInInspector]
    public Text debugText;

    #endregion

    private static HUDController _instance;

    public static HUDController instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<HUDController>();
                //this will not destroy object at start and do not create another
            }
            return _instance;
        }
    }

    private void Start()
    {
        if (debugPanel != null)
            debugText = debugPanel.GetComponentInChildren<Text>();
    }

    public void Init(PlayerController cc)
    {
    }

    public virtual void UpdateHUD(PlayerController cc)
    {
        UpdateDebugWindow(cc);
    }

    void UpdateDebugWindow(PlayerController cc)
    {
        if(cc.debugWindow)
        {
            if (debugPanel != null && !debugPanel.activeSelf)
                debugPanel.SetActive(true);
            if (debugText)
                debugText.text = cc.DebugInfo();
        }
        else
        {
            if (debugPanel != null && debugPanel.activeSelf)
                debugPanel.SetActive(false);
        }
    }
}
