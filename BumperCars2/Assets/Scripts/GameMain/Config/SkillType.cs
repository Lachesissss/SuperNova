using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lachesis.GamePlay
{
    [Serializable]
    public class SkillType
    {
        [SerializeField]
        private string typeName;

        public Type GetSkillType()
        {
            return Type.GetType(typeName);
        }

        public void SetType(Type type)
        {
            typeName = type.AssemblyQualifiedName;
        }
    }
    
    [CustomPropertyDrawer(typeof(SkillType))]
    public class SkillTypeDrawer : PropertyDrawer
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
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(Skill)))
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

    public class NetworkSkillType
    {
        [SerializeField] private string typeName;

        public Type GetSkillType()
        {
            return Type.GetType(typeName);
        }

        public void SetType(Type type)
        {
            typeName = type.AssemblyQualifiedName;
        }
    }

    [CustomPropertyDrawer(typeof(NetworkSkillType))]
    public class NetworkSkillTypeDrawer : PropertyDrawer
    {
        private static string[] typeNames;
        private static Type[] types;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeNameProperty = property.FindPropertyRelative("typeName");

            // 加载所有继承自Skill的技能类型
            if (types == null || typeNames == null)
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(NetworkSkill)))
                    .ToArray();
                typeNames = types.Select(t => t.FullName).ToArray();
            }

            // 获得当前Type
            var currentType = Type.GetType(typeNameProperty.stringValue);
            var currentIndex = -1;

            // Find the current index
            for (var i = 0; i < types.Length; i++)
                if (types[i] == currentType)
                {
                    currentIndex = i;
                    break;
                }

            // 绘制下拉列表
            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);
            if (newIndex != currentIndex) typeNameProperty.stringValue = types[newIndex].AssemblyQualifiedName;

            EditorGUI.EndProperty();
        }
    }
}

