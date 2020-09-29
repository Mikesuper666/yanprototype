using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class ReadOnlyAttribute : PropertyAttribute
{
    public readonly bool justInPlayMode;

    public ReadOnlyAttribute(bool justInPlayMode = true)
    {
        this.justInPlayMode = justInPlayMode;
    }
}
