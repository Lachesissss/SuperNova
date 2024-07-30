using UnityEngine;
using UnityEngine.Serialization;

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
    public float carAISearchRange = 12f;//AI索敌范围
    public float playerReviveTime = 5f;//玩家复活间隔
    public int targetScore = 10;//获胜分数
}
