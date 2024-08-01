using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Entity : MonoBehaviour
    {
        private List<EntityComponent> entityComponents;

        public virtual void OnInit(object userData = null)
        {
            GetEntityComponents();
            foreach (var component in entityComponents) component.OnEntityInit(userData);
        }

        private void GetEntityComponents()
        {
            entityComponents = new List<EntityComponent>();
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
                if (field.FieldType.IsSubclassOf(typeof(EntityComponent)) || field.FieldType == typeof(EntityComponent))
                {
                    var value = field.GetValue(this) as EntityComponent;
                    if (value != null) entityComponents.Add(value);
                }
        }
        
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var component in entityComponents) component.OnEntityUpdate(elapseSeconds,realElapseSeconds);
        }
        
        public virtual void OnFixedUpdate(float fixedElapseSeconds)
        {
            foreach (var component in entityComponents) component.OnEntityFixedUpdate(fixedElapseSeconds);
        }
        
        public virtual void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }
        
        public virtual void OnReCreateFromPool(object userData = null)
        {
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }
       
        //如果是ShutDown时触发的回收，应避免gameobject相关操作，gameobject可能在实体被回收前被Destory
       public virtual void OnReturnToPool(bool isShutDown = false)
       {
           foreach (var component in entityComponents) component.OnEntityReturnToPool(isShutDown);
       }
    }
}


