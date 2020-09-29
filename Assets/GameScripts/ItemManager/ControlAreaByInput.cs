using UnityEngine;
using System.Collections;
using Invector.vCharacterController;
using System.Collections.Generic;


    public class ControlAreaByInput : MonoBehaviour
    {
        public List<SlotsSelector> slotsSelectors;
        public EquipArea equipArea;
        public Inventory inventory;

        void Start()
        {
            inventory = GetComponentInParent<Inventory>();
        }

        void Update()
        {
            if (!inventory || !equipArea || inventory.lockInput) return;

            for (int i = 0; i < slotsSelectors.Count; i++)
            {
                if(slotsSelectors[i].input.GetButtonDown() && (inventory && !inventory.isOpen && inventory.canEquip))
                {
                    if(slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                    {                        
                        equipArea.SetEquipSlot(slotsSelectors[i].indexOfSlot);
                    }
                }

                if (slotsSelectors[i].equipDisplay != null && slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                {
                    if(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item != slotsSelectors[i].equipDisplay.item)
                    {
                        slotsSelectors[i].equipDisplay.AddItem(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item);
                    }
                }
            }
        }

        [System.Serializable]
        public class SlotsSelector
        {
            public GenericInput input;
            public int indexOfSlot;
            public EquipmentDisplay equipDisplay;
        }
    }

