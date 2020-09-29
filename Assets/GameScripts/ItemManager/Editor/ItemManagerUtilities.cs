using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Events;

    public partial class ItemManagerUtilities
    {
        protected static ItemManagerUtilities instance;

        public static void CreateDefaultEquipPoints(ItemManager itemManager, MeleeManager meleeManager)
        {
            instance = new ItemManagerUtilities();
            instance._CreateDefaultEquipPoints(itemManager, meleeManager);
            instance._InitItemManager(itemManager);
        }
        partial void _CreateDefaultEquipPoints(ItemManager itemManager, MeleeManager meleeManager);

        partial void _InitItemManager(ItemManager itemManager);
    }

    public partial class ItemManagerUtilities
    {
        partial void _CreateDefaultEquipPoints(ItemManager itemManager, MeleeManager meleeManager)
        {
            var animator = itemManager.GetComponent<Animator>();
            if (itemManager.equipPoints == null)
                itemManager.equipPoints = new List<EquipPoint>();

            #region LeftEquipPoint
            var equipPointL = itemManager.equipPoints.Find(p => p.equipPointName == "LeftArm");
            if (equipPointL == null)
            {
                EquipPoint pointL = new EquipPoint();
                pointL.equipPointName = "LeftArm";
                if (meleeManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(pointL.onInstantiateEquiment, meleeManager.SetLeftWeapon);
#else
                    pointL.onInstantiateEquiment.AddListener(manager.SetLeftWeapon);
#endif
                }

                if (animator)
                {
                    var defaultEquipPointL = new GameObject("defaultEquipPoint");
                    var parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    defaultEquipPointL.transform.SetParent(parent);
                    defaultEquipPointL.transform.localPosition = Vector3.zero;
                    defaultEquipPointL.transform.forward = itemManager.transform.forward;
                    defaultEquipPointL.gameObject.tag = "Ignore Ragdoll";
                    pointL.handler = new Handler();
                    pointL.handler.defaultHandler = defaultEquipPointL.transform;
                }
                itemManager.equipPoints.Add(pointL);
            }
            else
            {
                if (equipPointL.handler.defaultHandler == null)
                {
                    if (animator)
                    {
                        var parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        var defaultPoint = parent.Find("defaultEquipPoint");

                        if (defaultPoint)
                            equipPointL.handler.defaultHandler = defaultPoint;
                        else
                        {
                            var _defaultPoint = new GameObject("defaultEquipPoint");
                            _defaultPoint.transform.SetParent(parent);
                            _defaultPoint.transform.localPosition = Vector3.zero;
                            _defaultPoint.transform.forward = itemManager.transform.forward;
                            _defaultPoint.gameObject.tag = "Ignore Ragdoll";
                            equipPointL.handler.defaultHandler = _defaultPoint.transform;
                        }
                    }
                }

                bool containsListener = false;
                for (int i = 0; i < equipPointL.onInstantiateEquiment.GetPersistentEventCount(); i++)
                {
                    if (equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(MeleeManager)) && equipPointL.onInstantiateEquiment.GetPersistentMethodName(i).Equals("SetLeftWeapon"))
                    {
                        containsListener = true;
                        break;
                    }
                }

                if (!containsListener && meleeManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(equipPointL.onInstantiateEquiment, meleeManager.SetLeftWeapon);
#else
                    equipPointL.onInstantiateEquiment.AddListener(manager.SetLeftWeapon);
#endif
                }
            }
            #endregion

            #region RightEquipPoint
            var equipPointR = itemManager.equipPoints.Find(p => p.equipPointName == "RightArm");
            if (equipPointR == null)
            {
                EquipPoint pointR = new EquipPoint();
                pointR.equipPointName = "RightArm";
                if (meleeManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(pointR.onInstantiateEquiment, meleeManager.SetRightWeapon);
#else
                    pointR.onInstantiateEquiment.AddListener(manager.SetRightWeapon);
#endif
                }

                if (animator)
                {
                    var defaultEquipPointR = new GameObject("defaultEquipPoint");
                    var parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    defaultEquipPointR.transform.SetParent(parent);
                    defaultEquipPointR.transform.localPosition = Vector3.zero;
                    defaultEquipPointR.transform.forward = itemManager.transform.forward;
                    defaultEquipPointR.gameObject.tag = "Ignore Ragdoll";
                    pointR.handler = new Handler();
                    pointR.handler.defaultHandler = defaultEquipPointR.transform;
                }
                itemManager.equipPoints.Add(pointR);
            }
            else
            {
                if (equipPointR.handler.defaultHandler == null)
                {
                    if (animator)
                    {
                        var parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        var defaultPoint = parent.Find("defaultEquipPoint");
                        if (defaultPoint) equipPointR.handler.defaultHandler = defaultPoint;
                        else
                        {
                            var _defaultPoint = new GameObject("defaultEquipPoint");
                            _defaultPoint.transform.SetParent(parent);
                            _defaultPoint.transform.localPosition = Vector3.zero;
                            _defaultPoint.transform.forward = itemManager.transform.forward;
                            _defaultPoint.gameObject.tag = "Ignore Ragdoll";
                            equipPointR.handler.defaultHandler = _defaultPoint.transform;
                        }
                    }
                }

                bool containsListener = false;
                for (int i = 0; i < equipPointR.onInstantiateEquiment.GetPersistentEventCount(); i++)
                {

                    if (equipPointR.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(MeleeManager)) && equipPointR.onInstantiateEquiment.GetPersistentMethodName(i).Equals("SetRightWeapon"))
                    {
                        containsListener = true;
                        break;
                    }
                }

                if (!containsListener && meleeManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(equipPointR.onInstantiateEquiment, meleeManager.SetRightWeapon);
#else
                    equipPointR.onInstantiateEquiment.AddListener(manager.SetRightWeapon);
#endif
                }
            }
            #endregion
        }
    }

