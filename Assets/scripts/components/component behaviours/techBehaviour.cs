using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class techBehaviour : componentBehaviour {
    public enum TechCondition : int {
        startOfTurn,
        endOfTurn, 
        timesYouRemove,
        timesYouSpawn,
        timesAComponentIs_EffectOfAPack
    }
    public enum TechEffect : int {
        itActivates,
        itIsPermanentlyWorth1More,
        itIsRemoved,
        itIsWorth2x,
        itIs_effectOfItsPacks
    }

    public TechCondition TechC;
    public TechEffect TechE;

    public componenttemplate it;

    public override bool activate(Vector2Int pos) {
        effect(pos);

        return false;
    }

    private void effect(Vector2Int pos) {
        foreach (component c in componentManager.GetAll().removeNulls())
            if (c.template == it)
                switch (TechE) {
                    case TechEffect.itActivates : 
                        c.Activate(componentManager.GetPosition(c));
                    break;

                    case TechEffect.itIsPermanentlyWorth1More : 
                        c.permaGainCoins(1);
                    break;

                    case TechEffect.itIsWorth2x : 
                        c.multiplyCoins(2);
                    break;

                    case TechEffect.itIsRemoved :
                        //componentManager.remove(Vector2Int.left, c);
                    break;
                }
    }

    public override bool click(Vector2Int pos) {
        return false;
    }

    public override componentBehaviour copy() {
        techBehaviour c = ScriptableObject.CreateInstance<techBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.TechC = TechC;
        c.TechE = TechE;
        c.it = it;

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

        s += $"{Indent}TechC:{TechC};\n";
        s += $"{Indent}TechE:{TechE};\n";
        s += $"{Indent}it:{((it == null)? "null" : it.name)};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        throw new NotImplementedException();
    }
}
