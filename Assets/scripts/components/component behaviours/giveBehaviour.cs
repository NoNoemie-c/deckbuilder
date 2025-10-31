using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component give behaviour", menuName = "give behaviour")]
public class giveBehaviour : componentBehaviour
{
    [Space(5)]
    [Space(5)]
    public int coins, cranks, movers, removals, rerolls, permanentCoins, permanentCranks;

    public override bool activate(Vector2Int pos) {
        component c = componentManager.getComponent(pos);

        if (cranks != 0)
            c.gainCranks(cranks);
        var.rerolls += rerolls;
        var.removes += removals;
        var.movers += movers;
        if (coins != 0)
            c.selfGainCoins(coins);
        if (permanentCoins != 0)
            c.permaGainCoins(permanentCoins);
        if (permanentCranks != 0)
            c.permaGainCoins(permanentCranks);

        if (coins != 0 && cranks != 0 && rerolls != 0 && movers != 0 && removals != 0 && permanentCoins != 0 && permanentCranks != 0 && triggerAnim)
            Anim(c, GetType().ToString());

        return triggerAnim;
    }

    public override bool click(Vector2Int pos) => false;

    public override void Anim(component c, string name) {
        componentAnimation anim = metaData.animations[GetType().ToString()];
        Color col = Color.white;

        if (permanentCoins != 0 || permanentCranks != 0 || coins != 0 || cranks != 0)
            return;
        else if (movers != 0) {
            col = metaData.colors["movers"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(movers);
            anim.targetImages = componentAnimation.TargetImages.Line;
        } else if (removals != 0) {
            col = metaData.colors["removals"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(removals);
            anim.targetImages = componentAnimation.TargetImages.Line;
        } else if (rerolls != 0) {
            col = metaData.colors["rerolls"];
            anim.yPositionCoef = Mathf.Abs(anim.yPositionCoef) * Mathf.Sign(rerolls);
            anim.targetImages = componentAnimation.TargetImages.Line;
        }

        anim.AColorCoef = col.a;
        anim.RColorCoef = col.r;
        anim.GColorCoef = col.g;
        anim.BColorCoef = col.b;

        c.StartCoroutine(anim.play(c.transform, componentManager.AnimTime));
    }
    
    public override componentBehaviour copy() {
        giveBehaviour c = ScriptableObject.CreateInstance<giveBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.cranks = cranks;
        c.rerolls = rerolls;
        c.removals = removals;
        c.movers = movers;
        c.coins = coins;
        c.permanentCoins = permanentCoins;
        c.permanentCranks = permanentCranks;

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

        s += $"{Indent}cranks:{cranks};\n";
        s += $"{Indent}rerolls:{rerolls};\n";
        s += $"{Indent}removals:{removals};\n";
        s += $"{Indent}movers:{movers};\n";
        s += $"{Indent}coins:{coins};\n";
        s += $"{Indent}permanentCoins:{permanentCoins};\n";
        s += $"{Indent}permanentCranks:{permanentCranks};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        cranks = Convert.ToInt32(e["cranks"]);
        rerolls = Convert.ToInt32(e["rerolls"]);
        removals = Convert.ToInt32(e["removals"]);
        movers = Convert.ToInt32(e["movers"]);
        coins = Convert.ToInt32(e["coins"]);
        permanentCranks = Convert.ToInt32(e["permanentCranks"]);
        permanentCoins = Convert.ToInt32(e["permanentCoins"]);
    }
}
