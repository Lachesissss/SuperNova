using UnityEngine;

namespace Lachesis.GamePlay
{
    public class EntityComponent : MonoBehaviour
    {
        public Entity entity;
        public virtual void OnEntityInit(object userData = null)
        {
            
        }
        
        public virtual void OnEntityReCreateFromPool(object userData = null)
        {
        }
        

        public virtual void OnEntityReturnToPool(bool isShutDown = false)
        {
        }
        
        public virtual void OnEntityUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }
        
        public virtual void OnEntityFixedUpdate(float fixedElapseSeconds)
        {
        }
    }
}