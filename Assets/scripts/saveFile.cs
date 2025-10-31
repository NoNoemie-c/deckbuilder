using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class saveFile : ScriptableObject
{   
    public struct data {
        public componenttemplate[][] starters;
        public List<string> packs;
        public List<Vector2Int> rents;
        public terrain[][] terrains;
        public List<Vector2Int> destroys;
    }

    public static List<string> Unlocked;
    public static int Progress;
    public int progress;
    public static saveFile instance;
    public pack[] unlocks;
    public List<ListWrapper<Vector2Int>> rents;
    public int terrainThreshold;
    public static bool tuto2ex;
    public bool Tuto2ex;
    public List<ListWrapper<Vector2Int>> startPoses;
    public List<ListWrapper<componenttemplate>> starters;
    public List<ListWrapper<Vector2Int>> destroys;

    public static List<string> New;

    public static void Awake() {
        Progress = instance.progress;

        Unlocked = new List<string>();
        for (int i = 0; i <= Progress; i ++)
            if (instance.unlocks[i] != null)
                Unlocked.Add(instance.unlocks[i].name);
        if (Progress > 1)
            Unlocked.AddSingle("electric");
        if (Progress >= instance.terrainThreshold)
            Unlocked.AddSingle("terrains");

        tuto2ex = instance.Tuto2ex;

        New = new List<string>();
    }

    public static void save(int i) {
        Progress = Mathf.Max(Progress, i);

        Progress = Mathf.Min(Progress, 19);

        Unlocked = new List<string>();
        for (int j = 0; j <= Progress; j ++)
            if (instance.unlocks[j] != null)
                Unlocked.Add(instance.unlocks[j].name);
        if (Progress > 1)
            Unlocked.AddSingle("electric");
        if (Progress >= instance.terrainThreshold)
            Unlocked.AddSingle("terrains");

        instance.progress = Progress;

        instance.Tuto2ex = tuto2ex || instance.Tuto2ex;
    }

    public static void next() {
        if (Progress > 12) {
            SceneManager.LoadScene("credits");
            return;
        }

        var.transform.GetComponent<var>().enabled = false;
        componentManager.transform.GetComponent<componentManager>().enabled = false;
        shop._Transform.GetComponent<shop>().enabled = false;

        if (instance.unlocks[Progress] == null) {
            New = new List<string>();
            coroutiner.start(clicker.winScreen());
            return;
        }
            
        New = new List<string>{instance.unlocks[Progress].name};
        if (Progress == 2)
            New.Add("electric");

        coroutiner.start(clicker.winScreen());
    }

    public static data generate(int level, int difficulty = 0, int length = 70, int rentLengthMin = 2, int rentLengthMax = 30, float destroyQuantity = .4f, int destroyAmount = 3, int terrainAmount = 5, List<string> packs = null) {
        data d = new data();

        if (level == instance.unlocks.Length) {
            d.terrains = componentManager.transform.GetComponent<terrainModule>().RandomiseTerrain(new List<Vector2Int>(), terrainAmount);
            if (packs == null)
                d.packs = Unlocked;
            else
                d.packs = packs;
            d.starters = new componenttemplate[componentManager.size.x][];
            for (int i = 0; i < d.starters.Length; i++)
                d.starters[i] = new componenttemplate[componentManager.size.y];
            
            List<Vector2Int> R = new List<Vector2Int>(), D = new List<Vector2Int>();
            
            int prevRentLength = Random.Range(rentLengthMin, rentLengthMax +1);
            int prevRentAmount = Mathf.FloorToInt(Mathf.Pow(prevRentLength, 1.5f));
            int rentTotal = prevRentLength;
            while (rentTotal < length) {
                R.Add((prevRentAmount, prevRentLength).v());

                if (Random.Range(0, 1) > 1 / prevRentLength || rentTotal + prevRentAmount < length) {
                    prevRentLength = Mathf.Max(rentLengthMin, Random.Range(rentLengthMin, rentLengthMax - Mathf.FloorToInt(difficulty / 2f) +1));
                    if (rentLengthMin + rentTotal > length)
                        prevRentLength = length - rentTotal;
                    while (prevRentLength + rentTotal > length)
                        prevRentLength = Random.Range(rentLengthMin, rentLengthMax +1);
                }

                prevRentAmount = Mathf.FloorToInt(Mathf.Pow(prevRentLength + difficulty, 1.5f) * rentTotal / prevRentLength) + Random.Range(-5, 5 +1);

                rentTotal += prevRentLength;
            }

            int RCount = 0;
            int DLength = 0;
            int DAmount = 0;
            foreach (Vector2Int v in R) {
                if (RCount > Random.Range(Mathf.FloorToInt(rentTotal / (float) destroyAmount - destroyAmount), Mathf.CeilToInt(rentTotal / (float)destroyAmount - destroyAmount) +1)) {
                    DLength = RCount;
                    RCount = 0;

                    DAmount = Mathf.Max(1, Mathf.FloorToInt(DLength * destroyQuantity) + Random.Range(-2, 1 +1));

                    R.Add((DAmount, DLength).v());
                }

                RCount += v.y;
            }

            d.rents = R;
            d.destroys = D;

            return d;
        }

        if (packs == null)
            d.packs = Unlocked;
        else
            d.packs = packs;
        d.rents = instance.rents[level];

        d.starters = new componenttemplate[componentManager.size.x][];
        for (int x = 0; x < d.starters.Length; x++)
            d.starters[x] = new componenttemplate[componentManager.size.y];
        for (int i = 0; i < instance.starters[level].Count; i++)
            d.starters[instance.startPoses[level][i].x][instance.startPoses[level][i].y] = instance.starters[level][i];

        d.destroys = instance.destroys[level];

        d.terrains = componentManager.transform.GetComponent<terrainModule>().RandomiseTerrain(instance.startPoses[level], terrainAmount);

        return d;
    }
}
