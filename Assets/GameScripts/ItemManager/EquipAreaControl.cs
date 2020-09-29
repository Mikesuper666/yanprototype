using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YanProject;

public class EquipAreaControl : MonoBehaviour
    {
        [HideInInspector]
        public List<EquipArea> equipAreas;

        void Start()
        {
            equipAreas = GetComponentsInChildren<EquipArea>().vToList();
            foreach (EquipArea area in equipAreas)
                area.onPickUpItemCallBack = OnPickUpItemCallBack;

            Inventory inventory = GetComponentInParent<Inventory>();
            if (inventory)
                inventory.onOpenCloseInventory.AddListener(OnOpen);
        }

        public void OnOpen(bool value)
        {

        }

        public void OnPickUpItemCallBack(EquipArea area, ItemSlot slot)
        {
            for (int i = 0; i < equipAreas.Count; i++)
            {
                var sameSlots = equipAreas[i].equipSlots.FindAll(slotInArea =>slotInArea!=slot &&  slotInArea.item != null && slotInArea.item == slot.item);
                for (int a = 0; a < sameSlots.Count; a++)
                {                   
                    equipAreas[i].RemoveItem(sameSlots[a]);
                }
            }
            CheckTwoHandItem(area, slot);
        }

        void CheckTwoHandItem(EquipArea area, ItemSlot slot)
        {
            if (slot.item == null) return;
            var opposite = equipAreas.Find(_area => _area != null && area.equipPointName.Equals("LeftArm") && _area.currentEquipedItem != null);
            //var RightEquipmentController = changeEquipmentControllers.Find(equipCtrl => equipCtrl.equipArea != null && equipCtrl.equipArea.equipPointName.Equals("RightArm"));
            if (area.equipPointName.Equals("LeftArm"))
                opposite = equipAreas.Find(_area => _area != null && area.equipPointName.Equals("RightArm") && _area.currentEquipedItem != null);
            else if (!area.equipPointName.Equals("RightArm"))
            {
                return;
            }
            if (opposite != null && (slot.item.twoHandWeapon || opposite.currentEquipedItem.twoHandWeapon))
            {
                opposite.onUnequipItem.Invoke(opposite, slot.item);
                opposite.RemoveItem(slot as EquipSlot);
            }
        }
    }


