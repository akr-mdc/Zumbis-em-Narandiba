using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandererController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 30;
    int currentHealth;

    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float moveSpeed = 2f;
    public int damage = 10;

    [Header("Components")]
    Animator animator;
    Rigidbody2D rb;
    Transform player;

    bool isDead = false;
    bool isAttacking = false;


    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else if (distance <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }
    }


    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsAttacking", false);
    }

    void ChasePlayer()
    {
        if (isAttacking) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        animator.SetFloat("Speed", rb.velocity.magnitude);

        // Flip horizontal
        if (dir.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (dir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void AttackPlayer()
    {
        if (isAttacking) return;

        rb.velocity = Vector2.zero;

        isAttacking = true;
        animator.SetBool("IsAttacking", true);

        Invoke(nameof(ResetAttack), 1f); // tempo da animação
    }

    void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    // ======================================
    // MÉTODO PARA CAUSAR DANO AO PLAYER
    // ======================================
    public void DamagePlayer()
    {
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(damage);
            }
        }
    }

    void Die()
    {
        isDead = true;

        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }


    System.Collections.IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        sr.color = Color.white;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
