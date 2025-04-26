using System.Collections;
using UnityEngine;

public class MoveNPCTask : DialogueTask
{
    public Vector2 targetPos = new Vector2(0f, 0f);
    public float moveSpeed = 7f;
    public string animationBool = "isWalking";
    public bool destroyOnFinish = false;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        if (dc.npcA == null)
        {
            yield break;
        }

        /*Vector2 startPos = dc.npcA.transform.position;
        Vector2 endPos = startPos + moveOffset;
        float duration = (endPos - startPos).magnitude / moveSpeed;

        dc.npcA.animator.SetBool(animationBool, true);
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            dc.npcA.transform.position = pos;
        }

        dc.npcA.transform.position = endPos;
        dc.npcA.animator.SetBool(animationBool, false);*/

        dc.npcA.SetTargetPosition(targetPos);

        float fallbackDuration = 6f;
        float elapsedTime = 0f;

        yield return new WaitForSeconds(0.1f);
        
        while (dc.npcA.isMoving)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > fallbackDuration)
            {
                yield break;
            }
            
            yield return null;
        }
        
        dc.npcA.transform.position = targetPos;
        
        if (destroyOnFinish)
        {
            GameObject.Destroy(dc.npcA.gameObject);
        }
    }
}
