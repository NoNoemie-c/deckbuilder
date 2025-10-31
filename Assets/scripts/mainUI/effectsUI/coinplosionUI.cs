using UnityEngine;

public class coinplosionUI : MonoBehaviour
{
    public int amount;
    private ParticleSystem ps;
    
    void Start() {
        ps = GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[]{new ParticleSystem.Burst(0, amount)});
        ps.Play(true);
    }

    void Update() {
        if (GetComponent<ParticleSystem>().isStopped) 
            Destroy(gameObject);
    }
}
