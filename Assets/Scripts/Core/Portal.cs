using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform destinationPortal; // Diğer portalın Transform bileşeni
    private bool playerInsidePortal = false; // Oyuncunun portalın içinde olup olmadığını takip eder
    private bool canTeleport = true; // Geçiş yapılabilirlik durumu
    public float teleportCooldown = 2f; // Geçiş arasındaki bekleme süresi

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canTeleport)
            {
                playerInsidePortal = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsidePortal = false;
        }
    }

    private void Update()
    {
        if (playerInsidePortal && canTeleport) // E tuşuna basıldığında geçiş yapar
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            Vector3 destinationPosition = destinationPortal.position;
            Quaternion destinationRotation = destinationPortal.rotation;

            player.position = destinationPosition;
            player.rotation = destinationRotation;

            StartCoroutine(TeleportCooldown()); // Teleport bekleme süresini başlatır
        }
    }

    private IEnumerator TeleportCooldown()
    {
        canTeleport = false; // Geçiş yapılamaz durumda

        yield return new WaitForSeconds(teleportCooldown);

        canTeleport = true; // Geçiş yapılabilir duruma döner
    }
}
