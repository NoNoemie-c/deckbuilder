using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.U2D;
using System.Collections;

public class component : MonoBehaviour
{
    public componenttemplate template;
    [NonSerialized] public bool isProposal;
    [NonSerialized] public Vector2Int pos;
    public terrain under;

    private TextMeshProUGUI Name, cranks, coins, strength;
    private Image cranksImg, coinsImg, strengthImg;
    [NonSerialized] public new Image renderer;
    private Image glimmer;
    [NonSerialized] public new RectTransform transform;
    public float progress = 1, activation;

    public bool anim, feedback;
    [NonSerialized] public float speed = 0;

    // the var used for storing the coins made on the turn. this is the var used for buffing
    [NonSerialized] public int coin;

    [NonSerialized] public bool activated;
    [NonSerialized] public bool isPowered;
    [NonSerialized] public bool elec;
    [NonSerialized] public bool upgraded;
    [NonSerialized] public bool destroyed;

    [NonSerialized] public bool discrete;

    [NonSerialized] public bool trail;
    [NonSerialized] public Vector2 trailStart, trailEnd;
    private int trailFlags;

    public Color baseCoinsColor, baseCoinImgColor, baseCranksColor, baseCrankImgColor, baseStrengthColor, baseStrengthImgColor, baseNameColor, baseClockColor, 
        baseTerrainColor, baseTerrainBackgroundColor, basePackBackgroundColor, baseImageColor, baseLineColor, baseButtonColor;

    public void Awake() {
        transform = GetComponent<RectTransform>();
    }

    void Start() {
        if (template == componentManager.unassigned)
            template = null;

        TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        Image[] imgs = GetComponentsInChildren<Image>(true);
        
        foreach (TextMeshProUGUI t in texts)
            t.enabled = false;
        foreach (Image img in imgs)
            img.enabled = false;
        
        isProposal = !TryGetComponent<lineGraphic>(out lineGraphic line);
        renderer = Array.Find(imgs, t => t.name == "description");
        Name = Array.Find(texts, t => t.name == "name");
        coins = Array.Find(texts, t => t.name == "coins");
        coinsImg = Array.Find(imgs, t => t.name == "coinImg");
        cranks = Array.Find(texts, t => t.name == "crank");
        cranksImg = Array.Find(imgs, t => t.name == "crankImg");
        strength = Array.Find(texts, t => t.name == "strength");
        strengthImg = Array.Find(imgs, t => t.name == "strengthImg");
        glimmer = Array.Find(imgs, t => t.name == "glimmer");
        Image packBackground = Array.Find(imgs, t => t.name == "pack");
        Image button = Array.Find(imgs, t => t.name == "square");
        
        if (template == null) {
            if (isProposal)
                return;

            button.enabled = true;
            line.enabled = false;
            Image terrain = Array.Find(imgs, t => t.name == "terrain");
            Image terrainBackground = Array.Find(imgs, t => t.name == "terrainBackground");

            if (under != null) {
                terrain.sprite = under.symbol;
                terrainBackground.color = under.color;
                if (!under.superposable) {
                    line.enabled = true;
                    line.thickness = 5;
                    line.color = new Color(.3f, .3f, .3f, 1);
                }
            } 
            
            terrain.enabled = terrainBackground.enabled = under != null;
                
            return;
        }
            
        Name.text = template.name;
        if (template.rarity < metaData.maxRarity)
            Name.color = metaData.RarityColors[template.rarity];
        else
            Name.color = Color.grey;

        if (speed != 0)
            GetComponentInChildren<Button>().targetGraphic.color = Color.clear;
        
        renderer.sprite = template.symbol;

        if (template.pack.Count == 1)
            packBackground.color = componentManager.allPacks[template.pack[0]].color;
        else
            packBackground.sprite = template.packTexture;

        coin = template.coin;

        foreach (Image img in imgs)
            img.enabled = true;
        foreach (TextMeshProUGUI t in texts)
            t.enabled = true;

        if (!isProposal) {
            Image terrain = Array.Find(imgs, t => t.name == "terrain");
            Image terrainBackground = Array.Find(imgs, t => t.name == "terrainBackground");
            SpriteShapeRenderer clock = GetComponentInChildren<SpriteShapeRenderer>();

            if (under) {
                terrain.sprite = under.symbol;
                terrainBackground.color = under.color;
            } 
            terrain.enabled = terrainBackground.enabled = under != null;
            
            baseClockColor = clock.color;
            baseTerrainColor = terrain.color;
            baseTerrainBackgroundColor = terrainBackground.color;
            baseLineColor = line.color;
        }
        
        baseNameColor = Name.color;
        baseCoinsColor = coins.color;
        baseCoinImgColor = coinsImg.color;
        baseCranksColor = cranks.color;
        baseCrankImgColor = cranksImg.color;
        baseStrengthColor = strength.color;
        baseStrengthImgColor = strengthImg.color;
        basePackBackgroundColor = packBackground.color;
        baseButtonColor = button.color;
        baseImageColor = renderer.color;

        print(template.rotation);
        transform.localRotation = Quaternion.AngleAxis(template.rotation, Vector3.back);
        if (template.rotation != 0)
            GetComponent<RectMask2D>().padding = Vector4.one * -200;

        destroyed = false;

        StartCoroutine(butt());
    }

    private IEnumerator butt() {
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(.05f);

        GetComponentInChildren<Button>().enabled = true;
    }

    public void preTurn(Vector2Int pos) {
        feedback = false;

        template?.PreTurn(pos);
    }

    public bool Activate(Vector2Int pos) {
        if (activated)
            return false;
            
        Debug.Log(template.name);

        anim = template.Activate(pos);
        StartCoroutine(metaData.animations["feedback"].play(transform, componentManager.AnimTime / 2));
        feedback = true;
        activated = true;
        
        return true;
    }

    public bool giveCranks(Vector2Int pos) {
        if (template.crank == 0)
            return true;

        gainCranks(template.crank);

        if (var.cranks < 0) {
            var.cranks = 0;
            component gear;
            if ((gear = componentManager.getAdjacents(pos).Find(x => x.name.Contains("inverted gear"))) != null) {
                gear.sparkle();
                component c = componentManager.GetNext(pos);
                c?.Activate(componentManager.GetPosition(c));

                return true;
            } else {
                StartCoroutine(metaData.animations["!cranks"].play(transform, componentManager.AnimTime));
                if (componentManager.GetNext(pos) != null)
                    StartCoroutine(metaData.animations["!cranks"].play(componentManager.GetNext(pos).transform, componentManager.AnimTime));

                return false;
            }
        } 

        coroutiner.start(metaData.sounds["enoughcranks"].play());

        return true;
    }

    public void postTurn(Vector2Int pos) =>
        template?.PostTurn(pos);

    public int giveCoins() {
        if (coin != 0) {
            GameObject g = Instantiate(meta.coinsUIPrefab, transform.position, Quaternion.identity, renderer.canvas.transform);
            g.GetComponent<TextMeshProUGUI>().text = coin.ToString();
            if (template == null)
                g.transform.localScale *= .5f;
            StartCoroutine(metaData.animations["giveCoins"].play(transform, componentManager.CoinTime));
        }

        upgraded = false;
        activated = false;

        return coin;
    }

    public void endTurn() =>
        coin = template.coin;

    public bool click(Vector2Int pos) => 
        anim = template.click(pos);

    public void gainCoins(int amount) {
        componentAnimation anim = metaData.animations[typeof(bufferBehaviour).ToString()];

        anim.rotationCoef = Mathf.Abs(anim.rotationCoef) * -Mathf.Sign(amount);

        StartCoroutine(anim.play(transform, componentManager.AnimTime));
        
        string s = "";
        s = amount.ToString();
        if (amount > 0)
            s = $"+{s}";

        coinsUI v = GetComponentInChildren<coinsUI>();
        if (v != null) {
            v.timer = v.maxTime;
            TextMeshProUGUI t = v.GetComponent<TextMeshProUGUI>();
            t.text += $" {s}";
        } else
            Instantiate(meta.multiplierPrefab, transform.position, Quaternion.identity, transform).GetComponent<TextMeshProUGUI>().text = s;

        coin += amount;
    }

    public void multiplyCoins(float amount) {
        componentAnimation anim = metaData.animations[typeof(bufferBehaviour).ToString()];

        anim.rotationCoef = Mathf.Abs(anim.rotationCoef) * -Mathf.Sign(amount);

        StartCoroutine(anim.play(transform, componentManager.AnimTime));
        
        string s = "";
        if (amount == 0)
            s = $"x0";
        else if (amount > 0)
            s = $"x{amount}";
        else
            s = $"/{1 / amount}";

        coinsUI v = GetComponentInChildren<coinsUI>();
        if (v != null) {
            v.timer = v.maxTime;
            TextMeshProUGUI t = v.GetComponent<TextMeshProUGUI>();
            t.text += $" {s}";
        } else
            Instantiate(meta.multiplierPrefab, transform.position, Quaternion.identity, transform).GetComponent<TextMeshProUGUI>().text = s;

        coin = Mathf.CeilToInt(coin * amount);
    }

    public void selfMultiplyCoins(float amount) {
        componentAnimation anim = metaData.animations[typeof(giveBehaviour).ToString()];
        Color col = Color.white;

        if (amount != 0) {
            col = metaData.colors["coins"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(amount);
        }

        anim.targetImages = componentAnimation.TargetImages.Line;

        anim.AColorCoef = col.a;
        anim.RColorCoef = col.r;
        anim.GColorCoef = col.g;
        anim.BColorCoef = col.b;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));

        StartCoroutine(metaData.sounds["gainCoins"].play());

        string s = "";
        if (amount == 0)
            s = $"x0";
        else if (amount > 0)
            s = $"x{amount}";
        else
            s = $"/{1 / amount}";

        coinsUI v = GetComponentInChildren<coinsUI>();
        if (v != null) {
            v.timer = v.maxTime;
            TextMeshProUGUI t = v.GetComponent<TextMeshProUGUI>();
            t.text += $" {s}";
        } else
            Instantiate(meta.multiplierPrefab, transform.position + Vector3.forward, Quaternion.identity, transform).GetComponent<TextMeshProUGUI>().text = s;

        coin = Mathf.CeilToInt(coin * amount);
    }

    public void selfGainCoins(int amount) {
        componentAnimation anim = metaData.animations[typeof(giveBehaviour).ToString()];
        Color col = Color.white;

        if (amount != 0) {
            col = metaData.colors["coins"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(amount);
        }

        anim.targetImages = componentAnimation.TargetImages.Line;

        anim.AColorCoef = col.a;
        anim.RColorCoef = col.r;
        anim.GColorCoef = col.g;
        anim.BColorCoef = col.b;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));

        StartCoroutine(metaData.sounds["gainCoins"].play());

        string s = "";
        s = amount.ToString();
        if (amount > 0)
            s = "+" + s;

        coinsUI v = GetComponentInChildren<coinsUI>();
        if (v != null) {
            v.timer = v.maxTime;
            TextMeshProUGUI t = v.GetComponent<TextMeshProUGUI>();
            t.text += $" {s}";
        } else
            Instantiate(meta.multiplierPrefab, transform.position + Vector3.forward, Quaternion.identity, transform).GetComponent<TextMeshProUGUI>().text = s;

        coin += amount;
    }

    public void gainCranks(int amount) {
        componentAnimation anim = metaData.animations["cranks"];

        var.addCranks(amount);

        if (amount != 0) {
            varsUI v = GetComponentInChildren<varsUI>();
            if (v != null) {
                v.timer = 100;
                TextMeshProUGUI t = v.GetComponent<TextMeshProUGUI>();
                t.text = t.text.Replace($"<sprite={informationWindow.crankIconID}>", "");

                string s = $"{amount}<sprite={informationWindow.crankIconID}>";
                if (amount >= 0)
                    s = $"+{s}";

                t.text += $" {s}";
            } else {
                TextMeshProUGUI t = Instantiate(meta.coinsUIPrefab, transform.position + Vector3.forward, Quaternion.identity, transform).GetComponent<TextMeshProUGUI>();
                t.text = $"{amount}<sprite={informationWindow.crankIconID}>";
                if (amount >= 0) 
                    t.text = $"+{t.text}";
                t.transform.localScale *= .6f;
                t.color = cranks.color;
                anim.yPositionCoef = 5 * Mathf.Sign(amount);
            }
        }

        StartCoroutine(anim.play(transform, componentManager.CoinTime));
    }

    public void permaGainCoins(int amount) {
        componentAnimation anim = metaData.animations[typeof(giveBehaviour).ToString()];
        Color col = Color.white;

        template.coin += amount;

        if (amount != 0) {
            upgraded = true;

            col = metaData.colors["permanentCoins"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(amount);
        }

        anim.targetImages = componentAnimation.TargetImages.Line | componentAnimation.TargetImages.CoinText;

        anim.AColorCoef = col.a;
        anim.RColorCoef = col.r;
        anim.GColorCoef = col.g;
        anim.BColorCoef = col.b;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));

        StartCoroutine(metaData.sounds["permacoins"].play());
        anim = metaData.animations["perma"];
        anim.targetImages = componentAnimation.TargetImages.CoinText;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));
    }

    public void permaGainCranks(int amount) {
        componentAnimation anim = metaData.animations[typeof(giveBehaviour).ToString()];
        Color col = Color.white;

        anim.targetImages = componentAnimation.TargetImages.Line | componentAnimation.TargetImages.CrankText;

        template.crank += amount;

        if (amount != 0) {
            col = metaData.colors["permanentCranks"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(amount);
        }

        anim.AColorCoef = col.a;
        anim.RColorCoef = col.r;
        anim.GColorCoef = col.g;
        anim.BColorCoef = col.b;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));

        StartCoroutine(metaData.sounds["permacranks"].play());
        anim = metaData.animations["perma"];
        anim.targetImages = componentAnimation.TargetImages.CrankText;

        StartCoroutine(anim.play(transform, componentManager.AnimTime));
    }

    public void sparkle() { 
        if (Array.Find(GetComponentsInChildren<ParticleSystem>(), ps => ps.name.Contains("sparkles")) == null) 
            Instantiate(meta.sparklePrefab, transform.position, Quaternion.identity, transform);
    }

    //------graphics------

    void Update() {
        GetComponentInChildren<Button>().transform.SetSiblingIndex(transform.parent.childCount-1);

        if (!componentAnimation.currentTargets.ContainsValue(transform)) {
            if (transform.localEulerAngles.z % 45 != 0) {
                float f = Mathf.Round(transform.localEulerAngles.z / 45) * 45;
                transform.localRotation = Quaternion.AngleAxis(f, Vector3.forward);
                if (template != null)
                    template.rotation = transform.localEulerAngles.z;
            }
        } else if (template != null)
            template.rotation = transform.localEulerAngles.z;


        if (!isProposal) {
            if (Mouse.current.delta.ReadValue() != Vector2.zero && GetComponent<RectTransform>().rect.Contains(transform.InverseTransformPoint(clicker.mousePos))) {
                foreach (component c in componentManager.GetAll().removeNulls())
                    c.GetComponent<lineGraphic>().thickness = 2;

                foreach (component c in componentManager.getAdjacents(componentManager.GetPosition(this)).removeNulls())
                    c.GetComponent<lineGraphic>().thickness = 3.5f;
            }
        }

        if (template == null) {
            if (!isProposal)
                GetComponentInChildren<Button>().targetGraphic.enabled = componentManager.canGrid;
            return;
        }

        if (isProposal)
            GetComponentInChildren<Button>().targetGraphic.enabled = (!shop.rolling /*|| clicker.endGame*/);
        else
            GetComponentInChildren<Button>().targetGraphic.enabled = !tutorialModule.validation && (componentAnimation.currentTargets.ContainsValue(transform) || 
            ((!template.clicked || clicker.state != clicker.DIDNTSHOP) && componentManager.canGrid) || componentManager.canDestroy);
            
        if (template.rotation != 0)
            print(template.rotation);

        coins.text = $"{coin}{(coin != template.coin ? $"({template.coin})" : "")}";
        if (tutorialModule.showCoins)
            coins.color = new Color(coins.color.r, coins.color.g, coins.color.b, Mathf.Min(coins.color.a + .02f, 1));
        else
            coins.color = new Color(coins.color.r, coins.color.g, coins.color.b, 0);

        cranks.enabled = cranksImg.enabled = template.crank != 0;
        cranks.text = $"{template.crank}";

        strength.enabled = strengthImg.enabled = template.strength != 0;
        strength.text = $"{template.strength}";

        if (!elec && (isPowered || template.electricGenerator)) {
            elec = true;

            float a = UnityEngine.Random.Range(0, 60), b = UnityEngine.Random.Range(100, 160);
            Vector2 A = new Vector2(a, a) - Vector2.one * 80, B = new Vector2(b, b) - Vector2.one * 80;
            Instantiate(meta.elecPrefab, transform.position, Quaternion.identity, transform).GetComponent<elecUI>().SetPos(A, B);
        }

        if (GetComponentInChildren<elecUI>(true) == null)
            elec = false;

        glimmer.enabled = template.conductive;
    }

    void FixedUpdate() {
        if (template == null)
            return;

        transform.position += Vector3.down * speed;

        if (template.conductive) {
            glimmer.transform.localPosition += ((Vector2.down + Vector2.right).normalized * 1f).V3();
            if (glimmer.transform.localPosition.x > 160)
                glimmer.transform.localPosition = (-160f, 160f).v();
        }
        
        if (!isProposal) {
            lineGraphic line = GetComponent<lineGraphic>();
            progress += .01f;
            if (progress > 1)
                progress = 1;

            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(200, 160, progress));

            line.points = new List<Vector2>{(transform.rect.xMin, transform.rect.yMin).v(), 
                                            (transform.rect.xMin, transform.rect.yMax).v(),
                                            (transform.rect.xMax, transform.rect.yMax).v(),
                                            (transform.rect.xMax, transform.rect.yMin).v()};

            line.color = new Color(line.color.r, line.color.g, line.color.b, progress);
            renderer.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, Mathf.Lerp(120, 160, progress));
            renderer.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(120, 160, progress), renderer.rectTransform.sizeDelta.y);
            GetComponentInChildren<Image>().rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, Mathf.Lerp(120, 160, progress));
            GetComponentInChildren<Image>().rectTransform.sizeDelta = new Vector2(Mathf.Lerp(120, 160, progress), renderer.rectTransform.sizeDelta.y);
            renderer.enabled = true;
            RectTransform r = GetComponentInChildren<Button>().GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(Mathf.Lerp(200, 160, progress), Mathf.Lerp(120, 160, progress));

            Image[] imgs = GetComponentsInChildren<Image>(true);
            if (under != null) {
                Image img = Array.Find(imgs, t => t.name == "terrain");
                img.color = new Color(img.color.r, img.color.g, img.color.b, under.color.a * progress);
                img = Array.Find(imgs, t => t.name == "terrainBackground");
                img.color = new Color(img.color.r, img.color.g, img.color.b, progress);
            }

            bool isNameAnimed() {
                foreach (componentAnimation c in componentAnimation.currentTargets.keysOf(transform))
                    if (c.targetImages.HasFlag(componentAnimation.TargetImages.NameText))
                        return true;

                return false;
            }

            if (!isNameAnimed())
                Name.color = new Color(Name.color.r, Name.color.g, Name.color.b, 1 - progress);
            cranksImg.rectTransform.localPosition = new Vector2(Mathf.Lerp(-95f, -62.5f, progress), Mathf.Lerp(55f, 62.5f, progress));
            coinsImg.rectTransform.localPosition = new Vector2(Mathf.Lerp(95f, 62.5f, progress), Mathf.Lerp(55f, 62.5f, progress));
            strengthImg.rectTransform.localPosition = new Vector2(Mathf.Lerp(-95f, -62.5f, progress), Mathf.Lerp(-55f, -62.5f, progress));
            coins.fontSize = Mathf.Lerp(42, 36, progress);

            if (activated)
                activation -= .02f;
            else
                activation += .02f;

            activation = Mathf.Clamp(activation, 0, 1);

            if (!componentAnimation.currentTargets.ContainsValue(transform))
                foreach (Graphic g in GetComponentsInChildren<Graphic>())
                    if (g != Name && g.name != "pack" && g.name != "square")
                        g.color = new Color(g.color.r, g.color.g, g.color.b, Mathf.Lerp(.5f, 1, activation));

            if (trail) {
                float dist = (trailStart - transform.position.V2()).magnitude / (trailStart - trailEnd).magnitude;
                if (dist >= (trailFlags / 10f) + .1f) {
                    trailFlags ++;
                    Transform t = Instantiate(this.gameObject, transform.position, transform.rotation, transform.parent.parent).transform;
                    t.localScale = (.5f, .5f).v();
                    t.SetSiblingIndex(0);
                    t.gameObject.AddComponent<compTrail>().start();
                }
            } else
                trailFlags = 0;
        }  
    }

    public void setActive(bool b, float f = .25f) {
        if (template == null)
            return;

        foreach (TextMeshProUGUI m in GetComponentsInChildren<TextMeshProUGUI>(true)) {
            if (b)
                m.color = new Color(m.color.r, m.color.g, m.color.b, 1); 
            else
                m.color = new Color(m.color.r, m.color.g, m.color.b, f);
        }

        foreach (Image m in GetComponentsInChildren<Image>(true)) {
            float c = 0;
            
            switch(m.name) {
                case "square" :
                    c = b? 1 : 0;
                break;

                case "terrain"  :
                    c = b? baseTerrainColor.a : f;
                break;

                case "terrainBackground" :
                    c = b? baseTerrainBackgroundColor.a : f;
                break;

                case "roll background" : 
                    c = m.color.a;
                break;

                default :
                    c = b? 1 : f;
                break;
            }

            m.color = new Color(m.color.r, m.color.g, m.color.b, c);
        }

        foreach (lineGraphic m in GetComponentsInChildren<lineGraphic>(true))
            if (m.GetComponent<elecUI>() == null)
                m.enabled = b;
    }
    
    public override bool Equals(object other) {
        if (other is component)
            return other as component == this;
        else 
            return false;
    }

    public override int GetHashCode() =>
        gameObject.GetHashCode();

    public static bool operator ==(component c1, component c2) {
        if (c1 && c2)
            return c1.gameObject == c2.gameObject;
        
        return !c1 && !c2;
    }

    public static bool operator !=(component c1, component c2) {
        if (c1 && c2)
            return c1.gameObject != c2.gameObject;
        
        return c1 || c2;
    }

    public override string ToString() =>
        template?.ToString();
}