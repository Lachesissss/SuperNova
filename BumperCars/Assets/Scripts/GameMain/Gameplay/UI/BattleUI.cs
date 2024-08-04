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
        public List<SkillSlot> player1SkillSlots;
        public List<SkillSlot> player2SkillSlots;
        public Transform popupTipsRootTrans;
        private string m_p1Name;
        private string m_p2Name;
        private int m_targetScore;
        private CarController m_controller1;
        private CarController m_controller2;
        private static BattleUI m_instance;
        private Queue<string> popupTextQueue;
        private List<PopupTips> curShowTips;
        
        public struct BattleUIData
        {
            public string p1Name;
            public string p2Name;
            public int targetScore;
            public CarController carController1;
            public CarController carController2;
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
            GameEntry.EventManager.AddListener(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
            GameEntry.EventManager.AddListener(PlayerBattleUIUpdateEventArgs.EventId, OnPlayerBattleUIUpdate);
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
            GameEntry.EventManager.RemoveListener(ScoreUpdateEventArgs.EventId, OnScoreUpdate);
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
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
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
            m_controller1 = battleUIData.carController1;
            m_controller2 = battleUIData.carController2;
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
            if (m_controller1 != null)
            {
                var len = GameEntry.ConfigManager.GetConfig<GlobalConfig>().maxSkillCount;
                for (int i=0;i<len;i++)
                {
                    var skill = m_controller1.skillSlots[i];
                    if(skill!=null)
                    {
                        player1SkillSlots[i].skillImage.sprite =
                            GameEntry.AtlasManager.GetSprite(AtlasEnum.Skill, GameEntry.SkillManager.GetSkillIconName(skill.skillEnum));
                        player1SkillSlots[i].skillGO.SetActive(true);
                    }
                    else
                    {
                        player1SkillSlots[i].skillGO.SetActive(false);
                    }
                    
                }
            }

            if (m_controller2 != null)
            {
                var len = GameEntry.ConfigManager.GetConfig<GlobalConfig>().maxSkillCount;
                for (var i = 0; i < len; i++)
                {
                    var skill = m_controller2.skillSlots[i];
                    if (skill != null)
                    {
                        player2SkillSlots[i].skillImage.sprite =
                            GameEntry.AtlasManager.GetSprite(AtlasEnum.Skill, GameEntry.SkillManager.GetSkillIconName(skill.skillEnum));
                        player2SkillSlots[i].skillGO.SetActive(true);
                    }
                    else
                    {
                        player2SkillSlots[i].skillGO.SetActive(false);
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

