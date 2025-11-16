using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 input;

    [Header("Combat")]
    public float attackDuration = 0.35f;
    private bool isAttacking = false;

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
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead || isTransforming) return;

        HandleMovement();
        HandleAttack();
        UpdateAnimator();

        // Transformar
        if (Input.GetKeyDown(KeyCode.T) && !isTransformed)
        {
            Transform();
        }

        // Voltar ao normal
        if (Input.GetKeyDown(KeyCode.G) && isTransformed)
        {
            RevertTransformation();
        }
    }

    // -------------------------------
    // MOVEMENT
    // -------------------------------
    private void HandleMovement()
    {
        if (isAttacking) return;

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(input.x, input.y, 0f).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Flip
        if (input.x > 0) sr.flipX = false;
        else if (input.x < 0) sr.flipX = true;
    }

    // -------------------------------
    // ATTACK
    // -------------------------------
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(attackDuration);

        anim.SetBool("IsAttacking", false);
        isAttacking = false;
    }

    // -------------------------------
    // TRANSFORMATION
    // -------------------------------
    public void Transform()
    {
        if (isTransforming || isDead) return;
        isTransforming = true;
        StartCoroutine(TransformRoutine(true));
    }

    public void RevertTransformation()
    {
        if (isTransforming || isDead) return;
        isTransforming = true;
        StartCoroutine(TransformRoutine(false));
    }

    private System.Collections.IEnumerator TransformRoutine(bool goingToTransformed)
    {
        // Muda parâmetro do Animator
        anim.SetBool("IsTransformed", goingToTransformed);
        isTransformed = goingToTransformed;

        // Piscar vermelho ao transformar
        // Piscar azul ao voltar ao normal
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

    // -------------------------------
    // HEALTH & DEATH
    // -------------------------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("IsDead", true);
        input = Vector2.zero;
    }

    // -------------------------------
    // ANIMATOR VALUES
    // -------------------------------
    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", input.magnitude);
    }
}
