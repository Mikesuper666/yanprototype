using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;


    [ClassHeader("vInventory")]
    public class Inventory : mMonoBehaviour
    {
        #region Item Variables

        public delegate List<Item> GetItemsDelegate();
        public GetItemsDelegate GetItemsHandler;
        public InventoryWindow firstWindow;
        [Range(0, 1)]
        public float timeScaleWhileIsOpen = 0;
        public float originalTimeScale = 1f;
        public bool updatedTimeScale;
        public bool dontDestroyOnLoad = true;
        public List<ChangeEquipmentControl> changeEquipmentControllers;
        [HideInInspector]
        public List<InventoryWindow> windows = new List<InventoryWindow>();
        [HideInInspector]
        public InventoryWindow currentWindow;

        [Header("Input Mapping")]
        public GenericInput openInventory = new GenericInput("I", "Start", "Start");
        public GenericInput openCurrentSlot = new GenericInput("O", "A", "A");
        public GenericInput removeEquipment = new GenericInput("Backspace", "X", "X");
        [Header("This fields will override the EventSystem Input")]
        public GenericInput horizontal = new GenericInput("Horizontal", "D-Pad Horizontal", "Horizontal");
        public GenericInput vertical = new GenericInput("Vertical", "D-Pad Vertical", "Vertical");
        public GenericInput submit = new GenericInput("Return", "A", "A");
        public GenericInput cancel = new GenericInput("Backspace", "B", "B");        
       
        [Header("Inventory Events")]
        [HideInInspector]
        public OnOpenCloseInventory onOpenCloseInventory;
        [HideInInspector]
        public OnHandleItemEvent onUseItem;
        [HideInInspector]
        public OnChangeItemAmount onLeaveItem, onDropItem;
        [HideInInspector]
        public OnChangeEquipmentEvent onEquipItem, onUnequipItem;     
        [HideInInspector]
        public bool isOpen, canEquip,lockInput;        
        [HideInInspector]
        public EquipArea[] equipAreas;
        public List<Item> items
        {
            get
            {
                if (GetItemsHandler != null) return GetItemsHandler();
                return new List<Item>();
            }
        }

        private EquipArea currentEquipArea;
        private StandaloneInputModule inputModule;
        #endregion

        void Start()
        {
            canEquip = true;
            inputModule = FindObjectOfType<StandaloneInputModule>();
            equipAreas = GetComponentsInChildren<EquipArea>(true);
            foreach (EquipArea equipArea in equipAreas)
            {
                equipArea.Init();
                equipArea.onEquipItem.AddListener(EquipItem);
                equipArea.onUnequipItem.AddListener(UnequipItem);
                equipArea.onSelectEquipArea.AddListener(SelectArea);
            }
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            if(GameController.instance)
                GameController.instance.OnReloadGame.AddListener(OnReloadGame);
        }

        public void OnReloadGame()
        {
            StartCoroutine(ReloadEquipment());
        }

        IEnumerator ReloadEquipment()
        {
            yield return new WaitForEndOfFrame();
            inputModule = FindObjectOfType<StandaloneInputModule>();
            isOpen = true;
            foreach (ChangeEquipmentControl changeEquip in changeEquipmentControllers)
            {
                if (changeEquip.equipArea != null)
                {
                    foreach (EquipSlot slot in changeEquip.equipArea.equipSlots)
                    {
                        //if (slot.item != null && items.Find(i => i.id == slot.item.id) != null)
                        //{
                        //    EquipItem(changeEquip.equipArea, slot.item);
                        //}
                        //else 
                        if (changeEquip.equipArea.currentEquipedItem == null)
                        {
                            UnequipItem(changeEquip.equipArea, slot.item);
                            changeEquip.equipArea.RemoveItem(slot);
                        }
                        else
                        {
                            changeEquip.equipArea.RemoveItem(slot);
                        }
                    }
                }
            }
            isOpen = false;
        }

        void LateUpdate()
        {
            if (lockInput) return;
            ControlWindowsInput();

            if (!isOpen)
                ChangeEquipmentInput();
            else
            {
                UpdateEventSystemInput();
                RemoveEquipmentInput();
                OpenCurrentSlot();
            }
        }

        public virtual void ControlWindowsInput()
        {
            // enable first window
            if ((windows.Count == 0 || windows[windows.Count - 1] == firstWindow))
            {
                if (!firstWindow.gameObject.activeSelf && openInventory.GetButtonDown() && canEquip)
                {
                    firstWindow.gameObject.SetActive(true);
                    isOpen = true;
                    onOpenCloseInventory.Invoke(true);

                    if(!updatedTimeScale)
                    {
                        updatedTimeScale = true;
                        originalTimeScale = Time.timeScale;
                    }
                    Time.timeScale = timeScaleWhileIsOpen;
                }

                else if (firstWindow.gameObject.activeSelf && (openInventory.GetButtonDown() || cancel.GetButtonDown()))
                {
                    currentEquipArea = null;
                    firstWindow.gameObject.SetActive(false);
                    isOpen = false;
                    onOpenCloseInventory.Invoke(false);

                    Time.timeScale = originalTimeScale;
                    if (updatedTimeScale)
                        updatedTimeScale = false;
                }
            }
            if (!isOpen) return;
            // disable current window
            if ((windows.Count > 0 && windows[windows.Count - 1] != firstWindow) && cancel.GetButtonDown())
            {
                currentEquipArea = null;
                if (windows[windows.Count - 1].ContainsPop_up())
                {
                    windows[windows.Count - 1].RemoveLastPop_up();
                    return;
                }
                else
                {
                    windows[windows.Count - 1].gameObject.SetActive(false);
                    windows.RemoveAt(windows.Count - 1);// remove last window of the window list
                    if (windows.Count > 0)
                    {
                        windows[windows.Count - 1].gameObject.SetActive(true);
                        currentWindow = windows[windows.Count - 1];
                    }
                    else
                        currentWindow = null; // clear currentWindow if  window list count == 0        
                }
            }
            // check if currenWindow  that was closed
            if (currentWindow != null && !currentWindow.gameObject.activeSelf)
            {
                // remove currentWindow of the window list
                if (windows.Contains(currentWindow))
                    windows.Remove(currentWindow);
                // set currentWindow if window list have more windows
                if (windows.Count > 0)
                {
                    windows[windows.Count - 1].gameObject.SetActive(true);
                    currentWindow = windows[windows.Count - 1];
                }
                else
                    currentWindow = null;// clear currentWindow if  window list count == 0        
            }
        }

        /// <summary>
        /// Input Button to remove equiped Item
        /// </summary>
        void RemoveEquipmentInput()
        {            
            if (currentEquipArea != null && removeEquipment.GetButtonDown())
            {
                currentEquipArea.RemoveItem();
            }
        }

        /// <summary>
        /// Input Button to open the selected equipSlot
        /// </summary>
        void OpenCurrentSlot()
        {
            if (sInput.instance.inputDevice == InputDevice.Mobile)
            {
                if (currentEquipArea != null && openCurrentSlot.GetButtonDown())
                {
                    if (currentEquipArea.currentSelectedSlot != null)
                        currentEquipArea.OnSubmitSlot(currentEquipArea.currentSelectedSlot);
                }
            }
        }

        void SelectArea(EquipArea equipArea)
        {
            currentEquipArea = equipArea;
        }

        /// <summary>
        /// Input for change equiped Item
        /// </summary>
        void ChangeEquipmentInput()
        {
            // display equiped itens
            if (changeEquipmentControllers.Count > 0 && canEquip)
            {
                foreach (ChangeEquipmentControl changeEquip in changeEquipmentControllers)
                {
                    UseItemInput(changeEquip);
                    if (changeEquip.equipArea != null)
                    {
                        if (sInput.instance.inputDevice == InputDevice.MouseKeyboard|| sInput.instance.inputDevice == InputDevice.Mobile)
                        {
                            if (changeEquip.previousItemInput.GetButtonDown())
                                changeEquip.equipArea.PreviousEquipSlot();
                            if (changeEquip.nextItemInput.GetButtonDown())
                                changeEquip.equipArea.NextEquipSlot();
                        }
                        else if (sInput.instance.inputDevice == InputDevice.Joystick)
                        {
                            if (changeEquip.previousItemInput.GetAxisButtonDown(-1))
                                changeEquip.equipArea.PreviousEquipSlot();
                            if (changeEquip.nextItemInput.GetAxisButtonDown(1))
                                changeEquip.equipArea.NextEquipSlot();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if Equipment in EquipArea is change while drop,leave or use item
        /// </summary>
        void CheckEquipmentChanges()
        {
            foreach (ChangeEquipmentControl changeEquip in changeEquipmentControllers)
            {
                foreach (EquipSlot slot in changeEquip.equipArea.equipSlots)
                {
                    if (slot != null)
                    {
                        if (slot.item != null && !items.Contains(slot.item))
                        {
                            changeEquip.equipArea.RemoveItem(slot);
                            if(changeEquip.display)
                                changeEquip.display.RemoveItem();
                        }
                    }
                }

            }

        }
        
        void UpdateEventSystemInput()
        {
            if (inputModule)
            {
                inputModule.horizontalAxis = horizontal.buttonName;
                inputModule.verticalAxis = vertical.buttonName;
                inputModule.submitButton = submit.buttonName;
                inputModule.cancelButton = cancel.buttonName;
            }
            else
            {
                inputModule = FindObjectOfType<StandaloneInputModule>();
            }
        }

        void UseItemInput(ChangeEquipmentControl changeEquip)
        {
            if (changeEquip.display != null && changeEquip.display.item != null && changeEquip.display.item.type == ItemType.Consumable)
            {
                if (changeEquip.useItemInput.GetButtonDown() && changeEquip.display.item.amount > 0)
                {
                    changeEquip.display.item.amount--;
                    OnUseItem(changeEquip.display.item);
                }
            }
        }

        internal void OnUseItem(Item item)
        {
            onUseItem.Invoke(item);
            CheckEquipmentChanges();
        }

        internal void OnLeaveItem(Item item, int amount)
        {
            onLeaveItem.Invoke(item, amount);
            CheckEquipmentChanges();
        }

        internal void OnDropItem(Item item, int amount)
        {
            onDropItem.Invoke(item, amount);
            CheckEquipmentChanges();
        }

        /// <summary>
        /// Set current window <see cref="vInventoryWindow"/> call automatically when Enabled
        /// </summary>
        /// <param name="inventoryWindow"></param>  
        internal void SetCurrentWindow(InventoryWindow inventoryWindow)
        {
            if (!windows.Contains(inventoryWindow))
            {
                windows.Add(inventoryWindow);
                if (currentWindow != null)
                {
                    currentWindow.gameObject.SetActive(false);
                }
                currentWindow = inventoryWindow;
            }
        }

        public void EquipItem(EquipArea equipArea, Item item)
        {          
            onEquipItem.Invoke(equipArea, item);
            ChangeEquipmentDisplay(equipArea, item, false);           
        }
       
        public void UnequipItem(EquipArea equipArea, Item item)
        {
            onUnequipItem.Invoke(equipArea, item);
            ChangeEquipmentDisplay(equipArea, item);
        }

        void ChangeEquipmentDisplay(EquipArea equipArea, Item item, bool removeItem = true)
        {
            if (changeEquipmentControllers.Count > 0)
            {
                var changeEquipControl = changeEquipmentControllers.Find(changeEquip => changeEquip.equipArea != null && changeEquip.equipArea.equipPointName == equipArea.equipPointName && changeEquip.display != null);
                if (changeEquipControl != null)
                {
                    if (removeItem && changeEquipControl.display.item == item)
                    {
                        changeEquipControl.display.RemoveItem();
                        //changeEquipControl.display.ItemIdentifier();
                    }                        
                    else if (equipArea.currentEquipedItem == item)
                    {
                        changeEquipControl.display.AddItem(item);
                        changeEquipControl.display.ItemIdentifier(changeEquipControl.equipArea.indexOfEquipedItem + 1, true);
                    }                        
                }
            }
        }
                
    }

    [System.Serializable]
    public class ChangeEquipmentControl
    {
        public GenericInput useItemInput = new GenericInput("U", "Start", "Start");
        public GenericInput previousItemInput = new GenericInput("LeftArrow", "D - Pad Horizontal", "D-Pad Horizontal");
        public GenericInput nextItemInput = new GenericInput("RightArrow", "D - Pad Horizontal", "D-Pad Horizontal");
        public EquipArea equipArea;
        public EquipmentDisplay display;
    }


