using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairControle : MonoBehaviour
{
    public Animator animatorCross;
    // Start is called before the first frame update
    void Start()
    {
        animatorCross = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animatorCross.SetFloat("inputVertical", Input.GetAxisRaw("Vertical"));
        animatorCross.SetBool("running", Input.GetKey(KeyCode.LeftShift));
    }
}
