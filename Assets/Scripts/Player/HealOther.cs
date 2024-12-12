using System.Collections;
using UnityEngine;

public class HealOther : MonoBehaviour
{
    [SerializeField] private float healTime = 5f;       // Time required to heal the survivor
    [SerializeField] private float escapeSpeed = 10f;   // Speed at which the survivor escapes
    private bool isHealed = false;
    private float healTimer = 0f;

    private Transform priestTransform;   // Reference to the Priest's position
    private Transform playerTransform;   // Reference to the Player's position

    private void Start()
    {
        // Find the Priest and Player by tag (assuming they are tagged appropriately)
        priestTransform = GameObject.FindGameObjectWithTag("Priest")?.transform;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // Only allow healing if this survivor has not been healed yet
        if (isHealed) return;

        // Check if the player is close enough and holding the heal key ('5')
        if (IsPlayerClose() && Input.GetKey(KeyCode.Alpha5))
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healTime)
            {
                HealSurvivor();
            }
        }
        else
        {
            // Reset the timer if the player moves away or releases the key
            healTimer = 0f;
        }
    }

    private bool IsPlayerClose()
    {
        // Check if player is within range of this specific survivor
        return Vector3.Distance(transform.position, playerTransform.position) <= 3f;
    }

    private void HealSurvivor()
    {
        isHealed = true;
        healTimer = 0f;

        Debug.Log($"{gameObject.name} is now healed and escaping."); // For debugging purposes

        // Start the survivor's escape behavior
        StartCoroutine(Escape());
    }

    private IEnumerator Escape()
    {
        Vector3 escapeDirection;

        // Determine escape direction as opposite of the Priest
        if (priestTransform != null)
        {
            escapeDirection = (transform.position - priestTransform.position).normalized;
        }
        else
        {
            // If the Priest is not found, escape in a random direction outside map bounds
            escapeDirection = Random.onUnitSphere;
            escapeDirection.y = 0; // Keep the escape direction on the horizontal plane
        }

        // Continuously move the Survivor NPC out of bounds
        while (true)
        {
            transform.position += escapeDirection * escapeSpeed * Time.deltaTime;

            // Optional: Add logic here to fade out the survivor for a disappearing effect

            yield return null;
        }
    }
}
