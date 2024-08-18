using Lachesis.Core;
using TMPro;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class WinSettlementUI : Entity
    {
        public TextMeshProUGUI winnerText;
        public Button goBackToTittleBtn;
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            if(userData is SettlementData data)
            {
                winnerText.text = $"胜利者: \n{data.winner}";
                GameEntry.SoundManager.PlayerSound(this, SoundEnum.Win);
            }
            goBackToTittleBtn.onClick.AddListener(OnGoBackToTitleBtnClicked);
        }

        private void OnGoBackToTitleBtnClicked()
        {
            GameEntry.SoundManager.PlayerSound(this, SoundEnum.ButtonPress);
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

