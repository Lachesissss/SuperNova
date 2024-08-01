using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SkillItem : Entity
    {
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
        }
    }
}

