using System;
using TMPro;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PopupTips : Entity
    {
        public TextMeshProUGUI tipsContentText;
        
        private RectTransform m_rectTransform;

        private Action<PopupTips> m_finishCallback;
        private float m_restLife = 0;
        private const float Life = 5f;
        
        protected void Finish()
        {
            if (m_finishCallback != null)
            {
                m_finishCallback(this);
                m_finishCallback = null;
            }
        }
        public RectTransform RectTransform
        {
            get
            {
                if (m_rectTransform == null)
                {
                    m_rectTransform = transform as RectTransform;
                }
                
                return m_rectTransform;
            }
        }
        
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            m_restLife = Life;
            m_finishCallback = null;
            RectTransform.anchoredPosition = Vector2.zero;
            if(userData is string content)
            {
                tipsContentText.text = content;
            }
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (m_restLife > 0)
            {
                m_restLife -= elapseSeconds;

                if (m_restLife <= 0)
                {
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.PopupTips, this);
                    Finish();
                }
            }
        }

        public void MoveUp(float y)
        {
            var pos = RectTransform.anchoredPosition;

            RectTransform.anchoredPosition = new Vector2(pos.x, pos.y + y);
        }
        
        public void BindFinish(Action<PopupTips> finishCallback)
        {
            m_finishCallback = finishCallback;
        }
    }
}

