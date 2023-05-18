using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TooManyPints : MonoBehaviour
{
    private float _wooziness_per_pint = 33f;
    private float _wooziness;
    // Makes wooziness last longer
    private float _wooziness_duration_mod = 3f;

    private PostProcessVolume _volume;
    private LensDistortion _ld;
    private ChromaticAberration _chromaticAberration;

    private void Start()
    {
        _wooziness = _wooziness_per_pint;

        _ld = ScriptableObject.CreateInstance<LensDistortion>();
        _ld.enabled.Override(true);
        _ld.intensity.Override(1);

        _chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        _chromaticAberration.enabled.Override(true);
        _chromaticAberration.intensity.Override(1);

        PostProcessEffectSettings[] settings = { _ld, _chromaticAberration };

        _volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, settings);
        _volume.isGlobal = true;
    }

    private void Update()
    {
        print(_ld.intensity.value);
        _ld.intensity.value = Mathf.PingPong(Time.time * (_wooziness / _wooziness_duration_mod), _wooziness * 2) - _wooziness;
        _chromaticAberration.intensity.value = _wooziness / 33;
    }

    public void UpdatePints(int pints)
    {
        _wooziness = _wooziness_per_pint * pints;
    }

    private void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(_volume, true);
    }
}
