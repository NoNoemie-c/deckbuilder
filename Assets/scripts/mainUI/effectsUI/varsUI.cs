using TMPro;
using UnityEngine;

public class varsUI : MonoBehaviour
{
    public Vector3 speed;
    public int timer;
    public bool explodeAtTheEnd;
    public int amount;
    public AnimationCurve alpha;

    void FixedUpdate() {
        timer --;
        speed *= .75f;

        TextMeshProUGUI t = GetComponent<TextMeshProUGUI>();
        t.color = new Color(t.color.r, t.color.g, t.color.b, alpha.Evaluate((100f - timer) / 100f));

        if (timer == 0) {
            if (explodeAtTheEnd) {
                var.coins += amount;
                Instantiate(meta.coinplosionPrefab, transform.position, Quaternion.AngleAxis(-90, Vector3.forward)).GetComponent<coinplosionUI>().amount = amount;
            }
            Destroy(gameObject);
        } else
            transform.position += speed;
    }
}
