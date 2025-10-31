using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class shop : MonoBehaviour
{
    public static component[] proposal; 
    public static List<componenttemplate> possibilities;
    public Vector3[] pos;
    public static Vector3[] Pos;
    public static int prevRarity;
    public static Transform _Transform;
    public static List<string> packs;
    [SerializeField] private float Speed = 0;
    public static float speed;
    public static AnimationCurve luckCurve;
    [SerializeField] private AnimationCurve _luckCurve = new AnimationCurve();
    private float maxPos;
    public static bool rolling;

    void Awake() {
        _Transform = transform;
        luckCurve = _luckCurve;
        Pos = pos;

        packs = new List<string>();

        speed = Speed;

        proposal = new component[3];
        int i  = 0;
        foreach (component c in _Transform.GetComponentsInChildren<component>()) {
            proposal[i] = c;
            i ++;
        }

        rolling = false;
        GetComponent<RectMask2D>().enabled = false;
    }

    public static IEnumerator roll(int amount, int reroll = 0, bool skip = false) {
        rolling = true;

        lineGraphic line = _Transform.GetComponent<lineGraphic>();
        line.points = new List<Vector2>{(-320f, 80f).v(), (-320f, -80f).v(), (400f, -80f).v(), (400f, 80f).v()};
        if (reroll != 0)
            line.color = metaData.RarityColors[prevRarity];
        else
            prevRarity = 0;

        possibilities = new List<componenttemplate>();
        List<componenttemplate> l = componentManager.allComponents;
        if (componentManager.actual <= 2 && clicker.tuto == 2) 
            l = l.FindAll(c => new string[]{"investor", "credit card", "temporary money", "loan", "coin component", "bank"}.Contains(c.name));
        foreach (componenttemplate temp in l) {
            if (temp.name == "")
                continue;

            for (int i = 1; i < (metaData.maxRarity - temp.rarity) * 10; i ++)
                possibilities.Add(temp);

            if (packs.ContainsRange(temp.pack))
                for (int i = 0; i < metaData.maxRarity - temp.rarity; i ++)
                    possibilities.Add(temp);

            foreach (component c in componentManager.GetAll().FindAll(c => c.name == "mycologist"))
                if (componentManager.hasComponent(comp => temp == comp))
                    possibilities.Add(temp);
        }

        if (componentManager.AnimTime == 0) {
            foreach (component c in proposal)
                Destroy(c.gameObject);

            for (int i = 0; i < amount; i++) {
                coroutiner.start(clicker.ScreenShake(2));
                coroutiner.start(metaData.sounds["shopRollEnd"].play());
                component c = proposal[i] = addComponent(randomise(i, reroll * prevRarity, clicker.tuto == 2), i);
                c.GetComponentInChildren<RightClick>(true).enabled = true;
                c.GetComponentInChildren<Button>(true).enabled = false;

                prevRarity = Mathf.Max(prevRarity, proposal[i].template.rarity);
            }

            rolling = false;

            line.color = Color.white;

            yield break;
        }

        _Transform.GetComponent<RectMask2D>().enabled = true;

        for (int i = 0; i < amount; i++) {
            proposal[i].speed = speed;
            yield return new WaitForSeconds(componentManager.AnimTime * .2f);

            proposal[i] = Instantiate(meta.componentPrefab, _Transform.TransformPoint(Pos[i] + Vector3.up * 160), Quaternion.identity, _Transform).GetComponent<component>();
            proposal[i].template = randomise(i, reroll * prevRarity);
            proposal[i].GetComponentInChildren<RightClick>(true).enabled = false;
            proposal[i].speed = (12.4f - 1f / 10f * speed) * .5f / componentManager.AnimTime;
        }

        for (int j = 1; j < 8; j++) {
            for (int i = 0; i < amount; i++) {
                yield return new WaitUntil(() => proposal[i].transform.localPosition.y < 0);
                yield return new WaitForSeconds(.02f);
                proposal[i] = Instantiate(meta.componentPrefab, _Transform.TransformPoint(Pos[i] + Vector3.up * 160), Quaternion.identity, _Transform).GetComponent<component>();
                proposal[i].template = randomise(i, reroll * prevRarity, j == 7 && clicker.tuto == 2);
                proposal[i].GetComponentInChildren<RightClick>(true).enabled = false;
                /*if (reroll != 0)
                    Array.Find(proposal[i].GetComponentsInChildren<Image>(), I => I.name == "roll background").color = metaData.rerollColor;
                else if (skip)
                    Array.Find(proposal[i].GetComponentsInChildren<Image>(), I => I.name == "roll background").color = metaData.skipColor;*/
                proposal[i].speed = (12.4f - (float)j) / 10f * speed * (reroll != 0 ? 1.5f : 1) * .5f / componentManager.AnimTime;
            }

            foreach (component c in _Transform.GetComponentsInChildren<component>(true))
                if (c.transform.localPosition.y < -160)
                    Destroy(c.gameObject);
        }

        for (int i = 0; i < amount; i++) {
            yield return new WaitUntil(() => proposal[i].transform.localPosition.y < 0);
            coroutiner.start(clicker.ScreenShake(2));
            coroutiner.start(metaData.sounds["shopRollEnd"].play());
            component c = addComponent(proposal[i].template, i);
            c.GetComponentInChildren<RightClick>(true).enabled = true;
            c.GetComponentInChildren<Button>(true).enabled = false;

            prevRarity = Mathf.Max(prevRarity, proposal[i].template.rarity);
        }

        foreach (component c in _Transform.GetComponentsInChildren<component>(true))
            if (!proposal.Contains(c))
                Destroy(c.gameObject);

        _Transform.GetComponent<RectMask2D>().enabled = false;

        if (!componentManager.aboutToDestroy)
            rolling = false;

        line.color = Color.white;
    }

    public static component addComponent(componenttemplate t, int i) {
        component c = Instantiate(meta.componentPrefab, _Transform.TransformPoint(Pos[i]), Quaternion.identity, _Transform).GetComponent<component>();
        c.transform.localRotation = Quaternion.identity;
        c.template = t;
        if (proposal[i] != null)
            Destroy(proposal[i].gameObject);
        proposal[i] = c;

        return c;
    }

    public static component pick(Vector2 v) {
        for (int i = 0; i < proposal.Length; i++) {
            RectTransform t = proposal[i].GetComponent<RectTransform>();
            if (v.x > t.position.x - t.TransformVector((t.rect.width / 2, 0f, 0f).v()).x && v.x < t.position.x + t.TransformVector((t.rect.width / 2, 0f, 0f).v()).x)
                return proposal[i];
        }

        return null;
    }

    private static componenttemplate randomise(int i, int rarity, bool decide = false) {
        if (!componentManager.updating && decide) 
            switch(i) {
                case 0 :
                    return componentManager.allComponents.Find(a => a.name == "credit card");

                case 1 :
                    return componentManager.allComponents.Find(a => a.name == "bank");

                case 2 :
                    return componentManager.allComponents.Find(a => a.name == "temporary money");
            }
        if (componentManager.actual == 2 && i == 1 && rarity != 0 && decide)
            return componentManager.allComponents.Find(a => a.name == "wallet");

        List<componenttemplate> l = possibilities;
        if (Mathf.Abs(clicker.tuto) == 2)
            l = possibilities.FindAll(b => b.pack.Contains("coin"));
        if (Mathf.Abs(clicker.tuto) == 3) { 
            if (componentManager.actual < 4)
                l = possibilities.FindAll(b => b.pack.Contains("electric"));
            else if (componentManager.actual < 6) {
                if (componentManager.actual == 4 && componentManager.turns == 0) {
                    if (componentManager.updating)
                        l = componentManager.allComponents.FindAll(a => a.name == "cog" || a.name == "electrical converter" || a.name == "crank market");
                    else
                        l = componentManager.allComponents.FindAll(a => a.name == "spinner" || a.name == "windmill" || a.name == "mechanical overflow");
                } else
                    l = possibilities.FindAll(b => b.pack.Contains("mechanic"));
            } else
                l = possibilities.FindAll(b => b.pack.Contains("electric") || b.pack.Contains("mechanic"));
        }

        componenttemplate c = l[UnityEngine.Random.Range(0, l.Count)];

        if (componentManager.hasComponent(delegate(componenttemplate C) { 
            if (C == null) 
                return false;

            return C.name == "cloverleaf";
        }))
            rarity ++;

        rarity = Mathf.Min(rarity, metaData.maxRarity-1/*, Mathf.CeilToInt(luckCurve.Evaluate(componentManager.turns))*/);

        switch (i) {
            case 0 : 
                if (c == null || c == componentManager.unassigned || c.rarity < rarity)
                    c = randomise(i, rarity);
            break;

            case 1 : 
                if (c == null || c == componentManager.unassigned || c == proposal[0].template || c.rarity < rarity)
                    c = randomise(i, rarity);
            break;

            case 2 : 
                if (c == null || c == componentManager.unassigned || c == proposal[0].template || c == proposal[1].template || c.rarity < rarity)
                    c = randomise(i, rarity);
            break;
        }

        return c;
    }
    
    public static componenttemplate getComponent(string name) {
        if (name == "")
            return null;

        if (name == "#") {
            _Transform.GetComponent<shop>().StartCoroutine(roll(3));
            return null;
        }

        string[] parts = name.Split(' ');
        if (parts.Length == 2)
            if (char.IsDigit(parts[1], 0)) {
                switch(parts[0]) {
                    case "reroll" :
                        var.rerolls += System.Convert.ToInt32(parts[1][0]);
                    break;

                    case "move" :
                        var.movers += System.Convert.ToInt32(parts[1][0]);
                    break;

                    case "remove" :
                        var.removes += System.Convert.ToInt32(parts[1][0]);
                    break;
                }

                return null;
            }


        foreach (componenttemplate c in componentManager.allComponents)
            if (c?.name == name)
                return c;

        return null;
    }

    void Update() {
        foreach (component c in proposal)
            c?.BroadcastMessage("OnDeselect", new BaseEventData(EventSystem.current), SendMessageOptions.DontRequireReceiver);
    }

    void FixedUpdate() {
        if (GetComponent<RectMask2D>().enabled || rolling) {
            lineGraphic line = GetComponent<lineGraphic>();
            for (int i = 1; i < line.points.Count - 1; i ++) 
                line.points[i] = Vector2.Lerp(line.points[i], (line.points[i].x, -80f).v(), .3f);

            return;
        }

        float MaxPos;
        if ((MaxPos = Mathf.Min(new float[]{proposal[0].transform.localPosition.y, proposal[1].transform.localPosition.y, proposal[2].transform.localPosition.y})) != maxPos) {
            maxPos = MaxPos;
            lineGraphic line = GetComponent<lineGraphic>();

            line.points = new List<Vector2>(){(-320f, 80f).v()};
            for (int i = 0; i < proposal.Length; i++) {   
                float y = proposal[i].transform.localPosition.y - 80;
                if (i == 0 || y != proposal[i - 1].transform.localPosition.y - 80) 
                    line.points.Add((-320f + i * 240, y).v());
                
                if (i == proposal.Length - 1 || y != proposal[i + 1].transform.localPosition.y - 80)
                    line.points.Add((-80f + i * 240, y).v());
            }
            line.points.Add((400f, 80f).v());
        }
    }
}