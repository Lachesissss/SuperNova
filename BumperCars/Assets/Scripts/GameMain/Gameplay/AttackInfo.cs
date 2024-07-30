using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Lachesis.GamePlay
{
    public class AttackInfo
    {
        public string attacker;
        public string underAttacker;
        public DateTime attackTime;
        public AttackType attackType;
    }

    public enum AttackType
    {
        Collide,
        Skill1,
        Skill2
    }
    
}

