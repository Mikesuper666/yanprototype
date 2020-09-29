using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    #region SerializedProperties in customEditor
    [HideInInspector]
    public int id;
    [HideInInspector]
    public string description = "Item Description";
    [HideInInspector]
    public ItemType type;
    [HideInInspector]
    public Sprite icon;
    [HideInInspector]
    public bool stackable = true;
    [HideInInspector]
    public int maxStack;
    // [HideInInspector]
    public int amount;
    [HideInInspector]
    public GameObject originalObject;
    [HideInInspector]
    public GameObject dropObject;
    [HideInInspector]
    public List<ItemAttribute> attributes = new List<ItemAttribute>();
    [HideInInspector]
    public bool isInEquipArea;
    #endregion

    #region Properties in defaultInspector
    public bool displayAttributes = true;
    public bool twoHandWeapon;
    //[Header("Usable Settings")]
    //public int UseID;
    //public float useDelayTime = 0.5f;
    [Header("Equipable Settings")]
    public int EquipID;
    public string customEquipPoint = "defaultPoint";
    public float equipDelayTime = 0.5f;
    #endregion

    public Texture2D iconTexture
    {
        get
        {
            if (!icon) return null;
            try
            {
                if (icon.rect.width != icon.texture.width || icon.rect.height != icon.texture.height)
                {
                    Texture2D newText = new Texture2D((int)icon.textureRect.width, (int)icon.textureRect.height);
                    newText.name = icon.name;
                    Color[] newColors = icon.texture.GetPixels((int)icon.textureRect.x, (int)icon.textureRect.y, (int)icon.textureRect.width, (int)icon.textureRect.height);
                    newText.SetPixels(newColors);
                    newText.Apply();

                    return newText;
                }
                else
                    return icon.texture;
            }
            catch
            {
                Debug.LogWarning("Icon texture of the " + name + " is not Readable", icon.texture);
                return icon.texture;
            }
        }
    }// Convert Sprite icon to texture

    public ItemAttribute GetItemAttribute(string name)
    {
        if (attributes != null)
            return attributes.Find(attribute => attribute.name.ToString().Equals(name));
        return null;
    }
}
