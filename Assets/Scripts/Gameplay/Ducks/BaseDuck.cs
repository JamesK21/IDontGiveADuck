using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseDuck : MonoBehaviour
{
    [Header("Base Duck Properties")]
    [SerializeField] protected int pointValue = 1;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected float moveSpeed = 0f;

    [Header("Visual Feedback")]
    [SerializeField] protected ParticleSystem destroyEffect;
    [SerializeField] protected AudioClip clickSound;

    [Header("Animation Timing")]
    [SerializeField] protected float destroyAnimationDelay = 0.3f; // how long the destroy anim lasts

    protected float currentLifetime;
    protected bool isClicked = false;
    protected bool isInitialized = false;

    protected Animator animator;

    public int PointValue => pointValue;
    public bool IsClicked => isClicked;

    #region Unity Lifecycle

    protected virtual void Start()
    {
        Initialize();
        AutoFitCollider();

        animator = GetComponent<Animator>();
    }

    private void AutoFitCollider()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer != null && boxCollider != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            boxCollider.size = spriteSize;
            boxCollider.offset = Vector2.zero;
        }
        else
        {
            Debug.LogWarning($"Could not auto-fit collider for {gameObject.name} - missing components");
        }
    }

    protected virtual void Update()
    {
        if (!isInitialized) return;

        HandleLifetime();
        HandleMovement();
        HandleClickDetection();
    }

    private void HandleClickDetection()
    {
        if (isClicked) return;

        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                RegisterClick();
            }
        }
    }

    protected virtual void OnMouseDown()
    {
        if (!isClicked && isInitialized)
        {
            RegisterClick();
        }
    }

    private void RegisterClick()
    {
        isClicked = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        OnClicked(); // child handles score, sounds, etc., then calls DestroyDuck()
    }

    #endregion

    #region Initialization

    public virtual void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        currentLifetime = customLifetime > 0 ? customLifetime : lifetime;
        if (customPointValue > 0) pointValue = customPointValue;

        isInitialized = true;
        OnDuckSpawned();
    }

    #endregion

    #region Core Behaviours

    protected virtual void HandleLifetime()
    {
        currentLifetime -= Time.deltaTime;

        if (currentLifetime <= 0 && !isClicked)
        {
            OnLifetimeExpired();
            DestroyDuck();
        }
    }

    protected virtual void HandleMovement()
    {
        // no base movement
    }

    protected virtual void DestroyDuck()
    {
        // trigger destroy animation
        if (animator != null)
        {
            animator.SetTrigger("Destroy");
        }

        // particles
        if (destroyEffect != null)
        {
            ParticleSystem effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // sound
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }

        // wait a bit so the destroy anim can play
        float delay = animator != null ? destroyAnimationDelay : 0f;
        Destroy(gameObject, delay);
    }

    #endregion

    #region Abstract / Virtual Methods

    // now just a hook for children; no default animation here
    protected virtual void OnClicked()
    {
        // children (GoodDuck/DecoyDuck) do their own logic and then call DestroyDuck()
    }

    protected abstract void OnLifetimeExpired();

    protected virtual void OnDuckSpawned()
    {
        Debug.Log($"{GetType().Name} spawned at {transform.position} with {currentLifetime}s lifetime");
    }

    #endregion

    #region Debug

    protected virtual void OnDrawGizmos()
    {
        if (Application.isPlaying && isInitialized)
        {
            float lifetimePercent = currentLifetime / lifetime;
            Gizmos.color = Color.Lerp(Color.red, Color.green, lifetimePercent);
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
    }

    #endregion
}
