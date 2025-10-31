using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.U2D;

public class var : MonoBehaviour
{
    public static int cranks = 0;
    public static int coins = 0, Rent = 0, highScore = 0;
    public static int removes = 2, movers = 2, rerolls = 2;

    public static new RectTransform transform;

    private TextMeshProUGUI coin, rent, crank, remove, move, reroll, timer, destroyTimer, destroyCount;

    public Vector2 coinPos, crankPos;
    public Color crankColor, coinColor;
    public static  Vector2 basePosCoin, basePosCrank;
    public static Color cranksColor, coinsColor;

    public static List<Vector2Int> historicCoin;

    private float t, t_;

    private static GameObject obj;

    void Awake() {
        transform = GetComponent<RectTransform>();

        coins = cranks = highScore = 0;
        removes = 2; movers = 2; rerolls = 2;

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();

        coin = texts[0];
        rent = texts[2];
        crank = texts[3];
        reroll = texts[4];
        remove = texts[5];
        move = texts[6];
        timer = texts[7];
        destroyTimer = texts[8];
        destroyCount = texts[9];

        basePosCoin = coinPos;
        basePosCrank = crankPos;
        coinsColor = coinColor;
        cranksColor = crankColor;

        historicCoin = new List<Vector2Int>();

        foreach (glow g in GetComponentsInChildren<glow>())
            g.active = true;
    }

    void Update() {
        coin.text = coins.ToString();
        crank.text = $"{cranks}";
        rent.text = $"{Rent}";
        timer.text = (componentManager.Rent[componentManager.actual].y - componentManager.turns).ToString();
        GetComponentsInChildren<glow>(true)[0].active = timer.text == "1" && timer.gameObject.activeInHierarchy;

        if (componentManager.destroys.Length > componentManager.destroyId) {
            destroyTimer.text = (componentManager.destroys[componentManager.destroyId].y - (componentManager.currentTurn() - componentManager.allDestroyTimes())).ToString();
            GetComponentsInChildren<glow>(true)[1].active = destroyTimer.text == "1" && destroyTimer.gameObject.activeInHierarchy;
            destroyCount.text = $"x{componentManager.destroys[componentManager.destroyId].x}";
        } else
            destroyTimer.transform.parent.gameObject.SetActive(false);
    
        crank.transform.parent.gameObject.SetActive(shop.packs.Contains("mechanic"));
        
        reroll.text = rerolls.ToString();
        remove.text = removes.ToString();
        move.text = movers.ToString();
    }

    void FixedUpdate() {
        SpriteShapeController[] s = GetComponentsInChildren<SpriteShapeController>(true);

        if (s.Length > 0) {
            t += (componentManager.turns / (float) componentManager.Rent[componentManager.actual].y - t) / 10;
            s[0].squareClock(t, 2);
        } if (s.Length > 1 && componentManager.destroys.Length > componentManager.destroyId) {
            t_ += ((float) (componentManager.currentTurn() - componentManager.allDestroyTimes()) / componentManager.destroys[componentManager.destroyId].y - t_) / 10;
            s[1].squareClock(t_, 2);
        }
    }

    public static void addCoins(int val) {
        if (val == 0)
            return;

        obj = Instantiate(meta.varUIPrefab, transform.parent);
        obj.transform.localPosition = basePosCoin;
        TextMeshProUGUI t = obj.GetComponent<TextMeshProUGUI>();
        t.color = coinsColor;
        string s = $"{val}<sprite={informationWindow.coinIconID}>";

        if (val > 0)
            s = $"+{s}";

        coins += val;

        coroutiner.start(metaData.sounds["rent"].play());

        t.text = s;
        t.GetComponent<varsUI>().amount = val;
    }

    public static void collectCoins(int val, bool launch) {
        if (val == 0 && obj == null)
            return;
        
        TextMeshProUGUI t;

        if (obj == null || obj.GetComponent<varsUI>().enabled) {
            obj = Instantiate(meta.coinsPrefab, transform.parent);
            obj.transform.localPosition = basePosCoin + (-300f, 0f).v();
            t = obj.GetComponent<TextMeshProUGUI>();
            t.color = coinsColor;
        } else 
            t = obj.GetComponent<TextMeshProUGUI>();
        
        varsUI v = obj.GetComponent<varsUI>();
        v.amount += val;
        
        if (launch) {
            obj = null;

            v.enabled = true;
        }

        string s = $"{v.amount}<sprite={informationWindow.coinIconID}>";
        if (v.amount > 0)
            s = $"+{s}";
        t.text = s;

        if (t.GetComponent<textEffect>().enabled = v.amount > highScore) {
            t.text += " !";

            if (launch)
                highScore = v.amount;
        }
    }

    public static void addCranks(int val) {
        if (val == 0)
            return;

        obj = Instantiate(meta.varUIPrefab, transform.parent);
        obj.transform.localPosition = basePosCrank;
        TextMeshProUGUI t = obj.GetComponent<TextMeshProUGUI>();
        t.color = cranksColor;
        string s = $"{val}<sprite={informationWindow.crankIconID}>";
    
        if (val > 0) {
            coroutiner.start(metaData.sounds["giveCranks"].play());
            s = $"+{s}";
        }

        t.text = s;
        t.GetComponent<varsUI>().amount = val;
        cranks += val;
    }
}
