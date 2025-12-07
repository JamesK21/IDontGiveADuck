using UnityEngine;

public class GoodDuck : BaseDuck
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();
    }

    #region Abstract Implementation

    protected override void OnClicked()
    {
        Debug.Log($"Good duck clicked! Awarded {pointValue} points");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }

        PlaySuccessEffects();

        // This will trigger the destroy animation & delayed destroy
        DestroyDuck();
    }

    protected override void OnLifetimeExpired()
    {
        Debug.Log("Good duck expired - player missed it!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckMissed(this);
        }
    }

    #endregion

    #region Virtual Overrides

    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        gameObject.tag = "GoodDuck";
    }

    #endregion

    #region Good Duck Specific Methods

    private void PlaySuccessEffects()
    {
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickGood(transform.position);
        }

        if (successTextPrefab != null)
        {
            Instantiate(successTextPrefab, transform.position, Quaternion.identity);
        }
    }

    #endregion
}
