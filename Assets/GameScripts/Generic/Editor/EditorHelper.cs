using UnityEngine;
using System.Linq.Expressions;
using System;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using YanProject;

public class EditorHelper :Editor
{
    public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
    {
        var me = propertyLambda.Body as MemberExpression;

        if (me == null)
        {
            throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
        }

        return me.Member.Name;
    }/// Get PropertyName

    public static bool IsUnityEventyType(Type type)
    {
        if (type.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
        if (type.BaseType.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
        if (type.Name.Contains("UnityEvent") || type.BaseType.Name.Contains("UnityEvent")) return true;
        return false;
    }/// Check if type is a <see cref="UnityEngine.Events.UnityEvent"/>
}

[CanEditMultipleObjects]
[CustomEditor(typeof(mMonoBehaviour), true)]
public class EditorBase : Editor
{
    #region Variables   
    public string[] ignoreEvents;
    public string[] notEventProperties;
    public string[] ignore_vMono = new string[] { "openCloseWindow", "openCloseEvents", "selectedToolbar" };
    public ClassHeaderAttribute headerAttribute;
    public GUISkin skin;
    public Texture2D m_Logo;
    public List<ToolBar> toolbars;
    #endregion

    public class ToolBar
    {
        public string title;
        public bool useIcon;
        public string iconName;
        public List<string> variables;
        public int order;
        public ToolBar()
        {
            title = string.Empty;
            variables = new List<string>();
        }
    }

    protected virtual void OnEnable()
    {
        var targetObject = serializedObject.targetObject;
        var hasAttributeHeader = targetObject.GetType().IsDefined(typeof(ClassHeaderAttribute), true);
        if (hasAttributeHeader)
        {
            var attributes = Attribute.GetCustomAttributes(targetObject.GetType(), typeof(ClassHeaderAttribute), true);
            if (attributes.Length > 0)
                headerAttribute = (ClassHeaderAttribute)attributes[0];
        }

        skin = Resources.Load("skin") as GUISkin;
        m_Logo = Resources.Load("icon") as Texture2D;
        var prop = serializedObject.GetIterator();

        if (((mMonoBehaviour)target) != null)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            List<string> events = new List<string>();

            toolbars = new List<ToolBar>();
            var toolbar = new ToolBar();
            toolbar.title = "Default";

            toolbars.Add(toolbar);
            var index = 0;
            bool needReorder = false;
            int oldOrder = 0;
            while (prop.NextVisible(true))
            {
                var fieldInfo = targetObject.GetType().GetField(prop.name, flags);
                if (fieldInfo != null)
                {
                    var toolBarAttributes = fieldInfo.GetCustomAttributes(typeof(EditorToolbarAttribute), true);

                    if (toolBarAttributes.Length > 0)
                    {
                        var attribute = toolBarAttributes[0] as EditorToolbarAttribute;
                        var _toolbar = toolbars.Find(tool => tool != null && tool.title == attribute.title);

                        if (_toolbar == null)
                        {
                            toolbar = new ToolBar();
                            toolbar.title = attribute.title;
                            toolbar.useIcon = attribute.useIcon;
                            toolbar.iconName = attribute.icon;
                            toolbars.Add(toolbar);
                            toolbar.order = attribute.order;
                            if (oldOrder < attribute.order) needReorder = true;
                            index = toolbars.Count - 1;

                        }
                        else
                        {
                            index = toolbars.IndexOf(_toolbar);
                            if (index < toolbars.Count)
                            {
                                if (attribute.overrideChildOrder)
                                {
                                    if (oldOrder < attribute.order) needReorder = true;
                                    toolbars[index].order = attribute.order;
                                }
                                if (attribute.overrideIcon)
                                {
                                    toolbars[index].useIcon = true;
                                    toolbars[index].iconName = attribute.icon;
                                }
                            }
                        }
                    }
                    if (index < toolbars.Count)
                    {
                        toolbars[index].variables.Add(prop.name);
                    }

                    if ((EditorHelper.IsUnityEventyType(fieldInfo.FieldType) && !events.Contains(fieldInfo.Name)))
                    {
                        events.Add(fieldInfo.Name);
                    }
                }
            }
            if (needReorder)
                toolbars.Sort((a, b) => a.order.CompareTo(b.order));
            var nullToolBar = toolbars.FindAll(tool => tool != null && (tool.variables == null || tool.variables.Count == 0));
            for (int i = 0; i < nullToolBar.Count; i++)
            {
                if (toolbars.Contains(nullToolBar[i]))
                    toolbars.Remove(nullToolBar[i]);
            }

            ignoreEvents = events.vToArray();
            if (headerAttribute != null)
                m_Logo = Resources.Load(headerAttribute.iconName) as Texture2D;
            //else headerAttribute = new vClassHeaderAttribute(target.GetType().Name);
        }
    }

    protected bool openCloseWindow
    {
        get
        {
            return serializedObject.FindProperty("openCloseWindow").boolValue;
        }
        set
        {
            var _openClose = serializedObject.FindProperty("openCloseWindow");
            if (_openClose != null && value != _openClose.boolValue)
            {
                _openClose.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    protected bool openCloseEvents
    {
        get
        {
            var _openCloseEvents = serializedObject.FindProperty("openCloseEvents");
            return _openCloseEvents != null ? _openCloseEvents.boolValue : false;
        }
        set
        {
            var _openCloseEvents = serializedObject.FindProperty("openCloseEvents");
            if (_openCloseEvents != null && value != _openCloseEvents.boolValue)
            {
                _openCloseEvents.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    protected int selectedToolBar
    {
        get
        {
            var _selectedToolBar = serializedObject.FindProperty("selectedToolbar");
            return _selectedToolBar != null ? _selectedToolBar.intValue : 0;
        }
        set
        {
            var _selectedToolBar = serializedObject.FindProperty("selectedToolbar");

            if (_selectedToolBar != null && value != _selectedToolBar.intValue)
            {
                _selectedToolBar.intValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (toolbars != null && toolbars.Count > 1)
        {
            GUILayout.BeginVertical(headerAttribute != null ? headerAttribute.header : target.GetType().Name, skin.window);
            GUILayout.Label(m_Logo, skin.label, GUILayout.MaxHeight(25));
            if (headerAttribute.openClose)
            {
                openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
            }

            if (!headerAttribute.openClose || openCloseWindow)
            {
                var titles = getToobarTitles();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                GUILayout.Space(10);//Espaço acima no inspector
                var customToolbar = skin.GetStyle("customToolbar");
                selectedToolBar = GUILayout.SelectionGrid(selectedToolBar, titles, titles.Length > 2 ? 3 : titles.Length, customToolbar, GUILayout.MinWidth(250));
                if (!(selectedToolBar < toolbars.Count)) selectedToolBar = 0;
                GUILayout.Space(10);//Espaço entre os botões de hidden no inspector
                //GUILayout.Box(toolbars[selectedToolBar].title, skin.box, GUILayout.ExpandWidth(true));
                var ignore = getIgnoreProperties(toolbars[selectedToolBar]);
                var ignoreProperties = ignore.Append(ignore_vMono);
                DrawPropertiesExcluding(serializedObject, ignoreProperties);
            }
            GUILayout.EndVertical();
        }
        else
        {
            if (headerAttribute == null)
            {
                if (((mMonoBehaviour)target) != null)
                    DrawPropertiesExcluding(serializedObject, ignore_vMono);
                else
                    base.OnInspectorGUI();
            }
            else
            {
                GUILayout.BeginVertical(headerAttribute.header, skin.window);
                GUILayout.Label(m_Logo, skin.label, GUILayout.MaxHeight(25));
                if (headerAttribute.openClose)
                {
                    openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
                }

                if (!headerAttribute.openClose || openCloseWindow)
                {
                    if (headerAttribute.useHelpBox)
                        EditorGUILayout.HelpBox(headerAttribute.helpBoxText, MessageType.Info);

                    if (ignoreEvents != null && ignoreEvents.Length > 0)
                    {
                        var ignoreProperties = ignoreEvents.Append(ignore_vMono);
                        DrawPropertiesExcluding(serializedObject, ignoreProperties);
                        openCloseEvents = GUILayout.Toggle(openCloseEvents, (openCloseEvents ? "Close " : "Open ") + "Events ", skin.button);

                        if (openCloseEvents)
                        {
                            foreach (string propName in ignoreEvents)
                            {
                                var prop = serializedObject.FindProperty(propName);
                                if (prop != null)
                                    EditorGUILayout.PropertyField(prop);
                            }
                        }
                    }
                    else
                    {
                        var ignoreProperties = ignoreEvents.Append(ignore_vMono);
                        DrawPropertiesExcluding(serializedObject, ignoreProperties);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }

    public GUIContent[] getToobarTitles()
    {
        List<GUIContent> props = new List<GUIContent>();
        for (int i = 0; i < toolbars.Count; i++)
        {
            if (toolbars[i] != null)
            {
                Texture icon = null;
                var _icon = Resources.Load(toolbars[i].iconName);
                if (_icon) icon = _icon as Texture;
                GUIContent content = new GUIContent(toolbars[i].useIcon ? "" : toolbars[i].title, icon, "");

                props.Add(content);
            }

        }
        return props.vToArray();
    }

    public string[] getIgnoreProperties(ToolBar toolbar)
    {
        List<string> props = new List<string>();
        for (int i = 0; i < toolbars.Count; i++)
        {
            if (toolbars[i] != null && toolbar != null && toolbar.variables != null)
            {
                for (int a = 0; a < toolbars[i].variables.Count; a++)
                {
                    if (!props.Contains(toolbars[i].variables[a]) && !toolbar.variables.Contains(toolbars[i].variables[a]))
                    {
                        props.Add(toolbars[i].variables[a]);
                    }
                }
            }
        }

        props.Add("m_Script");
        return props.vToArray();
    }
}