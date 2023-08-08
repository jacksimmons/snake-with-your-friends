using UnityEngine;

public struct BodyPartRotationData
{
    public float CornerAngle;
    public float RegularAngle;

    public BodyPartRotationData(float cornerAngle, float regularAngle)
    {
        CornerAngle = cornerAngle;
        RegularAngle = regularAngle;
    }
}

/// <summary>
/// Class to represent the two states of rotation of a BodyPart.
/// Assigning to either angle type changes to the respective state, by assigning
/// to transform's rotation.
/// </summary>
public class BodyPartRotation
{
    private Transform m_transform { get; set; }

    private float _corAngle;
    public float CornerAngle
    {
        get { return _corAngle; }
        set
        {
            // Assign corner angle to transform (implicit state change)
            _corAngle = value % 360;
            m_transform.rotation = Quaternion.Euler(Vector3.forward * _corAngle);
        }
    }

    private float _regAngle;
    public float RegularAngle
    {
        get { return _regAngle; }
        set
        {
            // Assign regular angle to transform (implicit state change)
            _regAngle = value % 360;
            m_transform.rotation = Quaternion.Euler(Vector3.forward * _regAngle);
        }
    }

    public BodyPartRotation(Transform transform)
    {
        m_transform = transform;
        _corAngle = 0;
        _regAngle = transform.rotation.eulerAngles.z;
    }

    public BodyPartRotation(Transform transform, int cornerAngle, int regularAngle)
    {
        m_transform = transform;
        _corAngle = cornerAngle;
        _regAngle = regularAngle;
    }

    public BodyPartRotation(Transform transform, BodyPartRotationData data)
    {
        m_transform = transform;
        FromData(data);
    }

    public void FromData(BodyPartRotationData data)
    {
        _corAngle = data.CornerAngle;
        _regAngle = data.RegularAngle;
    }

    public BodyPartRotationData ToData()
    {
        BodyPartRotationData data = new(_corAngle, _regAngle);
        return data;
    }
}