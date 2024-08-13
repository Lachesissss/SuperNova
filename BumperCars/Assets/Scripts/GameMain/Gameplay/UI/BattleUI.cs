using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class CoolingInfo
    {
        public string playerName;
        public bool isBoostCoolingInfoChanged = false;
        public bool isSwitchCoolingInfoChanged = false;
        public float BoostCoolingTime;
        public float SwitchCoolingTime;
    }
    public class BattleUI : Entity
    {
        public TextMeshProUGUI player1ScoreText;
        public TextMeshProUGUI player2ScoreText;
        public TextMeshProUGUI player1CarriedScoreText;
        public TextMeshProUGUI player2CarriedScoreText;
        public TextMeshProUGUI targetScoreText;
        public Button settingBtn;
        public Button closeBtn;
        public Button continueBtn;
        public Button backToTittleBtn;
        public Button p2JoySticksSwitchOnBtn;
        public Button p2JoySticksSwitchOffBtn;
        public GameObject p2JoySticksSwitchOnGO;
        public GameObject p2JoySticksSwitchOffGO;
        public GameObject p2JoySticksTextGO;
        public GameObject p2KeyBoardTextGO;
        public Image p1BoostCoolingImg;
        public Image p2BoostCoolingImg;
        public Image p1SwitchCoolingImg;
        public Image p2SwitchCoolingImg;
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
            var isP2Js = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2UsingJoySticks;
            p2JoySticksSwitchOffGO.SetActive(!isP2Js);
            p2JoySticksSwitchOnGO.SetActive(isP2Js);
            p2KeyBoardTextGO.SetActive(!isP2Js);
            p2JoySticksTextGO.SetActive(isP2Js);
        }
        
        private void InitCoolingImg(Image img)
        {
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillAmount = 0;
        }
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            ResetBattleUI(userData);
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            ResetBattleUI(userData);
        }

        private void ResetBattleUI(object userData)
        {
            m_instance = this;
            InitCoolingImg(p1BoostCoolingImg);
            InitCoolingImg(p2BoostCoolingImg);
            InitCoolingImg(p1SwitchCoolingImg);
            InitCoolingImg(p2SwitchCoolingImg);
            GameEntry.EventManager.AddListener(ScoreUIUpdateEventArgs.EventId, OnScoreUpdate);
            GameEntry.EventManager.AddListener(CarriedScoreUIUpdateEventArgs.EventId, OnCarriedScoreUpdate);
            GameEntry.EventManager.AddListener(PlayerSkillSlotsUIUpdateEventArgs.EventId, OnPlayerBattleUIUpdate);
            GameEntry.EventManager.AddListener(PlayerCoolingUIUpdateEventArgs.EventId, OnPlayerCoolingUIUpdate);
            GameEntry.EventManager.AddListener(ShowUITipsEventArgs.EventId, OnUITipsShow);
            settingBtn.onClick.AddListener(OnSettingBtnClicked);
            continueBtn.onClick.AddListener(OnContinueBtnClicked);
            closeBtn.onClick.AddListener(OnContinueBtnClicked);
            backToTittleBtn.onClick.AddListener(OnBackToTittleBtnClicked);
            p2JoySticksSwitchOnBtn.onClick.AddListener(SwitchOnP2JoySticks);
            p2JoySticksSwitchOffBtn.onClick.AddListener(SwitchOffP2JoySticks);
            if(userData is BattleUIData battleUIData)
            {
                RefreshAll(battleUIData);
            }
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
            GameEntry.EventManager.RemoveListener(ScoreUIUpdateEventArgs.EventId, OnScoreUpdate);
            GameEntry.EventManager.RemoveListener(CarriedScoreUIUpdateEventArgs.EventId, OnCarriedScoreUpdate);
            GameEntry.EventManager.RemoveListener(PlayerSkillSlotsUIUpdateEventArgs.EventId, OnPlayerBattleUIUpdate);
            GameEntry.EventManager.RemoveListener(PlayerCoolingUIUpdateEventArgs.EventId, OnPlayerCoolingUIUpdate);
            GameEntry.EventManager.RemoveListener(ShowUITipsEventArgs.EventId, OnUITipsShow);
            settingBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.RemoveAllListeners();
            backToTittleBtn.onClick.RemoveAllListeners();
            p2JoySticksSwitchOnBtn.onClick.RemoveAllListeners();
            p2JoySticksSwitchOffBtn.onClick.RemoveAllListeners();
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
        
        private void SwitchOnP2JoySticks()
        {
            GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2UsingJoySticks = true;
            p2JoySticksSwitchOffGO.SetActive(false);
            p2JoySticksSwitchOnGO.SetActive(true);
            p2KeyBoardTextGO.SetActive(false);
            p2JoySticksTextGO.SetActive(true);
        }
        
        private void SwitchOffP2JoySticks()
        {
            GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2UsingJoySticks = false;
            p2JoySticksSwitchOffGO.SetActive(true);
            p2JoySticksSwitchOnGO.SetActive(false);
            p2KeyBoardTextGO.SetActive(true);
            p2JoySticksTextGO.SetActive(false);
        }
        
        private void RefreshScore(int p1Score, int p2Score)
        {
            player1ScoreText.text = $"{p1Score}";
            player2ScoreText.text = $"{p2Score}";
            targetScoreText.text = $"目标:{m_targetScore}";
        }

        private void RefreshCarriedScore(int p1CarriedScore, int p2CarriedScore)
        {
            player1CarriedScoreText.text = $"{p1CarriedScore}";
            player2CarriedScoreText.text = $"{p2CarriedScore}";
        }

        public void RefreshAll(BattleUIData battleUIData)
        {
            m_p1Name = battleUIData.p1Name;
            m_p2Name = battleUIData.p2Name;
            
            m_controller1 = battleUIData.carController1;
            m_controller2 = battleUIData.carController2;
            m_targetScore = battleUIData.targetScore;
            RefreshScore(0,0);
            RefreshCarriedScore(0, 0);
            RefreshSkillSlotsUI();
        }

        private void OnScoreUpdate(object sender, GameEventArgs e)
        {
            if (e is ScoreUIUpdateEventArgs scoreUpdateEventArgs)
            {
                RefreshScore(scoreUpdateEventArgs.p1NewScore, scoreUpdateEventArgs.p2NewScore);
            }
        }

        private void OnCarriedScoreUpdate(object sender, GameEventArgs e)
        {
            if (e is CarriedScoreUIUpdateEventArgs args) RefreshCarriedScore(args.p1NewCarriedScore, args.p2NewCarriedScore);
        }
        private void OnPlayerBattleUIUpdate(object sender, GameEventArgs e)
        {
            if(e is PlayerSkillSlotsUIUpdateEventArgs args)
            {
                RefreshSkillSlotsUI();
            }
        }
        
        private void OnPlayerCoolingUIUpdate(object sender, GameEventArgs e)
        {
            if(e is PlayerCoolingUIUpdateEventArgs args)
            {
                RefreshCooling(args.coolingInfo);
            }
        }
        
        private void OnUITipsShow(object sender, GameEventArgs e)
        {
            if(e is ShowUITipsEventArgs args)
            {
                ShowPopupTips(args.content);
            }
        }
        
        public void RefreshSkillSlotsUI()
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
        
        private void RefreshCooling(CoolingInfo info)
        {
            if(info.playerName == m_p1Name)
            {
                if(info.isSwitchCoolingInfoChanged)
                {
                    if(p1SwitchCoroutine!=null) StopCoroutine(p1SwitchCoroutine);
                    p1SwitchCoroutine = StartCoroutine(StartCooling(p1SwitchCoolingImg, info.SwitchCoolingTime));
                }
                if(info.isBoostCoolingInfoChanged)
                {
                    if(p1BoostCoroutine!=null) StopCoroutine(p1BoostCoroutine);
                    p1BoostCoroutine = StartCoroutine(StartCooling(p1BoostCoolingImg, info.BoostCoolingTime));
                }
            }
            else if(info.playerName == m_p2Name)
            {
                if(info.isSwitchCoolingInfoChanged)
                {
                    if(p2SwitchCoroutine!=null) StopCoroutine(p2SwitchCoroutine);
                    p2SwitchCoroutine = StartCoroutine(StartCooling(p2SwitchCoolingImg, info.SwitchCoolingTime));
                }
                if(info.isBoostCoolingInfoChanged)
                {
                    if(p2BoostCoroutine!=null) StopCoroutine(p2BoostCoroutine);
                    p2BoostCoroutine = StartCoroutine(StartCooling(p2BoostCoolingImg, info.BoostCoolingTime));
                }
            }
            
        }
        
        private static void ShowPopupTips(string content)
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
        
        private Coroutine p1BoostCoroutine;
        private Coroutine p2BoostCoroutine;
        private Coroutine p1SwitchCoroutine;
        private Coroutine p2SwitchCoroutine;
        
        private IEnumerator StartCooling(Image img, float coolingTime)
        {
            var curTime = coolingTime;
            float startTime = Time.time;
            float endTime = Time.time;
            img.fillAmount = 1;
            while (curTime>0)
            {
                curTime-=(endTime - startTime);
                startTime = Time.time;
                yield return new WaitForEndOfFrame();
                endTime = Time.time;
                img.fillAmount = curTime/coolingTime;
            }
            img.fillAmount = 0;
        }
    }
}

