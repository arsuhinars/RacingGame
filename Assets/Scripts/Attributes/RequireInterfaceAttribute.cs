using System;
using UnityEngine;

public class RequireInterfaceAttribute : PropertyAttribute
{
    public Type RequiredType { get; private set; }

    public RequireInterfaceAttribute(Type type)
    {
        RequiredType = type;
    }
}
