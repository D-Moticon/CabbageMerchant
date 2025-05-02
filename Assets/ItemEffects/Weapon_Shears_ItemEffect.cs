using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class Weapon_Shears_ItemEffect : ItemEffect
{
    public PooledObjectData ballPooledObject;
    [Header("Shear Area")]
    [Tooltip("Size of the horizontal box: X = play-area width, Y = thickness")]
    public Vector2 boxSize = new Vector2(20f, 3f);

    [FormerlySerializedAs("bonkDamage")]
    [Header("Bonk")]
    [Tooltip("How much damage to deal to bonkables")]
    public float bonkValue = 1f;

    public int numberShears = 1;
    [Tooltip("Which layers count as 'bonkable' things")]
    public LayerMask bonkableLayerMask;

    [Header("Ball Splitting")]
    [Tooltip("Max angle (°) away from straight up that the new ball can shoot.")]
    public float splitUpwardAngleRange = 60f;
    public Vector2 splitSpeedRange = new Vector2(5f, 15f);
    [Tooltip("Which layer your Ball GameObjects sit on")]
    public LayerMask ballLayerMask;

    public PooledObjectData sliceVFX;
    public SFXInfo sliceSFX;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // 1) Get the full mouse world‐space position
        Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;

        // 2) Combine layers for bonkables + balls
        int combinedMask = bonkableLayerMask | ballLayerMask;

        // 3) How far apart each shear line is, over 180° so it mirrors
        float angleStep = 180f / Mathf.Max(1, numberShears);

        // Play the slice sound once
        sliceSFX.Play();

        // 4) For each shear line…
        for (int i = 0; i < numberShears; i++)
        {
            float angleDeg = i * angleStep;

            // ─── Cast a box centered on the FULL mouse position ───
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                mousePos,        // ← now uses full mousePos.x, mousePos.y
                boxSize,
                angleDeg,
                combinedMask
            );

            // ─── Bonk every bonkable in this box ───
            foreach (var col in hits)
            {
                if (((1 << col.gameObject.layer) & bonkableLayerMask) != 0
                    && col.TryGetComponent<IBonkable>(out var bonkable))
                {
                    bonkable.Bonk(new BonkParams { bonkerPower = bonkValue });
                }
            }

            // ─── Split each unique ball in this box ───
            var ballsThisLine = new HashSet<Ball>();
            foreach (var col in hits)
            {
                if (((1 << col.gameObject.layer) & ballLayerMask) != 0
                    && col.TryGetComponent<Ball>(out var ball))
                {
                    ballsThisLine.Add(ball);
                }
            }
            foreach (var ball in ballsThisLine)
                SplitBall(ball);

            // ─── Spawn the slice VFX at the mouse, rotated for this line ───
            Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg);
            sliceVFX.Spawn(mousePos, rot);
        }
    }


    private void SplitBall(Ball ball)
    {
        // compute half‐angle for the cone
        float halfRange = splitUpwardAngleRange * 0.5f;

        // pick two random angles in [-halfRange, +halfRange]
        float angleA = Random.Range(-halfRange, halfRange);
        float angleB = Random.Range(-halfRange, halfRange);

        // directions around straight up
        Vector2 dirA = Quaternion.Euler(0f, 0f, angleA) * Vector2.up;
        Vector2 dirB = Quaternion.Euler(0f, 0f, angleB) * Vector2.up;

        // pick random speeds for each
        float speedA = Random.Range(splitSpeedRange.x, splitSpeedRange.y);
        float speedB = Random.Range(splitSpeedRange.x, splitSpeedRange.y);

        // 1) reassign the original ball
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dirA * speedA;

        // 2) spawn and launch the new ball
        Ball cloneBall = ball.pooledObjectRef.Spawn(ball.transform.position).GetComponent<Ball>();
        Rigidbody2D cloneRb  = cloneBall.GetComponent<Rigidbody2D>();
        cloneRb.linearVelocity = dirB * speedB;
        cloneBall.transform.localScale = ball.transform.localScale;
        cloneBall.bonkValue = ball.bonkValue;
    }



    public override string GetDescription()
    {
        string plural = numberShears > 1 ? "s" : "";
        string antiPlural = numberShears > 1 ? "" : "s";
        return
            $"Create {numberShears} slice{plural} at the mouse position that bonk{antiPlural} cabbages for {bonkValue} and split{antiPlural} balls into 2.";
    }

    // -- Editor helper to visualize the box in-scene --
    void OnDrawGizmosSelected()
    {
        Vector2 mousePos = Application.isPlaying
            ? Singleton.Instance.playerInputManager.mousePosWorldSpace
            : (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(new Vector2(0f, mousePos.y), boxSize);
    }
}
