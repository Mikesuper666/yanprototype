using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
//Add an icon on hierachy component//
[InitializeOnLoad]
    public class SetComponetIcon
    {
        static Texture2D texturePanel;
        static List<int> markedObjects;
        static SetComponetIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += ThirdPersonControllerIcon;
            EditorApplication.hierarchyWindowItemOnGUI += ThirPersonCameraIcon;
        }
        static void ThirPersonCameraIcon(int instanceId, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;

            var tpCamera = go.GetComponent<MainCamera>();
            if (tpCamera != null) DrawIcon("camera", selectionRect);
        }

        static void ThirdPersonControllerIcon(int instanceId, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;

            var controller = go.GetComponent<PlayerController>();
            if (controller != null) DrawIcon("player", selectionRect);
        }


        private static void DrawIcon(string texName, Rect rect)
        {
            Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);
            GUI.DrawTexture(r, GetTex(texName));
        }

        private static Texture2D GetTex(string name)
        {
            return (Texture2D)Resources.Load(name);
        }
    }//Add an icon on hierachy component
