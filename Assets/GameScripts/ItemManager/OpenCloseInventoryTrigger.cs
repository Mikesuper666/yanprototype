using UnityEngine;
using System.Collections;


    [ClassHeader("OpenClose Inventory Trigger", false)]
    public class OpenCloseInventoryTrigger : mMonoBehaviour
    {

        public UnityEngine.Events.UnityEvent onOpen, onClose;
        protected virtual void Start()
        {
            var inventory = GetComponentInParent<Inventory>();
            if (inventory) inventory.onOpenCloseInventory.AddListener(OpenCloseInventory);
        }
        public void OpenCloseInventory(bool value)
        {
            if (value) onOpen.Invoke();
            else onClose.Invoke();
        }
    }


