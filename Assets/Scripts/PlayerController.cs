using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Movement")]
    public float normalMoveSpeed = 1f;
    public float transformedMoveSpeed = 1.5f;
    private float moveSpeed;
    private Vector2 input;

    [Header("Combat")]
    public int normalDamage = 10;
    public int transformedDamage = 40;
    private int currentDamage;
    public float attackDuration = 0.35f;
    private bool isAttacking = false;

    [Header("Attack Detection")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    [Header("Transformation")]
    public bool isTransformed = false;
    public float blinkDuration = 1f;
    public int blinkCount = 6;
    private bool isTransforming = false;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        moveSpeed = normalMoveSpeed;
        currentDamage = normalDamage;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead || isTransforming) return;

        HandleMovement();
        HandleAttack();
        UpdateAnimator();

        if (Input.GetKeyDown(KeyCode.T) && !isTransformed)
            Transform();

        if (Input.GetKeyDown(KeyCode.G) && isTransformed)
            RevertTransformation();
    }

    // ============================
    // MOVEMENT
    // ============================
    private void HandleMovement()
    {
        if (isAttacking) return;

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(input.x, input.y, 0f).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Flip
        if (input.x > 0) sr.flipX = false;
        if (input.x < 0) sr.flipX = true;
    }

    // ============================
    // COMBAT
    // ============================
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(attackDuration * 0.4f);

        DealDamage(); // agora funciona com qualquer inimigo

        yield return new WaitForSeconds(attackDuration * 0.6f);

        anim.SetBool("IsAttacking", false);
        isAttacking = false;
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D hit in hits)
        {
            // Pega QUALQUER inimigo que implemente a interface
            IEnemyDamageable enemy = hit.GetComponent<IEnemyDamageable>();

            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // ============================
    // TRANSFORMATION
    // ============================
    public void Transform()
    {
        if (isTransforming || isDead) return;
        StartCoroutine(TransformRoutine(true));
    }

    public void RevertTransformation()
    {
        if (isTransforming || isDead) return;
        StartCoroutine(TransformRoutine(false));
    }

    private IEnumerator TransformRoutine(bool goingToTransformed)
    {
        isTransforming = true;
        anim.SetBool("IsTransformed", goingToTransformed);
        isTransformed = goingToTransformed;

        moveSpeed = goingToTransformed ? transformedMoveSpeed : normalMoveSpeed;
        currentDamage = goingToTransformed ? transformedDamage : normalDamage;

        Color blinkColor = goingToTransformed ? Color.red : Color.cyan;

        for (int i = 0; i < blinkCount; i++)
        {
            sr.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
            sr.color = Color.white;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
        }

        isTransforming = false;
    }

    // ============================
    // HEALTH
    // ============================
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("IsDead", true);
        input = Vector2.zero;
    }

    // ============================
    // ANIMATOR
    // ============================
    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", input.magnitude);
    }
}
