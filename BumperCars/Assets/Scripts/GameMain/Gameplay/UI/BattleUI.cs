using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using Unity.VisualScripting;
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
        public Transform popupTipsRootTrans;
        private string m_p1Name;
        private string m_p2Name;
        private int m_targetScore;
        private CarPlayer m_player;
        private static BattleUI m_instance;
        private Queue<string> popupTextQueue;
        private List<PopupTips> curShowTips;
        
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
            popupTextQueue = new();
            curShowTips = new();
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            m_instance = this;
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
            foreach (var tip in curShowTips)
            {
                GameEntry.EntityManager.ReturnEntity(EntityEnum.PopupTips, tip);
            }
            curShowTips.Clear();
            popupTextQueue.Clear();
        }
        
        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            m_instance = null;
            GameEntry.EventManager.Unsubscribe(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            settingBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.RemoveAllListeners();
            backToTittleBtn.onClick.RemoveAllListeners();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            PopupTipsUpdate();
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
        
        public void RefreshPlayerBattleUI()
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
        
        public static void ShowPopupTips(string content)
        {
            if(m_instance==null)
            {
                Debug.LogError("当前没有BattleUI实例，无法弹Tips");
            }
            m_instance.AddPopup(content);
        }
        
        public void AddPopup(string content)
        {
            if (content == string.Empty)
            {
                return;
            }

            bool isEmptyQueue = (popupTextQueue.Count == 0);
            popupTextQueue.Enqueue(content);

            if (isEmptyQueue)
            {
                if (curShowTips.Count > 0)
                {
                    SetNextPopupWaitTime();
                }
                else
                {
                    m_nextPopupWaitTime = 0;
                }
            }
        }
        
        /// <summary>
        /// 下一个显示间隔
        /// </summary>
        private float m_nextPopupWaitTime = 0;
        private void SetNextPopupWaitTime()
        {
            if (popupTextQueue.Count > 0)
            {
                m_nextPopupWaitTime = 0.2f;
            }
        }
        
        private float time;
        private const int MaxPopup = 3;
        
        public void PopupTipsUpdate()
        {
            if (popupTextQueue.Count > 0)
            {
                if (Time.time - time > m_nextPopupWaitTime)
                {
                    var text = popupTextQueue.Dequeue();
                    var item = GameEntry.EntityManager.CreateEntity<PopupTips>(EntityEnum.PopupTips, popupTipsRootTrans, text);
                    var rectTransFrom = item.transform as RectTransform;
                    if (rectTransFrom != null)
                    {
                        float delta = rectTransFrom.rect.height;

                        foreach (var it in curShowTips)
                        {
                            it.MoveUp(delta + 20);
                        }
                    }
                    //绑定移除数组的回调
                    item.BindFinish(RemoveItem);
                    curShowTips.Insert(0, item);

                    if (curShowTips.Count > MaxPopup) //如果超出最大范围，立即回收
                    {
                        var destroyItem = curShowTips[curShowTips.Count - 1];
                        RemoveItem(destroyItem);
                        GameEntry.EntityManager.ReturnEntity(EntityEnum.PopupTips, destroyItem);
                    }
                    
                    time = Time.time;
                    SetNextPopupWaitTime();
                }
            }
        }
        
        private void RemoveItem(PopupTips item)
        {
            curShowTips.Remove(item);
        }
    }
}

