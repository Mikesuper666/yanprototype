using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, ICharacter
{
    #region Character Variables

    // get the animator component of character
    [HideInInspector]
    public Animator anime { get; private set; }

    public bool ragdolled { get; set; }

    #endregion

    protected virtual void Start()
    {

    }

    public virtual void Init()
    {
        anime = GetComponent<Animator>();

        if (anime)
        {
            Debug.Log("pegou o componente Animator**********************************************");
        }
    }
}
