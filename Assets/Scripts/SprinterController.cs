using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprinterController : MonoBehaviour, IEnemyDamageable
{
    [Header("Stats")]
    public int maxHealth = 20;
    private int currentHealth;

    public float detectionRange = 6f;     // maior que Wanderer
    public float attackRange = 0.45f;
    public float moveSpeed = 3.5f;        // bem mais rápido
    public int damage = 15;               // ataque mais forte

    public float attackCooldown = 0.55f;  // ataque rápido

    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;

    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
            AttackPlayer();
        else if (distance <= detectionRange)
            ChasePlayer();
        else
            Idle();
    }

    // ------------------ IDLE ------------------
    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsAttacking", false);
    }

    // ---------------- MOVE / CHASE -----------------
    void ChasePlayer()
    {
        if (isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;

        rb.velocity = direction * moveSpeed;
        animator.SetFloat("Speed", rb.velocity.magnitude);

        sr.flipX = direction.x < 0;
    }

    // ---------------- ATTACK -----------------
    void AttackPlayer()
    {
        if (isAttacking || isDead) return;

        isAttacking = true;
        rb.velocity = Vector2.zero;

        animator.SetBool("IsAttacking", true);

        Invoke(nameof(DealDamage), 0.06f);  // ataque bem rápido!
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    void DealDamage()
    {
        if (player == null || isDead) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange + 0.35f)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(damage);
        }
    }

    // ---------------- DAMAGE TAKEN -----------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.yellow;      // cor diferente do Wanderer
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    // ---------------- DEATH -----------------
    void Die()
    {
        isDead = true;

        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }

    // ---------------- GIZMOS -----------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
