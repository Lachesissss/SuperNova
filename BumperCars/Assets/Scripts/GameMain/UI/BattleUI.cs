using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class BattleUI : Entity
    {
        public struct BattleUIData
        {
            public string p1Name;
            public string p2Name;
            public int targetScore;
        }
        
        public Text player1ScoreText;
        public Text player2ScoreText;
        public Text targetScoreText;
        private string m_p1Name;
        private string m_p2Name;
        private int m_targetScore;
        
        
        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            GameEntry.EventManager.Subscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            if(userData is BattleUIData battleUIData)
            {
                m_p1Name = battleUIData.p1Name;
                m_p2Name = battleUIData.p2Name;
                m_targetScore = battleUIData.targetScore;
                Refresh(0,0);
                targetScoreText.text = $"目标：{m_targetScore}分";
            }
        }
        
        private void Refresh(int p1Score, int p2Score)
        {
            player1ScoreText.text = $"{m_p1Name}得分：{p1Score}";
            player2ScoreText.text = $"{m_p2Name}得分：{p2Score}";
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            GameEntry.EventManager.Unsubscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
        }

        private void OnScoreUpdate(object sender, GameEventArgs e)
        {
            if(e is ScoreUpdateEventArgs scoreUpdateEventArgs)
            {
                Refresh(scoreUpdateEventArgs.p1NewScore, scoreUpdateEventArgs.p2NewScore);
            }
            
        }
    }
}

