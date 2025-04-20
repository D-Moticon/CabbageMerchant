using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SineBonkableSpawner : BonkableSlotSpawner
{
    [Header("Sine Settings")]
    public int count = 10;
    public float spacing = 1f;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float speed = 1f;
    [Tooltip("Phase offset in radians to shift the wave")]
    public float phaseOffset = 0f;

    private float phase = 0f;

    private void Start()
    {
        phase = 0f;
        SpawnBonkableSlots();
    }

    private void Update()
    {
        phase += Time.deltaTime * speed;
        float totalSpan = (count - 1) * spacing;
        for (int i = 0; i < bonkableSlots.Count; i++)
        {
            var slot = bonkableSlots[i];
            float x = -totalSpan * 0.5f + i * spacing;
            Vector3 basePos = transform.TransformPoint(new Vector3(x, 0f, 0f));
            float y = Mathf.Sin((phase + phaseOffset + i) * frequency) * amplitude;
            slot.transform.position = basePos + Vector3.up * y;
        }
    }

    public override void SpawnBonkableSlots()
    {
        foreach (var slot in bonkableSlots)
            if (slot != null) Destroy(slot.gameObject);
        bonkableSlots.Clear();

        float totalSpan = (count - 1) * spacing;
        for (int i = 0; i < count; i++)
        {
            float x = -totalSpan * 0.5f + i * spacing;
            Vector3 worldPos = transform.TransformPoint(new Vector3(x, 0f, 0f));

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
        Gizmos.color = Color.green;
        float totalSpan = (count - 1) * spacing;
        // Draw base positions
        for (int i = 0; i < count; i++)
        {
            float x = -totalSpan * 0.5f + i * spacing;
            Vector3 pos = transform.TransformPoint(new Vector3(x, 0f, 0f));
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
        // Draw sine envelope
        int steps = count * 10;
        Vector3 prev = transform.TransformPoint(new Vector3(-totalSpan * 0.5f, 0f, 0f));
        for (int s = 1; s <= steps; s++)
        {
            float t = (float)s / steps * totalSpan;
            float x = -totalSpan * 0.5f + t;
            float y = Mathf.Sin((phaseOffset + t / spacing) * frequency) * amplitude;
            Vector3 curr = transform.TransformPoint(new Vector3(x, y, 0f));
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
    }
}