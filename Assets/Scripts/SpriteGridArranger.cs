using UnityEngine;

[ExecuteAlways]
public class SpriteGridArranger : MonoBehaviour
{
    [Tooltip("Desired size of each child in world units before any down-scaling.")]
    public Vector2 initialCellSize = new Vector2(1f, 1f);

    [Tooltip("Max total width & height the grid is allowed to occupy.")]
    public Vector2 gridSize        = new Vector2(10f, 10f);

    [Tooltip("Gap between cells in world units.")]
    public Vector2 spacing         = new Vector2(0.5f, 0.5f);

    // Whenever you tweak in inspector or add/remove in editor/play
    void OnValidate()                  => ArrangeGrid();
    void OnEnable()                   => ArrangeGrid();
    void OnTransformChildrenChanged() => ArrangeGrid();

    // End‐of‐frame catch-all (so even a last-second AddChild gets included)
    void LateUpdate()
    {
        ArrangeGrid();
    }

    void ArrangeGrid()
    {
        int count = transform.childCount;
        if (count == 0) return;

        // 1) nearly‐square layout
        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / cols);

        // 2) "ideal" full‐grid size
        float fullW = initialCellSize.x * cols + spacing.x * (cols - 1);
        float fullH = initialCellSize.y * rows + spacing.y * (rows - 1);

        // 3) uniform scale so it never overflows gridSize
        float scale = Mathf.Min(1f, gridSize.x / fullW, gridSize.y / fullH);

        // 4) actual extents after scaling
        Vector2 cellSize = initialCellSize * scale;
        float actualW = cellSize.x * cols + spacing.x * (cols - 1);
        float actualH = cellSize.y * rows + spacing.y * (rows - 1);

        // 5) center that box on (0,0)
        float startX = -actualW * 0.5f + cellSize.x * 0.5f;
        float startY =  actualH * 0.5f - cellSize.y * 0.5f;

        // 6) position & scale every child
        for (int i = 0; i < count; i++)
        {
            Transform c = transform.GetChild(i);
            c.localScale = Vector3.one * scale;

            int row = i / cols;
            int col = i % cols;
            float x = startX + col * (cellSize.x + spacing.x);
            float y = startY - row * (cellSize.y + spacing.y);

            c.localPosition = new Vector3(x, y, c.localPosition.z);
        }
    }

    // Draw the *actual* grid‐bounds where sprites will sit
    void OnDrawGizmosSelected()
    {
        int count = transform.childCount;
        if (count == 0) return;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / cols);

        float fullW = initialCellSize.x * cols + spacing.x * (cols - 1);
        float fullH = initialCellSize.y * rows + spacing.y * (rows - 1);

        float scale = Mathf.Min(1f, gridSize.x / fullW, gridSize.y / fullH);

        Vector2 cellSize = initialCellSize * scale;
        float actualW = cellSize.x * cols + spacing.x * (cols - 1);
        float actualH = cellSize.y * rows + spacing.y * (rows - 1);

        Gizmos.color  = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(actualW, actualH, 0f));
    }
}
