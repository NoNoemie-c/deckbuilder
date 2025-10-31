using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class informationWindow : MonoBehaviour, ISerializationCallbackReceiver
{
    // texts referances
    private static TextMeshProUGUI Name, description, cranks, coins, rarity, pack, strength;

    public Sprite powered, generator;
    public static Sprite Powered, Generator;
    private static Image electricity;

    private static Image terrainColor, terrainImage, image, strengthImg, cranksImg;
    [SerializeField] private List<List<string>> words = new List<List<string>>();
    [SerializeField] private List<ListWrapper<string>> w = new List<ListWrapper<string>>(); 
    [SerializeField] private List<Color> colors = new List<Color>();
    public static Dictionary<List<string>, Color> wordsColor = new Dictionary<List<string>, Color>();

    public static Dictionary<string, string> overrides;

    public static Dictionary<string, (string, bool)> tips;

    private static TMP_SpriteAsset Sprites;
    public static int coinIconID, crankIconID;

    private component c;

    public static new RectTransform transform;

    private float t;

    public void OnBeforeSerialize() {}
    public void OnAfterDeserialize() {
        words.Clear();

        for (int i = 0; i < w.Count; i++)
            words.Add(w[i].InnerList);
    }

    void Awake() {
        Sprites = Resources.Load<TMP_SpriteAsset>("images/sprite sheets/sprite sheet");
        coinIconID = Sprites.GetSpriteIndexFromName("coin");
        crankIconID = Sprites.GetSpriteIndexFromName("crank");

        wordsColor = new Dictionary<List<string>, Color>();
        for (int i = 0; i <words.Count; i++)
            wordsColor.Add(words[i], colors[i]);

        Powered = powered;
        Generator = generator;

        transform = GetComponent<RectTransform>();

        overrides = new Dictionary<string, string>();
        tips = new Dictionary<string, (string, bool)>();

        TextMeshProUGUI[] texts = transform.GetChild(0).GetComponentsInChildren<TextMeshProUGUI>(true);
        Name = Array.Find(texts, i => i.gameObject.name == "name");
        (description = Array.Find(texts, i => i.gameObject.name == "description")).maskable = true;
        description.spriteAsset = Sprites;
        cranks = Array.Find(texts, i => i.gameObject.name == "crank");
        coins = Array.Find(texts, i => i.gameObject.name == "coins");
        rarity = Array.Find(texts, i => i.gameObject.name == "rarity");
        pack = Array.Find(texts, i => i.gameObject.name == "pack");
        strength = Array.Find(texts, i => i.gameObject.name == "strength");

        Image[] images = transform.GetChild(0).GetComponentsInChildren<Image>(true);
        image = Array.Find(images, i => i.gameObject.name == "image");
        electricity = Array.Find(images, i => i.gameObject.name == "electricity");
        cranksImg = Array.Find(images, i => i.gameObject.name == "crank");
        terrainImage = Array.Find(images, i => i.gameObject.name == "terrain");
        terrainColor = Array.Find(images, i => i.gameObject.name == "terraincolor"); 
        strengthImg = Array.Find(images, i => i.gameObject.name == "strength");
    }
    
    void FixedUpdate() {
        t ++;

        if (tutorialModule.validation) {
            Transform val = transform.Find("first comp/validate/Image");

            float f = (Mathf.Sin(t / 20) + 2) * .5f;
            val.localScale = (f, f).v();
        }
    }

    public static void Display(component c, baseObject t = null) {
        if (tutorialModule.validation)
            return;

        if (c.template != null && t == null)
            t = c.template;

        if (!t) {
            if (!c.under)
                return;
            else {
                Display(c, c.under);
                return;
            }
        }

        if (transform.GetComponent<informationWindow>().c != c)
            coroutiner.start(metaData.sounds["infowindowNorm"].play());

        transform.GetComponent<informationWindow>().c = c;

        transform.Find("first comp/terrain").GetComponent<Button>().interactable = c.under != null;

        electricity.enabled = true;
        rarity.enabled = true;
        pack.enabled = true;
        cranks.enabled = true;
        coins.enabled = true;
        terrainImage.enabled = true;
        terrainColor.enabled = true;
        
        Name.text = t.name; 

        if (overrides.ContainsKey(t.name))
            description.text = overrides[t.name];
        else if (tips.ContainsKey(t.name)) {
            description.text = tips[t.name].Item1;
            if (tips[t.name].Item2)
                coroutiner.start(tutorialModule.WaitForValidate(clicker.debug? 0 : 2));
            tips[t.name] = (tips[t.name].Item1, false);
        } else
            description.text = descTextEffects(t.description);

        coins.text = t.coin.ToString();
        if (c != null)
            if (c.coin != t.coin)
                coins.text = $"{c.coin}({t.coin})";
        
        cranks.text = t.crank.ToString();
        cranksImg.enabled = (cranks.enabled = t.crank != 0);

        if (t.rarity < metaData.maxRarity) {
            rarity.text = metaData.RarityNames[t.rarity];
            rarity.color = metaData.RarityColors[t.rarity];
        } else {
            rarity.text = "cannot be bought";
            rarity.color = Color.black;
        }

        if (isAllPacks(t.pack))
            pack.text = "all packs";
        else {
            pack.text = "";
            for (int i = 0; i < t.pack.Count; i++) {
                pack.text += $"<color=#{componentManager.allPacks[t.pack[i]].color.ToBase(16, true)}>{t.pack[i]}</color>";

                if (i < t.pack.Count - 1)
                    pack.text += ", ";
            }
            pack.text += " pack";
        }

        if (t.electricGenerator)
            electricity.sprite = Generator;
        else if (t.conductive)
            electricity.sprite = Powered;
        else
            electricity.enabled = false;

        strengthImg.enabled = strength.enabled = t.strength != 0;
        strength.text = t.strength.ToString();

        if (!c.under) {
            terrainColor.color = Color.clear;
            terrainImage.color = Color.clear;
        } else {
            terrainColor.color = c.under.color;
            terrainImage.sprite = c.under.symbol;
            terrainImage.color = Color.white;
        }

        image.enabled = true;
        image.sprite = t.symbol;

        if (clicker.tuto != 0)
            tutorialModule.OnDisplay(c, t);
    }

    public static void DisplayString(string str, string name = "", Sprite icon = null) {
        if (tutorialModule.validation)
            return;

        if (Name.text != name && image.sprite != icon)
            coroutiner.start(metaData.sounds["infowindowNorm"].play());

        if (overrides.ContainsKey(name))
            description.text = overrides[name];
        else
            description.text = descTextEffects(str);
    
        Name.text = name;
        image.enabled = icon != null;
        image.sprite = icon;
        
        terrainImage.enabled = false;
        electricity.enabled = false;
        rarity.enabled = false;
        pack.enabled = false;
        cranks.enabled = false;
        coins.enabled = false;
        terrainColor.enabled = false;
        strengthImg.enabled = false;
        strength.enabled = false;
        cranksImg.enabled = false;

        if (clicker.tuto != 0)
            tutorialModule.OnDisplayStr(name);
    }

    public void displayTerrain() {
        Display(c, c.under);
    }

    public static void Additional(baseObject t) {
        if (Name.text == t?.name)
            return;

        electricity.enabled = true;
        rarity.enabled = true;
        pack.enabled = true;
        cranks.enabled = true;
        coins.enabled = true;
        terrainImage.enabled = true;
        terrainColor.enabled = true;
        
        Name.text = t.name; 

        if (overrides.ContainsKey(t.name))
            description.text = overrides[t.name];
        else if (tips.ContainsKey(t.name)) {
            description.text = tips[t.name].Item1;
            if (tips[t.name].Item2)
                coroutiner.start(tutorialModule.WaitForValidate(clicker.debug? 0 : 2));
            tips[t.name] = (tips[t.name].Item1, false);
        } else
            description.text = descTextEffects(t.description);

        coins.text = t.coin.ToString();
        
        cranks.text = cranks.ToString();
        cranksImg.enabled = (cranks.enabled = t.crank != 0);

        if (t.rarity < metaData.maxRarity) {
            rarity.text = metaData.RarityNames[t.rarity];
            rarity.color = metaData.RarityColors[t.rarity];
        } else {
            rarity.text = "cannot be bought";
            rarity.color = Color.black;
        }

        if (isAllPacks(t.pack))
            pack.text = "all packs";
        else {
            pack.text = "";
            for (int i = 0; i < t.pack.Count; i++) {
                pack.text += $"<color=#{componentManager.allPacks[t.pack[i]].color.ToBase(16, true)}>{t.pack[i]}</color>";

                if (i < t.pack.Count - 1)
                    pack.text += ", ";
            }
            pack.text += " pack";
        }

        strengthImg.enabled = strength.enabled = t.strength != 0;
        strength.text = t.strength.ToString();

        if (t.electricGenerator)
            electricity.sprite = Generator;
        else if (t.conductive)
            electricity.sprite = Powered;
        else
            electricity.sprite = null;

        image.enabled = true;
        image.sprite = t.symbol;
    }

    public static void AdditionalString(string str, string name = "", Sprite icon = null) {
        if (tutorialModule.validation)
            return;

        if (overrides.ContainsKey(name))
            description.text = overrides[name];
        else
            description.text = descTextEffects(str);

        /*description.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Mathf.Max(description.GetRenderedValues(true).y, 40));
        (description.transform.parent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 10, 40);
        (description.transform.parent.parent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 10, 40);*/
        
        Name.text = name;
        image.enabled = true;
        if (icon != null)
            image.sprite = icon;
        else
            image.enabled = false;

        terrainImage.enabled = false;
        electricity.enabled = false;
        rarity.enabled = false;
        pack.enabled = false;
        cranks.enabled = false;
        coins.enabled = false;
        terrainColor.enabled = false;
        strengthImg.enabled = false;
        strength.enabled = false;
        cranksImg.enabled = false;
    }

    public static void Override(string s) {
        description.text = descTextEffects(s);
        if (overrides.ContainsKey(Name.text))
            overrides[Name.text] = description.text;
        else
            overrides.Add(Name.text, description.text);
    }

    public static string getDesc() => 
        description.text;

    public static void tipOverride(string name, string s) {
        tips.Add(name, (descTextEffects(s), true));
    }

    public static bool isAllPacks(List<string> l) {
        foreach (string s in metaData.allPacks)
            if (!l.Contains(s))
                return false;

        return true;
    }

    private static string descTextEffects(string str) {
        string desc = "";

        int sprite;
        bool B = false, perma = false, star = false, interval = false, verb = false, calcul = false;

        string[] array = str.Split('\n');
        for (int i1 = 0; i1 < array.Length; i1++) {
            string actions = array[i1];

            string[] array1 = actions.Split(',');
            for (int i2 = 0; i2 < array1.Length; i2++) {
                string action = array1[i2];

                string[] words = action.Split(' ');
                if (words.Length > 0)
                    if (words[0] == "*") {
                        desc += "<i>";
                        star = true;
                    }

                for (int i = 0; i < words.Length; i++) {
                    string s = words[i];

                    if (s.simplify() == "permanent" || s.simplify() == "permanently") {
                        perma = true;
                        desc += $"<u>{s} ";
                        continue;
                    }

                    if (s.Contains("["))
                        interval = true;

                    if (interval) {
                        foreach (char ch in s)
                            if (char.IsDigit(ch))
                                desc += $"<color=#FF0000><b>{ch}</b></color>";
                            else
                                desc += ch;

                        desc += " ";
                        continue;
                    }

                    if (s.Contains("]"))
                        interval = false;

                    if (s.Contains("{"))
                        calcul = true;
                    else if (s.Contains("}"))
                        calcul = false;

                    if (calcul) {
                        desc += $"<b><size=2.5>{s.ToUpper()}</size></b>";

                        desc += " ";
                        continue;
                    }

                    if ((s.simplify() == "give" || s.simplify() == "gives") && i < words.Length - 1)
                        if (words[i + 1].StartsWith("x") || words[i + 1].StartsWith("+") || words[i + 1].StartsWith("-")) {
                            desc += $"<color=#C3B800FF><b>{s}</b> ";
                            verb = true;
                            continue;
                        }
                    if ((s.simplify() == "produce" || s.simplify() == "produces") && i < words.Length - 1)
                        if (words[i + 1].StartsWith("x") || words[i + 1].StartsWith("+") || words[i + 1].StartsWith("-")) {
                            desc += $"<color=#AB611CFF><b>{s}</b> ";
                            verb = true;
                            continue;
                        }
                    if ((s.simplify() == "you") && i < words.Length - 3)
                        if (words[i + 1].simplify() == "gain")
                            if (words[i + 3].simplify() == "reroll" || words[i + 3].simplify() == "rerolls") {
                                desc += $"<color=#30B714FF><b>{s} gain</b> ";
                                verb = true;
                                i += 1;
                                continue;
                            }
                    if ((s.simplify() == "you") && i < words.Length - 3)
                        if (words[i + 1].simplify() == "gain")
                            if (words[i + 3].simplify() == "mover" || words[i + 3].simplify() == "movers") {
                                desc += $"<color=#2242C5FF><b>{s} gain</b> ";
                                verb = true;
                                i += 1;
                                continue;
                            }
                    if ((s.simplify() == "you") && i < words.Length - 3)
                        if (words[i + 1].simplify() == "gain")
                            if (words[i + 3].simplify() == "removal" || words[i + 3].simplify() == "removals") {
                                desc += $"<color=#000000FF><b>{s} gain</b> ";
                                verb = true;
                                i += 1;
                                continue;
                            }

                    
                    if (s != "is") {
                        if (i < words.Length - 1)
                            if (words[i + 1] != "") {
                                if ((sprite = Sprites.GetSpriteIndexFromName(($"{s} {words[i + 1]}").simplify())) != -1) {
                                    desc += $"<size=4><sprite={sprite}></size> {s} {words[i + 1]} ";
                                    i ++;
                                    continue;
                                } else if ((sprite = Sprites.GetSpriteIndexFromName(($"{s} {words[i + 1]}").simplify() + "s")) != -1) {
                                    desc += $"<size=4><sprite={sprite}></size> {s} {words[i + 1]} ";
                                    i ++;
                                    continue;
                                } else if (words[i + 1].EndsWith("s")) {
                                    if ((sprite = Sprites.GetSpriteIndexFromName(($"{s} {words[i + 1]}").Remove((s + words[i + 1]).Length).simplify())) != -1) {
                                        desc += $"<size=4><sprite={sprite}></size> {s} {words[i + 1]} ";
                                        i ++;
                                        continue;
                                    }
                                } 
                            }

                        if ((sprite = Sprites.GetSpriteIndexFromName(s.simplify())) != -1) {
                            desc += $"<size=4><sprite={sprite}></size> {s} ";
                            continue;
                        } else if (s.EndsWith("s")) {
                            if ((sprite = Sprites.GetSpriteIndexFromName(s.Remove(s.Length - 1).simplify())) != -1) {
                                desc += $"<size=4><sprite={sprite}></size> {s} ";
                                continue;
                            }
                        } else if ((sprite = Sprites.GetSpriteIndexFromName(s.simplify() + "s")) != -1) {
                            desc += $"<size=4><sprite={sprite}></size> {s} ";
                            continue;
                        }
                    }

                    Color col;
                    if (B) {
                        B = false;

                        desc += s + "</color></size></b> ";

                        continue;
                    }
                    bool b = false;

                    if (i != words.Length - 1)
                        if (words[i + 1] != "") {
                            if ((col = wordsColor.GetElement((s + " " + words[i + 1]).Remove((s + words[i + 1]).Length).simplify())) != new Color()) {
                                desc += $"<b><size=3.5><color=#{col.ToBase(16)}>{s} ";
                                B = true;
                                continue;
                            } else if ((col = wordsColor.GetElement((s + " " + words[i + 1]).simplify())) != new Color()) {
                                desc += $"<b><size=3.5><color=#{col.ToBase(16)}>{s} ";
                                B = true;
                                continue;
                            } else if ((col = wordsColor.GetElement((s + " " + words[i + 1]).simplify() + "s")) != new Color()) {
                                desc += $"<b><size=3.5><color=#{col.ToBase(16)}>{s} ";
                                B = true;
                                continue;
                            }
                        }

                    if ((col = wordsColor.GetElement(s.simplify())) != new Color()) {
                        desc += $"<b><size=3.5><color=#{col.ToBase(16)}>";
                        b = true;
                    } else if (s.EndsWith("s")) {
                        if ((col = wordsColor.GetElement(s.Remove(s.Length - 1).simplify())) != new Color()) {
                            desc += $"<b><size=3.5><color=#{col.ToBase(16)}>";
                            b = true;
                        }
                    } else if ((col = wordsColor.GetElement(s.simplify() + "s")) != new Color()) {
                        desc += $"<b><size=3.5><color=#{col.ToBase(16)}>";
                        b = true;
                    }

                    desc += s;

                    if (desc.EndsWith(".")) {
                        if (perma) {
                            perma = false;
                            desc = $"{desc.Substring(0, desc.Length -1)}</u>.";
                        }

                        if (star) {
                            star = false;
                            desc = $"{desc.Substring(0, desc.Length -1)}</i>.";
                        }
                        
                        if (verb) {
                            verb = false;
                            if (desc.EndsWith("."))
                                desc = $"{desc.Substring(0, desc.Length -1)}</color>.";
                            else
                                desc += "</color>";
                        }

                        if (b)
                            desc = $"{desc.Substring(0, desc.Length -1)}</color></size></b>.";
                    } else if (b)
                        desc += "</color></size></b>";
                    
                    if (i != words.Length - 1)
                        desc += " ";
                }

                if (perma) {
                    perma = false;
                    desc += "</u>";
                }

                if (star) {
                    star = false;
                    desc += "</i>";
                }
                
                if (verb) {
                    verb = false;
                    if (desc.EndsWith("."))
                        desc.Insert(desc.Length - 2, "</color>");
                    else
                        desc += "</color>";
                }

                if (i2 != array1.Length - 1)
                    desc += ",";
            }

            if (perma) {
                perma = false;
                desc += "</u>";
            }

            if (star) {
                star = false;
                desc += "</i>";
            }
            
            if (verb) {
                verb = false;
                if (desc.EndsWith("."))
                    desc = $"{desc.Substring(desc.Length - 2, 1)}</color>.";
                else
                    desc += "</color>";
            }

            if (i1 != array.Length - 1)
                desc += "\n";
        }

        return desc.Replace("(", "(<i><size=3>").Replace(")", "</size></i>)");
    }
}