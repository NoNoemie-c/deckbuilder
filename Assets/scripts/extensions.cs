using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class meta
{
    public static Vector2[] dir = {(1, 1).v(), (1, -1).v(), (-1, -1).v(), (-1, 1).v()};
    public static Vector2Int[] directions = {(0, 1).v(), (1, 1).v(), (1, 0).v(), (1, -1).v(), (0, -1).v(), (-1, -1).v(), (-1, 0).v(), (-1, 1).v()};
    public static string[] alphanumeric = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"};
    public static Dictionary<int, string> romans = new Dictionary<int, string>{{50, "C"}, {10, "X"}, {5, "V"}, {1, "I"}};
    public static GameObject componentPrefab, gridComponentPrefab, elecPrefab, coinplosionPrefab, coinsPrefab, coinsUIPrefab, varUIPrefab, multiplierPrefab, buyDustPrefab, deathPrefab, sparklePrefab, graphTextPrefab, ghostPackPrefab, soundPrefab;

    public static void Awake() {
        varUIPrefab = Resources.Load<GameObject>("prefabs/varsUI");
        componentPrefab = Resources.Load<GameObject>("prefabs/component");
        gridComponentPrefab = Resources.Load<GameObject>("prefabs/grid component");
        elecPrefab = Resources.Load<GameObject>("prefabs/elecUI");
        coinsUIPrefab = Resources.Load<GameObject>("prefabs/coins UI");
        coinsPrefab = Resources.Load<GameObject>("prefabs/varCoinUI");
        coinplosionPrefab = Resources.Load<GameObject>("prefabs/coinPlosion");
        multiplierPrefab = Resources.Load<GameObject>("prefabs/multiplier");
        buyDustPrefab = Resources.Load<GameObject>("prefabs/buyDust");
        deathPrefab = Resources.Load<GameObject>("prefabs/death");
        sparklePrefab = Resources.Load<GameObject>("prefabs/sparkles");
        graphTextPrefab = Resources.Load<GameObject>("prefabs/graphText");
        ghostPackPrefab = Resources.Load<GameObject>("prefabs/ghost component newPack");
        soundPrefab = Resources.Load<GameObject>("prefabs/sound");

        /*List<Color> l = new List<Color>();
        for (int i = 0; i < 512 * 512; i++)
            l.Add(Color.clear);
        PNGResizer.clear = l.ToArray();
        PNGResizer.recurseResize(PNGResizer.resources + "/images/dice faces", true, 20);
        PNGResizer.recurseResize(PNGResizer.resources + "/components", true, 20);*/
    }

    public static Vector3 V3(this Vector2 v) => new Vector3(v.x, v.y, 0);
    public static Vector2 V2(this Vector3 v) => new Vector2(v.x, v.y);

    public static void AddSingle<T>(this List<T> list, List<T> add) {
        foreach (T c in add)
            list.AddSingle(c);
    }
    public static void AddSingle<T>(this List<T> list, T add) {
        if (!list.Contains(add))
            list.Add(add);
    }

    public static Dictionary<T, M> AddSingle<T, M>(this Dictionary<T, M> dict, List<T> addKey, M val = null, List<M> addValue = null) where M : class {
        for (int i = 0; i < addKey.Count; i++) {
            T c = addKey[i];
            if (!dict.ContainsKey(c)) {
                if (val != null)
                    dict.Add(c, val);
                else
                    dict.Add(c, addValue[i]);
            }
        }

        return dict;
    }

    public static List<T> removeNulls<T>(this List<T> list) {
        List<T> result = new List<T>();

        foreach (T c in list)
            if (c != null)
                result.Add(c);

        return result;
    }
    public static List<component> removeNulls(this List<component> list) {
        List<component> result = new List<component>();

        foreach (component c in list)
            if (c != null && c.template != null)
                result.Add(c);

        return result;
    }
    public static Dictionary<component, Vector2Int> removeNulls(this Dictionary<component, Vector2Int> list) {
        Dictionary<component, Vector2Int> result = new Dictionary<component, Vector2Int>();

        foreach (component c in list.Keys)
            if (c != null && c.template != null)
                result.Add(c, list[c]);

        return result;
    }
    public static Dictionary<M, T> removeNulls<M, T>(this Dictionary<M, T> list) {
        Dictionary<M, T> result = new Dictionary<M, T>();

        foreach (M c in list.Keys)
            if (c != null && list[c] != null)
                result.Add(c, list[c]);

        return result;
    }
    public static Dictionary<M, List<T>> removeNulls<M, T>(this Dictionary<M, List<T>> list) {
        Dictionary<M, List<T>> result = new Dictionary<M, List<T>>();

        foreach (M c in list.Keys)
            if (c != null && list[c] != null)
                result.Add(c, list[c].removeNulls());

        return result;
    }


    public static T[] getAll<T>(this T[][] table) {
        List<T> result = new List<T>();

        foreach (T[] t in table)
            foreach(T item in t)
                result.Add(item);

        return result.ToArray();
    }

    public static bool Contains<T>(this T[] table, T element) {
        foreach (T item in table)
            if (element.Equals(item))
                return true;

        return false;
    }

    public static bool ContainsOnly<T>(this T[] table, T element) {
        foreach (T item in table)
            if (!element.Equals(item))
                return false;

        return true;
    }

    public static bool ContainsOnly<T, U>(this Dictionary<T, U> table, U element) {
        foreach (U item in table.Values)
            if (!element.Equals(item))
                return false;

        return true;
    }

    public static bool Contains<T>(this List<(T, T)> table, T ellement1 = null, T ellement2 = null) where T : class {
        foreach ((T, T) item in table)
            if ((ellement2 == null && ellement1.Equals(item.Item1)) || (ellement1 == null && ellement2.Equals(item.Item2)))
                return true;

        return false;
    }

    public static bool ContainsRange<T>(this List<T> table, List<T> range) {
        foreach (T item in table)
            if (range.Contains(item))
                return true;

        return false;
    }
    public static bool ContainsRange<T>(this T[] table, List<T> range) {
        foreach (T item in table)
            if (range.Contains(item))
                return true;

        return false;
    }
    public static bool ContainsRange<T>(this List<T> table, T[] range) {
        foreach (T item in table)
            if (range.Contains(item))
                return true;

        return false;
    }
    public static bool ContainsRange<T>(this T[] table, T[] range) {
        foreach (T item in table)
            if (range.Contains(item))
                return true;

        return false;
    }

    public static T GetElement<M, T>(this Dictionary<List<M>, T> dict, M element) where M : class where T : struct {
        foreach (List<M> list in dict.Keys)
            if (list.Contains(element))
                return dict[list];

        return new T();
    }

    public static void Add<M, T>(this Dictionary<M, T> dict, (M, T) element) => 
        dict.Add(element.Item1, element.Item2);

    public static string ToBase(this Color c, int Base, bool ignoreAlpha = false) {
        string s = "";

        s += alphanumeric[(int)Mathf.Floor((c.r * 255) / 16) % Base];
        s += alphanumeric[(int)(c.r * 255) % Base];

        s += alphanumeric[(int)Mathf.Floor((c.g * 255) / 16) % Base];
        s += alphanumeric[(int)(c.g * 255) % Base];
        
        s += alphanumeric[(int)Mathf.Floor((c.b * 255) / 16) % Base];
        s += alphanumeric[(int)(c.b * 255) % Base];
        
        if (ignoreAlpha)
            return s + "ff";

        s += alphanumeric[(int)Mathf.Floor((c.a * 255) / 16) % Base];
        s += alphanumeric[(int)(c.a * 255) % Base];

        return s;
    }

    public static Vector2Int v(this (int, int) t) => new Vector2Int(t.Item1, t.Item2);

    public static Vector2 v(this (float, float) t) => new Vector2(t.Item1, t.Item2);

    public static Vector3 v(this (float, float, float) t) => new Vector3(t.Item1, t.Item2, t.Item3);

    public static string encode(this List<componentBehaviour> list, int indent) {
        string output = "";
        string Indent = "";

        list = list.removeNulls();

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++)
            output += $"{Indent}{i}({list[i].GetType()}):" + "{\n" + list[i].encode(indent + 1) + Indent + "};\n";

        return output;
    }
    public static string encode(this List<componenttemplate> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++)
            output += $"{Indent}{i}:{list[i].name};\n"; //+ "{\n" + list[i].encode(indent + 1) + "};\n";

        return output;
    }
    public static string encode(this List<Vector2Int> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++)
            output += $"{Indent}{i}:{list[i].x} {list[i].y};\n";

        return output;
    }
    public static string encode(this Vector2Int[] list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Length; i++)
            output += $"{Indent}{i}:{list[i].x} {list[i].y};\n";

        return output;
    }
    public static string encode(this List<terrain> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++) 
            output += $"{Indent}{i}:{list[i].name};\n"; //+ "{\n" + list[i].encode(indent + 1) + "};\n";

        return output;
    }
    public static string encode(this Dictionary<baseObject.time, List<componentBehaviour>> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        foreach (baseObject.time t in baseObject.times)
            output += $"{Indent}{t}:" + "{\n" + list[t].encode(indent + 1) + Indent + "};\n";

        return output;
    }
    public static string encode(this List<string> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++)
            output += $"{Indent}{i}:{list[i]};\n";

        return output;
    }
    public static string encode(this List<int> list, int indent) {
        string output = "";
        string Indent = "";

        for (int i = 0; i < indent * 4; i++) 
            Indent += " ";

        for (int i = 0; i < list.Count; i++)
            output += $"{Indent}{i}:{list[i]};\n";

        return output;
    }

    public static void decode(this List<int> list, gameSave.element e) {
        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            list.Add(Convert.ToInt32(e.contents[i.ToString()]));
            i ++;
        }
    }
    public static void decode(this List<string> list, gameSave.element e) {
        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            list.Add(e.contents[i.ToString()]);
            i ++;
        }
    }
    public static void decode(this List<Vector2Int> list, gameSave.element e) {
        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            string s = e[i.ToString()];
            Vector2Int v = new Vector2Int();
            v = v.decode(e[i.ToString()]);
            list.Add(v);
            i ++;
        }
    }
    public static Vector2Int[] decode(this Vector2Int[] array, gameSave.element e) {
        List<Vector2Int> list = new List<Vector2Int>();

        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            string s = e[i.ToString()];
            Vector2Int v = new Vector2Int();
            v = v.decode(e[i.ToString()]);
            list.Add(v);
            i ++;
        }

        return array = list.ToArray();
    }
    public static void decode(this List<terrain> list, gameSave.element e) {
        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            if (e[i.ToString()] == "null")
                list.Add(null);
            else
                list.Add(componentManager.allTerrains.Find(c => c.name == e[i.ToString()]));
            i ++;
        }
    }
    public static void decode(this List<componenttemplate> list, gameSave.element e) {
        int i = 0;

        while (e.contents.ContainsKey(i.ToString())) {
            if (e[i.ToString()] == "null")
                list.Add(null);
            else
                list.Add(componentManager.allComponents.Find(c => c.name == e[i.ToString()]));
            i ++;
        }
    }
    public static void decode(this Dictionary<baseObject.time, List<componentBehaviour>> list, gameSave.element e) {
        list.Clear();

        foreach (baseObject.time t in baseObject.times) {
            List<componentBehaviour> l = new List<componentBehaviour>();
            l.decode(e[t.ToString()]);
            list.Add(t, l);
        }
    }
    public static Vector2Int decode(this Vector2Int v, gameSave.element e) {
        string[] s = e.This.Split(' ');
        v.x = s[0][0] == '-'? - Convert.ToInt32(s[0][1].ToString()): Convert.ToInt32(s[0]); 
        v.y = s[1][0] == '-'? - Convert.ToInt32(s[1][1].ToString()): Convert.ToInt32(s[1]); 
        return v;
    }
    public static Color decode(this Color v, gameSave.element e) {
        string[] s = e.This.Split(' ');
        v = new Color((float) Convert.ToDouble(s[0]), 
        (float) Convert.ToDouble(s[1]), 
        (float) Convert.ToDouble(s[2]),
        (float) Convert.ToDouble(s[3]));

        return v;
    }
    public static void decode(this List<componentBehaviour> list, gameSave.element e) {
        List<string> types = new List<string>();
        foreach (string s in e.contents.Keys)
            types.Add(s.Substring(s.IndexOf('(') + 1, s.LastIndexOf(')') - 1 - s.IndexOf('(')));
        
        for (int i = 0; i < types.Count; i++) {
            switch (types[i]) {
                case "animalBehaviour" : 
                    animalBehaviour a = ScriptableObject.CreateInstance<animalBehaviour>();
                    a.decode(e.contents[$"{i}(animalBehaviour)"]);
                    list.Add(a);
                break;

                case "bufferBehaviour" : 
                    bufferBehaviour b = ScriptableObject.CreateInstance<bufferBehaviour>();
                    b.decode(e.contents[$"{i}(bufferBehaviour)"]);
                    list.Add(b);
                break;

                case "comboBehaviour" : 
                    comboBehaviour c = ScriptableObject.CreateInstance<comboBehaviour>();
                    c.decode(e.contents[$"{i}(comboBehaviour)"]);
                    list.Add(c);
                break;

                case "cyclicBehaviour" : 
                    cyclicBehaviour d = ScriptableObject.CreateInstance<cyclicBehaviour>();
                    d.decode(e.contents[$"{i}(cyclicBehaviour)"]);
                    list.Add(d);
                break;

                case "diceBehaviour" : 
                    diceBehaviour f = ScriptableObject.CreateInstance<diceBehaviour>();
                    f.decode(e.contents[$"{i}(diceBehaviour)"]);
                    list.Add(f);
                break;

                case "eaterBehaviour" : 
                    eaterBehaviour g = ScriptableObject.CreateInstance<eaterBehaviour>();
                    g.decode(e.contents[$"{i}(eaterBehaviour)"]);
                    list.Add(g);
                break;

                case "giveBehaviour" : 
                    giveBehaviour h = ScriptableObject.CreateInstance<giveBehaviour>();
                    h.decode(e.contents[$"{i}(giveBehaviour)"]);
                    list.Add(h);
                break;

                case "killerBehaviour" : 
                    killerBehaviour j = ScriptableObject.CreateInstance<killerBehaviour>();
                    j.decode(e.contents[$"{i}(killerBehaviour)"]);
                    list.Add(j);
                break;

                case "moverBehaviour" : 
                    moverBehaviour k = ScriptableObject.CreateInstance<moverBehaviour>();
                    k.decode(e.contents[$"{i}(moverBehaviour)"]);
                    list.Add(k);
                break;

                case "spawnerBehaviour" : 
                    spawnerBehaviour l = ScriptableObject.CreateInstance<spawnerBehaviour>();
                    l.decode(e.contents[$"{i}(spawnerBehaviour)"]);
                    list.Add(l);
                break;

                case "specialBehaviour" : 
                    specialBehaviour m = ScriptableObject.CreateInstance<specialBehaviour>();
                    m.decode(e.contents[$"{i}(specialBehaviour)"]);
                    list.Add(m);
                break;

                case "techBehaviour" : 
                    techBehaviour n = ScriptableObject.CreateInstance<techBehaviour>();
                    n.decode(e.contents[$"{i}(techBehaviour)"]);
                    list.Add(n);
                break;
            }
        }
    }

    public static List<B> GetAll<A, B>(this Dictionary<A, B> dict, List<A> list) {
        List<B> l = new List<B>();

        foreach (A a in list)
            l.Add(dict[a]);
        
        return l;
    }

    public static void squareClock(this SpriteShapeController s, float t, float size = 80) {
        t = Mathf.Clamp(t, 0, 1);
        t = (int) (t * 100) / 100f;
        
        s.spline.Clear();

        s.spline.InsertPointAt(0, Vector3.zero);
        s.spline.InsertPointAt(0, (0f, size, 0f).v());

        float corners = .125f;
        Vector2 last = (0, 0).v();
        while (t >= corners) {
            last = dir[Mathf.FloorToInt((corners + .125f) * 4) - 1] * size;
            s.spline.InsertPointAt(s.spline.GetPointCount() - 1, last);

            corners += .25f;
        }
        
        corners -= .125f;
        if (corners > .125f)
            corners -= .125f;

        t -= corners;
        if (t == 0)
            return;
        
        if (corners == 0)
            s.spline.InsertPointAt(s.spline.GetPointCount() - 1, (t * 8 * size, size).v());
        else {
            Vector2 v = (Mathf.Sin((corners + .375f) * 2 * Mathf.PI), Mathf.Cos((corners + .375f) * 2 * Mathf.PI)).v() * size * 2;
            s.spline.InsertPointAt(s.spline.GetPointCount() - 1, last + v * t * 4);
        }
    }

    public static string simplify(this string s) => s.ToLower().Replace(".", "").Replace(",", "").Replace("\n", "");

    public static List<A> keysOf<A, B>(this Dictionary<A, B> dict, B ellement) {
        List<A> result = new List<A>();

        foreach (A key in dict.Keys)
            if (dict[key].Equals(ellement))
                result.Add(key);
        
        return result;
    }

    public static float Total<A>(this Dictionary<A, float> dict) {
        float f = 0;

        foreach (float F in dict.Values)
            f += F;

        return f;
    }
    public static Vector2 Total<A>(this Dictionary<A, Vector2> dict) {
        Vector2 f = (0, 0).v();

        foreach (Vector2 F in dict.Values)
            f += F;

        return f;
    }

    public static float growth(this AnimationCurve anim, float time, float dir = .02f) {
        if (time - dir < 0)
            return anim.Evaluate(time);

        return anim.Evaluate(time) - anim.Evaluate(time - dir);
    }

    public static void AddAll<A, B>(this Dictionary<A, List<B>> dict, Dictionary<A, List<B>> add) {
        foreach (A a in dict.Keys)
            if (add.ContainsKey(a))
                dict[a].AddRange(add[a]);
    }

    public static float nextMultOf(this float x, float n) => x + n - (x % n);

    public static List<Transform> GetChildren(this Transform t) {
        List<Transform> l = new List<Transform>();

        for (int i = 0; i < t.childCount; i ++)
            l.Add(t.GetChild(i));

        return l;
    }

    public static void SetActiveSelf(this GameObject g, bool b) {
        List<bool> previousActive = new List<bool>();
        List<Transform> list = g.transform.GetChildren();
        foreach (Transform t in list)
            previousActive.Add(t.gameObject.activeSelf);
            
        g.SetActive(b);

        for (int i = 0; i < list.Count; i ++)
            list[i].gameObject.SetActive(!b);
    }

    public static void changeBaseColor(this Button b, Color c) {
        ColorBlock C = new ColorBlock();

        C.disabledColor = b.colors.disabledColor;
        C.highlightedColor = b.colors.highlightedColor;
        C.selectedColor = b.colors.selectedColor;
        C.pressedColor = b.colors.pressedColor;
        C.normalColor = c;
        C.fadeDuration = b.colors.fadeDuration;
        C.colorMultiplier = b.colors.colorMultiplier;

        b.colors = C;
    }

    public static int[] IndexesOf(this string s, char c) {
        List<int> result = new List<int>();

        for (int i = 0; i < s.Length; i ++)
            if (s[i] == c)
                result.Add(i);

        return result.ToArray();
    }

    public static int IndexOf<T>(this T[] array, T t) {
        for (int i = 0; i < array.Length; i++)
            if (array[i].Equals(t))
                return i;

        return -1;
    }

    public static T[] ToArray<T, M>(this Dictionary<T, M>.KeyCollection d) {
        List<T> l = new List<T>();

        Dictionary<T, M>.KeyCollection.Enumerator e = d.GetEnumerator();
        for (int i = 0; i < d.Count; i++) {
            l.Add(e.Current);
            e.MoveNext();
        }

        return l.ToArray();
    }
    public static M[] ToArray<T, M>(this Dictionary<T, M>.ValueCollection d) {
        List<M> l = new List<M>();

        Dictionary<T, M>.ValueCollection.Enumerator e = d.GetEnumerator();
        for (int i = 0; i < d.Count; i++) {
            l.Add(e.Current);
            e.MoveNext();
        }

        return l.ToArray();
    }

    public static string[] Split(this string s, char split, params (char, char)[] ignoreBetween) {
        string fill = "";
        Dictionary<int, int> nest = new Dictionary<int, int>();
        List<string> result = new List<string>();

        for (int i = 0; i < ignoreBetween.Length; i++) 
            nest.Add(i, 0);

        for (int i = 0; i < s.Length; i ++) {
            for (int j = 0; j < ignoreBetween.Length; j++) {
                if (s[i] == ignoreBetween[j].Item1)
                    nest[j] ++;
                else if (s[i] == ignoreBetween[j].Item2) {
                    nest[j] --;
                    if (nest[j] < 0)
                        nest[j] = 0;
                }
            }

            if (s[i] == split && nest.ContainsOnly(0)) {
                result.Add(fill);
                fill = "";
            } else
                fill += s[i];
        }

        if (fill != "")
            result.Add(fill);

        return result.ToArray();
    }
    public static int IndexOf(this string s, char of, params (char, char)[] ignoreBetween) {
        Dictionary<int, int> nest = new Dictionary<int, int>();

        for (int i = 0; i < ignoreBetween.Length; i++) 
            nest.Add(i, 0);

        for (int i = 0; i < s.Length; i ++) {
            for (int j = 0; j < ignoreBetween.Length; j++) {
                if (s[i] == ignoreBetween[j].Item1)
                    nest[j] ++;   
                else if (s[i] == ignoreBetween[j].Item2) {
                    nest[j] --;
                    if (nest[j] < 0)
                        nest[j] = 0;
                }
            }

            if (s[i] == of && nest.ContainsOnly(0))
                return i;
        }

        return -1;
    }
    public static int LastIndexOf(this string s, char of, params (char, char)[] ignoreBetween) {
        Dictionary<int, int> nest = new Dictionary<int, int>();
        for (int i = 0; i < ignoreBetween.Length; i++) 
            nest.Add(i, 0);

        int result = -1;

        for (int i = 0; i < s.Length; i ++) {
            for (int j = 0; j < ignoreBetween.Length; j++) {
                if (s[i] == ignoreBetween[j].Item1)
                    nest[j] ++;
                else if (s[i] == ignoreBetween[j].Item2) {
                    nest[j] --;
                    if (nest[j] < 0)
                        nest[j] = 0;
                }
            }

            if (s[i] == of && nest.ContainsOnly(0))
                result = i;
        }

        return result;
    }
    public static bool Contains(this string s, char contain, params (char, char)[] ignoreBetween) {
        Dictionary<int, int> nest = new Dictionary<int, int>();

        for (int i = 0; i < ignoreBetween.Length; i++) 
            nest.Add(i, 0);

        for (int i = 0; i < s.Length; i ++) {
            for (int j = 0; j < ignoreBetween.Length; j++) {
                if (s[i] == ignoreBetween[j].Item1)
                    nest[j] ++;
                else if (s[i] == ignoreBetween[j].Item2) {
                    nest[j] --;
                    if (nest[j] < 0)
                        nest[j] = 0;
                }
            }

            if (s[i] == contain && nest.ContainsOnly(0))
                return true;
        }

        return false;
    }

    public static Vector2 inverseTransformPoint(this Transform t, Vector2 pos) => 
        t.InverseTransformPoint(SceneManager.GetActiveScene().GetRootGameObjects()[0].transform.TransformPoint(pos));
}

[System.Serializable]
public class ListWrapper<T> {
    public List<T> InnerList;

    public ListWrapper() => InnerList = new List<T>();

    public T this[int i] {
        get {
           return InnerList[i];
        } 
    }

    public int Count {
        get {
            return InnerList.Count;
        }
    }

    public static implicit operator List<T>(ListWrapper<T> l) => 
        l.InnerList;
}

/*
public static class packTextureCreator {
    public static Sprite Create(Color[] colors, int width = 512, int height = 512)  {
        if (colors.Length == 0) return null;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color c;
        Vector2 pos;
        float w = width / 2f, h = height / 2f;

        for (int x = 0; x < width; x ++)
            for (int y = 0; y < height; y ++) {
                pos = (x - w, -y + h).v();
                float angle = Vector2.SignedAngle(Vector2.down, pos) / 360 + .5f;
                if ((int) angle > 0)
                    angle = 0;
                c = colors[Mathf.FloorToInt(angle * colors.Length)];
                c.a = .5f - .5f * (Mathf.Clamp(pos.magnitude, 1, w) / w);
                tex.SetPixel(x, y, c);
            }

        tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, width, height), (.5f, .5f).v());
        s.name = "pack";
        return s;
    }
}

public static class PNGResizer {
    public static string resources = "/Users/gaellequartierditmaire/Documents/unity projects/New Unity Project/Assets/Resources";
    public static Color[] clear;
    
    public static void resize(string path, int size = 512) {
        path = path.Replace(resources + "/", "").Replace(".png", "");
        Texture2D oldTex = Resources.Load<Texture2D>(path);
        if (oldTex.width == 512 && oldTex.height == 512)
            return;
        Texture2D newTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        int oldSize = Mathf.Max(oldTex.width, oldTex.height);

        for (int x = 0; x < size; x ++)
            for (int y = 0; y < size; y ++)
                newTex.SetPixel(x, y, 
                    x < (oldSize - oldTex.width) / 2 || y < (oldSize - oldTex.height) / 2 || x > oldSize - oldTex.width || y < oldSize - oldTex.height ? 
                    Color.clear : oldTex.GetPixel(Mathf.RoundToInt(x / (float) size * oldSize), Mathf.RoundToInt(y / (float) size * oldSize)));
        
            File.Move(resources + "/" + path + ".png", resources + "/" + path + " save.png");
        File.WriteAllBytes($"/Users/gaellequartierditmaire/Documents/unity projects/New Unity Project/Assets/Resources/{path}.png", newTex.EncodeToPNG());
    }

    public static void resizeInternal(string path, bool blackOnly = false, int margin = 0) {
        path = path.Replace(resources + "/", "").Replace(".png", "");
        Texture2D oldTex = Resources.Load<Texture2D>(path);
        if (path.Contains("save") || !oldTex.isReadable || oldTex.width != 512 || oldTex.height != 512 || (margin == 0 &&
            (Array.Find(oldTex.GetPixels(margin, 0, 1, 512), c => c.a > .5f) != new Color() || Array.Find(oldTex.GetPixels(0, margin, 512, 1), c => c.a > .5f) != new Color())))
            return;

        int s = 0, e = 511;
        for (int x = 0; x < 512; x ++)
            if (Array.Find(oldTex.GetPixels(x, 0, 1, 512), c => c.a > .5f) != new Color()) {
                s = x;
                break;
            }
        for (int y = 0; y < 512; y ++)
            if (Array.Find(oldTex.GetPixels(0, y, 512, 1), c => c.a > .5f) != new Color()) {
                s = Mathf.Min(s, y);
                break;
            }
        for (int x = 511; x >= 0; x --)
            if (Array.Find(oldTex.GetPixels(x, 0, 1, 512), c => c.a > .5f) != new Color()) {
                e = x;
                break;
            }
        for (int y = 511; y >= 0; y --)
            if (Array.Find(oldTex.GetPixels(0, y, 512, 1), c => c.a > .5f) != new Color()) {
                e = Mathf.Max(e, y);
                break;
            }

        Color col;
        Texture2D newTex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        newTex.SetPixels(clear);
        for (int x = margin; x < 512 - margin; x ++)
            for (int y = margin; y < 512 - margin; y ++)
                newTex.SetPixel(x, y, (col = oldTex.GetPixel(
                    Mathf.RoundToInt(s + (x - margin) / (512f - 2 * margin) * (e - s)) ,
                    Mathf.RoundToInt(s + (y - margin) / (512f - 2 * margin) * (e - s)) )));
                    //.a > .5f ? (blackOnly ? Color.black : col) : Color.clear);
        
        File.Move($"{resources}/{path}.png", $"{resources}/{path} save.png");
        File.WriteAllBytes($"{resources}/{path}.png", newTex.EncodeToPNG());
    }

    public static Color average(Texture2D texture, float x, float y, float step) {
        float dX = x % step, dY = y % step;
        
        List<(Color, float)> cols = new List<(Color, float)>();
        for (int stepX = Mathf.CeilToInt(step); stepX > -1; stepX --)
            for (int stepY = Mathf.CeilToInt(step); stepY > -1; stepY --)
                ;//cols.Add((texture.GetPixel(stepX, stepY), ));

        /*texture.GetPixel(x.nextMultOf(step), y.nextMultOf(step)) * (step - dX) + 
        //texture.GetPixel(Mathf.FloorToInt(x), Mathf.CeilToInt(y)) * (1 - dX) / 2 + 
        //texture.GetPixel(Mathf.CeilToInt(x), Mathf.FloorToInt(y)) * (1 - dX) / 2 +
        texture.GetPixel(x.nextMultOf(step), y.nextMultOf(step)) * dX;

        Color col = Color.clear;
        foreach ((Color, float) c in cols)
            col += c.Item1 * c.Item2;

        return col;
    }

    public static void recurseResize(string Path, bool blackOnly = false, int margin = 0) {
        if (Path != resources)
            foreach (string path in Directory.GetFiles(Path))
                if (path.EndsWith(".png") && !path.EndsWith("save.png"))
                    PNGResizer.resizeInternal(path, blackOnly, margin);
        foreach (string path in Directory.GetDirectories(Path)) 
            if (!path.Contains("terrains"))
                recurseResize(path, blackOnly, margin);
    }
}*/