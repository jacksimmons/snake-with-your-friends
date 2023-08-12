using UnityEngine;

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

    public BodyPartRotation(Transform transform, float cornerAngle, float regularAngle)
    {
        m_transform = transform;
        _corAngle = cornerAngle;
        _regAngle = regularAngle;
    }
}