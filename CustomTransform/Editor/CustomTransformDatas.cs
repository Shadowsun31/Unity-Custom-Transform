using UnityEngine;

public class CustomTransformDatas : ScriptableObject
{
    public bool m_is2D = false;
    public bool m_lockScale = true;

    public Vector3 m_copyPosition;
    public Vector3 m_copyRotation;
    public Vector3 m_copyScale;
}
