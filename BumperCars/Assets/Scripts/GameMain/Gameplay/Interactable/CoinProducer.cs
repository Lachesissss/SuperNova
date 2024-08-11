using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CoinProducer : EntityComponent
    {
        private float curInterval;
        private GlobalConfig m_globalConfig;
        private Vector3 m_spawnPosition;

        public override void OnEntityReCreateFromPool(object userData = null)
        {
            base.OnEntityReCreateFromPool(userData);
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            curInterval = 0;
        }

        public override void OnEntityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnEntityUpdate(elapseSeconds, realElapseSeconds);
            curInterval += realElapseSeconds;
            if (curInterval > m_globalConfig.coinRefreshTime)
            {
                curInterval = 0;
                var randomPoint = Random.insideUnitCircle * 3f; // 生成半径范围内的随机点
                m_spawnPosition.x = transform.position.x + randomPoint.x;
                m_spawnPosition.y = transform.position.y + 0.5f;
                m_spawnPosition.z = transform.position.z + randomPoint.y;
                GameEntry.EntityManager.CreateEntity<Coin>(EntityEnum.Coin, m_spawnPosition, Quaternion.identity);
            }
        }
    }
}