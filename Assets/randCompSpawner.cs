using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randCompSpawner : MonoBehaviour
{
    public List<componenttemplate> allComponents;
    public List<terrain> allTerrains;
    public Dictionary<string, pack> allPacks;

    public static GameObject ghostPrefab;

    public static bool active;
    
    void Awake() {
        meta.Awake();
        metaData.Awake();
       
        allComponents = new List<componenttemplate>();
        allTerrains = new List<terrain>();
        allPacks = new Dictionary<string, pack>();
        
        active = true;

        pack[] packs = Resources.LoadAll<pack>("components/packs");
        foreach(pack p in packs) {
            foreach(componenttemplate c in p.getComponents()) {
                if (c.name == "")
                    continue;

                allComponents.Add(c);
            }

            foreach(terrain t in p.getTerrains()) {
                if (t.name == "")
                    continue;
                    
                allTerrains.Add(t);
            }

            allPacks.Add(p.name, p);
        }

        componentManager.allPacks = allPacks;
        componentManager.allComponents = allComponents;
        componentManager.allTerrains = allTerrains;

        ghostPrefab = Resources.Load<GameObject>("prefabs/ghost component");

        foreach (componenttemplate c in allComponents)
            if (c.packTexture == null && c.pack.Count > 1)
                c.createPackBackground();
    }

    void Start() {
        StartCoroutine(spawn());
    }

    private IEnumerator spawn() {
        yield return new WaitForSeconds(Random.Range(2, 3));

        ghostComponent c = Instantiate(ghostPrefab, transform).GetComponent<ghostComponent>();

        float what = Random.Range(0, 100);

        if (what < 1)
            c.terrain = allTerrains[Random.Range(0, allTerrains.Count)];
        else if (what < 5)
            c.pack = allPacks[allPacks.Keys.ToArray()[Random.Range(0, allPacks.Count)]];
        else
            c.component = allComponents[Random.Range(0, allComponents.Count)];

        float x;
        if (transform.childCount % 2 == 0)
            x = Random.Range(150, 460); 
        else
            x = Random.Range(2121, 2410);

        c.transform.position = (x, 1700f).v();
        c.speed = Random.Range(3f, 5f);
        c.rotationSpeed = Random.Range(1f, 3f);

        if (transform.childCount < 6)
            StartCoroutine(spawn());
    }
}
