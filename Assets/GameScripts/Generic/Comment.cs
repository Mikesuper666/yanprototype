using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ClassHeader("Comment", false, "barkIcon")]
public class Comment : mMonoBehaviour
{
#if UNITY_EDITOR
    [TextAreaAttribute(5, 3000)]
    public string comment;
#endif
}
