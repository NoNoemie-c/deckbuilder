using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName = "new component dice behaviour", menuName = "dice behaviour")]
public class diceBehaviour : componentBehaviour
{
    public List<int> possibleFaces = new List<int>();
    public int currentFace = 0;

    public override bool activate(Vector2Int pos) {
        component This = componentManager.getComponent(pos);

        if (!possibleFaces.Contains(currentFace) || currentFace == 0) {
            This.StartCoroutine(roll(This, pos, true));
            return false;
        }

        component comp;
        int count = currentFace;
        if ((comp = componentManager.getAdjacents(pos).removeNulls().Find(c => c.template.tags.Contains("dice buffer"))) != null) {
            count += currentFace;
            comp.sparkle();
        }

        bool card;
        if (card = (comp = componentManager.getAdjacents(pos).removeNulls().Find(c => c.template.tags.Contains("ace (dice)"))) != null)
            comp.sparkle();


        foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
            if (c.template.tags.Contains("die"))
                foreach (diceBehaviour behaviour in c.template.getBehaviours<diceBehaviour>()) {
                    if (triggerAnim)
                        c.StartCoroutine(metaData.animations[GetType().ToString() + "0"].play(c.transform, componentManager.AnimTime));

                    count += behaviour.currentFace;
                    if (behaviour.currentFace == currentFace)
                        count += behaviour.currentFace;
                }
            if (card && c.template.tags.Contains("card"))
                c.Activate(componentManager.GetPosition(c));
        }

        This.gainCoins(count);

        return triggerAnim;
    }

    public override bool click(Vector2Int pos) {
        component c = componentManager.getComponent(pos);

        c.StartCoroutine(roll(c, pos));

        return true;
    }

    private IEnumerator roll(component c, Vector2Int pos, bool b = false) {
        currentFace = possibleFaces[UnityEngine.Random.Range(0, possibleFaces.Count)];

        if (triggerAnim)
            yield return c.StartCoroutine(metaData.animations[GetType().ToString()].play(c.transform, componentManager.AnimTime));

        component comp;
        if ((comp = componentManager.getAdjacents(pos).Find(C => C.template.name.Contains("dice cup"))) != null) {
            comp.sparkle();
            c.anim = activate(pos);
        }

        c.GetComponentInChildren<Image>().sprite = componentManager.diceFaces[currentFace - 1];
        if (b)
            c.template.getBehaviours<diceBehaviour>()[0].currentFace = currentFace;
    }

    public override componentBehaviour copy() {
        diceBehaviour c = ScriptableObject.CreateInstance<diceBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.possibleFaces = possibleFaces;
        c.currentFace = currentFace;

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

        s += $"{Indent}possibleFaces:" + "{\n" + possibleFaces.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}currentFace:{currentFace};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        possibleFaces.decode(e["possibleFaces"]);
        currentFace = Convert.ToInt32(e["currentFace"]);
    }
}
