using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class DialogueTask
{
    public abstract IEnumerator RunTask(DialogueContext dc);
}
