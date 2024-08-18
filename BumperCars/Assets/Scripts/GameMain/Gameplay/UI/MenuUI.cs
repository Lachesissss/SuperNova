using Lachesis.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class MenuUI : Entity
    {
        public Button singleModeBtn;
        public Button doubleModeBtn;
        public Button pveModeBtn;
        public Button quitGameBtn;
        public Button illustrateBtn;
        public Button illustrateReturnBtn;
        public TabItem ruleTab;
        public TabItem operationTab;
        public TabItem storyTab;
        public GameObject illustratePageGO;
        public AudioSource m_menuUIAudioSource;
        

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            singleModeBtn.onClick.AddListener(OnEnterSingleMode);
            doubleModeBtn.onClick.AddListener(OnEnterDoubleMode);
            pveModeBtn.onClick.AddListener(OnEnterPveMode);
            quitGameBtn.onClick.AddListener(OnQuitGame);
            illustrateBtn.onClick.AddListener(OpenIllustratePage);
            illustrateReturnBtn.onClick.AddListener(CloseIllustratePage);
            ruleTab.selfButton.onClick.AddListener(OnRuleTabClicked);
            operationTab.selfButton.onClick.AddListener(OnOperationTabClicked);
            storyTab.selfButton.onClick.AddListener(OnStoryTabClicked);
            InitTab();
            illustratePageGO.SetActive(false);
            m_menuUIAudioSource = GameEntry.SoundManager.PlayerSound(this, SoundEnum.MenuBg, true, 1, false, 0.1f);
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            singleModeBtn.onClick.RemoveAllListeners();
            doubleModeBtn.onClick.RemoveAllListeners();
            pveModeBtn.onClick.RemoveAllListeners();
            quitGameBtn.onClick.RemoveAllListeners();
            illustrateBtn.onClick.RemoveAllListeners();
            illustrateReturnBtn.onClick.RemoveAllListeners();
            ruleTab.selfButton.onClick.RemoveAllListeners();
            operationTab.selfButton.onClick.RemoveAllListeners();
            storyTab.selfButton.onClick.RemoveAllListeners();
        }
        
        private void OnEnterSingleMode()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            BattleModel.Instance.currentDungeonMode = DungeonMode.Single;
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle)));
        }
        private void OnEnterDoubleMode()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            BattleModel.Instance.currentDungeonMode = DungeonMode.Double;
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureSelectSecret)));
        }
        
        private void OnEnterPveMode()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattlePve)));
        }
        
        private void OnQuitGame()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
#if UNITY_EDITOR
            // 在编辑器中停止播放模式
            EditorApplication.isPlaying = false;
#else
            // 在构建后的应用程序中退出
            Application.Quit();
#endif
        }
        
        private void OpenIllustratePage()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            illustratePageGO.SetActive(true);
        }
        
        private void CloseIllustratePage()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            illustratePageGO.SetActive(false);
        }
        
        private void OnRuleTabClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.Tab);
            ruleTab.Enable();
            operationTab.Disable();
            storyTab.Disable();
        }

        private void InitTab()
        {
            ruleTab.Enable();
            operationTab.Disable();
            storyTab.Disable();
        }
        
        private void OnOperationTabClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.Tab);
            operationTab.Enable();
            ruleTab.Disable();
            storyTab.Disable();
        }
        private void OnStoryTabClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.Tab);
            storyTab.Enable();
            operationTab.Disable();
            ruleTab.Disable();
        }
    }
    
    public enum DungeonMode
    {
        Single,
        Double,
    }
}

