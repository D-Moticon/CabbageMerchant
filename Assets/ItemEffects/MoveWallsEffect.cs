using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Effect that moves side walls closer to center over time with a lerp.
/// </summary>
public class MoveWallsEffect : ItemEffect
{
    [Tooltip("Horizontal distance each wall moves towards center")]
    public float wallMoveDistance = 0.5f;

    [Tooltip("Duration of the move in seconds")]
    public float moveDuration = 1f;

    /// <summary>
    /// Trigger the effect: gather walls and start the lerp coroutine.
    /// </summary>
    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Find all side walls (layer=Wall, x offset > threshold)
        var walls = GameSingleton.Instance.transform.parent
            .GetComponentsInChildren<BoxCollider2D>()
            .Where(x => x.gameObject.layer == LayerMask.NameToLayer("Wall")
                        && Mathf.Abs(x.transform.position.x) > 2f)
            .Select(x => x.transform)
            .ToList();

        // Start the move coroutine on a MonoBehaviour
        var runner = GameSingleton.Instance.gameStateMachine;
        runner.StartCoroutine(MoveWallsCoroutine(walls));
    }

    /// <summary>
    /// Coroutine that smoothly moves each wall transform
    /// towards center by wallMoveDistance over moveDuration.
    /// </summary>
    private IEnumerator MoveWallsCoroutine(List<Transform> walls)
    {
        // Cache original positions and target positions
        var originals = new Vector3[walls.Count];
        var targets   = new Vector3[walls.Count];
        for (int i = 0; i < walls.Count; i++)
        {
            var t = walls[i];
            originals[i] = t.position;
            float dir    = t.position.x > 0 ? -1f : 1f;
            targets[i]   = t.position + Vector3.right * wallMoveDistance * dir;
        }

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].position = Vector3.Lerp(originals[i], targets[i], t);
            }
            yield return null;
        }

        // ensure final alignment
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i].position = targets[i];
        }
    }

    public override string GetDescription()
    {
        return $"Move walls {wallMoveDistance:F1}m closer to the center";
    }
}
