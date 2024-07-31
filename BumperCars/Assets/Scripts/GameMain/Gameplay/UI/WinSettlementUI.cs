using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class WinSettlementUI : Entity
    {
        public Text winnerText;
        public Button goBackToTittleBtn;
        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            if(userData is SettlementData data)
            {
                winnerText.text = $"胜利者：{data.winner}！！" ;
            }
            goBackToTittleBtn.onClick.AddListener(OnGoBackToTitleBtnClicked);
        }

        private void OnGoBackToTitleBtnClicked()
        {
            GameEntry.EventManager.Fire(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            goBackToTittleBtn.onClick.RemoveAllListeners();
        }
    }
}

