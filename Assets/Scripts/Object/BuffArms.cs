using UnityEngine;

public class BuffArms : MonoBehaviour
{
    // 0 or 1 to guarantee it exists (snakes have a size >= 2)
    private const int BP_INDEX_TO_FOLLOW = 0;

    private PlayerMovement m_pm;
    private BodyPart m_bp;


    public void Setup(PlayerMovement pm)
    {
        m_pm = pm;
        m_bp = m_pm.BodyParts[BP_INDEX_TO_FOLLOW];
    }


    private void FixedUpdate()
    {
        if (m_bp != null)
        {
            transform.SetPositionAndRotation(m_bp.Position, m_bp.Transform.rotation);
        }
    }
}