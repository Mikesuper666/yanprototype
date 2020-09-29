using UnityEngine;
using System.Collections;
using System.Collections.Generic;


    [System.Serializable]
    public class ItemAttribute
    {
        public ItemAttributes name = 0;
        public int value = 0;
        public bool isBool;        
        public ItemAttribute(ItemAttributes name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }

    public static class ItemAttributeHelper
    {
        public static bool Contains(this List<ItemAttribute> attributes, ItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute != null;
        }

        public static ItemAttribute GetAttributeByType(this List<ItemAttribute> attributes, ItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute;
        }

        public static bool Equals(this ItemAttribute attributeA, ItemAttribute attributeB)
        {
            return attributeA.name == attributeB.name;
        }

        public static List<ItemAttribute> CopyAsNew(this List<ItemAttribute> copy)
        {
            var target = new List<ItemAttribute>();

            if (copy != null)
            {
                for (int i = 0; i < copy.Count; i++)
                {
                    ItemAttribute attribute = new ItemAttribute(copy[i].name, copy[i].value);
                    target.Add(attribute);
                }
            }
            return target;
        }
    }


