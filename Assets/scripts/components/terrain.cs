using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "terrain", fileName = "new terrain")]
public class terrain : baseObject
{
    [Space(5)] public float probability;

    [Space(5)] public bool removable;
    public bool superposable;

    [Space(5)] public int localMultiplier = 1;

    [Space(10)] public Color color;

    public override void setTo(baseObject c) {
        terrain t = c as terrain;
        name = t.name;
        description = t.description;

        crank = t.crank;
        coin = t.coin;
        symbol = t.symbol;
        tags = t.tags;
        if (!tags.Contains(name))
            tags.Add(name);

        rarity = t.rarity;

        behaviours = copyBehaviours(t.behaviours);

        conductive = t.conductive;
        electricGenerator = t.electricGenerator;
        electricBehaviours = copyBehaviours(t.electricBehaviours);

        coinsOnSpawn = t.coinsOnSpawn;
        cranksOnSpawn = t.cranksOnSpawn;
        spawnBehaviours = copyBehaviours(t.spawnBehaviours);

        coinOnDestroy = t.coinOnDestroy;
        deathBehaviours = copyBehaviours(t.deathBehaviours);

        removable = t.removable;
        superposable = t.superposable;
        localMultiplier = t.localMultiplier;
        probability = t.probability;
        color = t.color;
    }

    public override string encode(int indent) {
        //setTo(this);
        
        string Indent = "";
        for (int i = 0; i < indent * 4; i++)
            Indent += " ";

        string s = "";
        s += $"{Indent}name:{name};\n";
        s += $"{Indent}description:^{description}¨;\n";
        s += $"{Indent}crank:{crank};\n";
        s += $"{Indent}coin:{coin};\n";
        s += $"{Indent}symbol:{name};\n";
        s += $"{Indent}pack:" + "{\n" + pack.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}tags:" + "{\n" + tags.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}packTexture:{name};\n";
        s += $"{Indent}rarity:{rarity};\n";
        s += $"{Indent}behaviours:" + "{\n" + behaviours.removeNulls().encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}conductive:{conductive};\n";
        s += $"{Indent}electricGenerator:{electricGenerator};\n";
        s += $"{Indent}electricBehaviours:" + "{\n" + electricBehaviours.removeNulls().encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}coinsOnSpawn:{coinsOnSpawn};\n";
        s += $"{Indent}cranksOnSpawn:{cranksOnSpawn};\n";
        s += $"{Indent}spawnBehaviours:" + "{\n" + spawnBehaviours.removeNulls().encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}coinOnDestroy:{coinOnDestroy};\n";
        s += $"{Indent}deathBehaviours:" + "{\n" + deathBehaviours.removeNulls().encode(indent + 1) + Indent + "};\n";

        s += $"{Indent}removable:{removable};\n";
        s += $"{Indent}superposable:{superposable};\n";
        s += $"{Indent}localMultiplier:{localMultiplier};\n";
        s += $"{Indent}probability:{probability};\n";
        s += $"{Indent}color:{color.r} {color.g} {color.b} {color.a};\n";
    
        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        description = e["description"].This.Substring(1, e["description"].This.Length - 2);
        crank = Convert.ToInt32(e["crank"]);
        coin = Convert.ToInt32(e["coin"]);
        if (componentManager.allSprites.ContainsKey(e["symbol"]))
            symbol = componentManager.allSprites[e["symbol"]];
        else
            symbol = null;
        pack.decode(e["pack"]);
        tags.decode(e["tags"]);
        if (componentManager.allPackTextures.ContainsKey(e["packTexture"]))
            packTexture = componentManager.allPackTextures[e["packTexture"]];
        else 
            packTexture = null;
        rarity = Convert.ToInt32(e["rarity"]);
        behaviours.decode(e["behaviours"]);
        conductive = Convert.ToBoolean(e["conductive"]);
        electricGenerator = Convert.ToBoolean(e["electricGenerator"]);
        electricBehaviours.decode(e["electricBehaviours"]);
        coinsOnSpawn = Convert.ToInt32(e["coinsOnSpawn"]);
        cranksOnSpawn = Convert.ToInt32(e["cranksOnSpawn"]);
        spawnBehaviours.decode(e["spawnBehaviours"]);
        coinOnDestroy = Convert.ToInt32(e["coinOnDestroy"]);
        deathBehaviours.decode(e["deathBehaviours"]);

        removable = Convert.ToBoolean(e["removable"]);
        superposable = Convert.ToBoolean(e["superposable"]);
        localMultiplier = Convert.ToInt32(e["localMultiplier"]);
        probability = (float) Convert.ToDouble(e["probability"]);
        color = color.decode(e["color"]);
    }

    public override bool Activate(Vector2Int pos) {
        bool b = false;

        component c = componentManager.getComponent(pos);

        c.gainCoins(c.coin * (localMultiplier-1));

        foreach (time t in times)
            foreach (componentBehaviour behaviour in behaviours[t])
                if (behaviour.isActive)
                    if (behaviour.activate(pos)) 
                        b = true;

        if (c.isPowered)
            foreach (componentBehaviour behaviour in electricBehaviours)
                b = behaviour.activate(componentManager.GetPosition(c));

        clicked = false;
        
        return b;
    }

    public static terrain operator +(terrain c1, terrain c2) { 
        c1.description += "\n" + c2.description;

        c1.crank += c2.crank;
        c1.coin += c2.coin;

        c1.pack.AddSingle(c2.pack);
        c1.tags.AddSingle(c2.tags);

        c1.rarity = metaData.maxRarity;

        foreach (time t in times)
            c1.behaviours[t].AddRange(copyBehaviours(c2.behaviours[t]));

        c1.conductive = c1.conductive || c2.conductive;
        c1.electricGenerator = c1.electricGenerator || c2.electricGenerator;
        c1.electricBehaviours.AddRange(copyBehaviours(c2.electricBehaviours));

        c1.coinsOnSpawn += c2.coinsOnSpawn;
        c1.cranksOnSpawn += c2.cranksOnSpawn;
        c1.spawnBehaviours.AddRange(copyBehaviours(c2.spawnBehaviours));

        c1.coinOnDestroy += c2.coinOnDestroy;
        c1.deathBehaviours.AddRange(copyBehaviours(c2.deathBehaviours));

        c1.removable = c1.removable || c2.removable;
        c1.superposable = c1.superposable || c2.superposable;
        c1.localMultiplier *= c2.localMultiplier;
        c1.probability *= c2.probability;
        c1.color += c2.color;
        
        return c1;
    }

    public static terrain operator *(terrain c1, int scalar) { 
        terrain c = c1;

        while (scalar > 0) {
            scalar --;
            c += c1;
        }

        return c;
    }
}
