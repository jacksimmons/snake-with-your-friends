using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TooManyPints : MonoBehaviour
{
    // Lens Distortion bounce
    private float _dizziness = 0f;
    private float _dizzinessPerPint = 33f;
    // Makes wooziness last longer
    private float _dizzinessDurationMod = 3f;

    // Chromatic Aberration
    private float _wooziness = 0f;
    private float _woozinessPerPint = 1f;

    private PostProcessVolume _volume;
    private LensDistortion _ld;
    private ChromaticAberration _chromaticAberration;

    private void Start()
    {
        _ld = ScriptableObject.CreateInstance<LensDistortion>();
        _ld.enabled.Override(true);
        _ld.intensity.Override(0);

        _chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        _chromaticAberration.enabled.Override(true);
        _chromaticAberration.intensity.Override(0);

        PostProcessEffectSettings[] settings = { _ld, _chromaticAberration };

        _volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, settings);
        _volume.isGlobal = true;
    }

    private void Update()
    {
        if (!Mathf.Approximately(_dizziness, 0f))
        {
            _ld.intensity.value = Mathf.PingPong(Time.time * (_dizziness / _dizzinessDurationMod), _dizziness * 2) - _dizziness;
        }

        if (!Mathf.Approximately(_wooziness, 0f))
        {
            _chromaticAberration.intensity.value = _wooziness;
        }
    }

    /// <summary>
    /// Converts pints to wooziness/dizziness.
    /// </summary>
    /// <param name="pints">Number of pints consumed.</param>
    public void UpdatePints(int pints)
    {
        _dizziness = _dizzinessPerPint * pints;
        if (Mathf.Approximately(_dizziness, 0f))
        {
            _ld.intensity.value = 0f;
        }
        _wooziness = _woozinessPerPint * pints;
        if (Mathf.Approximately(_wooziness, 0f))
        {
            _chromaticAberration.intensity.value = 0f;
        }
    }

    private void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(_volume, true);
    }
}
