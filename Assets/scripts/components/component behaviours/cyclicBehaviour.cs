using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component cyclic behaviour", menuName = "cyclic behaviour")]
public class cyclicBehaviour : componentBehaviour
{
    [Space(10)]
    public int cyclingTime = 1;
    public int turn;

    [Space(5)]
    public int multiplier = 1;
    public int increaser = 0;
    public List<componentBehaviour> behavioursToActivate = new List<componentBehaviour>();

    public override bool activate(Vector2Int pos) {
        return advance(pos);
    }

    public bool advance(Vector2Int pos) {
        component This = componentManager.getComponent(pos);
        componentAnimation anim = metaData.animations[GetType().ToString()];

        void replaceTurnDesc() {
            string s = "";
            int index = -1;
            if (isTerrain) {
                s = This.under.description;
                index = This.under.getBehaviours<cyclicBehaviour>().IndexOf(this);

                for (int i = 0; i < s.Length; i++)
                    if (s[i] == '(' && (Convert.ToInt32(s[i+1].ToString()) == turn-1 || Convert.ToInt32(s[i+1].ToString()) == cyclingTime-1) && s[i+3] == '/') {
                        index --;
                        if (index < 0) {
                            s = s.Remove(i+1, 1).Insert(i+1, turn.ToString());
                            break;
                        }
                    }

                This.under.description = s;

                return;
            }

            s = This.template.description;
            index = This.template.getBehaviours<cyclicBehaviour>().IndexOf(this);

            for (int i = 0; i < s.Length; i++)
                if (s[i] == '(' && (Convert.ToInt32(s[i+1].ToString()) == turn-1 || Convert.ToInt32(s[i+1].ToString()) == cyclingTime-1) && s[i+3] == '/') {
                    index --;
                    if (index < 0) {
                        s = s.Remove(i+1, 1).Insert(i+1, turn.ToString());
                        break;
                    }
                }

            This.template.description = s;
        }

        turn ++;

        if (turn >= cyclingTime) {
            This.coin *= multiplier;
            This.coin += increaser;

            int clockCount = componentManager.getAdjacents(pos).removeNulls().FindAll(delegate(component c) { 
                if (c.template.tags.Contains("clock")) {
                    c.sparkle();
                    return true;
                } 
                return false;
            }).Count;
            for (int i = 0; i < clockCount * cyclingTime + 1; i++)
                foreach (componentBehaviour behaviour in behavioursToActivate)
                    behaviour.activate(pos);

            if (triggerAnim) {
                anim = metaData.animations[GetType().ToString() + "Complete"];
                anim.previousTime = (float)(turn - 1) / cyclingTime;
                anim.currentTime = 1;
                This.StartCoroutine(anim.play(This.transform, componentManager.AnimTime));
            }

            turn -= cyclingTime;
            replaceTurnDesc();
            
            return true;
        } else  {
            replaceTurnDesc();

            if (triggerAnim) {
                anim.previousTime = (float)(turn - 1) / cyclingTime;
                anim.currentTime = (float)turn / cyclingTime;
                This.StartCoroutine(anim.play(This.transform, componentManager.AnimTime));
            }
            
            return false;
        }
    }

    public override bool click(Vector2Int pos) => false;

    public override componentBehaviour copy() {
        cyclicBehaviour c = ScriptableObject.CreateInstance<cyclicBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.cyclingTime = cyclingTime;
        c.turn = turn;
        c.multiplier = multiplier;
        c.increaser = increaser;
        c.behavioursToActivate = behavioursToActivate.removeNulls();

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

        s += $"{Indent}cyclingTime:{cyclingTime};\n";
        s += $"{Indent}turn:{turn};\n";
        s += $"{Indent}multiplier:{multiplier};\n";
        s += $"{Indent}increaser:{increaser};\n";
        s += $"{Indent}behavioursToActivate:" + "{\n" + behavioursToActivate.encode(indent + 1) + Indent + "};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        cyclingTime = Convert.ToInt32(e["cyclingTime"]);
        turn = Convert.ToInt32(e["turn"]);
        multiplier = Convert.ToInt32(e["multiplier"]);
        increaser = Convert.ToInt32(e["increaser"]);
        behavioursToActivate.decode(e["behavioursToActivate"]);
    }
}
