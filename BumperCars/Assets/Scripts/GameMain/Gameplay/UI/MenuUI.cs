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
        public Button settingBtn;
        public Button settingCloseBtn;
        public TabItem ruleTab;
        public TabItem operationTab;
        public TabItem storyTab;
        public GameObject illustratePageGO;
        public GameObject settingPageGO;
        public Slider mainVolumeSld;
        public Slider audioEffectVolumeSld;
        public Slider musicVolumeSld;
        public Button p2JoySticksSwitchOnBtn;
        public Button p2JoySticksSwitchOffBtn;
        public GameObject p2JoySticksSwitchOnGO;
        public GameObject p2JoySticksSwitchOffGO;
        private AudioSource m_menuUIAudioSource;
        
        private GlobalConfig m_globalConfig;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            singleModeBtn.onClick.AddListener(OnEnterSingleMode);
            doubleModeBtn.onClick.AddListener(OnEnterDoubleMode);
            pveModeBtn.onClick.AddListener(OnEnterPveMode);
            quitGameBtn.onClick.AddListener(OnQuitGame);
            illustrateBtn.onClick.AddListener(OpenIllustratePage);
            illustrateReturnBtn.onClick.AddListener(CloseIllustratePage);
            settingBtn.onClick.AddListener(OpenSettingPage);
            settingCloseBtn.onClick.AddListener(CloseSettingPage);
            ruleTab.selfButton.onClick.AddListener(OnRuleTabClicked);
            operationTab.selfButton.onClick.AddListener(OnOperationTabClicked);
            storyTab.selfButton.onClick.AddListener(OnStoryTabClicked);
            mainVolumeSld.onValueChanged.AddListener(OnMainVolumeSldChanged);
            audioEffectVolumeSld.onValueChanged.AddListener(OnAudioEffectVolumeSldChanged);
            musicVolumeSld.onValueChanged.AddListener(OnMusicVolumeSldChanged);
            p2JoySticksSwitchOnBtn.onClick.AddListener(SwitchOnP2JoySticks);
            p2JoySticksSwitchOffBtn.onClick.AddListener(SwitchOffP2JoySticks);
            InitTab();
            InitVolumeSld();
            illustratePageGO.SetActive(false);
            settingPageGO.SetActive(false);
            var isP2Js = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2UsingJoySticks;
            p2JoySticksSwitchOffGO.SetActive(!isP2Js);
            p2JoySticksSwitchOnGO.SetActive(isP2Js);
            m_menuUIAudioSource = GameEntry.SoundManager.PlayerSound(this, SoundEnum.MenuBg, true, 1, false);
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
            settingBtn.onClick.RemoveAllListeners();
            settingCloseBtn.onClick.RemoveAllListeners();
            ruleTab.selfButton.onClick.RemoveAllListeners();
            operationTab.selfButton.onClick.RemoveAllListeners();
            storyTab.selfButton.onClick.RemoveAllListeners();
            mainVolumeSld.onValueChanged.RemoveAllListeners();
            audioEffectVolumeSld.onValueChanged.RemoveAllListeners();;
            musicVolumeSld.onValueChanged.RemoveAllListeners();;
            p2JoySticksSwitchOnBtn.onClick.RemoveAllListeners();
            p2JoySticksSwitchOffBtn.onClick.RemoveAllListeners();
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
        
        private void OpenSettingPage()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            settingPageGO.SetActive(true);
        }
        
        private void CloseSettingPage()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            settingPageGO.SetActive(false);
        }
        
        private void OnRuleTabClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.Tab);
            ruleTab.Enable();
            operationTab.Disable();
            storyTab.Disable();
        }

        private void OnMainVolumeSldChanged(float value)
        {
            m_globalConfig.mainVolume = value;
            GameEntry.EventManager.Invoke(this, VolumeChangeEventArgs.Create());
        }
        
        private void OnAudioEffectVolumeSldChanged(float value)
        {
            m_globalConfig.audioEffectVolume = value;
            GameEntry.EventManager.Invoke(this, VolumeChangeEventArgs.Create());
        }
        
        private void OnMusicVolumeSldChanged(float value)
        {
            m_globalConfig.musicVolume = value;
            GameEntry.EventManager.Invoke(this, VolumeChangeEventArgs.Create());
        }
        
        private void InitTab()
        {
            ruleTab.Enable();
            operationTab.Disable();
            storyTab.Disable();
        }
        
        private void InitVolumeSld()
        {
            mainVolumeSld.value = m_globalConfig.mainVolume;
            audioEffectVolumeSld.value = m_globalConfig.audioEffectVolume;
            musicVolumeSld.value = m_globalConfig.musicVolume;
        }
        
        private void OnBackToTittleBtnClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
        }
        
        private void SwitchOnP2JoySticks()
        {
            SwitchP2JoySticks(true);
        }
        
        private void SwitchOffP2JoySticks()
        {
            SwitchP2JoySticks(false);
        }
        
        private void SwitchP2JoySticks(bool isOn)
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.Tab);
            GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2UsingJoySticks = isOn;
            p2JoySticksSwitchOffGO.SetActive(!isOn);
            p2JoySticksSwitchOnGO.SetActive(isOn);
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

