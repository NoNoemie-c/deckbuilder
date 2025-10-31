using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class gameSave : ScriptableObject
{
    public List<pack> current;
    public List<string> Current;
    public int coins, movers, rerolls, removes, highScore;
    public int turns, actual, destroyId;
    public Vector2Int[] rent, destroys;
    public List<Vector2Int> historicCoin;
    [TextArea(1, 1000)]
    public string data;

    public class element {
        public Dictionary<string, element> contents;
        public string This;

        public element() =>
            contents = new Dictionary<string, element>();

        public element this[string key] => 
            contents[key];
        public static implicit operator string(element e) => 
            e.This;
    }

    public void reset() {
        rent = null;
        destroys = null;
        historicCoin = new List<Vector2Int>();
        turns = actual = 0;
        coins = movers = removes = rerolls = highScore = 0;
        data = "";
    }

    public void Save() {
        string output = "";

        int count = 0;

        output += "components:{\n";
        foreach (component c in componentManager.GetAll()) {
            output += $"    {count}:"; 
            count ++;

            if (c.template == null || componentManager.toDestroy.Contains(c.gameObject)) {
                output += "null;\n";
                continue;
            }

            output += "{\n" + $"{c.template.encode(2)}" + "    };\n";
        }  
        output += "};\n";

        output += "terrains:{\n";
        count = 0;
        foreach (component c in componentManager.GetAll()) {
            output += $"    {count}:";
            count ++;

            if (c.under == null) {
                output += "null;\n";
                continue;
            }

           output += "{\n" + $"{c.under.encode(2)}" + "    };\n";
        }  
        output += "};\n";

        output += "proposal:{\n";
        count = 0;
        for (int i = 0; i < shop.proposal.Length; i++){
            output += $"    {count}:";   
            count ++;

            output += "{\n" + $"{shop.proposal[i].template.encode(2)}" + "    };\n";
        }
        output += "}\n";
        
        data = output;
        
        movers = var.movers;
        rerolls = var.rerolls;
        removes = var.removes;
        rent = componentManager.Rent;
        destroys = componentManager.destroys;
        destroyId = componentManager.destroyId;
        historicCoin = var.historicCoin;
        highScore = var.highScore;
        if (componentManager.updating) {
            int coinsAtTurn = 0;
            if (componentManager.updating)
                foreach (component c in componentManager.GetAll().removeNulls())
                    coinsAtTurn += c.coin;
            coins = var.coins + coinsAtTurn;
            turns = componentManager.turns + 1;
            actual = componentManager.actual + ((turns == componentManager.Rent[componentManager.actual].y)? 1 : 0);
        } else {
            coins = var.coins;
            turns = componentManager.turns;
            actual = componentManager.actual;
        }
    }

    public void Load() {
        element Data = new element();
        foreach (string line in data.Split(';', ('{', '}')))
            Data.contents.Add(decodeLine(line));

        for (int x = 0; x < componentManager.size.x; x ++)
            for (int y = 0; y < componentManager.size.y; y ++) {
                if (Data["components"][$"{y + x * componentManager.size.y}"] == "null")
                    continue;

                componenttemplate temp = ScriptableObject.CreateInstance<componenttemplate>();
                temp.decode(Data["components"][$"{y + x * componentManager.size.y}"]);
                componentManager.addComponentDiscrete((x, y).v(), temp);
            }
        for (int x = 0; x < componentManager.size.x; x ++)
            for (int y = 0; y < componentManager.size.y; y ++) {
                if (Data["terrains"][$"{y + x * componentManager.size.y}"] == "null")
                    continue;

                terrain temp = ScriptableObject.CreateInstance<terrain>();
                temp.decode(Data["terrains"][$"{y + x * componentManager.size.y}"]);
                componentManager.addTerrain((x, y).v(), temp);
            }
        shop.proposal = new component[3];
        for (int i = 0; i < shop.proposal.Length; i++) {
            componenttemplate temp = ScriptableObject.CreateInstance<componenttemplate>();
            temp.decode(Data["proposal"][i.ToString()]);
            shop.addComponent(temp, i);
        }

        var.coins = coins;
        var.movers = movers;
        var.rerolls = rerolls;
        var.removes = removes;
        componentManager.actual = actual;
        componentManager.turns = turns;
        componentManager.Rent = rent;
        componentManager.destroys = destroys;
        componentManager.destroyId = destroyId;
        var.historicCoin = historicCoin;
        var.highScore = highScore;
    }

    public (string, element) decodeLine(string line) {
        element e = new element();

        //Debug.Log(line);

        line = line.Replace("\n", "").Replace("    ", "");

        string[] tokens = line.Split(':', ('^', '¨'), ('{', '}'));
        if (tokens.Length > 2) {
            string s = tokens[1];
            for (int i = 2; i < tokens.Length; i++)
                s += ":" + tokens[i];

            tokens = new string[2]{tokens[0], s};
        }

        if (!tokens[1].Contains('{', ('^', '¨'))) {
            e.This = tokens[1];
        } else if (tokens[1].Contains('}', ('^', '¨'))) {
            int bracketSIndex = tokens[1].IndexOf('{', ('^', '¨')) + 1, bracketEIndex = tokens[1].LastIndexOf('}', ('^', '¨'));
            string[] lines = tokens[1].Split(';', ('^', '¨'), ('{', '}'));
            if (bracketEIndex > bracketSIndex)
                lines = tokens[1].Substring(bracketSIndex, bracketEIndex - bracketSIndex).Split(';', ('^', '¨'), ('{', '}'));

            for (int i = 0; i < lines.Length; i++)
                if (lines[i] != "{}" && lines[i] != "")
                    e.contents.Add(decodeLine(lines[i]));
        }
        
        return (tokens[0], e);
    }
}


