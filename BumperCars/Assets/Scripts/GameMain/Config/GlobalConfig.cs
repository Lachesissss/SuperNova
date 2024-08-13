using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConfig", menuName = "ScriptableObject/GlobalConfig", order = 0)]
public class GlobalConfig : ScriptableObject
{
    public float defaultCarForwardFrictionStiffness = 1;
    public float defaultCarSidewaysFrictionStiffness = 2;
    public float underAttackCarForwardFrictionStiffness = 0.1f;
    public float bossUnderAttackCarForwardFrictionStiffness = 0.5f;
    public float underAttackSidewaysFrictionStiffness = 0.1f;
    public float bossUnderAttackSidewaysFrictionStiffness = 1f;
    public float impactSpeed = 5f;//撞击时给对方的初速度
    public float frictionRestoreDelay = 1f;//打滑效果恢复的事件
    public float carAISearchRange = 12f;//AI索敌范围
    public float playerReviveTime = 5f;//玩家复活间隔
    public int targetScore = 10;//获胜分数
    public float carBoostCoolingTime = 5f;//冲刺冷却
    public float carSwitchCoolingTimeMin = 2f;//切换冷却随机下限
    public float carSwitchCoolingTimeMax = 2f;//切换冷却随机上限
    public string p1Name = "玩家1";
    public string p2Name = "玩家2";
    public int maxSkillCount = 3;//最大技能数量
    public bool isUnlimitedFire = true;//是否无限火力(技能卡不会被消耗)
    public int defaultCarMass = 1500;
    public int strongerCarMass = 2500;
    public float flipRecoverTime = 8f;
    public float extremeDodgeTime = 0.5f;
    public string pveBossName = "Boss";
    public int pveBossMaxHealth = 100;
    public float coinRefreshTime = 30f; //硬币刷新周期
    public float switchMinDistance = 8f; //切换最短距离
    public int maxCoinSpawnNum = 3; //同时生成的最大硬币（分数币）数量
    public bool p2UsingJoySticks = true; //P2控制
}
