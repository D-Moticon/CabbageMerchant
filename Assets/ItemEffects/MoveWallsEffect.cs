using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MoveWallsEffect : ItemEffect
{
    [Tooltip("Horizontal distance each wall moves towards center")]
    public float wallMoveDistance = 0.5f;

    [Tooltip("Duration of the move in seconds")]
    public float moveDuration = 1f;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // 1) find your side walls
        var walls = GameSingleton.Instance.transform.parent
            .GetComponentsInChildren<BoxCollider2D>()
            .Where(x => x.gameObject.layer == LayerMask.NameToLayer("Wall")
                        && Mathf.Abs(x.transform.position.x) > 2f)
            .Select(x => x.transform)
            .ToList();

        // 2) find all tiled-board-background sprite renderers under GameSingleton
        var bgRenderers = GameSingleton.Instance.transform.parent
            .GetComponentsInChildren<SpriteRenderer>(true)
            .Where(sr => sr.CompareTag("BoardBG") && sr.drawMode == SpriteDrawMode.Tiled)
            .ToList();

        // hand both lists into the coroutine
        GameSingleton.Instance.gameStateMachine
            .StartCoroutine(MoveWallsCoroutine(walls, bgRenderers));
    }

    private IEnumerator MoveWallsCoroutine(
        List<Transform> walls,
        List<SpriteRenderer> bgRenderers
    ) {
        // cache original wall positions & compute their targets
        var originals = new Vector3[walls.Count];
        var targets   = new Vector3[walls.Count];
        for (int i = 0; i < walls.Count; i++)
        {
            originals[i] = walls[i].position;
            float dir = walls[i].position.x > 0 ? -1f : 1f;
            targets[i] = originals[i] + Vector3.right * wallMoveDistance * dir;
        }

        // cache each BG’s original size (before transform scale)
        var originalBgSizes = new Vector2[bgRenderers.Count];
        for (int i = 0; i < bgRenderers.Count; i++)
            originalBgSizes[i] = bgRenderers[i].size;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // move walls
            for (int i = 0; i < walls.Count; i++)
                walls[i].position = Vector3.Lerp(originals[i], targets[i], t);

            // compute the new board width between the two outermost walls
            float minX = walls.Min(w => w.position.x);
            float maxX = walls.Max(w => w.position.x);
            float boardWidth = maxX - minX;

            // resize each tiled background so that:
            // (spriteRenderer.size.x * transform.localScale.x) == boardWidth
            for (int i = 0; i < bgRenderers.Count; i++)
            {
                var sr = bgRenderers[i];
                float scaleX = sr.transform.localScale.x;
                sr.size = new Vector2(boardWidth / scaleX,
                                      originalBgSizes[i].y);
            }

            yield return null;
        }

        // final snap to target positions
        for (int i = 0; i < walls.Count; i++)
            walls[i].position = targets[i];

        // final background resize (same logic as above)
        float finalMinX = walls.Min(w => w.position.x);
        float finalMaxX = walls.Max(w => w.position.x);
        float finalWidth = finalMaxX - finalMinX;

        for (int i = 0; i < bgRenderers.Count; i++)
        {
            var sr = bgRenderers[i];
            float scaleX = sr.transform.localScale.x;
            sr.size = new Vector2(finalWidth / scaleX,
                                  originalBgSizes[i].y);
        }
    }

    public override string GetDescription()
    {
        return $"Move walls {wallMoveDistance:F1}m closer to the center";
    }
}
