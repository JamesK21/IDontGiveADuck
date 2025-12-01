using UnityEngine;

/// <summary>
/// Good duck that players should click for points
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs/// </summary>
public class GoodDuck : BaseDuck
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Score")] public int scoreValue = 10; // this is the score for when clicking ducks

    [Header("VFX")] public ParticleSystem hitEffect;  // refrence to particles for when clickung ducks

    [Header("SFX")] public AudioClip hitSound; // sound when clicking dcuk 

    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isHit = false;

    protected override void Start()
    {
        base.Start();
        
    }
    
    #region Abstract Implementation
    
    protected override void OnClicked()
    {
        Debug.Log($"Good duck clicked! Awarded {pointValue} points");
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }
        
        // Play success feedback
        PlaySuccessEffects();
        
        // Destroy duck
        DestroyDuck();
    }
    
    protected override void OnLifetimeExpired()
    {
        Debug.Log("Good duck expired - player missed it!");
        
        // Notify game manager about missed duck
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckMissed(this);
        }
        
        // No special effects for missed ducks
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Good duck specific spawn behaviour
        // Could add spawn animation, sound, etc.
        
        // Ensure proper tag for identification
        gameObject.tag = "GoodDuck";
    }
    
    protected override void OnLifetimeLow()
    {
        base.OnLifetimeLow();
        // Could add sprite swap or animation here if needed
    }
    
    #endregion
    
    #region Good Duck Specific Methods
    
    /// <summary>
    /// Play success effects when clicked
    /// </summary>
    private void PlaySuccessEffects()
    {
        // Particle effect
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Sound effect - use AudioManager for consistency
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickGood(transform.position);
        }
        
        // Floating score text (optional)
        if (successTextPrefab != null)
        {
            GameObject scoreText = Instantiate(successTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale; // <----
    }

    void OnMouseDown()
    {
        if (isHit) return;   // avoid double-click spam
        isHit = true;

        // 1. Animation
        StartCoroutine(ClickAnimation());

        // 2. Particles
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // sound 
        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitSound);
        }
        // Score for Score text that appears
        GameManager.Instance.AddScore(scoreValue, transform.position);

        
        Destroy(gameObject, 0.2f);// destroy duck after period of time when clicked

    }


    private System.Collections.IEnumerator ClickAnimation()
    {
        Vector3 big = originalScale * 1.2f;
        float duration = 0.1f;
        float t = 0f;

        // Scale change
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            transform.localScale = Vector3.Lerp(originalScale, big, lerp);
            yield return null;
        }

        // Scale back to normal
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            transform.localScale = Vector3.Lerp(big, originalScale, lerp);
            yield return null;
        }
    }
    #endregion

}