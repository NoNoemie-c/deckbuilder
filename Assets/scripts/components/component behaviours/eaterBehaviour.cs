using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component eater behaviour", menuName = "eater behaviour")]
public class eaterBehaviour : componentBehaviour
{
    [Space(10)]
    public List<componenttemplate> possibilities = new List<componenttemplate>();
    public bool adjacencyRequirement = true;

    [Space(10)] [Header("local effects")]
    public int localPermanentUpgrade;
    public int localIncreaser;
    public int localMultiplier = 1;
    public List<componentBehaviour> localBehavioursToTrigger = new List<componentBehaviour>();
    

    [Space(5)] [Header("effects on target")]
    public int targetIncreaser;
    public int targetMultiplier = 1;

    public override bool activate(Vector2Int pos) {
        List<component> list;
        if (adjacencyRequirement) 
            list = componentManager.getAdjacents(pos);
        else
            list = componentManager.GetAll();

        bool b = false;
        component This = componentManager.getComponent(pos);

        foreach(component c in list) {
            if (c.template == null)
                continue;

            

            if (canTarget(c.template)) {
                // target effects
                if (targetMultiplier != 1)
                    c.gainCoins(c.coin * (targetMultiplier - 1));
                if (targetIncreaser != 0)
                    c.gainCoins(targetIncreaser);

                // local effects
                if (localMultiplier != 1)
                    This.selfGainCoins(This.coin * (localMultiplier - 1));
                if (localIncreaser != 0)
                    This.selfGainCoins(localIncreaser);
                if (localPermanentUpgrade != 0)
                    This.permaGainCoins(localPermanentUpgrade);
                b = true;

                foreach (componentBehaviour behaviour in localBehavioursToTrigger)
                    This.anim = behaviour.activate(pos);

                // eaten item destruction
                componentManager.remove(new Vector2Int(-1, -1), pos, c);
            }
        }

        if (b) 
            Anim(componentManager.getComponent(pos), GetType().ToString());

        return triggerAnim;
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

    public override componentBehaviour copy()
    {
        eaterBehaviour c = ScriptableObject.CreateInstance<eaterBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.possibilities = possibilities.removeNulls();
        c.adjacencyRequirement = adjacencyRequirement;
        c.localPermanentUpgrade = localPermanentUpgrade;
        c.localIncreaser = localIncreaser;
        c.localMultiplier = localMultiplier;
        c.localBehavioursToTrigger = localBehavioursToTrigger;
        c.targetIncreaser = targetIncreaser;
        c.targetMultiplier = targetMultiplier;

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
        s += $"{Indent}localPermanentUpgrade:{localPermanentUpgrade};\n";
        s += $"{Indent}localIncreaser:{localIncreaser};\n";
        s += $"{Indent}localMultiplier:{localMultiplier};\n";
        s += $"{Indent}localBehavioursToTrigger:" + "{\n" + localBehavioursToTrigger.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}targetIncreaser:{targetIncreaser};\n";
        s += $"{Indent}targetMultiplier:{targetMultiplier};\n";
        s += $"{Indent}possibilities:" + "{\n" + possibilities.encode(indent + 1) + Indent + "};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        adjacencyRequirement = Convert.ToBoolean(e["adjacencyRequirement"]);
        localPermanentUpgrade = Convert.ToInt32(e["localPermanentUpgrade"]);
        localIncreaser = Convert.ToInt32(e["localIncreaser"]);
        localMultiplier = Convert.ToInt32(e["localMultiplier"]);
        localBehavioursToTrigger.decode(e["localBehavioursToTrigger"]);
        targetIncreaser = Convert.ToInt32(e["targetIncreaser"]);
        targetMultiplier = Convert.ToInt32(e["targetMultiplier"]);
        possibilities.decode(e["possibilities"]);
    }
}
