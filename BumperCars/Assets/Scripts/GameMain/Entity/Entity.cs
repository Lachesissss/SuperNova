using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Entity : MonoBehaviour
    { 
        public virtual void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
        }
        
        public virtual void OnInit(object userData = null)
        {
            
        }
       
       public virtual void OnReturnToPool()
       {
           
       }
    }
}


