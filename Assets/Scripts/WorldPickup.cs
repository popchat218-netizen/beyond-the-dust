using UnityEngine;

public sealed class WorldPickup : MonoBehaviour
{
    public enum PickupKind { Salvage, Water, Beacon }

    public PickupKind Kind;
    public int Amount = 1;
    public bool IsCollected { get; private set; }

    private Vector3 startPosition;
    private float phase;

    private void Start()
    {
        startPosition = transform.position;
        phase = Random.Range(0f, 6.28f);
    }

    private void Update()
    {
        if (IsCollected)
            return;
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * 1.5f + phase) * 0.12f);
        transform.Rotate(Vector3.up, 28f * Time.deltaTime, Space.World);
    }

    public void Collect()
    {
        if (IsCollected)
            return;
        IsCollected = true;
        Destroy(gameObject);
    }
}
