using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component buffer behaviour", menuName = "buffer behaviour")]
public class bufferBehaviour : componentBehaviour {

    [Space(10)]
    public bool adjacencyRequirement = true;
    public int multiplier = 1;
    public int diviser = 1;
    public int increaser = 0;

    [Space(5)]
    public List<componenttemplate> possibilities = new List<componenttemplate>();

    public override bool activate(Vector2Int pos) {
        bool b = false;

        List<component> list;
        if (adjacencyRequirement) 
            list = componentManager.getAdjacents(pos);
        else
            list = componentManager.GetAll();

        foreach (component c in list.removeNulls())
            if (canTarget(c.template)) {
                if (increaser != 0)
                    c.gainCoins(increaser);
                if (multiplier != 1)
                    c.gainCoins(multiplier);
                if (diviser != 1)
                    c.gainCoins(1 / diviser);

                    
                b = true;
            }

        if (b) 
            Anim(componentManager.getComponent(pos), GetType().ToString());

        return b && triggerAnim;
    }

    private bool canTarget(componenttemplate c) {
        if (possibilities.Count < 1) 
            return true;

        foreach (componenttemplate comp in possibilities)
            if (c.tags.Contains(comp.name) || c == componentManager.polymorph)
                return true;

        return false;
    }

    public override bool click(Vector2Int pos) => false;

    public override componentBehaviour copy() {
        bufferBehaviour c = ScriptableObject.CreateInstance<bufferBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;
        
        c.adjacencyRequirement = adjacencyRequirement;
        c.multiplier = multiplier;
        c.increaser = increaser;
        c.diviser = diviser;
        c.possibilities = possibilities.removeNulls();
        
        return c;
    }

    public override string encode(int indent) {
        string Indent = "";
        for (int i = 0; i < indent * 4; i++)
            Indent += " ";

        string s = "";
        s += $"{Indent}name:{name};\n";
        s += $"{Indent}isActive:{isActive};\n";
        s += $"{Indent}triggerAnim:{triggerAnim};\n";
        s += $"{Indent}isTerrain:{isTerrain};\n";

        s += $"{Indent}adjacencyRequirement:{adjacencyRequirement};\n";
        s += $"{Indent}multiplier:{multiplier};\n";
        s += $"{Indent}increaser:{increaser};\n";
        s += $"{Indent}diviser:{diviser};\n";
        s += $"{Indent}possibilities:" + "{\n" + possibilities.encode(indent + 1) + Indent + "};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        adjacencyRequirement = Convert.ToBoolean(e["adjacencyRequirement"]);
        multiplier = Convert.ToInt32(e["multiplier"]);
        increaser = Convert.ToInt32(e["increaser"]);
        diviser = Convert.ToInt32(e["diviser"]);
        possibilities.decode(e["possibilities"]);
    }
}