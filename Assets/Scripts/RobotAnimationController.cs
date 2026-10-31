using UnityEngine;

public class RobotAnimationController : MonoBehaviour
{
    public Animator animator;
    public GameObject gunVisual;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetGunVisible(bool visible)
    {
        if (gunVisual != null)
            gunVisual.SetActive(visible);

        if (animator != null)
            animator.SetBool("HasGun", visible);
    }

    public void PlayFireAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Fire");
    }

    public void PlayBackfireAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Backfire");
    }
}