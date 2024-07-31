using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Entity : MonoBehaviour
    {
        private List<EntityComponent> entityComponents;

        private void Awake()
        {
            GetEntityComponents();
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
        
        public virtual void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
            foreach (var component in entityComponents) component.OnEntityInit(userData);
        }
        
        public virtual void OnInit(object userData = null)
        {
            foreach (var component in entityComponents) component.OnEntityInit(userData);
        }
       
       public virtual void OnReturnToPool()
       {
           foreach (var component in entityComponents) component.OnEntityReturnToPool();
       }
    }
}


