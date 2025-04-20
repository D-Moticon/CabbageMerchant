using UnityEngine;

public class GridBonkableSpawner : BonkableSlotSpawner
{
    [Header("Grid Settings")]
    public int rows = 3;
    public int columns = 5;
    public Vector2 spacing = new Vector2(1f, 1f);
    public Vector2 gridOrigin = Vector2.zero;

    private void Start()
    {
        SpawnBonkableSlots();
    }

    public override void SpawnBonkableSlots()
    {
        // Clear existing slots
        foreach (var slot in bonkableSlots)
            if (slot != null) Destroy(slot.gameObject);
        bonkableSlots.Clear();

        float totalW = (columns - 1) * spacing.x;
        float totalH = (rows - 1)    * spacing.y;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector2 offset = new Vector2(
                    c * spacing.x - totalW * 0.5f,
                    -r * spacing.y + totalH * 0.5f
                );
                Vector3 worldPos = transform.TransformPoint(gridOrigin + offset);

                // Create GameObject and BonkableSlot component
                var goObj = new GameObject("BonkableSlot");
                goObj.transform.SetParent(transform);
                goObj.transform.position = worldPos;
                var slot = goObj.AddComponent<BonkableSlot>();
                bonkableSlots.Add(slot);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float totalW = (columns - 1) * spacing.x;
        float totalH = (rows - 1)    * spacing.y;
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
        {
            Vector2 offset = new Vector2(
                c * spacing.x - totalW * 0.5f,
                -r * spacing.y + totalH * 0.5f
            );
            Vector3 worldPos = transform.TransformPoint(gridOrigin + offset);
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.5f);
        }
    }
}