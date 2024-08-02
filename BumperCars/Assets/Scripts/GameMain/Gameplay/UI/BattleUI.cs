using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.Serialization;
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
        public List<SkillSlot> skillSlots;
        private string m_p1Name;
        private string m_p2Name;
        private int m_targetScore;
        private CarPlayer m_player;
        
        public struct BattleUIData
        {
            public string p1Name;
            public string p2Name;
            public int targetScore;
            public CarPlayer carPlayer;
        }

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            //m_maxSkillCount = GameEntry.ConfigManager.GetConfig<GlobalConfig>().maxSkillCount;
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            GameEntry.EventManager.Subscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            GameEntry.EventManager.Subscribe(PlayerBattleUIUpdateEventArgs.EventId, OnPlayerBattleUIUpdate);
            if(userData is BattleUIData battleUIData)
            {
                RefreshAll(battleUIData);
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
        
        private void RefreshScore(int p1Score, int p2Score)
        {
            player1ScoreText.text = $"{m_p1Name}得分：{p1Score}";
            player2ScoreText.text = $"{m_p2Name}得分：{p2Score}";
            targetScoreText.text = $"目标：{m_targetScore}分";
        }

        public void RefreshAll(BattleUIData battleUIData)
        {
            m_p1Name = battleUIData.p1Name;
            m_p2Name = battleUIData.p2Name;
            m_player = battleUIData.carPlayer;
            m_targetScore = battleUIData.targetScore;
            RefreshScore(0,0);
            RefreshPlayerBattleUI();
        }
        
        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            GameEntry.EventManager.Unsubscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            settingBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.RemoveAllListeners();
            backToTittleBtn.onClick.RemoveAllListeners();
        }

        private void OnScoreUpdate(object sender, GameEventArgs e)
        {
            if(e is ScoreUpdateEventArgs scoreUpdateEventArgs)
            {
                RefreshScore(scoreUpdateEventArgs.p1NewScore, scoreUpdateEventArgs.p2NewScore);
            }
        }
        
        private void OnPlayerBattleUIUpdate(object sender, GameEventArgs e)
        {
            if(e is PlayerBattleUIUpdateEventArgs args)
            {
                RefreshPlayerBattleUI();
            }
        }
        
        private void RefreshPlayerBattleUI()
        {
            if(m_player!=null)
            {
                var len = GameEntry.ConfigManager.GetConfig<GlobalConfig>().maxSkillCount;
                for (int i=0;i<len;i++)
                {
                    var skill = m_player.skillSlots[i];
                    if(skill!=null)
                    {
                        skillSlots[i].skillImage.sprite = GameEntry.AtlasManager.GetSprite(AtlasEnum.Skill, GameEntry.SkillManager.GetSkillIconName(skill.skillEnum));
                        skillSlots[i].skillGO.SetActive(true);
                    }
                    else
                    {
                        skillSlots[i].skillGO.SetActive(false);
                    }
                    
                }
                
                
            }
        }
    }
}

