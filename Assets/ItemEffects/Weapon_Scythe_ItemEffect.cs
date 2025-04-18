using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon_Scythe_ItemEffect : ItemEffect
{
    // The pooled object data (or prefab info) for the scythe that gets attached to the ball.
    public PooledObjectData scythe; 
    // The pooled object data (or prefab info) for the line renderer that is drawn from the mouse point to the scythe.
    public PooledObjectData lineRenderer; 

    public float boostSpeed = 10f;   // How much extra impulse to add to the ball.
    public float duration = 0.5f;    // How long the scythe-chain effect is active.
    public float scytheRotationOffset = 90f;
    public string scytheDescription = "Chain Scythe";
    public string extraDescriptionInfo = "";

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Get the list of active balls and the current mouse position in world space.
        List<Ball> activeBalls = GameSingleton.Instance.gameStateMachine.activeBalls;
        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;

        foreach (Ball ball in activeBalls)
        {
            // 1) Attach a fixed-distance joint so the ball will pivot around the fixed anchor.
            // Here we use a DistanceJoint2D set to the distance between the ball and the fixed position.
            DistanceJoint2D joint = ball.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedAnchor = mouseWorldPos;
            joint.autoConfigureDistance = false;
            float currentDistance = Vector2.Distance(ball.transform.position, mouseWorldPos);
            joint.distance = currentDistance;
            joint.enableCollision = true;

            // 2) Spawn and attach the scythe object.
            // Assumes ObjectPooler.Instance.SpawnFromPool takes a pool tag (string) and returns a GameObject.
            GameObject scytheInstance = GameSingleton.Instance.objectPoolManager.Spawn(scythe, ball.transform.position, Quaternion.identity);
            // Parent the scythe to the ball (so it moves with the ball).
            scytheInstance.transform.SetParent(ball.transform);
            // Optionally, reset its local position (or adjust it according to your visuals).
            scytheInstance.transform.localPosition = Vector3.zero;

            // Initial rotation: make the scythe face the mouse (i.e. the fixed anchor).
            Vector2 direction = mouseWorldPos - (Vector2)ball.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            scytheInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 3) Instantiate and configure the line renderer.
            GameObject lrInstance = GameSingleton.Instance.objectPoolManager.Spawn(lineRenderer, Vector3.zero, Quaternion.identity);
            LineRenderer lr = lrInstance.GetComponent<LineRenderer>();
            if (lr != null)
            {
                // Set the first point at the mouse (fixed anchor) and the second at the scythe’s position.
                lr.SetPosition(0, mouseWorldPos);
                lr.SetPosition(1, scytheInstance.transform.position);
            }

            // 4) Boost the ball’s speed by applying an impulse in the direction toward the mouse.
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 tangent = new Vector2(direction.y, -direction.x).normalized;
                rb.AddForce(tangent * boostSpeed, ForceMode2D.Impulse);
            }

            ball.gameObject.layer = LayerMask.NameToLayer("BallWallsOnly");
            
            // 5) Start a coroutine that updates the rotation and the line renderer while the effect is active,
            // then cleans up after the duration.
            GameSingleton.Instance.gameStateMachine.StartCoroutine(ChainEffectCoroutine(ball, joint, scytheInstance, lr, duration, mouseWorldPos));
        }
    }

    /// <summary>
    /// Updates the effect over the duration. While active, it rotates the scythe so its handle always points toward the fixed anchor,
    /// and updates the line renderer to follow the scythe. After the duration, the joint is removed and the scythe and line renderer deactivated.
    /// </summary>
    private IEnumerator ChainEffectCoroutine(Ball ball,
                                               DistanceJoint2D joint,
                                               GameObject scytheInstance,
                                               LineRenderer lr,
                                               float effectDuration,
                                               Vector2 anchorPos)
    {
        float timer = 0f;
        while (timer < effectDuration && !Singleton.Instance.playerInputManager.weaponFireUp)
        {
            // Recalculate the vector from the ball to the anchor.
            Vector2 diff = anchorPos - (Vector2)ball.transform.position;
            // Compute the angle so that the scythe always points toward the anchor.
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            scytheInstance.transform.rotation = Quaternion.Euler(0, 0, angle+scytheRotationOffset);

            // Update the line renderer: the first point is always at the fixed anchor,
            // and the second point follows the scythe's position.
            if (lr != null)
            {
                lr.SetPosition(0, anchorPos);
                lr.SetPosition(1, scytheInstance.transform.position);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (Singleton.Instance.playerInputManager.weaponFireUp && timer < 0.2f)
        {
            Singleton.Instance.gameHintManager.QueueHintUntilBouncingDone("Looks like I need to hold the weapon button to get the full effect!");
        }

        // After the effect duration, clean up:
        // Remove the joint from the ball.
        if (joint != null)
        {
            GameObject.Destroy(joint);
        }

        // Deactivate (or return to pool) the scythe and the line renderer.
        if (scytheInstance != null)
        {
            scytheInstance.SetActive(false);
        }

        if (lr != null && lr.gameObject != null)
        {
            lr.gameObject.SetActive(false);
        }
        
        ball.gameObject.layer = LayerMask.NameToLayer("Ball");
    }

    public override string GetDescription()
    {
        string desc = $"Attach a chain scythe between each ball and the mouse position. {extraDescriptionInfo}";
        desc += "\n";
        desc += $"Boost Speed: {boostSpeed}";
        desc += "\n";
        desc += $"Duration: {duration}s or until fire released.";
        return desc;
    }
}
