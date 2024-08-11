using System;

namespace Lachesis.GamePlay
{
    public class AttackInfo
    {
        public string attacker;
        public string underAttacker;
        public DateTime attackTime;
        public AttackType attackType;
        public object userData;
        public int attackDamge;
        public bool canDodge = true;
    }

    public enum AttackType
    {
        Collide,
        Skill,
    }
    
}

