using UnityEngine;

public class BatteryPackPickup : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip itemPickupSound;

    [Header("Ammo Rewards")]
    public int smallPackBullets = 2;
    public int mediumPackBullets = 4;
    public int largePackBullets = 6;

    [Header("Probabilities")]
    [Range(0f, 1f)]
    public float smallPackChance = 0.50f;

    [Range(0f, 1f)]
    public float mediumPackChance = 0.35f;

    [Range(0f, 1f)]
    public float largePackChance = 0.15f;

    public KeyCode pickupKey = KeyCode.E;

    private RobotWeapon nearbyWeapon;

    private void Update()
    {
        if (nearbyWeapon == null)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        InteractionMessageUI.SetPrompt(
            this,
            "PICK UP BATTERY PACK",
            Color.white
        );

        if (Input.GetKeyDown(pickupKey))
            CollectBatteryPack();
    }

    private void CollectBatteryPack()
    {
        int bulletsBefore =
            nearbyWeapon.GetReserveBullets();

        int bulletsAwarded =
            GetRandomBulletReward();

        nearbyWeapon.AddAmmo(bulletsAwarded);

        int bulletsAfter =
            nearbyWeapon.GetReserveBullets();

        int bulletsAdded =
            bulletsAfter - bulletsBefore;

        if (bulletsAdded <= 0)
        {
            InteractionMessageUI.ShowNotification(
                "RESERVE AMMO FULL",
                Color.yellow
            );

            return;
        }

        InteractionMessageUI.ShowNotification(
            "PICKED UP: BATTERY PACK  +" +
            bulletsAdded +
            " AMMO",
            Color.cyan
        );

        InteractionMessageUI.ClearPrompt(this);

        PlayPickupSound();

        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (itemPickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                itemPickupSound,
                transform.position,
                1f
            );
        }
    }

    private int GetRandomBulletReward()
    {
        float randomValue =
            Random.value;

        if (randomValue < smallPackChance)
            return smallPackBullets;

        if (randomValue <
            smallPackChance + mediumPackChance)
        {
            return mediumPackBullets;
        }

        return largePackBullets;
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        SetNearbyWeapon(other);
    }

    private void OnTriggerStay2D(
        Collider2D other
    )
    {
        SetNearbyWeapon(other);
    }

    private void SetNearbyWeapon(
        Collider2D other
    )
    {
        RobotWeapon weapon =
            other.GetComponentInParent<
                RobotWeapon
            >();

        if (weapon != null)
            nearbyWeapon = weapon;
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        RobotWeapon weapon =
            other.GetComponentInParent<
                RobotWeapon
            >();

        if (weapon != null &&
            weapon == nearbyWeapon)
        {
            nearbyWeapon = null;
            InteractionMessageUI.ClearPrompt(this);
        }
    }

    private void OnDisable()
    {
        InteractionMessageUI.ClearPrompt(this);
    }
}
