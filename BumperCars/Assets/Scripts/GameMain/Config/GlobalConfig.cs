using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConfig", menuName = "ScriptableObject/GlobalConfig", order = 0)]
public class GlobalConfig : ScriptableObject
{
    public Vector3 Player1StarPosition;
    public float defaultCarForwardFrictionStiffness = 1;
    public float defaultCarSidewaysFrictionStiffness = 2;
    public float underAttackCarForwardFrictionStiffness = 0.1f;
    public float underAttackSidewaysFrictionStiffness = 0.1f;
    public float impactSpeed = 5f;//撞击时给对方的初速度
    public float frictionRestoreDelay = 1f;//打滑效果恢复的事件
    public float CarAISearchRange = 10f;
}
