using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class BattleField : Entity
    {
        public Transform spawnTrans1; 
        public Transform spawnTrans2; 
        public Transform dieOutTrans;
        public Transform fieldCenter;
        public Transform edge;
        public float Radius=>Vector2.Distance(new Vector2(fieldCenter.position.x, fieldCenter.position.z), new Vector2(edge.position.x, edge.position.z));
    }
}

