using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 movement;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRangeNormal = 0.5f;
    public float attackRangeTransformed = 1f;
    public LayerMask enemyLayers;
    public int damage = 20;
    private bool isAttacking;

    [Header("Transformation")]
    public bool isTransformed = false;
    public float transformFlashDuration = 0.08f;
    public int flashCount = 6;

    [Header("Colliders - Box")]
    public BoxCollider2D boxCollider; // optional: assign in inspector if you use BoxCollider2D
    public Vector2 normalBoxSize = new Vector2(1f, 2f);
    public Vector2 normalBoxOffset = Vector2.zero;
    public Vector2 transformedBoxSize = new Vector2(1.5f, 3f);
    public Vector2 transformedBoxOffset = new Vector2(0f, 0.5f);

    [Header("Colliders - Capsule")]
    public CapsuleCollider2D capsuleCollider; // optional: assign in inspector if you use CapsuleCollider2D
    public Vector2 normalCapsuleSize = new Vector2(1f, 2f);
    public Vector2 normalCapsuleOffset = Vector2.zero;
    public Vector2 transformedCapsuleSize = new Vector2(1.5f, 3f);
    public Vector2 transformedCapsuleOffset = new Vector2(0f, 0.5f);

    [Header("AttackPoint Offsets")]
    public Vector2 normalAttackPointOffset = new Vector2(0.7f, 0f);
    public Vector2 transformedAttackPointOffset = new Vector2(1.1f, 0.3f);

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    // internals
    private float currentAttackRange;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // If not assigned in inspector, try to find existing colliders
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();
        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        ApplyFormSettings(); // sets collider + attack range + attackPoint
    }

    private void Update()
    {
        if (isDead) return;

        // movement input (both axes)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // animator speed (use magnitude so diagonal movement counts)
        animator.SetFloat("Speed", movement.magnitude);

        // flip + attackPoint automatic flip
        if (movement.x > 0.1f) SetFlip(false);
        else if (movement.x < -0.1f) SetFlip(true);

        // Attack: Space
        if (Input.GetKeyDown(KeyCode.Space))
            Attack();

        // Transform: F -> transform; G -> revert
        if (Input.GetKeyDown(KeyCode.F) && !isTransformed)
        {
            StartCoroutine(TransformRoutine(true));
        }
        else if (Input.GetKeyDown(KeyCode.G) && isTransformed)
        {
            StartCoroutine(TransformRoutine(false));
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // -------------------------
    // ATTACK
    // -------------------------
    void Attack()
    {
        if (isAttacking || isDead) return;

        isAttacking = true;
        animator.SetBool("IsAttacking", true);

        float range = isTransformed ? attackRangeTransformed : attackRangeNormal;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<WandererController>(out var w))
                w.TakeDamage(damage);
        }

        StartCoroutine(AttackReset());
    }

    IEnumerator AttackReset()
    {
        yield return new WaitForSeconds(0.35f);
        animator.SetBool("IsAttacking", false);
        isAttacking = false;
    }

    // -------------------------
    // TRANSFORM / REVERT
    // -------------------------
    IEnumerator TransformRoutine(bool toTransformed)
    {
        // apply logical state immediately so collider/attackRange update before first frame
        isTransformed = toTransformed;
        animator.SetBool("IsTransformed", isTransformed);
        ApplyFormSettings();

        // choose flash color
        Color flashColor = toTransformed ? Color.red : Color.cyan;

        for (int i = 0; i < flashCount; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(transformFlashDuration);
            sr.color = Color.white;
            yield return new WaitForSeconds(transformFlashDuration);
        }
    }

    // -------------------------
    // APPLY FORM SETTINGS
    // -------------------------
    void ApplyFormSettings()
    {
        // attack range
        currentAttackRange = isTransformed ? attackRangeTransformed : attackRangeNormal;

        // attack point offset (respect current flip)
        bool flipped = sr.flipX;
        Vector2 offset = isTransformed ? transformedAttackPointOffset : normalAttackPointOffset;
        attackPoint.localPosition = new Vector3(offset.x * (flipped ? -1 : 1), offset.y, attackPoint.localPosition.z);

        // collider adjustments (support Box AND Capsule)
        if (boxCollider != null)
        {
            if (isTransformed)
            {
                boxCollider.size = transformedBoxSize;
                boxCollider.offset = transformedBoxOffset;
            }
            else
            {
                boxCollider.size = normalBoxSize;
                boxCollider.offset = normalBoxOffset;
            }
        }

        if (capsuleCollider != null)
        {
            if (isTransformed)
            {
                capsuleCollider.size = transformedCapsuleSize;
                capsuleCollider.offset = transformedCapsuleOffset;
            }
            else
            {
                capsuleCollider.size = normalCapsuleSize;
                capsuleCollider.offset = normalCapsuleOffset;
            }
        }
    }

    // -------------------------
    // FLIP
    // -------------------------
    void SetFlip(bool flip)
    {
        sr.flipX = flip;
        // reapply attackPoint X with proper sign
        Vector2 offset = isTransformed ? transformedAttackPointOffset : normalAttackPointOffset;
        attackPoint.localPosition = new Vector3(offset.x * (flip ? -1 : 1), offset.y, attackPoint.localPosition.z);
    }

    // -------------------------
    // DAMAGE
    // -------------------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        Color orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.12f);
        sr.color = orig;
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;

        if (boxCollider != null) boxCollider.enabled = false;
        if (capsuleCollider != null) capsuleCollider.enabled = false;

        rb.isKinematic = true;
    }

    // -------------------------
    // GIZMOS
    // -------------------------
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = isTransformed ? Color.red : Color.green;
        float range = isTransformed ? attackRangeTransformed : attackRangeNormal;
        Gizmos.DrawWireSphere(attackPoint.position, range);

        // collider preview
        Gizmos.matrix = transform.localToWorldMatrix;

        if (boxCollider != null)
        {
            Gizmos.color = isTransformed ? new Color(0, 0, 1, 0.35f) : new Color(1, 1, 0, 0.35f);
            Vector2 size = isTransformed ? transformedBoxSize : normalBoxSize;
            Vector2 offset = isTransformed ? transformedBoxOffset : normalBoxOffset;
            Gizmos.DrawCube(offset, size);
        }
        else if (capsuleCollider != null)
        {
            Gizmos.color = isTransformed ? new Color(0, 0, 1, 0.35f) : new Color(1, 1, 0, 0.35f);
            Vector2 size = isTransformed ? transformedCapsuleSize : normalCapsuleSize;
            Vector2 offset = isTransformed ? transformedCapsuleOffset : normalCapsuleOffset;
            Gizmos.DrawCube(offset, size);
        }
    }
}
