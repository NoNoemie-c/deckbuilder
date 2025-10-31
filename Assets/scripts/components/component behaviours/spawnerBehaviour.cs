using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new component spawner behaviour", menuName = "spawner behaviour")]
public class spawnerBehaviour : componentBehaviour
{
    [Space(10)]
    public List<componenttemplate> spawn = new List<componenttemplate>();

    public override bool activate(Vector2Int pos) {
        if (spawn.Count == 0)
            return false;

        if (spawn[0].name == "polymorph")
            spawn = componentManager.allComponents;

        if (name == "consumable spawner spawner behaviour")
            spawn = componentManager.allComponents.FindAll(t => t.getBehaviours<specialBehaviour>().Find(b => b.name == "kamikaze") != null || 
            t.getBehaviours<cyclicBehaviour>().Find(b => b.behavioursToActivate.Find(b => b.name == "kamikaze") != null) != null || 
            t.getBehaviours<eaterBehaviour>().Find(b => b.localBehavioursToTrigger.Find(b => b.name == "kamikaze") != null) != null);

        componenttemplate c = spawn[UnityEngine.Random.Range(0, spawn.Count)];
        if (componentManager.allComponents.ContainsRange(spawn))
            while (!componentManager.allComponents.Contains(c))
                c = spawn[UnityEngine.Random.Range(0, spawn.Count)];
        
        component s = componentManager.spawnComponent(pos, c);
        if (s == null)
            return false;
            
        componentAnimation anim = metaData.animations["spawned"];
        Vector2Int Pos = componentManager.GetPosition(s);
        anim.xPositionCoef = -(Pos - pos).x * componentManager.worldSize.x;
        anim.yPositionCoef = -(Pos - pos).y * componentManager.worldSize.y;

        coroutiner.start(anim.play(s.transform, componentManager.AnimTime));

        Anim(componentManager.getComponent(pos), GetType().ToString());

        return triggerAnim;
    }

    public override bool click(Vector2Int pos) => 
        false;

    public override componentBehaviour copy() {
        spawnerBehaviour c = ScriptableObject.CreateInstance<spawnerBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;
            
        c.spawn = spawn.removeNulls();
        
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

        s += $"{Indent}spawn:" + "{\n" + spawn.encode(indent + 1) + Indent + "};\n";

        return s;
    }

    public override void decode(gameSave.element e) { 
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        spawn.decode(e["spawn"]);
    }
}