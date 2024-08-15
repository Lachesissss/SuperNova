using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Lachesis.GamePlay
{
    public class TabItem : MonoBehaviour
    {
        public Button selfButton;
        public static string enableColor = "F6E19C";
        public static string disableColor = "BEB5B6";
        public TextMeshProUGUI tabText;
        public List<GameObject> activeObjects;
    
        public void Enable()
        {
            tabText.color = GetColorByHexString(enableColor);
            foreach (var go in activeObjects)
            {
                go.SetActive(true);
            }
        }
    
        public void Disable()
        {
            tabText.color = GetColorByHexString(disableColor);
            foreach (var go in activeObjects)
            {
                go.SetActive(false);
            }
        }
    
        private Color GetColorByHexString(string str)
        {
            if (ColorUtility.TryParseHtmlString("#" + str, out Color newColor))
            {
                // 修改材质的颜色
                return newColor;
            }
            else
            {
                return Color.white;
            }
        }
    }
}

