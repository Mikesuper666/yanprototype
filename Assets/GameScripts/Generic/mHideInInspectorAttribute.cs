using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class mHideInInspectorAttribute : PropertyAttribute
{
    public string refbooleanProperty;
    public bool invertValue;
    public mHideInInspectorAttribute(string refbooleanProperty, bool invertValue = false)
    {
        this.refbooleanProperty = refbooleanProperty;
        this.invertValue = invertValue;
    }


}
