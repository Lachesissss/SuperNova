using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class UltimateSkillBar:MonoBehaviour
    {
        public RectTransform barImageTrans;
        public int signelWidth = 110;
        
        public void SetLevel(float level)
        {
            if(level>3) level = 3;
            Vector2 sizeDelta = barImageTrans.sizeDelta;
            sizeDelta.x = level*signelWidth;
            barImageTrans.sizeDelta = sizeDelta;
        }
    }
}

