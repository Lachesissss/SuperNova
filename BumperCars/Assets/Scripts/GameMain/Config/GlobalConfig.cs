using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConfig", menuName = "ScriptableObject/GlobalConfig", order = 0)]
public class GlobalConfig : ScriptableObject
{
    public Vector3 Player1StarPosition;
    public float defaultCarForwardFrictionStiffness = 1;
    public float defaultCarSidewaysFrictionStiffness = 2;
    public float underAttackCarForwardFrictionStiffness = 0.1f;
    public float underAttackSidewaysFrictionStiffness = 0.1f;
}