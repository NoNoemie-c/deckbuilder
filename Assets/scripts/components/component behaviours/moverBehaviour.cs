using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component mover behaviour", menuName = "mover behaviour")]
public class moverBehaviour : componentBehaviour
{
    public List<Vector2Int> startPos = new List<Vector2Int>(), endPos = new List<Vector2Int>();
    public bool swap;
    [Space(10)]
    public bool clickToRotate;
    public bool clickToActivate;
    [Space(10)]
    public Vector2Int[] directions = new Vector2Int[1];
    [NonSerialized] public Vector2Int direction;
    private int dir = 0;

    public override bool activate(Vector2Int pos) {
        List<Vector2Int> Pos = new List<Vector2Int>();

        component This = componentManager.getComponent(pos);

        This.activated = true;

        for (int i = 0; i < startPos.Count; i ++) {
            Vector2Int p = pos + rotate(startPos[i],  direction);
            Vector2Int m = pos + rotate(endPos[i],  direction);
            
            if (componentManager.validPos(p) && componentManager.validPos(m))
                if (componentManager.getComponent(p).template != null && (componentManager.getComponent(m).template == null || swap)) {
                    if (swap && (Pos.Contains(p) || Pos.Contains(m)))
                        continue;
                    componentManager.moveComponent(p, m, swap);
                    Pos.Add(p);
                    Pos.Add(m);
                }
        }
        
        This.activated = false;

        return triggerAnim;
    }

    public override bool click(Vector2Int pos) {
        if (clickToRotate) {
            component This = componentManager.getComponent(pos); 

            This.template.clicked = false;
            dir ++;
            dir %= directions.Length;

            componentAnimation anim = metaData.animations["rotating"];
            Transform t = This.transform;

            anim.rotationCoef = Vector2.SignedAngle(direction, directions[dir]);
            This.template.rotation += anim.rotationCoef;
            direction = directions[dir];
            
            coroutiner.start(anim.play(t, componentManager.AnimTime));
        }

        if (clickToActivate)
            activate(pos);

        return false;
    }

    public override componentBehaviour copy() {
        moverBehaviour c = ScriptableObject.CreateInstance<moverBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.startPos = startPos;
        c.endPos = endPos;
        c.directions = directions;
        c.swap = swap;
        c.clickToRotate = clickToRotate;
        c.clickToActivate = clickToActivate;
        c.dir = dir;
        if (direction == default(Vector2Int))
            direction = directions[0];
        c.direction = direction;

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

        s += $"{Indent}startPos:" + "{\n" + startPos.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}endPos:" + "{\n" + endPos.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}directions:" + "{\n" + directions.encode(indent + 1) + Indent + "};\n";
        s += $"{Indent}swap:{swap};\n";
        s += $"{Indent}clickToRotate:{clickToRotate};\n";
        s += $"{Indent}clickToActivate:{clickToActivate};\n";
        s += $"{Indent}direction:{direction.x} {direction.y};\n";
        s += $"{Indent}dir:{dir};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        startPos.decode(e["startPos"]);
        endPos.decode(e["endPos"]);
        directions = directions.decode(e["directions"]);
        swap = Convert.ToBoolean(e["swap"]);
        clickToRotate = Convert.ToBoolean(e["clickToRotate"]);
        clickToActivate = Convert.ToBoolean(e["clickToActivate"]);
        direction = direction.decode(e["direction"]);
        dir = Convert.ToInt32(e["dir"]);
    }

    private Vector2Int rotate(Vector2Int v, Vector2Int rotator) {
        Vector2Int V = v;

        for (int i = 0; i < Mathf.Floor(Vector2.SignedAngle(rotator, Vector2Int.up) / 90); i ++)
            V = Vector2Int.RoundToInt(Vector2.Perpendicular(Vector2.Perpendicular(Vector2.Perpendicular(V))));

        return V;
    }
}
