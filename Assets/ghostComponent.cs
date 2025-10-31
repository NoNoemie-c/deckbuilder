using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ghostComponent : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;

    public pack pack;
    public componenttemplate component;
    public terrain terrain;

    private TextMeshProUGUI Name, cranks, coins, strength;
    private Image cranksImg, coinsImg, strengthImg;
    [NonSerialized] public new Image renderer;
    private Image glimmer;

    [NonSerialized] public bool isPowered;
    [NonSerialized] public bool elec;

    [NonSerialized] public bool trail;
    [NonSerialized] public Vector2 trailStart, trailEnd;
    private int trailFlags;

    void Start() {
        TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        Image[] imgs = GetComponentsInChildren<Image>(true);
        LineRenderer line = GetComponent<LineRenderer>();

        foreach (TextMeshProUGUI t in texts)
            t.enabled = false;
        foreach (Image img in imgs)
            img.enabled = false;

        renderer = Array.Find(imgs, t => t.name == "description");
        Name = Array.Find(texts, t => t.name == "name");
        coins = Array.Find(texts, t => t.name == "coins");
        coinsImg = Array.Find(imgs, t => t.name == "coinImg");
        cranks = Array.Find(texts, t => t.name == "crank");
        cranksImg = Array.Find(imgs, t => t.name == "crankImg");
        strength = Array.Find(texts, t => t.name == "strength");
        strengthImg = Array.Find(imgs, t => t.name == "strengthImg");
        glimmer = Array.Find(imgs, t => t.name == "glimmer");

        Image Pack = Array.Find(imgs, t => t.name == "pack");

        if (pack != null) {
            Name.enabled = true;
            Name.text = pack.name + " pack";
            Name.color = pack.color;

            renderer.enabled = true;
            renderer.sprite = pack.image;
        } else if (component != null) {
            renderer.enabled = true;
            renderer.sprite = component.symbol;

            Pack.enabled = true;
            if (component.pack.Count == 1) {
                Pack.sprite = Resources.Load<Sprite>("images/vfx/packTexture");
                Pack.color = componentManager.allPacks[component.pack[0]].color;
            } else
                Pack.sprite = component.packTexture;

            Name.enabled = true;
            Name.text = component.name;
            if (component.rarity < metaData.maxRarity)
                Name.color = metaData.RarityColors[component.rarity];
            else
                Name.color = Color.gray;

            coinsImg.enabled = coins.enabled = true;
            coins.text = component.coin.ToString();

            strength.enabled = strengthImg.enabled = component.strength != 0;
            strength.text = component.strength.ToString();

            cranks.enabled = cranksImg.enabled = component.crank != 0;
            cranks.text = component.crank.ToString();

            glimmer.enabled = component.conductive;
        } else {
            renderer.enabled = true;
            renderer.sprite = terrain.symbol;

            Pack.enabled = true;
            Pack.sprite = null;
            Pack.color = terrain.color;

            Name.enabled = true;
            Name.text = terrain.name;

            glimmer.enabled = terrain.conductive;
        }
    }

    //------graphics------

    void Update() {
        if (pack != null && pack.name == "electric" && !elec) {
            elec = true;

            float a = UnityEngine.Random.Range(0, 60), b = UnityEngine.Random.Range(100, 160);
            Vector2 A = new Vector2(a, a) - Vector2.one * 80, B = new Vector2(b, b) - Vector2.one * 80;
            Instantiate(meta.elecPrefab, transform.position, Quaternion.identity, transform).GetComponent<elecUI>().SetPos(A, B);

            return;
        }

        if (component != null && component.electricGenerator && !elec) {
            elec = true;

            float a = UnityEngine.Random.Range(0, 60), b = UnityEngine.Random.Range(100, 160);
            Vector2 A = new Vector2(a, a) - Vector2.one * 80, B = new Vector2(b, b) - Vector2.one * 80;
            Instantiate(meta.elecPrefab, transform.position, Quaternion.identity, transform).GetComponent<elecUI>().SetPos(A, B);
        } else if (terrain != null && terrain.electricGenerator && !elec) {
            elec = true;

            float a = UnityEngine.Random.Range(0, 60), b = UnityEngine.Random.Range(100, 160);
            Vector2 A = new Vector2(a, a) - Vector2.one * 80, B = new Vector2(b, b) - Vector2.one * 80;
            Instantiate(meta.elecPrefab, transform.position, Quaternion.identity, transform).GetComponent<elecUI>().SetPos(A, B);
        }

        if (GetComponentInChildren<elecUI>(true) == null)
            elec = false;
    }

    void FixedUpdate() {
        transform.position += (0f, -speed, 0f).v();
        transform.Rotate(Vector3.forward, rotationSpeed);

        if (component != null && component.conductive) {
            glimmer.transform.localPosition += ((Vector2.down + Vector2.right).normalized * 1f).V3();
            if (glimmer.transform.localPosition.x > 160)
                glimmer.transform.localPosition = new Vector2(-160, 160);
        } else if (terrain != null && terrain.conductive) {
            glimmer.transform.localPosition += ((Vector2.down + Vector2.right).normalized * 1f).V3();
            if (glimmer.transform.localPosition.x > 160)
                glimmer.transform.localPosition = new Vector2(-160, 160);
        }

        if (transform.position.y < -100) {
            pack = null;
            component = null;
            terrain = null;

            int what = UnityEngine.Random.Range(0, 100);
            
            if (what < 1)
                terrain = componentManager.allTerrains[UnityEngine.Random.Range(0, componentManager.allTerrains.Count)];
            else if (what < 5)
                pack = componentManager.allPacks[componentManager.allPacks.Keys.ToArray()[UnityEngine.Random.Range(0, componentManager.allPacks.Count)]];
            else
                component = componentManager.allComponents[UnityEngine.Random.Range(0, componentManager.allComponents.Count)];

            float x;
            if (transform.childCount % 2 == 0)
                x = UnityEngine.Random.Range(150, 460); 
            else
                x = UnityEngine.Random.Range(2121, 2410);

            transform.position = (x, 1700f).v();
            speed = UnityEngine.Random.Range(3f, 5f);
            rotationSpeed = UnityEngine.Random.Range(1f, 3f);

            Start();
        }
    }
}
