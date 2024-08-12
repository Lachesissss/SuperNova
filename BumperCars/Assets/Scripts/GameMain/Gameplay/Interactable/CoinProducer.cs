using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CoinProducer : EntityComponent
    {
        private float curInterval;
        private GlobalConfig m_globalConfig;
        private Vector3 m_spawnPosition;
        private int curCoinNum;

        public override void OnEntityInit(object userData = null)
        {
            base.OnEntityInit(userData);
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void OnEntityReCreateFromPool(object userData = null)
        {
            base.OnEntityReCreateFromPool(userData);
            curCoinNum = 0;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            GameEntry.EventManager.AddListener(CoinPickedUpEventArgs.EventId, OnCoinPickedUp);
            curInterval = 0;
        }

        private void OnCoinPickedUp(object sender, GameEventArgs e)
        {
            if(e is CoinPickedUpEventArgs args)
            {
                curCoinNum -= args.pickedNum;
            }
        }

        public override void OnEntityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnEntityUpdate(elapseSeconds, realElapseSeconds);
            if(curCoinNum>=m_globalConfig.maxCoinSpawnNum) 
                return;
            
            curInterval += realElapseSeconds;
            if (curInterval > m_globalConfig.coinRefreshTime)
            {
                curInterval = 0;
                var randomPoint = Random.insideUnitCircle * 3f; // 生成半径范围内的随机点
                m_spawnPosition.x = transform.position.x + randomPoint.x;
                m_spawnPosition.y = transform.position.y -1.4f;
                m_spawnPosition.z = transform.position.z + randomPoint.y;
                GameEntry.EntityManager.CreateEntity<Coin>(EntityEnum.Coin, m_spawnPosition, Quaternion.identity);
                curCoinNum++;
            }
        }
    }
}