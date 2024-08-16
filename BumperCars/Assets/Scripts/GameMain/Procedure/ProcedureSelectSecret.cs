using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class ProcedureSelectSecret : ProcedureBase
    {
        private bool isGoBattle;
        private bool isGoMenu;
        private object nextProcedureData;
        private ProcedureOwner procedureOwner;
        private SelectSecretUI m_selectUI;
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            GameEntry.EventManager.AddListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            this.procedureOwner = procedureOwner;
            Debug.Log("进入选择秘法流程");
            isGoBattle = false;
            isGoMenu = false;
            nextProcedureData = null;
            m_selectUI = GameEntry.EntityManager.CreateEntity<SelectSecretUI>(EntityEnum.SelectSecretUI, GameEntry.instance.canvasRoot.transform);

        }

        private void OnProcedureChange(object sender, GameEventArgs e)
        {
            var ne = (ProcedureChangeEventArgs)e;
            if (e is ProcedureChangeEventArgs args)
            {
                nextProcedureData = args.userData;
                var targProcedure = GameEntry.ProcedureManager.GetProcedure(args.TargetProcedureType);
                if (targProcedure is ProcedureBattle) isGoBattle = true;
                if (targProcedure is ProcedureMenu) isGoMenu = true;
            }
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (isGoBattle)
            {
                ChangeState<ProcedureBattle>(procedureOwner, nextProcedureData);
                return;
            } 
            if (isGoMenu)
            {
                ChangeState<ProcedureMenu>(procedureOwner, nextProcedureData);
                return;
            } 
        }

        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.SelectSecretUI, m_selectUI);
            m_selectUI = null;
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
            GameEntry.EventManager.RemoveListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
        }
    }
}

