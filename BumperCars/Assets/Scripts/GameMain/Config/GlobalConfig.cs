using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GlobalConfig", menuName = "ScriptableObject/GlobalConfig", order = 0)]
public class GlobalConfig : ScriptableObject
{
    public float defaultCarForwardFrictionStiffness = 1;
    public float defaultCarSidewaysFrictionStiffness = 2;
    public float underAttackCarForwardFrictionStiffness = 0.1f;
    public float underAttackSidewaysFrictionStiffness = 0.1f;
    public float impactSpeed = 5f;//撞击时给对方的初速度
    public float frictionRestoreDelay = 1f;//打滑效果恢复的事件
    public float carAISearchRange = 12f;//AI索敌范围
    public float playerReviveTime = 5f;//玩家复活间隔
    public int targetScore = 10;//获胜分数
    public float carBoostCoolingTime = 5f;//冲刺冷却
    public float carSwitchCoolingTimeMin = 3f;//冲刺冷却随机下限
    public float carSwitchCoolingTimeMax = 6f;//冲刺冷却随机上限
    public string p1Name = "玩家1";
    public string p2Name = "玩家2";
    public int maxSkillCount = 3;//最大技能数量
    public bool isUnlimitedFire = true;//是否无限火力(技能卡不会被消耗)
    public int defaultCarMass = 1500;
    public int strongerCarMass = 2500;
    public float flipRecoverTime = 8f;
    public float extremeDodgeTime = 0.5f;
}
