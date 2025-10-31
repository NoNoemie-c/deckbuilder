using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new component", menuName = "components")]
public class componenttemplate : baseObject {
    public override void setTo(baseObject c) {
        if (c is componenttemplate)
            c = c as componenttemplate;
        else throw new ArgumentException("componenttemplate attempted setTo with a non-componenttemplate");

        name = c.name;
        description = c.description;

        crank = c.crank;
        coin = c.coin;
        symbol = c.symbol;
        pack = c.pack;
        tags = c.tags;
        if (!tags.Contains(name)) {
            tags = new List<string>{name};
            tags.AddRange(c.tags);
        }

        packTexture = c.packTexture;

        rarity = c.rarity;

        behaviours = copyBehaviours(c.behaviours);

        conductive = c.conductive;
        electricGenerator = c.electricGenerator;
        electricBehaviours = copyBehaviours(c.electricBehaviours);

        coinsOnSpawn = c.coinsOnSpawn;
        cranksOnSpawn = c.cranksOnSpawn;
        spawnBehaviours = copyBehaviours(c.spawnBehaviours);

        coinOnDestroy = c.coinOnDestroy;
        deathBehaviours = copyBehaviours(c.deathBehaviours);

        Debug.Log(c.rotation);
        rotation = c.rotation;
    }

    public override string encode(int indent) {
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
        s += $"{Indent}rotation:{rotation};\n";

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
        rotation = (float) Convert.ToDouble(e["rotation"]);
    }

    public static componenttemplate operator +(componenttemplate c1, componenttemplate c2) { 
        componenttemplate c = ScriptableObject.CreateInstance<componenttemplate>();
        c.setTo(c1);

        c.name += " " + c2.name;
        c.description += "\n" + c2.description;

        c.crank += c2.crank;
        c.coin += c2.coin;

        c.pack.AddSingle(c2.pack);
        c.tags.AddSingle(c2.tags);

        c.rarity = metaData.maxRarity;

        foreach (time t in times)
            c.behaviours[t].AddRange(copyBehaviours(c2.behaviours[t]));

        c.conductive = c.conductive || c2.conductive;
        c.electricGenerator = c.electricGenerator || c2.electricGenerator;
        c.electricBehaviours.AddRange(copyBehaviours(c2.electricBehaviours));

        c.coinsOnSpawn += c2.coinsOnSpawn;
        c.cranksOnSpawn += c2.cranksOnSpawn;
        c.spawnBehaviours.AddRange(copyBehaviours(c2.spawnBehaviours));

        c.coinOnDestroy += c2.coinOnDestroy;
        c.deathBehaviours.AddRange(copyBehaviours(c2.deathBehaviours));
        
        return c;
    }

    public static componenttemplate operator *(componenttemplate c1, int scalar) { 
        componenttemplate c = c1;

        while (scalar > 1) {
            scalar --;
            c += c1;
        }

        return c;
    }

    
}