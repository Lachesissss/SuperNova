using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Lachesis.GamePlay
{
    public class SecretPanel : MonoBehaviour
    {
        public Image secretImage;
        public TextMeshProUGUI secretName;
        public TextMeshProUGUI secretDescribe;
        public TextMeshProUGUI minSecretDescribe;
        public Button nextBtn;
        public Button prevBtn;
        
        public void SetData(SkillEnum skillEnum)
        {
            var skillCfg = GameEntry.SkillManager.GetSkillConfigItem(skillEnum);
            secretImage.sprite = GameEntry.AtlasManager.GetSprite(AtlasEnum.Skill, skillCfg.iconName);
            secretName.text = skillCfg.skillName;
            secretDescribe.text = skillCfg.describe;
            minSecretDescribe.text = skillCfg.minDescribe;
        }
    }
}

