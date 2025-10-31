using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainModule : MonoBehaviour
{
    public bool terrainEnabled;
    public int amount = 0;
    public int maxBaseTerrain = 3;
    public int maxTerrain = 10;
    public int maxNonSuperposableTerrain = 5;
    private int index = 0;
    private int nonSupAmount = 0;
    private Vector2Int[] dirs = {new Vector2Int(0, 1), 
                                 new Vector2Int(1, 1), 
                                 new Vector2Int(1, 0), 
                                 new Vector2Int(1, -1), 
                                 new Vector2Int(0, -1), 
                                 new Vector2Int(-1, -1), 
                                 new Vector2Int(-1, 0), 
                                 new Vector2Int(-1, 1)};
    public float corners = .5f, borders = .45f, middles = .4f;
    private List<List<float>> spawnCoeff;

    void awake() {
        spawnCoeff = new List<List<float>>();
        for (int x = 0; x < componentManager.size.x; x++) {
            spawnCoeff.Add(new List<float>());
            for (int y = 0; y < componentManager.size.y; y++) {
                spawnCoeff[x].Add(0);
                if (x == 0 || y == 0 || x == componentManager.size.x-1 || y == componentManager.size.y-1) {
                    if ((x == 0 || y == 0) && (x == componentManager.size.x-1 || y == componentManager.size.y-1))
                        spawnCoeff[x][y] = corners;
                    else
                        spawnCoeff[x][y] = borders;
                } else
                    spawnCoeff[x][y] = middles;
            }
        }
    }

    public terrain[][] RandomiseTerrain(List<Vector2Int> avoid, int MaxTerrain) {
        maxTerrain = MaxTerrain;
        
        awake();

        terrain[][] t = new terrain[componentManager.size.x][];
        for (int i = 0; i < t.Length; i++)
            t[i] = new terrain[componentManager.size.y];

        if (terrainEnabled && componentManager.allTerrains.Count > 0 && clicker.tuto == 0) {
            terrain possible;
            
            // primary gen
            for (int x = 0; x < componentManager.size.x; x++) {
                for (int y = 0; y < componentManager.size.y; y++) {
                    if (avoid.Contains((x, y).v()))
                        continue;

                    possible = componentManager.allTerrains[index];
                    if (Random.Range(0, 1f) > spawnCoeff[x][y]) {
                        if (possible.probability > Random.Range(0, 1f)) {
                            t[x][y] = possible;
                            amount ++;
                            if (!possible.superposable)
                                nonSupAmount ++;
                        } else {
                            index ++;
                            index %= componentManager.allTerrains.Count;
                        }
                    }

                    if (amount < maxBaseTerrain)
                        continue;

                    break;
                }

                if (amount < maxBaseTerrain)
                        continue;

                break;
            }

            // secundary gen
            for (int x = 0; x < componentManager.size.x; x++) {
                for (int y = 0; y < componentManager.size.y; y++) {
                    if (t[x][y] != null || avoid.Contains((x, y).v())) 
                        continue;
                    
                    possible = null;
                    index = 0;
                    
                    while (possible == null && index < 8) {
                        Vector2Int next = new Vector2Int(x + dirs[index].x, y + dirs[index].y);
                        if (next.x >= 0 && next.x < t.Length && next.y >= 0 && next.y < t[0].Length)
                            possible = t[next.x][next.y];

                        index ++;
                    }

                    if (possible != null)
                        if (possible.probability > Random.Range(0, 1f) / 2) {
                            t[x][y] = possible;
                            amount ++;
                            if (!possible.superposable)
                                nonSupAmount ++;
                        }

                    if (amount < maxTerrain)
                        continue;
                    if (nonSupAmount < maxNonSuperposableTerrain)
                        continue;

                    break;
                }

                if (amount < maxTerrain)
                        continue;
                if (nonSupAmount < maxNonSuperposableTerrain)
                        continue;

                break;
            }
        }

        return t;
    }
}
