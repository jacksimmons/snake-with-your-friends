using UnityEngine;
using UnityEngine.UI;

public class SliderWithStep : MonoBehaviour
{
    [SerializeField]
    private float stepDiff;

    private Slider slider;
    private int numSteps;


    private void Start()
    {
        slider = GetComponent<Slider>();

        // Calculate number of steps based on max value & step difference
        numSteps = (int)(slider.maxValue / stepDiff);

        slider.onValueChanged.AddListener(OnValueChanged);
    }


    public void OnValueChanged(float value)
    {
        float range = (value / slider.maxValue) * numSteps;
        int ceil = Mathf.CeilToInt(range);
        slider.value = ceil * stepDiff;
    }
}
