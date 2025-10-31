using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component killer behaviour", menuName = "killer behaviour")]
public class killerBehaviour : componentBehaviour
{
    [Space(10)]
    public Vector2Int[] attack = new Vector2Int[0];

    [Space(5)]
    public List<componenttemplate> targets = new List<componenttemplate>();

    public override bool activate(Vector2Int pos) {
        foreach (Vector2Int v in attack) {
            if (!componentManager.validPos(v + pos))
                continue;

            component c = componentManager.getComponent(v + pos);
            if (c.template != null)
                if (canTarget(c.template))
                    componentManager.remove(v + pos, (0, -1).v());
        }

        return triggerAnim;
    }

    private bool canTarget(componenttemplate c) {
        if (targets.Count < 1)
            return true;

        foreach (componenttemplate comp in targets)
            if (c.tags.Contains(comp.name) || c == componentManager.polymorph)
                return true;

        return c == componentManager.polymorph;
    }

    public override bool click(Vector2Int pos) => false;

    public override componentBehaviour copy() {
        killerBehaviour c = ScriptableObject.CreateInstance<killerBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.targets = targets.removeNulls();
        c.attack = attack;
        
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

        s += $"{Indent}attack:" + "{\n" + attack.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}targets:" + "{\n" + targets.encode(indent + 1) + Indent + "};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        attack = attack.decode(e["attack"]);
        targets.decode(e["targets"]);
    }
}
