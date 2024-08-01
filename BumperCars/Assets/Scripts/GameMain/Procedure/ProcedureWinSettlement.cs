using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class SettlementData
    {
        public string winner;
    }
    
    public class ProcedureWinSettlement : ProcedureBase
    {
        private ProcedureOwner procedureOwner;
        private bool isGoMenu;
        private WinSettlementUI m_settlementUI;
        private SettlementData m_settlementData;
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }
        
        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            if(userData is SettlementData data)
            {
                m_settlementData = data;
            }
            this.procedureOwner = procedureOwner;
            isGoMenu = false;
            GameEntry.EventManager.Subscribe(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            m_settlementUI = GameEntry.EntityManager.CreateEntity<WinSettlementUI>(EntityEnum.WinSettlementUI, GameEntry.instance.canvasRoot.transform, m_settlementData);
        }
        
        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if(isGoMenu)
            {
                ChangeState<ProcedureMenu>(procedureOwner);
                return;
            }
        }
        
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.WinSettlementUI, m_settlementUI);
        }
        
        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }
        
        private void OnProcedureChange(object sender, GameEventArgs e)
        {
            var ne = (ProcedureChangeEventArgs)e;
            if (e is ProcedureChangeEventArgs args)
            {
                var targetProcedure = GameEntry.ProcedureManager.GetProcedure(args.TargetProcedureType);
                if (targetProcedure is ProcedureMenu) isGoMenu = true;
            }
        }
    }
}

