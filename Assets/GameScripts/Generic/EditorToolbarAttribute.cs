using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorToolbarAttribute : PropertyAttribute
{
    public readonly string title;
    public readonly string icon;
    public readonly bool useIcon;
    public readonly bool overrideChildOrder;
    public readonly bool overrideIcon;
    public EditorToolbarAttribute(string title, bool useIcon = false, string iconName = "", bool overrideIcon = false, bool overrideChildOrder = false)
    {
        this.title = title;
        this.icon = iconName;
        this.useIcon = useIcon;
        this.overrideChildOrder = overrideChildOrder;
        this.overrideIcon = overrideIcon;
    }
}
