using UnityEngine;

namespace Lachesis.GamePlay
{
    public class BattleField : Entity
    {
        public CoinProducer coinProducer;
        public PlayerHome player1Home;
        public PlayerHome player2Home;
        public Transform spawnTrans1; 
        public Transform spawnTrans2; 
        public Transform spawnTrans3; 
        public Transform spawnTrans4; 
        public Transform spawnTrans5; 
        public Transform spawnTrans6; 
        public Transform dieOutTrans;
        public Transform fieldCenter;
        public Transform edge;
        public float Radius=>Vector2.Distance(new Vector2(fieldCenter.position.x, fieldCenter.position.z), new Vector2(edge.position.x, edge.position.z));
    }
}

