using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector;


    public class ItemListData : ScriptableObject
    {
        public List<Item> items = new List<Item>();       
       
        public bool inEdition;
       
        public bool itemsHidden = true;
        
    }


