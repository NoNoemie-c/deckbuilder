using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class componentManager : MonoBehaviour
{
    static public List<componenttemplate> allComponents;
    static public Dictionary<string, pack> allPacks;
    static public List<terrain> allTerrains;
    static public Dictionary<string, Sprite> allSprites;
    static public Dictionary<string, Sprite> allPackTextures;
    static public componenttemplate unassigned;
    static public componenttemplate polymorph;
    
    static public List<Sprite> diceFaces;
    public List<Sprite> DiceFaces;

    private static component[][] grid;
    private static component[][] changedGrid;
    public static List<GameObject> toDestroy;

    public Vector2Int Size;
    public Vector2 WorldSize;
    public static Vector2Int size;
    public static Vector2 worldSize;
    
    public static new RectTransform transform;

    public static Dictionary<Vector2Int, Vector2Int> electrify = new Dictionary<Vector2Int, Vector2Int>();

    public List<componenttemplate> starting;
    public List<Vector2Int> startingPos;

    [SerializeField] private float animTime = 0, coinTime = 0;
    public static float AnimTime, CoinTime;

    private int coinsAtTurn;
    public static bool updating = false, canGrid;
    public static bool aboutToDestroy, canDestroy;
    public static int destroyId;
    private bool updateComp;

    public static int turns = 0;
    public Vector2Int[] rent;
    public static Vector2Int[] Rent;
    public static Vector2Int[] destroys;
    public Vector2Int[] Destroys;
    public static int actual = 0;
    public static bool fail = false;

    void Awake() {
        meta.Awake();
        metaData.Awake();

        transform = GetComponent<RectTransform>();

        Rent = rent;

        randCompSpawner.active = false;

        canGrid = true;
        updating = false;
        aboutToDestroy = false;
        canDestroy = false;
        destroyId = 0;

        allComponents = new List<componenttemplate>();
        allTerrains = new List<terrain>();
        allPacks = new Dictionary<string, pack>();
        allSprites = new Dictionary<string, Sprite>();
        allPackTextures = new Dictionary<string, Sprite>();

        transform.parent.Find("destroyText").GetComponent<appear>().InstantDisappear();

        AnimTime = animTime;
        CoinTime = coinTime;

        fail = false;
        actual = turns = 0;

        unassigned = Resources.Load<componenttemplate>("components/default");
        polymorph = Resources.Load<componenttemplate>("components/component pack/polymorph");

        size = Size;
        worldSize = WorldSize;
        diceFaces = DiceFaces;

        toDestroy = new List<GameObject>();

        grid = new component[size.x][];
        for (int i = 0; i < grid.Length; i++)
            grid[i] = new component[size.y];

        changedGrid = grid;

        for (int x = 0; x < grid.Length; x ++)
            for (int y = 0; y < grid[x].Length; y++)
                addComponent((x, y).v(), null);

        if (transform.gameObject.scene.name.Contains("tutorial")) {
            for (int i = 0; i < starting.Count; i++)
                addComponent(startingPos[i], starting[i]);

            Rent = rent;
            destroys = Destroys;
        }
    }

    public static void start() {
        pack[] packs = Resources.LoadAll<pack>("components/packs");
        foreach(pack p in packs) {
            foreach(componenttemplate c in p.getComponents()) {
                if (c.name == "")
                    continue;
                    
                allComponents.Add(c);

                allSprites.Add(c.name, c.symbol);
                allPackTextures.AddSingle(new List<string> {c.name}, addValue: new List<Sprite> {c.packTexture});
            }

            foreach(terrain t in p.getTerrains()) {
                if (t.name == "")
                    continue;

                allTerrains.Add(t);

                allSprites.Add(t.name, t.symbol);
                allPackTextures.AddSingle(new List<string> {t.name}, addValue: new List<Sprite> {t.packTexture});
            }

            allPacks.Add(p.name, p);
        }
    }

    public void setAnimTime(float f) {
        if (transform == null)
            return;

        AnimTime = f / 10;
        transform.parent.Find("information window/animTime/Slider").GetComponentInChildren<TextMeshProUGUI>().text = $"{AnimTime} s";
    }

    public void setCoinTime(float f) {
        if (transform == null)
            return;

        CoinTime = f / 10;
        transform.parent.Find("information window/coinTime/Slider").GetComponentInChildren<TextMeshProUGUI>().text = $"{CoinTime} s";
    }

    public IEnumerator update(bool skip = false) {
        changedGrid = grid;
        updating = true;
        updateComp = true;

        aboutToDestroy = destroyId < destroys.Length && destroys[destroyId].y == currentTurn() - allDestroyTimes() + 1;

        // change the shop pack variable
        shop.packs = new List<string>();
        foreach (component c in GetAll().removeNulls())
            if (!informationWindow.isAllPacks(c.template.pack))
                shop.packs.AddSingle(c.template.pack);

        if (clicker.tuto != 1 && !(clicker.tuto == 2 && actual < 1))
            StartCoroutine(shop.roll(3, 0, skip));
        
        //shop.rolling = false;
        Update();
        canGrid = false;

        // set isPowered to false everywhere
        foreach (component[] row in grid)
            foreach (component c in row) {
                c.isPowered = false;
                c.activated = false;
            }

        // compute electricity
        foreach (component c in GetAll().removeNulls())
            if (c.template.electricGenerator)
                c.isPowered = true;

        electrify = new Dictionary<Vector2Int, Vector2Int> {{Vector2Int.zero, Vector2Int.zero}};
        while (electrify.Count > 0) {
            electrify.Clear();

            List<component> list = new List<component>();
            foreach (component c in GetAll().removeNulls())
                if (c.isPowered)
                    list.Add(c);

            foreach (component c in list)
                ComputeElectricity(c);

            foreach (Vector2Int v in electrify.Keys) {
                componentManager.getComponent(v).isPowered = true;

                coroutiner.start(metaData.sounds["current"].play());
                Instantiate(meta.elecPrefab, transform.position, Quaternion.identity, transform).GetComponent<elecUI>().SetPos(transform.InverseTransformPoint(componentManager.GridToWorld(v)), transform.InverseTransformPoint(componentManager.GridToWorld(electrify[v])));
            }

            yield return new WaitForSeconds(AnimTime / 5);
        }

        // activate terrains
        for (int x = 0; x < size.x; x ++)
            for (int y = 0; y < size.y; y ++)
                grid[x][y].under?.Activate((x, y).v());

        // preturn
        for (int x = 0; x < grid.Length; x ++)
            for (int y = 0; y < grid[x].Length; y ++) {
                grid[x][y].anim = false;
                grid[x][y].preTurn((x, y).v());
                if (grid[x][y].anim)
                    yield return new WaitForSeconds(AnimTime);
            }

        // turn
        for (int x = 0; x < grid.Length; x++)
            for (int y = 0; y < grid[x].Length; y++)
                if (grid[x][y].template)
                    if (!grid[x][y].activated) {
                        grid[x][y].anim = false;
                        yield return StartCoroutine(turn(grid[x][y], (x, y).v()));
                        if (grid[x][y].anim)
                            yield return new WaitForSeconds(AnimTime);
                        foreach (GameObject g in toDestroy)
                            g.GetComponent<component>().setActive(false);
                    }

        // postTurn
        for (int x = 0; x < grid.Length; x ++)
            for (int y = 0; y < grid[x].Length; y ++) {
                grid[x][y].anim = false;
                grid[x][y].postTurn((x, y).v());
                if (grid[x][y].anim)
                    yield return new WaitForSeconds(AnimTime);
            }

        foreach (component[] row in grid)
            foreach (component c in row)
                c.isPowered = false;

        yield return new WaitUntil(() => !shop.rolling || aboutToDestroy);

        Debug.Log("update ended");

        if (clicker.tuto == 0 && !aboutToDestroy)
            clicker.save.Save();

        canGrid = true;

        List<component> components = GetAll().removeNulls();
        coinsAtTurn = 0;
        // collect everywhere

        components.Sort((component c1, component c2) => c1.coin.CompareTo(c2.coin));

        foreach (component component in components) {
            yield return StartCoroutine(collect(component));
            yield return new WaitForSeconds(.1f);
        }

        Debug.Log("coins ended (" + coinsAtTurn + ")");

        foreach (component c in GetAll().removeNulls())
            c.endTurn();

        var.collectCoins(0, true);

        grid = changedGrid;
        var.cranks = 0;
        
        var.historicCoin.Add((var.coins + coinsAtTurn, Rent[actual].x).v());

        turns ++;

        if (turns == Rent[actual].y || (tutorialModule.EndEarly && var.coins + coinsAtTurn >= Rent[actual].x)) {
            var.addCoins(-Rent[actual].x);

            int prev = var.coins;
            yield return new WaitUntil(() => var.coins != prev);

            if (var.coins < 0) {
                Debug.Log("player losed");
                fail = true;
                switch(tutorialModule.punishFail) {
                    case tutorialModule.fail.lose :
                        StartCoroutine(clicker.loseScreen());
                    break;

                    case tutorialModule.fail.loseEx :
                        StartCoroutine(clicker.loseScreen());
                        saveFile.tuto2ex = true;
                    break;
                }

                yield break;
            }

            actual ++;
            turns = 0;
            if (actual >= Rent.Length) {
                saveFile.save(saveFile.Progress + 1);
                Debug.Log("player won");
                saveFile.next();
            } else
                var.Rent = Rent[actual].x;
        }

        if (!aboutToDestroy) {
            yield return new WaitUntil(() => !shop.rolling);

            updating = false;

            yield break;
        }

        updating = false;
        Update();
        updating = true;

        int count = componentManager.GetAll().removeNulls().Count;
        TextMeshProUGUI destroyText = transform.parent.Find("destroyText").GetComponent<TextMeshProUGUI>();
        destroyText.GetComponent<appear>().Appear(.5f);
        canDestroy = true;
       
        yield return new WaitUntil(() => {
            shop._Transform.GetComponent<RectMask2D>().enabled = true;
            shop._Transform.GetComponent<RectMask2D>().padding = new Vector4(0, 500, 0, 0);

            bool b = componentManager.GetAll().removeNulls().Count == 0 || componentManager.GetAll().removeNulls().Count == count - destroys[destroyId].x;

            destroyText.text = $"Destroy {Mathf.Min(destroys[destroyId].x - (count - componentManager.GetAll().removeNulls().Count), componentManager.GetAll().removeNulls().Count)} components";
            return b;
        });

        shop._Transform.GetComponent<RectMask2D>().enabled = false;
        shop._Transform.GetComponent<RectMask2D>().padding = Vector4.zero;

        destroyText.GetComponent<appear>().Disappear(.5f);

        shop.rolling = false;
        updating = false;
        aboutToDestroy = false;
        canDestroy = false;

        destroyId ++;

        if (clicker.tuto == 0)
            clicker.save.Save();
    }

    private IEnumerator turn(component c, Vector2Int pos) {
        if (toDestroy.Contains(c.gameObject))
            yield break;

        if (updateComp)
            updateComp = c.giveCranks(pos);
        else
            updateComp = true;

        if (c.template.crank != 0)
            yield return new WaitForSeconds(CoinTime / 2);

        if (updateComp)
            c.Activate(pos);

        if (c.anim)
            yield return new WaitForSeconds(AnimTime);
        else if (c.feedback)
            yield return new WaitForSeconds(AnimTime / 2);

        c.activated = true;
    }

    private IEnumerator collect(component c) {
        int i = c.giveCoins();
        coinsAtTurn += i;
        var.collectCoins(i, false);

        if (i > 0)
            yield return new WaitForSeconds(CoinTime);
    }

    public void Update() {
        if (!updating) {
            grid = changedGrid;

            List<component> all = GetAll();
            foreach (component c in GetComponentsInChildren<component>(true))
                if (!all.Contains(c)) {
                    c.discrete = true;
                    StartCoroutine(delete(c.gameObject));
                }
                
            foreach (GameObject g in toDestroy) {
                if (g == null)
                    continue;

                Vector2Int v = GetPosition(g.GetComponent<component>());
                grid[v.x][v.y] = null;

                StartCoroutine(delete(g));
            }

            for (int x = 0; x < size.x; x ++)
                for (int y = 0; y < size.y; y++)
                    if (getComponent((x, y).v()) == null)
                        addComponentDiscrete((x, y).v(), null);

            toDestroy.Clear();
        }

        BroadcastMessage("OnDeselect", new BaseEventData(EventSystem.current), SendMessageOptions.DontRequireReceiver);
    }

    private IEnumerator delete(GameObject g) {
        if (!g.GetComponent<component>().discrete)
            yield return StartCoroutine(metaData.animations["destroyed"].play(g.transform, AnimTime));

        Destroy(g);
    }

    public void ComputeElectricity(component comp) {   
        if (comp.template == null)
            return;

        Vector2Int compPos = componentManager.GetPosition(comp);

        foreach (component c in getAdjacents(compPos, false).removeNulls())
            if (!c.isPowered && c.template.conductive) {
                Vector2Int cPos = componentManager.GetPosition(c);

                if (electrify.ContainsKey(cPos)) {
                    if ((compPos - cPos).magnitude < (electrify[cPos] - cPos).magnitude) 
                        electrify[cPos] = compPos;
                } else
                    electrify.Add(cPos, compPos);
            }
    }

    //-------utilitary functions-------

    public static Vector2Int WorldToGrid(Vector2 v) {
        Vector2 vec = transform.InverseTransformPoint(v) + new Vector3((transform.rect.size / 2).x - worldSize.x / 2, (transform.rect.size / 2).y - worldSize.y / 2, 0);
        return Vector2Int.RoundToInt(vec / worldSize.x);
    }

    public static Vector2 GridToWorld(Vector2Int v) {
        Vector2 vec = v;
        vec = vec * worldSize.x - new Vector2((transform.rect.size / 2).x, (transform.rect.size / 2).y) + worldSize / 2;
        return transform.TransformPoint(vec);
    }

    public static component getComponent(Vector2Int gridPos) {
        return changedGrid[gridPos.x][gridPos.y];
    }

    public static List<component> getUpgradedComponents() => 
        GetAll(c => c.upgraded);

    public static List<component> getNewComponents() {
        List<component> l = new List<component>();

        for (int x = 0; x < grid.Length; x++)
            for (int y = 0; y < grid[x].Length; y++)
                if (changedGrid[x][y].template)
                    if (grid[x][y].template) {
                        l.Add(changedGrid[x][y]);
                    } else if (changedGrid[x][y].template != grid[x][y].template)
                        l.Add(changedGrid[x][y]);

        return l;
    }

    public static List<component> getBuffedComponents() {
        List<component> l = new List<component>();

        for (int x = 0; x < grid.Length; x++)
            for (int y = 0; y < grid[x].Length; y++)
                if (changedGrid[x][y].template)
                    if (changedGrid[x][y].template.coin < changedGrid[x][y].coin)
                        l.Add(changedGrid[x][y]);

        return l;
    }

    public static List<component> getDeBuffedComponents() {
        List<component> l = new List<component>();

        for (int x = 0; x < grid.Length; x++)
            for (int y = 0; y < grid[x].Length; y++)
                if (changedGrid[x][y].template)
                    if (changedGrid[x][y].template.coin > changedGrid[x][y].coin)
                        l.Add(changedGrid[x][y]);

        return l;
    }

    public static bool hasComponent(Predicate<componenttemplate> match) {
        foreach (component comp in GetAll().removeNulls())
            if (match(comp.template))
                return true;

        return false;
    }

    public static List<component> GetAll() {
        List<component> list = new List<component>();

        foreach (component[] column in changedGrid)
            foreach (component comp in column)
                if (comp != null)
                    list.Add(comp);

        return list; 
    }
    public static List<component> GetAll(Predicate<component> match) => 
        GetAll().removeNulls().FindAll(match); 

    public static component spawnComponent(Vector2Int pos, componenttemplate c) {
        foreach (component comp in getAdjacents(pos))
            if (canPlaceOn(GetPosition(comp))) {
                coroutiner.start(metaData.sounds["spawn"].play());
                return componentManager.addComponent(GetPosition(comp), c);
            }

        return null;
    }

    public static component addComponent(Vector2Int pos, componenttemplate c, bool shop = false) {
        if (getComponent(pos) == null || c == null) {
            GameObject g = Instantiate(meta.gridComponentPrefab, GridToWorld(pos), Quaternion.identity, transform);
            g.name = $"component{pos.x}{pos.y}";
            component comp = g.GetComponent<component>();
            comp.template = null;
            changedGrid[pos.x][pos.y] = comp;

            return comp;
        } else if (canPlaceOn(pos)) {
            GameObject g = Instantiate(meta.gridComponentPrefab, GridToWorld(pos), Quaternion.identity, transform);
            g.name = $"component{pos.x}{pos.y}";
            component comp = g.GetComponent<component>();
            (comp.template = ScriptableObject.CreateInstance<componenttemplate>()).setTo(c);
            comp.under = getTerrain(pos);
            removeDiscrete(pos);
            changedGrid[pos.x][pos.y] = comp;

            if (comp.template.coinsOnSpawn != 0)
                Instantiate(meta.coinsUIPrefab, GridToWorld(pos).V3() + Vector3.forward, Quaternion.identity, comp.GetComponentInChildren<Graphic>().canvas.transform).GetComponent<TextMeshPro>().text = comp.template.coinsOnSpawn.ToString() + "<sprite=" + informationWindow.coinIconID + ">";
            var.coins += comp.template.coinsOnSpawn;
            var.addCranks(comp.template.cranksOnSpawn);
            
            foreach (componentBehaviour behaviour in comp.template.spawnBehaviours)
                behaviour.activate(pos);

            comp.Awake();
            if (! shop)
                comp.StartCoroutine(metaData.animations["appearing"].play(comp.transform, AnimTime));

            if (updating)
                comp.activated = true;

            return comp;
        }

        return null;
    }

    public static component addComponentDiscrete(Vector2Int pos, componenttemplate c) {
        if (getComponent(pos) == null) {
            GameObject g = Instantiate(meta.gridComponentPrefab, GridToWorld(pos), Quaternion.identity, transform);
            g.name = $"component{pos.x}{pos.y}";
            component comp = g.GetComponent<component>();
            if (c != null)
                (comp.template = ScriptableObject.CreateInstance<componenttemplate>()).setTo(c);
            changedGrid[pos.x][pos.y] = comp;

            return comp;
        } 

        if (canPlaceOn(pos)) {
            GameObject g = Instantiate(meta.gridComponentPrefab, GridToWorld(pos), Quaternion.identity, transform);
            g.name = $"component{pos.x}{pos.y}";
            component comp = g.GetComponent<component>();
            removeDiscrete(pos);
            if (c != null)
                (comp.template = ScriptableObject.CreateInstance<componenttemplate>()).setTo(c);
            comp.under = getTerrain(pos);
            changedGrid[pos.x][pos.y] = comp;
            if (updating && c != null)
                comp.activated = true;

            return comp;
        }

        return null;
    }

    public static component moveComponent(Vector2Int prevPos, Vector2Int nextPos, bool swap, bool tool = false) {
        if (!componentManager.canPlaceOn(nextPos))
            return null;

        component prev = getComponent(prevPos), next = getComponent(nextPos);

        if (next.template) {
            if (swap) {
                addComponentDiscrete(prevPos, next.template);
                componentAnimation anim = metaData.animations["moving"];
                Vector2Int v = nextPos - prevPos;
                anim.xPositionCoef = worldSize.x * v.x;
                anim.yPositionCoef = worldSize.y * v.y;
                next = getComponent(prevPos);
                next.activated = getComponent(nextPos).activated;
                coroutiner.start(anim.play(next.transform, AnimTime));

                removeDiscrete(nextPos);
                addComponentDiscrete(nextPos, prev.template);
                getComponent(nextPos).activated = prev.activated;
                anim = metaData.animations["moving"];
                anim.xPositionCoef *= -v.x;
                anim.yPositionCoef *= -v.y;
                coroutiner.start(anim.play(getComponent(nextPos).transform, AnimTime));

                return getComponent(nextPos);
            }

            return null;
        } else {
            addComponentDiscrete(nextPos, prev.template);
            getComponent(nextPos).activated = prev.activated;
            prev = getComponent(nextPos);
            removeDiscrete(prevPos);

            componentAnimation anim;
            if (tool) {
                anim = metaData.animations["movingTool"];
                prev.trail = true;
                prev.trailStart = GridToWorld(prevPos);
                prev.trailEnd = GridToWorld(nextPos);
            } else
                anim = metaData.animations["moving"];
            Vector2Int v = nextPos - prevPos;
            anim.xPositionCoef = worldSize.x * v.x;
            anim.yPositionCoef = worldSize.y * v.y;
            coroutiner.start(anim.play(prev.transform, AnimTime));

            return prev;
        }
    }

    public static component GetNext(Vector2Int pos) {
        while (true) {
            pos += Vector2Int.up;
            if (pos.y >= size.y)
                pos = new Vector2Int(pos.x + 1, 0);
            if (pos.x >= size.x)
                break;

            if (getComponent(pos).template)
                return getComponent(pos);
        }

        return null;
    }

    public static List<component> getAdjacents(Vector2Int pos, bool b = true) {
        List<component> list = new List<component>();

        void _getAdjacents(Vector2Int pos, bool b = true) {
            if (!validPos(pos))
                return;;

            GetAdjacents(pos, b);

            if (!b)
                return;;

            bool screenWrapping = false;
            foreach (component c in list.removeNulls())
                if (c.template.tags.Contains("screen wrapper") && !screenWrapping) {
                    c.sparkle();
                    GetAdjacents(pos, true, screenWrapping = true);
                    return;;
                }

            component comp;
            if ((comp = list.removeNulls().Find(c => c.template.tags.Contains("telescope"))) != null)
                if (comp.template.getBehaviours<cyclicBehaviour>()[0].turn == 0) {
                    comp.sparkle();
                    list = componentManager.GetAll();
                }

            if (getComponent(pos).isPowered && list.removeNulls().Find(c => c.template.tags.Contains("electric expander")) != null)
                list.AddSingle(getCircuit(pos));
        }

        void GetAdjacents(Vector2Int pos, bool b = true, bool screenWrapping = false) {
            foreach (Vector2Int dir in meta.directions)
                check(pos + dir, b, screenWrapping);
        
            if (b)
                foreach (component c in list)
                    if (c.template?.tags.Contains("space portal") != null)
                        foreach (component C in GetAll().removeNulls())
                            if (C.template.tags.Contains("space portal") && !list.Contains(C)) {
                                list.AddSingle(C);
                                GetAdjacents(GetPosition(C), false);
                                C.sparkle();
                                c.sparkle();
                            }
        }

        void check(Vector2Int pos, bool b, bool screenWrapping) {
            if (screenWrapping) {
                if (pos.x < 0) pos = new Vector2Int(size.x - pos.x, pos.y);
                else if (pos.x >= size.x) pos -= new Vector2Int(size.x, 0);
                if (pos.y < 0) pos = new Vector2Int(size.x, size.y - pos.y);
                else if (pos.y >= size.y) pos -= new Vector2Int(size.y, 0);
            }

            if (!validPos(pos))
                return;

            component c = getComponent(pos);

            if (list.Contains(c))
                return;

            list.Add(c);

            if (!c.template || !b)
                return;

            if (c.template.tags.Contains("range booster")) {
                c.sparkle();
                _getAdjacents(pos);
            } else if (c.template.tags.Contains("greenhouse")) {
                c.sparkle();
                List<component> l = new List<component>(list);
                _getAdjacents(pos);
                l.AddSingle(l.FindAll(comp => comp.template.tags.Contains("seed") || comp.template.tags.Contains("tree") || comp.template.tags.Contains("flower")));
                list = new List<component>(l);
            } else if (c.template.tags.Contains("line coordinator")) {
                c.sparkle();
                foreach (component comp in GetAll())
                    if (comp != null)
                        if (GetPosition(comp).x == pos.x || GetPosition(comp).y == pos.y)
                            list.AddSingle(c);
            }
        }

        _getAdjacents(pos);

        return list; 
    }

    public static List<component> getCircuit(Vector2Int pos) {
        List<component> list = new List<component>();

        if (getComponent(pos).isPowered) {
            foreach (Vector2Int dir in meta.directions)
                list.AddSingle(getCircuit(pos + dir));
            
            return list;
        } else
            return new List<component>();
    }

    public static void remove(Vector2Int pos = default(Vector2Int), Vector2Int eaterPos = default(Vector2Int), component c = null) {
        if (c) {
            for (int x = 0; x < grid.Length; x ++)
                for (int y = 0; y < grid[x].Length; y ++)
                    if (changedGrid[x][y].template)
                        if (changedGrid[x][y] == c) {
                            toDestroy.Add(changedGrid[x][y].gameObject);
                            coroutiner.start(changedGrid[x][y].template.isDestroyed((x, y).v(), eaterPos));
                        }
        } else if (validPos(pos)) {
            if (changedGrid[pos.x][pos.y])
                if (changedGrid[pos.x][pos.y].template) {
                    toDestroy.Add(changedGrid[pos.x][pos.y].gameObject);
                    coroutiner.start(changedGrid[pos.x][pos.y].template.isDestroyed(pos, eaterPos));
                }
        } else
            throw new ArgumentException("tried removing with c == null and a non valid pos");
    }

    public static void removeDiscrete(Vector2Int pos = default(Vector2Int), component c = null) {
        if (c) {
            for (int x = 0; x < grid.Length; x++)
                for (int y = 0; y < grid[x].Length; y++)
                    if (changedGrid[x][y].template)
                        if (changedGrid[x][y] == c) {
                            toDestroy.Add(changedGrid[x][y].gameObject);
                            changedGrid[x][y].discrete = true;
                            if (canGrid) {
                                terrain t = changedGrid[x][y].under;
                                changedGrid[x][y] = null;
                                addComponentDiscrete((x, y).v(), null).under = t;
                            }
                        }
        } else if (validPos(pos)) {
            if (changedGrid[pos.x][pos.y])
                if (changedGrid[pos.x][pos.y].template) {
                    toDestroy.Add(changedGrid[pos.x][pos.y].gameObject);
                    changedGrid[pos.x][pos.y].discrete = true;
                    if (canGrid) {
                        terrain t = changedGrid[pos.x][pos.y].under;
                        changedGrid[pos.x][pos.y] = null;
                        addComponentDiscrete(pos, null).under = t;
                    }
                }
        } else
            throw new ArgumentException("tried removingDiscrete with c == null and a non valid pos");
    }

    public static void clear(float diviser, bool discrete = false) { //clear a percentage of the grid
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                if (changedGrid[x][y] && (size.x * y + x) % diviser == 0) {
                    if (discrete) 
                        removeDiscrete((x, y).v());
                    else
                        remove((x, y).v());
                }
    }

    public static Vector2Int GetPosition(component c) {
        string s = c.gameObject.name;
        int a = Convert.ToInt32(s[s.Length - 2].ToString()), b = Convert.ToInt32(s[s.Length - 1].ToString());
        return (a, b).v();
    }

    public static bool validPos(Vector2Int v) {
        return v.x >= 0 && v.y >= 0 && v.x < size.x && v.y < size.y;
    }

    public static terrain getTerrain(Vector2Int pos) =>
        changedGrid[pos.x][pos.y].under;

    public static bool canPlaceOn(Vector2Int pos) => 
        validPos(pos) && getComponent(pos).template == null && (getTerrain(pos) == null || getTerrain(pos).superposable);
                
    public static bool removeTerrain(Vector2Int pos) {
        if (!validPos(pos) || !getTerrain(pos))
            return false;

        if (!getTerrain(pos).removable)
            return false;

        changedGrid[pos.x][pos.y].under = null;
        return true;
    }

    public static void clearTerrains(float diviser) { //clear a percentage of the grid
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                changedGrid[x][y].under = null;
                    
    }

    public static bool addTerrain(Vector2Int pos, terrain t) {
        if (!validPos(pos))
            return false;
            
        changedGrid[pos.x][pos.y].under = t;
        return true;
    }

    public static int currentTurn() {
        int count = 0;

        for (int i = 0; i < actual; i++)
            count += Rent[i].y;

        return count + turns;
    }

    public static int allDestroyTimes() {
        int count = 0;

        for (int i = 0; i < destroyId; i ++)
            count += destroys[i].y;

        return count;
    }
}