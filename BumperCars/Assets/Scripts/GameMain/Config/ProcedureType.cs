using System;
using System.Linq;
using Lachesis.Core;
using UnityEditor;
using UnityEngine;

namespace Lachesis.GamePlay
{

    [System.Serializable]
    public class ProcedureType
    {
        [SerializeField]
        private string typeName;

        public System.Type GetPrecedureType()
        {
            return System.Type.GetType(typeName);
        }

        public void SetType(System.Type type)
        {
            typeName = type.AssemblyQualifiedName;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ProcedureType))]
    public class ProcedureTypeDrawer : PropertyDrawer
    {
        private static string[] typeNames;
        private static Type[] types;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeNameProperty = property.FindPropertyRelative("typeName");

            // 加载所有继承自Skill的技能类型
            if (types == null || typeNames == null)
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(ProcedureBase)))
                    .ToArray();
                typeNames = types.Select(t => t.FullName).ToArray();
            }

            // 获得当前Type
            Type currentType = Type.GetType(typeNameProperty.stringValue);
            int currentIndex = -1;

            // Find the current index
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == currentType)
                {
                    currentIndex = i;
                    break;
                }
            }

            // 绘制下拉列表
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);
            if (newIndex != currentIndex)
            {
                typeNameProperty.stringValue = types[newIndex].AssemblyQualifiedName;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}

