using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class ProcedureMenu : ProcedureBase
    {
        private bool isGoBattle;
        private ProcedureOwner procedureOwner;

        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            this.procedureOwner = procedureOwner;
            isGoBattle = false;
            Debug.Log("进入主菜单流程");
            GameEntry.EventManager.Subscribe(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.Fire(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle)));
            if (isGoBattle) ChangeState<ProcedureBattle>(procedureOwner);

            // GameEntry.Event.Subscribe(ChangeSceneEventArgs.EventId, OnChangeScene);
            // GameEntry.Event.Subscribe(LoadLevelEventArgs.EventId, OnLoadLevel);
            //
            // GameEntry.UI.OpenUIForm(EnumUIForm.UIMainMenuForm);
            // GameEntry.UI.OpenDownloadForm();
            // GameEntry.Sound.PlayMusic(EnumSound.MenuBGM);
        }

        private void OnProcedureChange(object sender, GameEventArgs e)
        {
            var ne = (ProcedureChangeEventArgs)e;
            if (e is ProcedureChangeEventArgs args)
            {
                var targProcedure = GameEntry.ProcedureManager.GetProcedure(args.TargetProcedureType);
                if (targProcedure is ProcedureBattle) isGoBattle = true;
            }
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (isGoBattle) ChangeState<ProcedureBattle>(procedureOwner);
        }

        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            // GameEntry.Sound.StopMusic();
            //
            // GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            // GameEntry.Event.Unsubscribe(ChangeSceneEventArgs.EventId, OnChangeScene);
            // GameEntry.Event.Unsubscribe(LoadLevelEventArgs.EventId, OnLoadLevel);
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        // private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        // {
        //     OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
        //     if (ne.UserData != this)
        //     {
        //         return;
        //     }
        // }
        //
        // private void OnChangeScene(object sender, GameEventArgs e)
        // {
        //     ChangeSceneEventArgs ne = (ChangeSceneEventArgs)e;
        //     if (ne == null)
        //         return;
        //
        //     changeScene = true;
        //     procedureOwner.SetData<VarInt32>(Constant.ProcedureData.NextSceneId, ne.SceneId);
        // }
        //
        // private void OnLoadLevel(object sender, GameEventArgs e)
        // {
        //     LoadLevelEventArgs ne = (LoadLevelEventArgs)e;
        //     if (ne == null)
        //         return;
        //
        //     if (ne.LevelData == null)
        //     {
        //         Log.Error("Load level event param LevelData is null");
        //         return;
        //     }
        //
        //     if (ne.LevelData.SceneData == null)
        //     {
        //         Log.Error("Load level event param SceneData is null");
        //         return;
        //     }
        //
        //     changeScene = true;
        //     procedureOwner.SetData<VarInt32>(Constant.ProcedureData.NextSceneId, ne.LevelData.SceneData.Id);
        // }
    }
}