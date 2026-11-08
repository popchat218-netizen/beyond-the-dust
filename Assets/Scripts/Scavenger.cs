using UnityEngine;

public sealed class Scavenger : MonoBehaviour
{
    [SerializeField] private float noticeRadius = 22f;
    [SerializeField] private float attackRadius = 2.1f;
    [SerializeField] private float moveSpeed = 2.6f;
    private Vector3 home;
    private float phase;
    private float attackTimer;

    private void Start()
    {
        home = transform.position;
        phase = Random.Range(0f, 6.28f);
    }

    private void Update()
    {
        if (DesertGame.Instance == null || DesertGame.Instance.Player == null ||
            DesertGame.Instance.IsDead || DesertGame.Instance.IsComplete)
            return;

        Transform player = DesertGame.Instance.Player;
        Vector3 flatPlayer = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, flatPlayer);
        Vector3 destination = distance < noticeRadius
            ? flatPlayer
            : home + new Vector3(Mathf.Sin(Time.time * 0.35f + phase) * 4f, 0f,
                Mathf.Cos(Time.time * 0.27f + phase) * 4f);

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction.normalized), Time.deltaTime * 3f);
            float speed = distance < attackRadius ? 0f : moveSpeed * (distance < noticeRadius ? 1.35f : 0.7f);
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        attackTimer -= Time.deltaTime;
        if (distance < attackRadius && attackTimer <= 0f)
        {
            attackTimer = 1.35f;
            DesertGame.Instance.TakeDamage(11f);
        }
    }
}
