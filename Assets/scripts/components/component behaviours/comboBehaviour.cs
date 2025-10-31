using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component combo behaviour", menuName = "combo behaviour")]
public class comboBehaviour : componentBehaviour
{
    public List<componenttemplate> possibilities = new List<componenttemplate>();
    public int value;
    public List<string> houses = new List<string>();

    private List<component> alreadyChecked;

    public override bool activate(Vector2Int pos) {
        alreadyChecked = new List<component>();
        int gain = recurse(pos);

        alreadyChecked = new List<component>();
        int houseGain = houseRecurse(pos);
        
        componentManager.getComponent(pos).coin = gain + houseGain;
 
        return triggerAnim;
    }

    private int houseRecurse(Vector2Int pos) {
        alreadyChecked.Add(componentManager.getComponent(pos));
        if (triggerAnim)
            componentManager.getComponent(pos).StartCoroutine(metaData.animations[GetType().ToString()].play(componentManager.getComponent(pos).transform, componentManager.AnimTime));
        int i = value;

        foreach (component c in componentManager.getAdjacents(pos).removeNulls())
            if (!alreadyChecked.Contains(c) && c.template.tags.Contains("card"))
                foreach (comboBehaviour behaviour in c.template.getBehaviours<comboBehaviour>())
                    if (houses.ContainsRange(behaviour.houses))
                        i += houseRecurse(componentManager.GetPosition(c));

        return i;
    }

    private int recurse(Vector2Int pos) {
        alreadyChecked.Add(componentManager.getComponent(pos));
        if (triggerAnim)
            componentManager.getComponent(pos).StartCoroutine(metaData.animations[GetType().ToString()].play(componentManager.getComponent(pos).transform, componentManager.AnimTime));
        int i = value;

        foreach (component c in componentManager.getAdjacents(pos).removeNulls())
            if (!alreadyChecked.Contains(c) && c.template.tags.Contains("card")) {
                if (possibilities.Count > 1) {
                    foreach (componenttemplate template in possibilities)
                        if (template == c.template || c.template == componentManager.polymorph) {
                            i += recurse(componentManager.GetPosition(c));
                            break;
                        }
                } else {
                    i += recurse(componentManager.GetPosition(c));
                    break;
                }
            }

        return i;
    }

    public override bool click(Vector2Int pos) {
        return false;
    }

    public override componentBehaviour copy() {
        comboBehaviour c = ScriptableObject.CreateInstance<comboBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.houses = houses;
        c.possibilities = possibilities.removeNulls();
        if (possibilities.Count < 1)
            c.possibilities.AddRange(componentManager.allComponents);
        c.value = value;

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

        s += $"{Indent}houses:" + "{\n" + houses.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}possibilities:" + "{\n" + possibilities.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}value:{value};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        houses.decode(e["houses"]);
        possibilities.decode(e["possibilities"]);
        value = Convert.ToInt32(e["value"]);
    }
}
