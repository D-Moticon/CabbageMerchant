using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns two portals (top and bottom) that track the mouse X position for a duration.
/// </summary>
public class Weapon_Portal_ItemEffect : ItemEffect
{
    [Header("Portal Settings")]
    [Tooltip("Pooled prefab for the portal")]
    public PooledObjectData portalPrefab;
    [Tooltip("Vertical distance from screen center to portal positions")]
    public float portalY = 5f;
    [Tooltip("Duration portals remain active")]
    public float effectDuration = 3f;

    [Header("Ball Bonk Value")]
    [Tooltip("Bonk value applied when ball passes through portal")]
    public int bonkValue = 1;

    private GameObject topPortal;
    private GameObject bottomPortal;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Spawn portals at top and bottom
        topPortal = portalPrefab.Spawn();
        bottomPortal = portalPrefab.Spawn();

        // Initialize BallPortal component
        var topBP = topPortal.GetComponent<BallPortal>() ?? topPortal.AddComponent<BallPortal>();
        var botBP = bottomPortal.GetComponent<BallPortal>() ?? bottomPortal.AddComponent<BallPortal>();
        topBP.otherPortal = botBP;
        botBP.otherPortal = topBP;
        topBP.bonkValue = bonkValue;
        botBP.bonkValue = bonkValue;

        topBP.canEnter = false;
        
        // Parent to map (optional)
        topPortal.transform.SetParent(GameSingleton.Instance.gameStateMachine.transform, false);
        bottomPortal.transform.SetParent(GameSingleton.Instance.gameStateMachine.transform, false);

        // Start coroutine to track mouse and deactivate
        GameSingleton.Instance.gameStateMachine.StartCoroutine(
            PortalCoroutine()
        );
    }

    public override string GetDescription()
    {
        return $"Create a portal pair that teleports the ball to the top of the board and increases its bonk power by {bonkValue}";
        //throw new System.NotImplementedException();
    }

    private IEnumerator PortalCoroutine()
    {
        float elapsed = 0f;
        // initial vertical positions
        float screenMidY = Camera.main.transform.position.y;
        while (elapsed < effectDuration)
        {
            // update X from mouse
            float mouseX = Singleton.Instance.playerInputManager.mousePosWorldSpace.x;
            // set top and bottom
            topPortal.transform.position = new Vector3(mouseX, screenMidY + portalY, 0f);
            bottomPortal.transform.position = new Vector3(mouseX, screenMidY - portalY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // cleanup
        if (topPortal != null) topPortal.SetActive(false);
        if (bottomPortal != null) bottomPortal.SetActive(false);
    }
}