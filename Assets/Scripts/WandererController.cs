using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandererController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 30;
    int currentHealth;

    public float detectionRange = 4f;
    public float attackRange = 0.4f;
    public float moveSpeed = 2f;
    public int damage = 10;

    [Header("Components")]
    Animator animator;
    Rigidbody2D rb;
    Transform player;
    SpriteRenderer sr;

    bool isDead = false;
    bool isAttacking = false;


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

    // ---------------------------------
    // IDLE
    // ---------------------------------
    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsAttacking", false);
    }

    // ---------------------------------
    // MOVEMENT / CHASE
    // ---------------------------------
    void ChasePlayer()
    {
        if (isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        animator.SetFloat("Speed", rb.velocity.magnitude);

        // Flip visual
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // ---------------------------------
    // ATTACK
    // ---------------------------------
    void AttackPlayer()
    {
        if (isAttacking) return;

        isAttacking = true;
        rb.velocity = Vector2.zero;

        animator.SetBool("IsAttacking", true);

        // Aplicar o dano sincronizado com o golpe da animação
        Invoke(nameof(DealDamage), 0.08f);  // Ajustar conforme a animação

        // Tempo total do ataque / cooldown
        Invoke(nameof(ResetAttack), 1f);
    }

    void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    void DealDamage()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Confirma se o Player ainda está no range do golpe
        if (dist <= attackRange + 0.4f)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(damage);
                Debug.Log("Wanderer causou dano ao Player!");
            }
        }
    }

    // ---------------------------------
    // DAMAGE TAKEN
    // ---------------------------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    System.Collections.IEnumerator HitFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    // ---------------------------------
    // DEATH
    // ---------------------------------
    void Die()
    {
        isDead = true;

        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }

    // ---------------------------------
    // GIZMOS
    // ---------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
