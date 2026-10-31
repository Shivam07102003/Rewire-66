using System.Collections;
using UnityEngine;

public class WeaponVisualEffects : MonoBehaviour
{
    [Header("References")]
    public Transform gunVisual;
    public GameObject muzzleFlash;
    public GameObject backfireFlash;

    [Header("Animation")]
    public RobotAnimationController animationController;
    public float flashDuration = 0.12f;
    public float recoilDistance = 0.08f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip backfireSound;

    private Vector3 originalGunPosition;
    private Coroutine currentEffect;

    private void Awake()
    {
        if (gunVisual != null)
            originalGunPosition = gunVisual.localPosition;

        if (animationController == null)
            animationController =
                GetComponent<RobotAnimationController>();

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        if (backfireFlash != null)
            backfireFlash.SetActive(false);
    }

    public void PlayFireEffect()
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect =
            StartCoroutine(FireEffectRoutine());
    }

    public void PlayBackfireEffect()
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect =
            StartCoroutine(BackfireEffectRoutine());
    }

    private IEnumerator FireEffectRoutine()
    {
        if (animationController != null)
            animationController.PlayFireAnimation();

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (muzzleFlash != null)
            muzzleFlash.SetActive(true);

        ApplyRecoil();

        yield return new WaitForSeconds(flashDuration);

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        ResetGunPosition();
        currentEffect = null;
    }

    private IEnumerator BackfireEffectRoutine()
    {
        if (animationController != null)
            animationController.PlayBackfireAnimation();

        if (audioSource != null && backfireSound != null)
            audioSource.PlayOneShot(backfireSound);

        if (backfireFlash != null)
            backfireFlash.SetActive(true);

        ApplyRecoil();

        yield return new WaitForSeconds(flashDuration);

        if (backfireFlash != null)
            backfireFlash.SetActive(false);

        ResetGunPosition();
        currentEffect = null;
    }

    private void ApplyRecoil()
    {
        if (gunVisual == null)
            return;

        Vector3 barrelDirection =
            gunVisual.localRotation * Vector3.right;

        gunVisual.localPosition =
            originalGunPosition -
            barrelDirection * recoilDistance;
    }

    private void ResetGunPosition()
    {
        if (gunVisual != null)
            gunVisual.localPosition = originalGunPosition;
    }

    private void OnDisable()
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        if (backfireFlash != null)
            backfireFlash.SetActive(false);

        ResetGunPosition();
    }
}