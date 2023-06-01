using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ForegroundObject : MonoBehaviour
{
    private float _opacityDecrement = 1f/255f / 10f;
    private Image _img;

    private void Start()
    {
        _img = GetComponent<Image>();
        StartCoroutine(Lifecycle());
    }

    private IEnumerator Lifecycle()
    {
        while (_img.color.a > 0)
        {
            _img.color = new Color(_img.color.r, _img.color.g, _img.color.b, _img.color.a - _opacityDecrement);
            yield return null;
        }
        Destroy(gameObject);
        yield break;
    }
}