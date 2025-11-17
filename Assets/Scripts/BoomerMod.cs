using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerMod : MonoBehaviour
{
    [Header("Boomer Settings")]
    public float widthMultiplier = 1.6f;   // Aumenta tamanho no eixo X
    public Color boomerColor = new Color(0.3f, 1f, 0.3f);

    [Header("Explosion Settings")]
    public float explosionRadius = 2.5f;
    public int explosionDamage = 20;

    [Header("Pulse Settings")]
    public float pulseThreshold = 0.25f; // 25% da vida
    public float pulseSpeed = 4f;        // velocidade da pulsação
    public float pulseAmount = 0.1f;     // o quanto aumenta/diminui

    private SpriteRenderer sr;
    private WandererController wc;
    private Vector3 baseScale;           // escala original já multiplicada
    private bool exploded = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        wc = GetComponent<WandererController>();

        // Escala inicial do Boomer (mais largo)
        baseScale = transform.localScale = new Vector3(
            transform.localScale.x * widthMultiplier,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    void Update()
    {
        if (sr != null)
            sr.color = boomerColor;

        // Ativa pulsação se estiver com pouca vida
        HandlePulseEffect();

        // Explosão ao morrer
        if (wc != null && wc.IsDead() && !exploded)
        {
            exploded = true;
            Explode();
        }
    }

    void HandlePulseEffect()
    {
        if (wc == null) return;

        float lifePercent = wc.GetLifePercent(); // NECESSÁRIO no WandererController

        if (lifePercent <= pulseThreshold && !wc.IsDead())
        {
            float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            transform.localScale = new Vector3(
                baseScale.x + scaleOffset,
                baseScale.y + scaleOffset,
                baseScale.z
            );
        }
        else
        {
            // Caso volte a vida maior que 25%, retorna ao normal
            transform.localScale = baseScale;
        }
    }

    void Explode()
    {
        Debug.Log("BOOMER EXPLODIU!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TakeDamage(explosionDamage);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
