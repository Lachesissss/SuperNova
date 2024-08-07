using System.Collections.Generic;
using System.Reflection;
using Lachesis.Core;
using Mirror;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class NetworkEntity : NetworkBehaviour
    {
        private List<NetworkEntityComponent> entityComponents;
        public ProcedureBase BelongProcedure { get; private set; }
        [HideInInspector] public NetworkEntityEnum entityEnum;

        public bool IsValid { get; private set; }

        public virtual void OnInit(object userData = null)
        {
            GetEntityComponents();
            foreach (var component in entityComponents) component.OnEntityInit(userData);
        }

        //通过反射获取所有的实体组件，包括组件和组件的迭代器
        private void GetEntityComponents()
        {
            entityComponents = new List<NetworkEntityComponent>();
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
                if (typeof(NetworkEntityComponent).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(this) as NetworkEntityComponent;
                    if (value != null) entityComponents.Add(value);
                }
                else if (typeof(IEnumerable<NetworkEntityComponent>).IsAssignableFrom(field.FieldType))
                {
                    var collection = (IEnumerable<NetworkEntityComponent>)field.GetValue(this);
                    if (collection != null) entityComponents.AddRange(collection);
                }
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var component in entityComponents) component.OnEntityUpdate(elapseSeconds, realElapseSeconds);
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
            IsValid = true;
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }

        public virtual void OnReCreateFromPool(object userData = null)
        {
            IsValid = true;
            BelongProcedure = GameEntry.ProcedureManager.CurrentProcedure;
            foreach (var component in entityComponents) component.OnEntityReCreateFromPool(userData);
        }

        public virtual void OnReturnToPool(bool isShutDown = false)
        {
            IsValid = false;
            foreach (var component in entityComponents) component.OnEntityReturnToPool(isShutDown);
        }
    }
}