using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public abstract class LockOnBehaviour : mMonoBehaviour
{
    #region Properies

    private Transform watcher;
    [Tooltip("Tags of objects that can be found")]
    public string[] tagsToFind = new string[] { "Enemy" };
    [Tooltip("Layer of Obscatacles to prevent the find for targets")]
    public LayerMask layerOfObstacles = 1 << 0;
    [Range(0, 1)]
    [Tooltip("Use this to set a margin of Aim point ")]
    public float screenMarginX = 0.8f;
    [Range(0, 1)]
    [Tooltip("Use this to set a margin of Aim point ")]
    public float screenMarginY = 0.1f;
    [Tooltip("Range of the search for targets")]
    public float range = 10f;
    [Tooltip("Show the Gizmos and helpers")]
    public bool showDebug;
    public float timeToChangeTarget = 0.25f;

    private int index = 0;
    private List<Transform> visibles;
    private Transform target;
    private Rect rect;
    private bool _inLockOn;
    protected bool changingTarget;

    #endregion

    #region public methods   

    /// <summary>
    /// change the current target to next target of possibles target
    /// if exist more than 1 target in list
    /// </summary>
    public virtual void ChangeTarget(int value)
    {
        StartCoroutine(ChangeTargetRoutine(value));
    }

    public virtual Transform currentTarget
    {
        get { return target; }
    }/// Get current target

    public virtual bool isCharacterAlive()
    {
        if (currentTarget == null) return false;
        var ichar = currentTarget.GetComponent<Character>();
        //if (ichar == null) return false;
        //if (ichar.ragdolled) return false;
        //if (ichar.currentHealth > 0) return true;
        return true;//**********************************************************************************************
    } /// Check if Current target(vCharacter) is Alive 

    public virtual bool isCharacterAlive(Transform other)
    {
        var ichar = other.GetComponent<Character>();
        // if (ichar == null) return false;
        // if (ichar.ragdolled) return false;
        //if (ichar.currentHealth > 0) return true;
        return true;//**********************************************************************************************;
    }/// Check if target is a vCharacter Alive

    public virtual void ResetLockOn()
    {
        target = null;
        _inLockOn = false;
    }/// Reset Lock on (removing currentTarget)

    public virtual List<Transform> allTargets
    {
        get { if (visibles != null && visibles.Count > 0) return visibles; return null; }
    }/// Get all target possibles

    #endregion

    #region protected methods

    protected virtual void UpdateLockOn(bool value)
    {
        if (value == true && value != _inLockOn)
        {
            _inLockOn = value;
            visibles = GetPossibleTargets();
            index = 0;
            if (visibles != null && visibles.Count > 0)
            {
                target = visibles[index];
            }
        }
        else if (value == false && value != _inLockOn)
        {
            _inLockOn = value;
            index = 0;
            target = null;
            if (visibles != null)
                visibles.Clear();
        }
    }

    protected virtual IEnumerator ChangeTargetRoutine(int value)
    {
        if (!changingTarget)
        {
            changingTarget = true;
            visibles = GetPossibleTargets();
            if ((_inLockOn == true && visibles != null && visibles.Count > 1))
            {
                if (index + value > visibles.Count - 1)
                    index = 0;
                else if (index + value < 0)
                    index = visibles.Count - 1;
                else
                    index += value;
                target = visibles[index];
                SetTarget();
            }
            yield return new WaitForSeconds(timeToChangeTarget);
            changingTarget = false;
        }
    }

    protected virtual void SetTarget()
    {

    }

    protected void OnGUI()
    {
        if (showDebug)
        {
            var width = Screen.width - (Screen.width * screenMarginX);
            var height = Screen.height - (Screen.height * screenMarginY);
            var posX = (Screen.width * 0.5f) - (width * 0.5f);
            var posY = (Screen.height * 0.5f) - (height * 0.5f);
            rect = new Rect(posX, posY, width, height);
            GUI.Box(rect, "");
        }
    }/// Draw GUI of Rect

    protected void Init()
    {
        if (Camera.main == null)
            this.enabled = false;
        var width = Screen.width - (Screen.width * screenMarginX);
        var height = Screen.height - (Screen.height * screenMarginY);
        var posX = (Screen.width * 0.5f) - (width * 0.5f);
        var posY = (Screen.height * 0.5f) - (height * 0.5f);
        rect = new Rect(posX, posY, width, height);
    }/// Init the properts

    protected void OnDrawGizmos()
    {
        if (showDebug && watcher)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(watcher.position, range + 0.1f);
            if (visibles != null && visibles.Count > 0 && target != null)
            {
                visibles.ForEach(delegate (Transform _transform)
                {
                    Gizmos.color = _transform.Equals(currentTarget) ? Color.red : Color.yellow;
                    Gizmos.DrawSphere(_transform.GetComponent<Collider>().bounds.center, .5f);
                });
            }
        }
    }/// Draw the range gizmo

    protected List<Transform> GetPossibleTargets()
    {
        if (MainCamera.instance != null && MainCamera.instance.target != null)
            watcher = MainCamera.instance.target;
        else
            watcher = transform;
        var listPrimary = new List<Transform>();
        var targets = Physics.SphereCastAll(watcher.position, range, watcher.forward, .01f);
        for (int i = 0; i < targets.Length; i++)
        {
            var hitOther = targets[i];
            if (tagsToFind.Contains(hitOther.transform.tag))
            {
                if (isCharacterAlive(hitOther.transform.GetComponent<Transform>()))
                {
                    RaycastHit hit;
                    var boundPoints = BoundPoints(hitOther.collider);
                    for (int a = 0; a < boundPoints.Length; a++)
                    {
                        var point = boundPoints[a];
                        if (Physics.Linecast(transform.position, point, out hit, layerOfObstacles))
                        {
                            if (hit.transform == hitOther.transform)
                            {
                                listPrimary.Add(hitOther.transform);
                                if (showDebug)
                                    Debug.DrawLine(transform.position, point, Color.green, 2);
                                break;
                            }
                            else if (showDebug)
                            {
                                Debug.DrawLine(transform.position, point, Color.red, 2);
                            }
                        }
                        else
                        {
                            listPrimary.Add(hitOther.transform);
                            if (showDebug)
                                Debug.DrawLine(transform.position, point, Color.green, 2);
                            break;
                        }
                    }
                }
            }
        }
        SortTargets(ref listPrimary);
        return listPrimary;
    }/// get all possibles targets

    protected void SortTargets(ref List<Transform> list)
    {
        var lpriority_01 = new List<Transform>();
        var lpriority_02 = new List<Transform>();
        var lpriority_03 = new List<Transform>();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        for (int i = 0; i < list.Count; i++)
        {
            var _transform = list[i];
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(_transform.transform.position);
            if (GeometryUtility.TestPlanesAABB(planes, _transform.GetComponent<Collider>().bounds) && rect.Contains(screenPoint))
                lpriority_01.Add(_transform);
            else if (GeometryUtility.TestPlanesAABB(planes, _transform.GetComponent<Collider>().bounds))
                lpriority_02.Add(_transform);
            else
                lpriority_03.Add(_transform);
        }
        lpriority_01.Sort(delegate (Transform t1, Transform t2)
        {
            Vector2 screenPoint_01 = Camera.main.WorldToScreenPoint(t1.transform.position);
            Vector2 screenPoint_02 = Camera.main.WorldToScreenPoint(t2.transform.position);
            return Vector2.Distance(screenPoint_01, rect.center)
                .CompareTo(Vector2.Distance(screenPoint_02, rect.center));
        });
        lpriority_02.Sort(delegate (Transform t1, Transform t2)
        {
            Vector2 screenPoint_01 = Camera.main.WorldToScreenPoint(t1.transform.position);
            Vector2 screenPoint_02 = Camera.main.WorldToScreenPoint(t2.transform.position);
            return Vector2.Distance(screenPoint_01, rect.center)
                .CompareTo(Vector2.Distance(screenPoint_02, rect.center));
        });
        lpriority_03.Sort(delegate (Transform t1, Transform t2)
        {
            return Vector3.Distance(t1.transform.position, transform.position)
                .CompareTo(Vector3.Distance(t2.transform.position, transform.position));
        });

        list = lpriority_01.Union(lpriority_02).Union(lpriority_03).ToList();
    }/// Sort the targets of possible targets by order of priority

    protected Vector3[] BoundPoints(Collider collider)
    {
        var boundPoint1 = collider.bounds.min;
        var boundPoint2 = collider.bounds.max;
        var boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
        var boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
        var boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
        var boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
        var boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
        var boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);
        var lineColor = Color.white;
        if (showDebug)
        {
            Debug.DrawLine(boundPoint6, boundPoint2, lineColor, 1);
            Debug.DrawLine(boundPoint2, boundPoint8, lineColor, 1);
            Debug.DrawLine(boundPoint8, boundPoint4, lineColor, 1);
            Debug.DrawLine(boundPoint4, boundPoint6, lineColor, 1);
            // bottom of rectangular cuboid (3-7-5-1)
            Debug.DrawLine(boundPoint3, boundPoint7, lineColor, 1);
            Debug.DrawLine(boundPoint7, boundPoint5, lineColor, 1);
            Debug.DrawLine(boundPoint5, boundPoint1, lineColor, 1);
            Debug.DrawLine(boundPoint1, boundPoint3, lineColor, 1);
            // legs (6-3, 2-7, 8-5, 4-1)
            Debug.DrawLine(boundPoint6, boundPoint3, lineColor, 1);
            Debug.DrawLine(boundPoint2, boundPoint7, lineColor, 1);
            Debug.DrawLine(boundPoint8, boundPoint5, lineColor, 1);
            Debug.DrawLine(boundPoint4, boundPoint1, lineColor, 1);
        }
        return new Vector3[] { boundPoint1, boundPoint2, boundPoint3, boundPoint4, boundPoint5, boundPoint6, boundPoint7, boundPoint8 };
    }/// return 8 bound points

    #endregion

}

//Get point of Target relative to Screen
public static class vLockOnHelper
{

    public static Vector2 GetScreenPointOffBoundsCenter(this Transform target, Canvas canvas, Camera cam, float _heightOffset)
    {
        var bounds = target.GetComponent<Collider>().bounds;
        var middle = bounds.center;
        var height = Vector3.Distance(bounds.min, bounds.max);

        var point = middle + new Vector3(0, height * _heightOffset, 0);
        var rectTransform = canvas.transform as RectTransform;
        Vector2 ViewportPosition = cam.WorldToViewportPoint(point);
        Vector2 WorldObject_ScreenPosition = new Vector2(
         ((ViewportPosition.x * rectTransform.sizeDelta.x) - (rectTransform.sizeDelta.x * 0.5f)),
         ((ViewportPosition.y * rectTransform.sizeDelta.y) - (rectTransform.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    public static Vector3 GetPointOffBoundsCenter(this Transform target, float _heightOffset)
    {
        var bounds = target.GetComponent<Collider>().bounds;
        var middle = bounds.center;
        var height = Vector3.Distance(bounds.min, bounds.max);

        var point = middle + new Vector3(0, height * _heightOffset, 0);
        return point;
    }
}