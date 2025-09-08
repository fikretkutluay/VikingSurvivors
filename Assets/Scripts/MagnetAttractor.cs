using UnityEngine;

public class MagnetAttractor : MonoBehaviour
{
    [SerializeField] private bool isActive = false;
    [SerializeField] private float magnetRange = 3f;
    [SerializeField] private float magnetSpeed = 3f;

    public void Configure(float range, float speed)
    {
        magnetRange = range;
        magnetSpeed = speed;
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    void Update()
    {
        if (!isActive) return;

        GameObject[] xpItems = GameObject.FindGameObjectsWithTag("XPItem");
        foreach (GameObject item in xpItems)
        {
            if (item == null) continue;
            float distance = Vector2.Distance(transform.position, item.transform.position);
            if (distance <= magnetRange)
            {
                Vector3 direction = (transform.position - item.transform.position).normalized;
                item.transform.position += direction * magnetSpeed * Time.deltaTime;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}


