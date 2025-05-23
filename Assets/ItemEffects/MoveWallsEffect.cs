using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MoveWallsEffect : ItemEffect
{
    [Tooltip("Horizontal distance each wall moves towards center (min, max)")]
    public Vector2 wallMoveDistanceRange = new Vector2(0.3f, 0.7f);

    [Tooltip("Rotation angle to apply to walls (min, max) in degrees")]
    public Vector2 wallRotationRange = new Vector2(-15f, 15f);

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
    )
    {
        int count = walls.Count;
        
        //XXXX
        
        int bgCount = bgRenderers.Count;
        var fillMeshes = new Mesh[bgCount];

        for (int i = 0; i < bgCount; i++)
        {
            var sr = bgRenderers[i];
            sr.enabled = false; // hide the old sprite

            // make a new mesh‐object
            var go = new GameObject($"BGFill_{i}");
            go.transform.SetParent(sr.transform.parent, false);

            // match its sorting
            var sg = go.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sg.sortingLayerID = sr.sortingLayerID;
            sg.sortingOrder   = sr.sortingOrder - 1;

            // copy material
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = sr.sharedMaterial;

            // mesh filter
            var mf = go.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mf.mesh = mesh;
            fillMeshes[i] = mesh;
        }
        
        //XXXX

        // cache originals
        var originals    = new Vector3[count];
        var originalRots = new Quaternion[count];
        for (int i = 0; i < count; i++)
        {
            originals[i]    = walls[i].position;
            originalRots[i] = walls[i].rotation;
        }

        // compute random targets
        var targets    = new Vector3[count];
        var targetRots = new Quaternion[count];
        for (int i = 0; i < count; i++)
        {
            float dist = Random.Range(
                wallMoveDistanceRange.x, 
                wallMoveDistanceRange.y
            );
            // push toward center
            float dir = walls[i].position.x > 0f ? -1f : 1f;
            targets[i] = originals[i] + Vector3.right * dist * dir;

            // random rotation around Z
            float angle = Random.Range(
                wallRotationRange.x, 
                wallRotationRange.y
            );
            targetRots[i] = originalRots[i] * Quaternion.Euler(0, 0, angle);
        }

        // cache each BG’s original size
        var originalBgSizes = new Vector2[bgRenderers.Count];
        for (int i = 0; i < bgRenderers.Count; i++)
            originalBgSizes[i] = bgRenderers[i].size;

        // animate
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // move & rotate walls
            for (int i = 0; i < count; i++)
            {
                walls[i].position = Vector3.Lerp(
                    originals[i], targets[i], t
                );
                walls[i].rotation = Quaternion.Lerp(
                    originalRots[i], targetRots[i], t
                );
            }

            // resize backgrounds to fit new wall positions
            float minX = walls.Min(w => w.position.x);
            float maxX = walls.Max(w => w.position.x);
            float boardWidth = maxX - minX;

            for (int i = 0; i < bgRenderers.Count; i++)
            {
                var sr = bgRenderers[i];
                float scaleX = sr.transform.localScale.x;
                sr.size = new Vector2(
                    boardWidth / scaleX,
                    originalBgSizes[i].y
                );
            }
            
            
            //XXXX
            
            // find the two outer walls
            var left  = walls.OrderBy(w => w.position.x).First();
            var right = walls.OrderByDescending(w => w.position.x).First();

            // grab their colliders so we know height
            var colL = left .GetComponent<BoxCollider2D>();
            var colR = right.GetComponent<BoxCollider2D>();

            // four local‐space points on each wall
            Vector2 localBL = colL.offset + new Vector2(0, -colL.size.y/2);
            Vector2 localTL = colL.offset + new Vector2(0,  colL.size.y/2);
            Vector2 localTR = colR.offset + new Vector2(0,  colR.size.y/2);
            Vector2 localBR = colR.offset + new Vector2(0, -colR.size.y/2);

            // transform them into world‐space
            Vector3 bottomLeft  = left .TransformPoint(localBL);
            Vector3 topLeft     = left .TransformPoint(localTL);
            Vector3 topRight    = right.TransformPoint(localTR);
            Vector3 bottomRight = right.TransformPoint(localBR);

            // build a quad
            foreach (var mesh in fillMeshes)
            {
                mesh.vertices  = new [] { bottomLeft, topLeft, topRight, bottomRight };
                mesh.triangles = new [] { 0,1,2, 0,2,3 };
                mesh.uv        = new [] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
                mesh.RecalculateBounds();
            }
            
            //XXXX

            yield return null;
        }

        // final snap to targets
        for (int i = 0; i < count; i++)
        {
            walls[i].position = targets[i];
            walls[i].rotation = targetRots[i];
        }

        // final background resize
        float finalMinX = walls.Min(w => w.position.x);
        float finalMaxX = walls.Max(w => w.position.x);
        float finalWidth = finalMaxX - finalMinX;
        for (int i = 0; i < bgRenderers.Count; i++)
        {
            var sr = bgRenderers[i];
            float scaleX = sr.transform.localScale.x;
            sr.size = new Vector2(
                finalWidth / scaleX,
                originalBgSizes[i].y
            );
        }
    }

    public override string GetDescription()
    {
        // movement part
        float minDist = wallMoveDistanceRange.x;
        float maxDist = wallMoveDistanceRange.y;
        string moveDesc;

        if (minDist == maxDist)
        {
            if (minDist > 0f)
                moveDesc = $"Move walls inward by {minDist:F1}m";
            else if (minDist < 0f)
                moveDesc = $"Move walls outward by {Mathf.Abs(minDist):F1}m";
            else
                moveDesc = ""; // no movement at all
        }
        else if (minDist > 0f && maxDist > 0f)
        {
            moveDesc = $"Move walls inward between {minDist:F1}–{maxDist:F1}m";
        }
        else if (minDist < 0f && maxDist < 0f)
        {
            // use absolute values for readability, ensure low→high
            float a = Mathf.Abs(maxDist), b = Mathf.Abs(minDist);
            moveDesc = $"Move walls outward between {a:F1}–{b:F1}m";
        }
        else
        {
            // mixed signs or zero: just state the raw range
            moveDesc = $"Move walls between {minDist:F1}–{maxDist:F1}m";
        }

        // rotation part (only if non-zero)
        string rotDesc = "";
        float minRot = wallRotationRange.x;
        float maxRot = wallRotationRange.y;
        if (minRot != 0f || maxRot != 0f)
        {
            if (minRot == maxRot)
                rotDesc = $" and rotate by {minRot:F0}°";
            else
                rotDesc = $" and rotate between {minRot:F0}°–{maxRot:F0}°";
        }

        // if there's no movement either (both ranges zero) you might want a default
        if (string.IsNullOrEmpty(moveDesc) && string.IsNullOrEmpty(rotDesc))
            return "No wall movement or rotation";

        return $"{moveDesc}{rotDesc}";
    }

}
