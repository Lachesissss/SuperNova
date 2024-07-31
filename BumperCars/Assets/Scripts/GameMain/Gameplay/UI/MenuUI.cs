using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class MenuUI : Entity
    {
        public Button singleModeBtn;
        public Button doubleModeBtn;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            singleModeBtn.onClick.AddListener(OnEnterSingleMode);
            doubleModeBtn.onClick.AddListener(OnEnterDoubleMode);
        }

        
        private void OnEnterSingleMode()
        {
            GameEntry.EventManager.Fire(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle), "Single"));
        }
        private void OnEnterDoubleMode()
        {
            GameEntry.EventManager.Fire(this, ProcedureChangeEventArgs.Create(typeof(ProcedureBattle), "Double"));
        }
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            singleModeBtn.onClick.RemoveAllListeners();
            doubleModeBtn.onClick.RemoveAllListeners();
        }
    }
}

