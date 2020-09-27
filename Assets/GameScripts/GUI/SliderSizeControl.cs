using UnityEngine;
using UnityEngine.UI;

public class SliderSizeControl : MonoBehaviour
{
    public Slider slider;
    public RectTransform rectTransform;
    public float multipleScale = .1f;
    float oldMaxValue;

    private void OnDrawGizmosSelected()
    {
        UpdateScale();
    }

    public void UpdateScale()
    {
        if(rectTransform && slider)
        {
            if(slider.maxValue != oldMaxValue)
            {
                var sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = slider.maxValue * multipleScale;
                rectTransform.sizeDelta = sizeDelta;
                oldMaxValue = slider.maxValue;
            }
        }
    }
}
