using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class BattleUI : Entity
    {
        public Text player1ScoreText;
        public Text player2ScoreText;
        public Text targetScoreText;
        public Button settingBtn;
        public Button continueBtn;
        public Button backToTittleBtn;
        public GameObject popupGo;
        private string m_p1Name;
        private string m_p2Name;
        private int m_targetScore;
        private Player m_player;
        
        public struct BattleUIData
        {
            public string p1Name;
            public string p2Name;
            public int targetScore;
            public Player carPlayer;
        }
        
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            GameEntry.EventManager.Subscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            if(userData is BattleUIData battleUIData)
            {
                m_p1Name = battleUIData.p1Name;
                m_p2Name = battleUIData.p2Name;
                m_player = battleUIData.carPlayer;
                m_targetScore = battleUIData.targetScore;
                Refresh(0,0);
                targetScoreText.text = $"目标：{m_targetScore}分";
            }
            settingBtn.onClick.AddListener(OnSettingBtnClicked);
            continueBtn.onClick.AddListener(OnContinueBtnClicked);
            backToTittleBtn.onClick.AddListener(OnBackToTittleBtnClicked);
            popupGo.SetActive(false);
        }

        private void OnSettingBtnClicked()
        {
            popupGo.SetActive(true);
        }

        private void OnContinueBtnClicked()
        {
            popupGo.SetActive(false);
        }

        private void OnBackToTittleBtnClicked()
        {
            GameEntry.EventManager.Fire(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
        }
        
        private void Refresh(int p1Score, int p2Score)
        {
            player1ScoreText.text = $"{m_p1Name}得分：{p1Score}";
            player2ScoreText.text = $"{m_p2Name}得分：{p2Score}";
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
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

