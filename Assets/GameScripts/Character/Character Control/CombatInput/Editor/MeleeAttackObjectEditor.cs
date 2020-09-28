using UnityEngine;
using UnityEditor;
using System;
using static MeleeWeapon;

[CanEditMultipleObjects]
[CustomEditor(typeof(MeleeAttackObject), true)]
public class MeleeAttackObjectEditor : Editor
{
    MeleeAttackObject attackObject;
    GUISkin skin;
    bool fodoutEvents;
    bool showDefenseRange;
    private Texture2D m_Logo = null;

    void OnSceneGUI()
    {
        if (!attackObject) return;

        var meleeWeapon = (attackObject as MeleeWeapon);
        if(meleeWeapon != null && showDefenseRange)
        {
            if (meleeWeapon.meleeType != MeleeType.OnlyAttack)
            {
                var root = meleeWeapon.GetComponentInParent<MeleeManager>();

                if (root)
                {
                    Handles.color = new Color(0, 1, 0, 0.2f);
                    var center = new Vector3(root.transform.position.x, meleeWeapon.transform.position.y, root.transform.position.z);
                    Handles.DrawSolidArc(center, root.transform.up, root.transform.forward, meleeWeapon.defenseRange, 1.5f);
                    Handles.DrawSolidArc(center, root.transform.up, root.transform.forward, -meleeWeapon.defenseRange, 1.5f);
                }
            }
        }
    }

    void OnEnable()
    {
        attackObject = (MeleeAttackObject)target;
        skin = Resources.Load("skin") as GUISkin;
        m_Logo = Resources.Load("meleeIcon") as Texture2D;
    }

    public override void OnInspectorGUI()
    {
        var olskin = GUI.skin;

        if (skin)
            GUI.skin = skin;

        serializedObject.Update();
        var meleeWeapon = (attackObject as MeleeWeapon);
        GUILayout.BeginVertical(meleeWeapon != null ? "Melee Weapon" : "Melee Attack Object", "window");
        GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

        if (skin) 
            GUILayout.Space(30);

        if(meleeWeapon != null)
        {
            if (meleeWeapon.meleeType == MeleeType.OnlyAttack)
                EditorGUILayout.HelpBox("The defense settingd will be ignored in this mode", MessageType.Info);
            else if (meleeWeapon.meleeType == MeleeType.OnlyDefense)
                EditorGUILayout.HelpBox("Tha attack settings is ignores in this mode", MessageType.Info);
        }

        base.OnInspectorGUI();

        if (meleeWeapon != null)
        {
            var root = meleeWeapon.GetComponentInParent<MeleeManager>();
            if (root && meleeWeapon.meleeType != MeleeType.OnlyAttack)
            {
                showDefenseRange = EditorGUILayout.Toggle("Show Defense Range", showDefenseRange);
            }
            else showDefenseRange = false;
        }

        GUILayout.BeginVertical("box");
        fodoutEvents = EditorGUILayout.Foldout(fodoutEvents, "Attack Object Events");
        if (fodoutEvents)
        {
            GUI.skin = olskin;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onEnableDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onDisableDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onDamageHit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onRecoilHit"));
            GUI.skin = skin;
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}
