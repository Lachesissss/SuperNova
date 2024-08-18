using System.Collections.Generic;
using System.Reflection;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Entity : MonoBehaviour
    {
        private List<EntityComponent> entityComponents;
        public ProcedureBase BelongProcedure { get; private set; }
        [HideInInspector]
        public EntityEnum entityEnum;
        private bool m_isValid;
        public bool IsValid =>m_isValid;
        public virtual void OnInit(object userData = null)
        {
            GetEntityComponents();
            foreach (var component in entityComponents)
            {
                component.entity = this;
                component.OnEntityInit(userData);
            }

            
        }

        //通过反射获取所有的实体组件，包括组件和组件的迭代器
        private void GetEntityComponents()
        {
            entityComponents = new List<EntityComponent>();
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
                if (typeof(EntityComponent).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(this) as EntityComponent;
                    if (value != null) entityComponents.Add(value);
                }
                else if (typeof(IEnumerable<EntityComponent>).IsAssignableFrom(field.FieldType))
                {
                    IEnumerable<EntityComponent> collection = (IEnumerable<EntityComponent>)field.GetValue(this);
                    if (collection != null)
                    {
                        entityComponents.AddRange(collection);
                    }
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
            BelongProcedure = GameEntry.ProcedureManager.CurrentProcedure;
            m_isValid = true;
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }
        
        public virtual void OnReCreateFromPool(object userData = null)
        {
            m_isValid = true;
            BelongProcedure = GameEntry.ProcedureManager.CurrentProcedure;
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }
       
        //如果是ShutDown时触发的回收，应避免gameobject相关操作，gameobject可能在实体被回收前被Destory
       public virtual void OnReturnToPool(bool isShutDown = false)
       {
           m_isValid = false;
           foreach (var component in entityComponents) component.OnEntityReturnToPool(isShutDown);
       }
    }
}


