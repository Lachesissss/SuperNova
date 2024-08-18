using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerHome : EntityComponent
    {
        public ParticleSystem levelUpEffect;
        public enum PlayerHomeType
        {
            Player1,
            Player2
        }

        private string playerName;

        public PlayerHomeType type;

        private int m_playerScore;

        public override void OnEntityInit(object userData = null)
        {
            base.OnEntityInit(userData);
            if (type == PlayerHomeType.Player1) playerName = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p1Name;
            if (type == PlayerHomeType.Player2) playerName = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2Name;
        }

        public override void OnEntityReCreateFromPool(object userData = null)
        {
            base.OnEntityReCreateFromPool(userData);
            m_playerScore = 0;
            levelUpEffect.Stop();
            GameEntry.EventManager.AddListener(ScoreUIUpdateEventArgs.EventId, OnGetScore);
        }

        public override void OnEntityReturnToPool(bool isShutDown = false)
        {
            base.OnEntityReturnToPool(isShutDown);
            GameEntry.EventManager.RemoveListener(ScoreUIUpdateEventArgs.EventId, OnGetScore);
        }

        private void OnGetScore(object sender, GameEventArgs e)
        {
            if (e is ScoreUIUpdateEventArgs args)
            {
                if (type == PlayerHomeType.Player1)
                {
                    if (m_playerScore != args.p1NewScore)
                    {
                        m_playerScore = args.p1NewScore;
                        levelUpEffect.Play();
                        GameEntry.SoundManager.PlayerSound(entity, SoundEnum.GetPoints, false);
                    }
                }
                else if (type == PlayerHomeType.Player2)
                {
                    if (m_playerScore != args.p2NewScore)
                    {
                        m_playerScore = args.p2NewScore;
                        levelUpEffect.Play();
                        GameEntry.SoundManager.PlayerSound(entity, SoundEnum.GetPoints, false);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BumperCar"))
            {
                var controller = other.GetComponent<CarBody>().carComponent;
                if (playerName == controller.carControllerName)
                    GameEntry.EventManager.Invoke(this, PlayerArriveHomeEventArgs.Create(controller.carControllerName));
            }
        }
    }
}