using UnityEngine;

public class ParasiteHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 2;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private int currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    private void Die()
    {
        isDead = true;

        if (deathSound != null)
        {
            Vector3 soundPosition =
                Camera.main != null
                    ? Camera.main.transform.position
                    : transform.position;

            AudioSource.PlayClipAtPoint(
                deathSound,
                soundPosition,
                1f
            );
        }

        Destroy(gameObject);
    }
}