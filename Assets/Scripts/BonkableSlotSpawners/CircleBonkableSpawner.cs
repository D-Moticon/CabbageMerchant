using UnityEngine;

public class CircleBonkableSpawner : BonkableSlotSpawner
{
    [Header("Circle Settings")]
    public int count = 8;
    public float radius = 2f;
    public float angularSpeed = 30f; // degrees per second

    private float angleOffset = 0f;

    private void Start()
    {
        SpawnBonkableSlots();
    }

    private void Update()
    {
        angleOffset += Time.deltaTime * angularSpeed;
        for (int i = 0; i < bonkableSlots.Count; i++)
        {
            float angle = angleOffset + i * 360f / count;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            bonkableSlots[i].transform.position = transform.TransformPoint(localPos);
        }
    }

    public override void SpawnBonkableSlots()
    {
        foreach (var slot in bonkableSlots)
            if (slot != null) Destroy(slot.gameObject);
        bonkableSlots.Clear();

        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            Vector3 worldPos = transform.TransformPoint(localPos);

            // Create GameObject and BonkableSlot component
            var goObj = new GameObject("BonkableSlot");
            goObj.transform.SetParent(transform);
            goObj.transform.position = worldPos;
            var slot = goObj.AddComponent<BonkableSlot>();
            bonkableSlots.Add(slot);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        // draw circle
        Gizmos.DrawWireSphere(transform.position, radius);
        // draw spawn points
        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = transform.TransformPoint(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius);
            Gizmos.DrawWireCube(pos, Vector3.one * 0.3f);
        }
    }
}
