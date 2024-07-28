using System;
using UnityEngine;

public class TypeNamePair
{
    public TypeNamePair(Type type, string name = null)
    {
        if (type == null)
        {
            Debug.LogError("状态机类型不能为空");
            return;
        }

        Type = type;
        Name = name ?? string.Empty;
    }

    public Type Type { get; }

    public string Name { get; }
}