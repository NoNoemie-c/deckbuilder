using System;
using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "new component animal behaviour", menuName = "animal behaviour")]
public class animalBehaviour : componentBehaviour
{
    public Vector2Int direction = Vector2Int.up;
    public int strength;
    public int coinsOnKill;
    public int attraction;

    public override bool activate(Vector2Int pos) {
        component This = componentManager.getComponent(pos);
        Vector2Int nextPos = pos + direction;

        componenttemplate egg = componentManager.allComponents.Find(c => c.name == "egg");

        for (int x = 0; x < componentManager.size.x; x ++)
            for (int y = 0; y < componentManager.size.y; y ++) {
                component c = componentManager.getComponent(new Vector2Int(x, y));
                if (c.template != null)
                    if (c.template.tags.Contains("animal")) {
                        int attract = 0;
                        foreach (animalBehaviour behaviour in c.template.getBehaviours<animalBehaviour>()) {
                            attract = behaviour.attraction;
                            break;
                        }

                        

                        if (x < pos.x)
                            nextPos += Vector2Int.right * attract;
                        if (x > pos.x)
                            nextPos += Vector2Int.left * attract;
                        if (y < pos.y)
                            nextPos += Vector2Int.up * attract;
                        if (y > pos.y)
                            nextPos += Vector2Int.down * attract;
                    }
            }

        if (!(nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= componentManager.size.x || nextPos.y >= componentManager.size.y)) {
            component c = componentManager.getComponent(nextPos);
            if (c.template == null) {
                coroutiner.start(metaData.sounds["animal"].play());
                componentManager.moveComponent(pos, nextPos, false);
                reorientate(componentManager.getComponent(nextPos));
            } else if (c.template.tags.Contains("animal")) {
                foreach (animalBehaviour behaviour in c.template.getBehaviours<animalBehaviour>()) {
                    This.template.tags.Remove("animal");
                    if (c.template.tags.ContainsRange(This.template.tags)) {
                        strength --; 
                        behaviour.strength --;
                        c.sparkle();

                        componentManager.spawnComponent(pos, egg);
                    
                        direction = -direction;
                        reorientate(componentManager.getComponent(pos));
                    }
                    This.template.tags.Add("animal");

                    if (strength < behaviour.strength)
                        return false;
                    if (strength == behaviour.strength) {
                        direction = -direction;
                        reorientate(componentManager.getComponent(pos));
                        return false;
                    }
                    
                    componentManager.getComponent(pos).coin += coinsOnKill;
                    componentManager.remove(nextPos, pos);
                    Anim(c, typeof(eaterBehaviour).ToString());

                    componentManager.moveComponent(pos, nextPos, false);
                    reorientate(componentManager.getComponent(nextPos));

                    break;
                }
            } else {
                direction = -direction;
                reorientate(componentManager.getComponent(pos));
            }
        } else {
            direction = -direction;
            reorientate(componentManager.getComponent(pos));
        }

        return triggerAnim;
    }

    public override bool click(Vector2Int pos) {
        if (Vector2.Dot(direction, Vector2Int.up) == 1)
            direction = Vector2Int.right * Convert.ToInt32(direction.magnitude);
        else if (Vector2.Dot(direction, Vector2Int.right) == 1)
            direction = Vector2Int.down * Convert.ToInt32(direction.magnitude);
        else if (Vector2.Dot(direction, Vector2Int.down) == 1)
            direction = Vector2Int.left * Convert.ToInt32(direction.magnitude);
        else if (Vector2.Dot(direction, Vector2Int.left) == 1)
            direction = Vector2Int.up * Convert.ToInt32(direction.magnitude);

        reorientate(componentManager.getComponent(pos));

        return false;
    }

    private void reorientate(component c) {
        componentAnimation anim = metaData.animations["rotating"];
        Transform t = c.transform;

        anim.rotationCoef = Vector2.SignedAngle(t.rotation.eulerAngles.V2(), direction);

        c.template.rotation += anim.rotationCoef;
        
        t.GetComponent<component>().StartCoroutine(anim.play(t, componentManager.AnimTime));
    }

    public override componentBehaviour copy() {
        animalBehaviour c = ScriptableObject.CreateInstance<animalBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;
        
        c.direction = direction;
        c.strength = strength;
        c.coinsOnKill = coinsOnKill;
        c.attraction = attraction;

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

        s += $"{Indent}direction:{direction};\n";
        s += $"{Indent}strength:{strength};\n";
        s += $"{Indent}coinsOnKill:{coinsOnKill};\n";
        s += $"{Indent}attraction:{attraction};\n";

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        direction = direction.decode(e["direction"]);
        strength = Convert.ToInt32(e["strength"]);
        coinsOnKill = Convert.ToInt32(e["coinsOnKill"]);
        attraction = Convert.ToInt32(e["attraction"]);
    }
}
