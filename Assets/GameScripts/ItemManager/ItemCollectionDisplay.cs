using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


    public class ItemCollectionDisplay : MonoBehaviour
    {
        private static ItemCollectionDisplay instance;
        public static ItemCollectionDisplay Instance
        {
            get
            {
                if (instance == null) { instance = GameObject.FindObjectOfType<ItemCollectionDisplay>(); }
                return ItemCollectionDisplay.instance;
            }
        }       
       
        public GameObject HeadsUpText;
        public Transform Contenet;            

        public void FadeText(string message, float timeToStay, float timeToFadeOut)
        {            
            var itemObj = Instantiate(HeadsUpText) as GameObject;
            itemObj.transform.SetParent(Contenet, false);

            ItemCollectionTextHUD textHud = itemObj.GetComponent<ItemCollectionTextHUD>();
            if (!textHud.inUse)
            {
                textHud.transform.SetAsFirstSibling();
                textHud.Init();
                textHud.Show(message, timeToStay, timeToFadeOut);                    
            }            
        }      
    }


