using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class SelectSecretUI : Entity
    {
        public SecretPanel p1Panel;
        public SecretPanel p2Panel;
        public Button enterBattleBtn;
        public Button returnBtn;
        
        private List<SkillEnum> m_ultimateList;
        private int curP1Index;
        private int curP2Index;
        private AudioSource m_selectUIAudioSource;
        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_ultimateList = GameEntry.SkillManager.GetAllUltimateSkill();
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            p1Panel.nextBtn.onClick.AddListener(OnP1Next);
            p1Panel.prevBtn.onClick.AddListener(OnP1Prev);
            p2Panel.nextBtn.onClick.AddListener(OnP2Next);
            p2Panel.prevBtn.onClick.AddListener(OnP2Prev);
            enterBattleBtn.onClick.AddListener(OnEnterDoubleGame);
            returnBtn.onClick.AddListener(OnReturnMenu);
            curP1Index = 0;
            curP2Index = 0;
            m_selectUIAudioSource = GameEntry.SoundManager.PlayerSound(this, SoundEnum.SelectBg, true, 1, false);
            Refresh(false);
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            p1Panel.nextBtn.onClick.RemoveAllListeners();
            p1Panel.prevBtn.onClick.RemoveAllListeners();
            p2Panel.nextBtn.onClick.RemoveAllListeners();
            p2Panel.prevBtn.onClick.RemoveAllListeners();
            enterBattleBtn.onClick.RemoveAllListeners();
            returnBtn.onClick.RemoveAllListeners();
        }
        
        private void OnEnterDoubleGame()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            BattleModel.Instance.currentDungeonMode = DungeonMode.Double;
            BattleModel.Instance.p1Ultimate = m_ultimateList[curP1Index];
            BattleModel.Instance.p2Ultimate = m_ultimateList[curP2Index];
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle)));
        }

        private void OnReturnMenu()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
        }
        
        private void OnP1Next()
        {
            curP1Index++;
            if(curP1Index>=m_ultimateList.Count) curP1Index = 0;
            Refresh();
        }

        private void OnP1Prev()
        {
            curP1Index--;
            if(curP1Index<0) curP1Index = m_ultimateList.Count-1;
            Refresh();
        }

        private void OnP2Next()
        {
            curP2Index++;
            if(curP2Index>=m_ultimateList.Count) curP2Index = 0;
            Refresh();
        }

        private void OnP2Prev()
        {
            curP2Index--;
            if(curP2Index<0) curP2Index = m_ultimateList.Count-1;
            Refresh();
        }

        private void Refresh(bool playSound = true)
        {
            if (playSound)
                GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            p1Panel.SetData(m_ultimateList[curP1Index]);
            p2Panel.SetData(m_ultimateList[curP2Index]);
        }
        
    }
}

