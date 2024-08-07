using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class SettlementPveData
    {
        public bool isWin;
    }
    public class WinSettlementPveUI : Entity
    {
        public Text winOrLoseText;
        public Button goBackToTittleBtn;
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            if(userData is SettlementPveData data)
            {
                winOrLoseText.text = data.isWin?"胜利":"失败" ;
            }
            goBackToTittleBtn.onClick.AddListener(OnGoBackToTitleBtnClicked);
        }

        private void OnGoBackToTitleBtnClicked()
        {
            GameEntry.EventManager.Invoke(this, ProcedureChangeEventArgs.Create(typeof(ProcedureMenu)));
            goBackToTittleBtn.onClick.RemoveAllListeners();
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            goBackToTittleBtn.onClick.RemoveAllListeners();
        }
    }
}