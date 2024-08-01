using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Lachesis.GamePlay
{
    public abstract class Skill
    {
        public string skillName;
        public bool isNeedTarget;
        public SkillEnum skillEnum;
        public abstract void Init(object userData = null);
        public abstract void Update(float elapseSeconds, float realElapseSeconds);
        public abstract void Activate(CarController source, CarController target, object userData = null);
    }
}

