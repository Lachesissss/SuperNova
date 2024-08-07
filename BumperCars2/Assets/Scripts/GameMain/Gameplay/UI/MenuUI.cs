using Lachesis.Core;
using UnityEditor;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class MenuUI : Entity
    {
        public Button singleModeBtn;
        public Button doubleModeBtn;
        public Button pveModeBtn;
        public Button quitGameBtn;
        public Button createRoomBtn;
        public Button joinRoomBtn;

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            singleModeBtn.onClick.AddListener(OnEnterSingleMode);
            doubleModeBtn.onClick.AddListener(OnEnterDoubleMode);
            pveModeBtn.onClick.AddListener(OnEnterPveMode);
            quitGameBtn.onClick.AddListener(OnQuitGame);
            createRoomBtn.onClick.AddListener(OnCreateMirrorRoom);
            joinRoomBtn.onClick.AddListener(OnJoinMirrorRoom);
            
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            singleModeBtn.onClick.RemoveAllListeners();
            doubleModeBtn.onClick.RemoveAllListeners();
            pveModeBtn.onClick.RemoveAllListeners();
            quitGameBtn.onClick.RemoveAllListeners();
            createRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.RemoveAllListeners();
        }
        
        private void OnEnterSingleMode()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle), DungeonMode.Single));
        }
        private void OnEnterDoubleMode()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle), DungeonMode.Double));
        }
        
        private void OnEnterPveMode()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattlePve)));
        }

        private void OnCreateMirrorRoom()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattleNetwork), true));
        }

        private void OnJoinMirrorRoom()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattleNetwork), false));
        }
        
        private void OnQuitGame()
        {
#if UNITY_EDITOR
            // 在编辑器中停止播放模式
            EditorApplication.isPlaying = false;
#else
            // 在构建后的应用程序中退出
            Application.Quit();
#endif
        }

    }
    
    public enum DungeonMode
    {
        Single,
        Double,
    }
}

