using UnityEngine;
using System.Collections;
using UnityEngine.Events;


    public class WindowPop_up : MonoBehaviour
    {
        public InventoryWindow inventoryWindow;
        public UnityEvent OnOpen;
        public UnityEvent OnClose;

        protected virtual void OnEnable()
        {
            inventoryWindow.AddPop_up(this);
            if (OnOpen != null)
                OnOpen.Invoke();
        }

        protected virtual void OnDisable()
        {
            inventoryWindow.RemovePop_up(this);
            if (OnClose != null)
                OnClose.Invoke();
        }
    }

