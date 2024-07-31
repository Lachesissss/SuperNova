using UnityEngine;

namespace Lachesis.GamePlay
{
    public class EntityComponent : MonoBehaviour
    {
        public virtual void OnEntityInit(object userData = null)
        {
        }

        public virtual void OnEntityReturnToPool()
        {
        }
    }
}