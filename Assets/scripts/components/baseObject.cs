using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

public abstract class baseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public enum time : int {
        preTurn,
        postTurn,
        turn
    } public static time[] times = {time.preTurn, time.turn, time.postTurn};

    public bool Lock = true;
    public new string name;
    [Space(5)] [TextArea(3, 6)] public string description;
    [Space(5)] public int coin;

    [Space(10)] public Sprite symbol;
    
    [Space(5)] public List<string> pack = new List<string>();
    public Sprite packTexture;
    
    [Space(5)] public List<string> tags = new List<string>();
    
    [Space(5)] public int rarity;

    [Space(20)] [SerializeField] [SerializeReference] private List<componentBehaviour> Behaviours = new List<componentBehaviour>();
    [SerializeReference] [SerializeField] private List<componentBehaviour>             PreTurnBehaviours = new List<componentBehaviour>(),
                                                                  PostTurnBehaviours = new List<componentBehaviour>();

    public Dictionary<time, List<componentBehaviour>> behaviours = new Dictionary<time, List<componentBehaviour>>() 
    {{time.preTurn, new List<componentBehaviour>()}, {time.turn, new List<componentBehaviour>()}, {time.postTurn, new List<componentBehaviour>()}};

    [Space(10)] public int crank;
    
    [Space(5)] public int strength;

    [NonSerialized] public float rotation;

    [Space(5)]
    public bool conductive;
    public bool electricGenerator;
    [SerializeReference] public List<componentBehaviour> electricBehaviours = new List<componentBehaviour>();

    [Space(10)]
    public int coinsOnSpawn;
    public int cranksOnSpawn;
    [SerializeReference] public List<componentBehaviour> spawnBehaviours = new List<componentBehaviour>();

    [Space(10)]
    public int coinOnDestroy;
    [SerializeReference] public List<componentBehaviour> deathBehaviours = new List<componentBehaviour>();

    [NonSerialized] public bool clicked;

    public void OnBeforeSerialize() {
        if (!Lock)
            return;

        if (PreTurnBehaviours.Count == 0)
            PreTurnBehaviours = behaviours[time.preTurn];

        if (PostTurnBehaviours.Count == 0)
            PostTurnBehaviours = behaviours[time.postTurn];

        if (Behaviours.Count == 0)
            Behaviours = behaviours[time.turn];
    }

    public void OnAfterDeserialize() {
        behaviours.Clear();

        foreach (time t in times) {
            List<componentBehaviour> l;
            switch (t) {
                case time.postTurn :
                    l = PreTurnBehaviours;
                break;

                case time.turn :
                    l = Behaviours;
                break;

                default :
                    l = PostTurnBehaviours;
                break;
            }

            behaviours.Add(t, new List<componentBehaviour>());
            behaviours[t].AddRange(l);
        }
    }

    public void Pack() {
#if UNITY_EDITOR
        if (packTexture == null)
            packTexture = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(this)?.Replace(".asset", "Pack.png"));
#endif
    
        if (pack.Count > 0 && pack[0] == "all")
            pack = metaData.allPacks;
    }

    public void createPackBackground() {
        /*if (pack.Count < 2)
            return;
            
        List<Color> packColors = new List<Color>();
        foreach (string s in pack)
            packColors.Add(componentManager.allPacks[s].color);

        if (packColors.Count > 1) {
            File.WriteAllBytes($"/Users/gaellequartierditmaire/Documents/unity projects/New Unity Project/{AssetDatabase.GetAssetPath(this)?.Replace(".asset", "Pack.png")}", packTextureCreator.Create(packColors.ToArray()).texture.EncodeToPNG());
            packTexture = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(this)?.Replace(".asset", "Pack.png"));
        } else 
            packTexture = Resources.Load<Sprite>("images/packTexture");*/
    }

    public abstract void setTo(baseObject c);

    public abstract string encode(int indent);
    public abstract void decode(gameSave.element e);

    public static List<componentBehaviour> copyBehaviours(List<componentBehaviour> list) {
        List<componentBehaviour> l = new List<componentBehaviour>();

        foreach (componentBehaviour behaviour in list.removeNulls())
            l.Add(behaviour.copy());

        return l;
    }
    public static Dictionary<time, List<componentBehaviour>> copyBehaviours(Dictionary<time, List<componentBehaviour>> dict) {
        Dictionary<time, List<componentBehaviour>> d = new Dictionary<time, List<componentBehaviour>>();

        foreach (time t in times) {
            d.Add(t, new List<componentBehaviour>());
            foreach (componentBehaviour behaviour in dict[t])
                if (behaviour != null)
                    d[t].Add(behaviour.copy());
        }

        return d;
    }

    public virtual bool Activate(Vector2Int pos) {
        bool b = false;

        component This = componentManager.getComponent(pos);

        for (int i = 0; i < behaviours[time.turn].Count; i++) {
            componentBehaviour behaviour = behaviours[time.turn][i];
            if (behaviour.isActive)
                b = b | behaviour.activate(pos);
        }

        if (This.isPowered)
            b = b | Electrify(pos);

        
        return b;
    }

    public virtual bool PreTurn(Vector2Int pos) {
        bool b = false;

        component c = componentManager.getComponent(pos);

        for (int i = 0; i < behaviours[time.preTurn].Count; i++) {
            componentBehaviour behaviour = behaviours[time.preTurn][i];
            if (behaviour.isActive)
                b = b || behaviour.activate(pos);
        }

        return b;
    }

    public virtual bool PostTurn(Vector2Int pos) {
        bool b = false;

        component c = componentManager.getComponent(pos);

        for (int i = 0; i < behaviours[time.postTurn].Count; i++) {
            componentBehaviour behaviour = behaviours[time.postTurn][i];
            if (behaviour.isActive)
                b = b || behaviour.activate(pos);
        }

        clicked = false;
        
        return b;
    }

    public bool Electrify(Vector2Int pos) {
        bool b = false;

        

        foreach (componentBehaviour behaviour in electricBehaviours)
            b = b || behaviour.activate(pos);

        if (b) {
            component This = componentManager.getComponent(pos);
            This.StartCoroutine(metaData.animations["electric"].play(This.transform, componentManager.AnimTime));
        }

        return b;
    }

    public IEnumerator isDestroyed(Vector2Int pos, Vector2Int eaterPos) {
        Debug.Log($"{name} was destroyed");

        yield return new WaitForSeconds(.02f);

        foreach (componentBehaviour behaviour in deathBehaviours)
            behaviour.activate(pos);

        component c = componentManager.getComponent(pos);
        c.destroyed = true;
        if (!componentManager.validPos(eaterPos))
            c.StartCoroutine(metaData.animations[typeof(killerBehaviour).ToString()].play(c.transform, componentManager.AnimTime));
        else {
            componentAnimation anim = metaData.animations["eaten"];

            anim.xPositionCoef = (eaterPos - pos).x * componentManager.worldSize.x;
            anim.yPositionCoef = (eaterPos - pos).y * componentManager.worldSize.y;

            c.StartCoroutine(anim.play(c.transform, componentManager.AnimTime));
        }

        if (coinOnDestroy != 0)
            Instantiate(meta.coinsUIPrefab, componentManager.GridToWorld(pos).V3() + Vector3.forward, Quaternion.identity, c.renderer.canvas.transform).GetComponent<TextMeshPro>().text = coinOnDestroy.ToString() + "<sprite=" + informationWindow.coinIconID + ">";
        var.coins += coinOnDestroy;
    }

    public bool click(Vector2Int pos) {
        if (clicked)
            return false;

        clicked = true;

        bool b = false;

        foreach (time t in times)
            foreach (componentBehaviour behaviour in behaviours[t])
                b = b || behaviour.click(pos);

        return b;
    }

    public List<type> getBehaviours<type>(time t = time.turn) where type : componentBehaviour {
        List<type> list = new List<type>();

        foreach (componentBehaviour behaviour in behaviours[t])
            if (behaviour.GetType() == typeof(type))
                list.Add((behaviour as type));

        return list;
    }

    public override bool Equals(object obj) {
        if (obj is baseObject)
            return obj as baseObject == this;
        else 
            return false;
    }

    public override int GetHashCode() =>
        tags.GetHashCode();

    public override string ToString() =>
        name;

    public static bool operator ==(baseObject a, baseObject b) { 
        if (a && b)
            return a.tags == b.tags; 
        
        return !a && !b;
    }

    public static bool operator !=(baseObject a, baseObject b) {
        if (a && b)
            return a.tags != b.tags; 
        
        return (a || b) && (!a || !b);
    }

    public static baseObject operator +(baseObject a, baseObject b) => null;

    public static baseObject operator *(baseObject a, int scalar) => null;
}
