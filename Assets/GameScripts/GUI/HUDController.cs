using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    #region General Variables

    #region Health/Stamina Variables
    [Header("Health/Stamina")]
    public Slider healthSlider;
    public Slider staminaSlider;
    [Header("DamageHUD")]
    public Image damageImage;
    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    [HideInInspector] public bool damaged;
    #endregion

    #region Display Controls Variables
    [Header("Controls Display")]
    [HideInInspector]
    public bool controllerInput;
    public Image displayControls;
    public Sprite joystickControls;
    public Sprite keyboardControls;
    #endregion

    #region Debug Info Variables
    [Header("Debug Window")]
    public GameObject debugPanel;
    [HideInInspector]
    public Text debugText;
    #endregion

    #region Change Input Text Variables
    [Header("Text with FadeIn/Out")]
    public Text fadeText;
    private float textDuration, fadeDuration, durationTimer, timer;
    private Color startColor, endColor;
    private bool fade;
    #endregion

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

    public void ShowText(string message, float textTime = 2f, float fadeTime = 0.5f)
    {
        if (fadeText != null && !fade)
        {
            fadeText.text = message;
            textDuration = textTime;
            fadeDuration = fadeTime;
            durationTimer = 0f;
            timer = 0f;
            fade = true;
        }
        else if (fadeText != null)
        {
            fadeText.text += "\n" + message;
            textDuration = textTime;
            fadeDuration = fadeTime;
            durationTimer = 0f;
            timer = 0f;
        }
    }

    public void ShowText(string message)
    {
        if (fadeText != null && !fade)
        {
            fadeText.text = message;
            textDuration = 2f;
            fadeDuration = 0.5f;
            durationTimer = 0f;
            timer = 0f;
            fade = true;
        }
        else if (fadeText != null)
        {
            fadeText.text += "\n" + message;
            textDuration = 2f;
            fadeDuration = 0.5f;
            durationTimer = 0f;
            timer = 0f;
        }
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
