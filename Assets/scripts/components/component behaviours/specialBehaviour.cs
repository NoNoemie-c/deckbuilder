using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName = "new component special behaviour", menuName = "special behaviour")]
public class specialBehaviour : componentBehaviour
{
    public string Name;
    public bool adjacencyRequirement = true;
    public int var1 = 0;
    public componenttemplate var2 = null;
    public static Vector2Int[] Directions = {(0, 1).v(), (1, 0).v(), (0, -1).v(), (-1, 0).v()};
    public static Vector2Int[][] mergePos = {
        new Vector2Int[]{(0, 0).v()},
        new Vector2Int[]{(0, 0).v(), (256, 256).v()}, 
        new Vector2Int[]{(0, 0).v(), (256, 0).v(), (128, 256).v()}, 
        new Vector2Int[]{(0, 0).v(), (256, 0).v(), (0, 256).v(), (256, 256).v()}, 
        new Vector2Int[]{(0, 0).v(), (342, 0).v(), (171, 171).v(), (0, 342).v(), (342, 342).v()},
        new Vector2Int[]{(0, 0).v(), (342, 0).v(), (0, 171).v(), (342, 171).v(), (0, 342).v(), (342, 342).v()},
        new Vector2Int[]{(0, 0).v(), (342, 0).v(), (0, 171).v(), (171, 171).v(), (342, 171).v(), (0, 342).v(), (342, 342).v()},
        new Vector2Int[]{(0, 0).v(), (171, 0).v(), (342, 0).v(), (0, 171).v(), (342, 171).v(), (0, 342).v(), (171, 342).v(), (342, 342).v()},
        new Vector2Int[]{(0, 0).v(), (171, 0).v(), (342, 0).v(), (0, 171).v(), (171, 171).v(), (342, 171).v(), (0, 342).v(), (171, 342).v(), (342, 342).v()}
    };
    
    public override bool activate(Vector2Int pos) {
        bool anim = false, buff = false, eat = false;

        component This = componentManager.getComponent(pos);

        switch (Name) {
            case "mechanism" :
                int count = 0;

                List<component> comps = componentManager.getAdjacents(pos);
                if (!adjacencyRequirement)
                    comps = componentManager.GetAll();

                foreach (component c in comps.removeNulls())
                    count += c.template.crank;

                This.selfGainCoins(count);
            break;

            case "electrical converter" :
                if (This.isPowered)
                    This.permaGainCranks(1);
                else 
                    This.permaGainCranks(-1);
            break;

            case "mechanical converter" : 
                if (var.cranks > 0) {
                    This.template.electricGenerator = true;
                    var.cranks --;
                } else
                    This.template.electricGenerator = false;
            break;

            case "wild component" : 
                int bestCoin = 0;

                List<component> compos = componentManager.getAdjacents(pos);
                if (!adjacencyRequirement)
                    comps = componentManager.GetAll();

                foreach (component c in compos.removeNulls())
                    if (c.coin > bestCoin)
                        bestCoin = c.coin;

                This.selfGainCoins(bestCoin);
            break;

            case "roulette" :
                int i = UnityEngine.Random.Range(0, 5);
                var.addCranks(i);

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("croupier")) {
                        c.sparkle();
                        This.gainCoins(i);
                    }
            break;

            case "looting glove" :
                foreach (component c in componentManager.GetAll())
                    if (c.template.tags.Contains("die") || c == componentManager.polymorph)
                        foreach (diceBehaviour behaviour in c.template.getBehaviours<diceBehaviour>())
                            c.anim = behaviour.click(componentManager.GetPosition(c));
            break;

            case "croupier" :
                List<Vector2Int> positions = new List<Vector2Int>();
                List<component> cards = new List<component>();

                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template.tags.Contains("card") || c == componentManager.polymorph)
                        foreach (comboBehaviour behaviour  in c.template.getBehaviours<comboBehaviour>())
                            if (!cards.Contains(c)) {
                                cards.Add(c);
                                positions.Add(componentManager.GetPosition(c));
                                componentManager.remove((0, -1).v(), (0, -1).v(), c);
                            }

                while (cards.Count > 0) {
                    int n = UnityEngine.Random.Range(0, cards.Count), m = UnityEngine.Random.Range(0, cards.Count);
                    componentManager.addComponent(positions[n], cards[m].template);
                    positions.RemoveAt(n);
                    cards.RemoveAt(m);
                }
            break;

            case "null quantifier" :
                foreach (component c in componentManager.GetAll())
                    if (!c.template)
                        This.selfGainCoins(This.coin);
            break;

            case "trash can" :
                foreach (component c in componentManager.getAdjacents(pos))
                    if (c.destroyed)
                        This.permaGainCoins(2);
            break;

            case "oil can" : 
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.crank != 0) {
                        c.multiplyCoins(2);
                        buff = true;
                    }
            break;

            case "mechanical clock 0" :
                This.selfGainCoins(var.cranks);
            break;

            case "mechanical clock 1" :
                if (var.cranks > 0)
                    This.selfGainCoins(-10);
            break;

            case "pearl" :
                foreach (component c in componentManager.GetAll().removeNulls())
                    if (!c.template.tags.Contains("pearl") && c != componentManager.polymorph)
                        foreach (componentBehaviour behaviour in c.template.spawnBehaviours)
                            c.anim = behaviour.activate(componentManager.GetPosition(c));
            break;

            // gems behaviours

            case "emerald" :
                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template.tags.Contains("emerald") || c == componentManager.polymorph)
                        This.selfGainCoins(1);
            break;

            case "ruby" :
                foreach (component c in componentManager.GetAll().removeNulls()) {
                    Vector2Int position = componentManager.GetPosition(c);
                    if (Mathf.Abs(position.x - pos.x) == Mathf.Abs(position.y - pos.y) && c.template.tags.Contains("gem") && pos != position)
                        componentManager.getComponent(position).selfGainCoins(This.coin);
                }
            break;

            case "dame" :
                bool saphired = false;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("saphire")) {
                        saphired = true;
                        c.sparkle();
                    }

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("bague") || c == componentManager.polymorph)
                        if (saphired) {
                            c.gainCoins(2);
                            buff = true;
                        } else {
                            c.multiplyCoins(.5f);
                            buff = true;
                        }
            break;

            case "topaz" :
                foreach (component c in componentManager.getUpgradedComponents())
                    This.selfMultiplyCoins(2);
            break;

            case "amethyst" :
                foreach (component c in componentManager.getBuffedComponents().removeNulls())
                    if (c == componentManager.polymorph || (c.template.tags.Contains("gem") && componentManager.GetPosition(c) != pos)) {
                        c.multiplyCoins(2);
                        buff = true;
                    }
            break;

            case "diamond" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.activated && c.template.tags.Contains("gem") && !c.template.tags.Contains("diamond") || c == componentManager.polymorph)
                        c.anim = c.Activate(componentManager.GetPosition(c));
            break;

            // metals behaviours (rings)

            case "silver" :
                foreach (component c in componentManager.GetAll().removeNulls()) {
                    Vector2Int position = componentManager.GetPosition(c);
                    if ((position.x == pos.x || position.y == pos.y) && c.template.tags.Find(s => s.Contains("silver")) != null && pos != position)
                        This.selfGainCoins(1);
                }
            break;

            case "gold" :
                foreach (component c in componentManager.GetAll().removeNulls()) {
                    Vector2Int position = componentManager.GetPosition(c);
                    if ((position.x == pos.x || position.y == pos.y) && c.template.tags.Find(s => s.Contains("gold")) != null && pos != position)
                        This.selfGainCoins(2);
                }
            break;

            case "platinium" :
                foreach (component c in componentManager.GetAll().removeNulls()) {
                    Vector2Int position = componentManager.GetPosition(c);
                    if ((position.x == pos.x || position.y == pos.y) && c.template.tags.Find(s => s.Contains("platinium")) != null && pos != position)
                        This.selfGainCoins(3);
                }
            break;

            case "jeweler" :
                component gem = null;
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("gem") || c == componentManager.polymorph)
                        gem = c;

                component ring = null;
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("ring") || c == componentManager.polymorph)
                        ring = c;

                if (gem == null || ring == null) 
                    break;

                componenttemplate bague = ring.template;
                bague.coin += gem.template.coin;
                foreach (baseObject.time time in baseObject.times)
                    bague.behaviours[time].AddRange(gem.template.behaviours[time]);
                
                bague.name = ring.template.name.Replace("ring", "") + gem.template.name + " bague";
                bague.tags.AddRange(gem.template.tags);
                bague.tags.Add("bague");
                bague.description += "\n" + gem.template.description;
                bague.symbol = spriteMerger.merge(new Sprite[]{bague.symbol, gem.template.symbol}, new Vector2Int[]{Vector2Int.zero, new Vector2Int(128, 256)}, new Vector2[]{Vector2.one, Vector2.one * .5f});
                
                componentManager.remove((0, -1).v(), (0, -1).v(), ring);
                componentManager.remove((0, -1).v(), (0, -1).v(), gem);

                componentManager.spawnComponent(pos, bague);
            break;

            case "arrow" :
                Debug.Log("var1 : " + var1 + " localEuler : " + This.transform.localEulerAngles.z + " temp rot : " + This.template.rotation);
                int bow = 2;

                component Bow = null;
                if ((Bow = componentManager.getAdjacents(pos).removeNulls().Find(c => c.template.tags.Contains("bow"))) != null) {
                    Bow.sparkle();
                    bow ++;
                }

                if (var1 < 0)
                    click(pos);

                List<component> arrowed = new List<component>();
                Vector2Int v = pos + meta.directions[var1];
                while (componentManager.validPos(v)) {
                    arrowed.Add(componentManager.getComponent(v));
                    v += meta.directions[var1];
                }

                foreach (component c in arrowed.removeNulls()) {
                    c.multiplyCoins(bow);
                    buff = true;
                    if (c.template.tags.Contains("target")) {
                        c.sparkle();
                        componentManager.remove(c: c);
                    }
                }
            break;

            case "piston" :
                component a = componentManager.GetNext(pos);
                if (a != null) {
                    a.Activate(componentManager.GetPosition(a));
                    a.activated = false;
                }
            break;

            case "renter" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.coin < 0)
                        This.gainCoins(10);
            break;

            case "thief" :
                int steal = 2;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("al capone")) {
                        c.sparkle();
                        steal *= 2;
                    }

                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                    if (c.coin > 0)
                        This.permaGainCoins(1);
                    else if (c.coin < 0)
                        This.permaGainCoins(-1);

                    
                    
                    c.multiplyCoins(1 - 1 / steal);
                    buff = true;
                }
            break;

            case "satellite" : 
                List<Vector2Int> generators = new List<Vector2Int>();
                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template.electricGenerator)
                        generators.Add(componentManager.GetPosition(c) - pos);

                int distance = 0;
                foreach (Vector2Int vec in generators)
                    if (vec.magnitude > distance)
                        distance = Mathf.CeilToInt(vec.magnitude);

                for (int j = 0; j < distance; j ++)
                    This.selfMultiplyCoins(2);
            break;

            case "long-term optimiser" :
                foreach (component c in componentManager.getAdjacents(pos))
                    if (componentManager.getUpgradedComponents().Contains(c)) {
                        c.multiplyCoins(2);
                        buff = true;
                    }
            break;

            case "plant" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) 
                    This.gainCoins(var1);
            break;

            case "leafblower" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("plant"))
                        var.movers ++;
            break;

            case "gardener" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("plant")) {
                        This.sparkle();
                        foreach(cyclicBehaviour behaviour in c.template.getBehaviours<cyclicBehaviour>())
                            behaviour.cyclingTime = 4;
                    }
            break;

            case "fertilizer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("plant"))
                        foreach(cyclicBehaviour behaviour in c.template.getBehaviours<cyclicBehaviour>())
                            c.anim = behaviour.advance(pos);
            break;

            case "thing" : 
                foreach(baseObject.time t in baseObject.times)
                    This.template.behaviours[t].Clear();
                This.template.behaviours[baseObject.time.preTurn].Add(this);

                foreach (baseObject.time tims in baseObject.times)
                    foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                        This.template.behaviours[tims].AddSingle(c.template.behaviours[tims]);
            break;

            case "double broadcaster" :
                if (var1 == 1)
                    foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                        c.multiplyCoins(2);
                        buff = true;
                    }

                var1 = 0;
                component comp = This;
                if (comp.template.coin * 2 <= comp.coin)
                    var1 = 1;
            break;

            case "void fragment" : 
                int adj = 0;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    adj ++;

                if (adj >= 8)
                    componentManager.remove(pos, (0, -1).v());
            break;

            case "void fragment death" : 
                int Adj = 0;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    Adj ++;

                This.gainCoins(Adj);
            break;

            case "equaliser" : 
                List<component> targets = componentManager.getAdjacents(pos);
                int coins = -1000000000;
                foreach (component c in targets.removeNulls()) {
                    coins = c.coin;
                    break;
                }

                bool b = false;
                foreach (component c in targets.removeNulls())
                    b = coins == c.coin;

                if (b)
                    This.selfMultiplyCoins(coins);

            break;

            case "sudoku master" :
                List<component> toDouble;
                bool B;

                foreach (component c in componentManager.getAdjacents(pos)) {
                    Vector2Int Pos = componentManager.GetPosition(c);
                    B = true;
                    toDouble = new List<component>();

                    for (int x = 0; x < componentManager.size.x; x++) {
                        if (!componentManager.getComponent(new Vector2Int(x, pos.y)).template)
                            B = false;
                        else
                            toDouble.Add(c);
                    }

                    if (!B) {
                        B = true;

                        for (int y = 0; y < componentManager.size.y; y++) {
                            if (!componentManager.getComponent(new Vector2Int(pos.x, y)).template)
                                B = false;
                            else
                                toDouble.Add(c);
                        }

                        if (B)
                            foreach (component Comp in toDouble) {
                                Comp.multiplyCoins(2);
                                buff = true;
                            }

                        continue;
                    }

                    foreach (component Comp in toDouble) {
                        Comp.multiplyCoins(2);
                        buff = true;
                    }
                }
            break;

            case "shaft" :
                foreach (component c in componentManager.getAdjacents(pos))
                    This.gainCoins(2 * c.template.crank);
            break;

            case "undoubler" :
                foreach (component c in componentManager.getAdjacents(pos))
                    if (componentManager.getBuffedComponents().Contains(c)) {
                        while ((c.coin/ c.template.coin) % 2 == 0)
                            c.multiplyCoins(.5f);

                        buff = true;
                    }
            break;

            case "recycle bin" :
                foreach (component c in componentManager.getAdjacents(pos))
                    if (c.destroyed) {
                        This.selfGainCoins(-10);
                        
                        componenttemplate random = componentManager.polymorph;

                        while(random.rarity >= c.template.rarity)
                            random = shop.possibilities[UnityEngine.Random.Range(0, shop.possibilities.Count)];

                        componentManager.addComponent(componentManager.GetPosition(c), random);
                    }
            break;

            case "traveler" :
                if (componentManager.getNewComponents().Contains(This))
                    This.gainCoins(5);
            break;

            case "buff conduit" :
                Vector2Int[] directions = {(1, 0).v(), (1, 1).v(), (0, 1).v(), (-1, 1).v(), (-1, 0).v(), (-1, -1).v(), (0, -1).v(), (1, -1).v()};
                
                component gaaaah;
                foreach (Vector2Int direction in directions) {
                    gaaaah = componentManager.getComponent(pos + direction);
                    if (gaaaah.template)
                        if (componentManager.getComponent(pos + direction * 2).template.tags.ContainsRange(This.template.tags)) {
                            componentManager.getComponent(pos + direction * 2).sparkle();
                            gaaaah.multiplyCoins(3);
                            buff = true;
                        }
                }
            break;

            case "moving buffer" :
                Vector2Int dir = Directions[var1], next = pos + dir;

                while (componentManager.validPos(next)) {
                    if (componentManager.getComponent(next).template != null)
                        break;

                    next += dir;
                }

                This.activated = true;
                componentManager.moveComponent(pos, next, false);
                This.activated = false;

                var1 ++;
                var1 %= 3;
                
                componentAnimation Anime = metaData.animations["rotating"];
                Anime.rotationCoef = 90;
                
                coroutiner.start(Anime.play(This.transform, componentManager.AnimTime));
            break;

            case "stonker" :
                if (This.coin > This.template.coin * var1) {
                    foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                        c.Activate(componentManager.GetPosition(c));

                    var1 ++;
                    This.template.getBehaviours<cyclicBehaviour>()[0].cyclingTime = var1;
                    This.template.description = This.template.description.Replace(var1.ToString(), (var1 + 1).ToString());
                    This.template.description = This.template.description.Replace((var1 -1).ToString(), var1.ToString());
                }
            break;

            case "graveyard" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.destroyed)
                        foreach (componentBehaviour behaviour in c.template.deathBehaviours)
                            behaviour.activate(componentManager.GetPosition(c));
            break;

            case "repetitor" :
                foreach (component c in componentManager.getAdjacents(pos))
                    if (c.isPowered)
                        c.template.Electrify(componentManager.GetPosition(c));
            break;

            case "battery" :
                if (This.template.electricGenerator)
                    This.template.electricGenerator = false;
                else if (This.isPowered)
                    This.template.electricGenerator = true;
            break;

            case "mohole" :
                bool bah = false;

                foreach(component c in componentManager.getAdjacents(pos)) {
                    if (!c.template)
                        continue;

                    if (c.template.rarity == 0) {
                        bah = true;

                        // eaten item destruction
                        componentManager.remove(new Vector2Int(-1, -1), (0, -1).v(), c);

                        Anim(c, typeof(eaterBehaviour).ToString());
                    }
                }

                if (bah) 
                    eat = true;
            break;

            case "comitee" :
                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template.tags.Contains("detritus") || c == componentManager.polymorph)
                        if (c.destroyed) {
                            c.sparkle();
                            This.gainCoins(10);
                            if (componentManager.getNewComponents().Contains(c))
                               This.gainCoins(10);
                        }
            break;

            case "engine" :
                int Count = 0;
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("core") || c == componentManager.polymorph) {
                        Count ++;
                        componentManager.remove((0, -1).v(), (0, -1).v(), c);
                        eat = true;
                    }

                if (Count >= var1) {
                    var1 ++;
                    This.permaGainCoins(This.template.coin);
                    This.template.description = This.template.description.Replace((var1 -1).ToString(), var1.ToString());
                }
            break;

            case "divorced" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains(name))
                        This.permaGainCoins(3 * Mathf.CeilToInt((pos - componentManager.GetPosition(c)).magnitude));
            break;

            case "gene modifier" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("animal") && componentManager.getNewComponents().Contains(c)) 
                        c.permaGainCoins(1);
            break;

            case "factory line" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                    List<eaterBehaviour> behaviours1 = c.template.getBehaviours<eaterBehaviour>();
                    if (behaviours1.Count > 0 && behaviours1[0].possibilities.Find(possible => possible.tags.Contains("core")))
                            c.activated = true;

                    List<specialBehaviour> behaviours2 = c.template.getBehaviours<specialBehaviour>();
                    if (behaviours2.Count > 0 && behaviours2[0].name == "catalyser special behaviour")
                            c.activated = true;
                }
            break;

            case "core container" :
                This.template.tags.Add("containing");
            break;

            case "core container1" :
                This.template.getBehaviours<cyclicBehaviour>()[0].isActive = This.template.tags.Contains("containing");
            break;

            case "core container2" :
                This.template.tags.Remove("containing");
            break;

            case "prioritizer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (!c.activated && !c.template.tags.Contains("time machine") && c.template != componentManager.polymorph) {
                        c.Activate(componentManager.GetPosition(c));
                        c.activated = false;
                    }
            break;

            case "preturner" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                    foreach (componentBehaviour behaviour in c.template.getBehaviours<componentBehaviour>(baseObject.time.preTurn))
                        behaviour.activate(componentManager.GetPosition(c));
                    c.activated = true;
                }
            break;

            case "postturner" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    foreach (componentBehaviour behaviour in c.template.getBehaviours<componentBehaviour>(baseObject.time.postTurn))
                        behaviour.activate(componentManager.GetPosition(c));
            break;

            case "welcomer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (componentManager.getNewComponents().Contains(c))
                        c.permaGainCoins(1);
            break;

            case "time machine" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.activated && !c.template.tags.Contains("time machine") && c.template != componentManager.polymorph)
                        c.activated = false;
            break;

            case "time buffer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.activated) {
                        c.multiplyCoins(2);
                        buff = true;
                    }
            break;

            case "pointer" :
                Vector2Int[] dirs = new Vector2Int[]{(1, 0).v(), (0, 1).v(), (-1, 0).v(), (0, -1).v()};
                Vector2Int aim = pos + dirs[var1];

                if (!componentManager.validPos(aim))
                    return false;

                component e = componentManager.getComponent(aim);

                while (!e.template) {
                    aim += dirs[var1];

                    if (!componentManager.validPos(aim))
                        return false;
                }

                e.Activate(aim);
                e.multiplyCoins(2);
                buff = true;
            break;

            case "money pile" :
                var1 ++;

                This.template.description.Replace((var1 - 1).ToString().ToCharArray()[0], var1.ToString().ToCharArray()[0]);
            break;

            case "time paradox" :
                component blurg = componentManager.GetNext(pos);

                if (blurg != null)
                    if (blurg.activated)
                        foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                            if (!c.template.tags.Contains("time machine") && !c.template.tags.Contains("time paradox") && c.template != componentManager.polymorph) {
                                c.Activate(componentManager.GetPosition(c));
                                c.activated = false;
                            }
            break;

            case "turn" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    c.Activate(componentManager.GetPosition(c));
            break;

            case "double mover" : 
                foreach (component c in componentManager.GetAll().removeNulls()) {
                    foreach (moverBehaviour behaviour in c.template.getBehaviours<moverBehaviour>())
                        behaviour.activate(componentManager.GetPosition(c));

                    foreach (animalBehaviour behaviour in c.template.getBehaviours<animalBehaviour>())
                        behaviour.activate(componentManager.GetPosition(c));
                }
            break;

            case "delayed buffer" :
                This.template.getBehaviours<bufferBehaviour>()[0].isActive = var1 == 1;

                if (This.coin > This.template.coin)
                    var1 = 1;
                else
                    var1 = 0;
            break;

            case "activated" :
                This.selfMultiplyCoins(2);
            break;

            case "change" :
                int current = componentManager.getAdjacents(pos).removeNulls().Count;

                This.selfGainCoins(5 * Mathf.Abs(current - var1));

                var1 = current;
            break;

            case "duplicate" :
                if (componentManager.getAdjacents(pos).removeNulls().Find(c => c.template == This.template) == null)
                    componentManager.spawnComponent(pos, This.template);
            break;

            case "noble" :
                int nobleCount = 0;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("noble"))
                        nobleCount ++;
    
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    c.multiplyCoins(c.template.rarity * nobleCount);
            break;

            case "duplicator" :
                This.template.getBehaviours<spawnerBehaviour>()[0].spawn.AddSingle(
                    new List<componenttemplate>{componentManager.getComponent(pos + (0, -1).v()).template});
            break;

            case "polyvalent" :
                List<string> packs = new List<string>();

                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) 
                    if (c.template.pack != metaData.allPacks)
                        packs.AddSingle(c.template.pack);

                This.selfMultiplyCoins(packs.Count);
            break;

            case "expert" : 
                Dictionary<string, int> Packs = new Dictionary<string, int>();

                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) 
                    if (c.template.pack != metaData.allPacks)
                        foreach (string s in c.template.pack) {
                            if (Packs.ContainsKey(s))
                                Packs[s] ++;
                            else
                                Packs.Add(s, 1);
                        }

                This.selfMultiplyCoins(Mathf.Max(Packs.Values.ToArray()));
            break;

            case "communist" :
                This.selfMultiplyCoins(.5f);
            break;

            case "merger" :
                List<component> checks = new List<component>(), merged = new List<component>();

                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                    if ((merged = checks.FindAll(C => c.template.tags.ContainsRange(C.template.tags))).Count != 0) {
                        string name = c.name;
                        componenttemplate  Spawn;
                        (Spawn = ScriptableObject.CreateInstance<componenttemplate>()).setTo(c.template);
                        int mergeCount = 0;

                        List<Sprite> symbols = new List<Sprite>();

                        foreach (component merge in merged) {
                            mergeCount ++;
                            Spawn += merge.template;
                            componentManager.remove((0, -1).v(), (0, -1).v(), merge);
                            checks.Remove(merge);
                        }

                        Spawn.symbol = spriteMerger.merge(symbols.ToArray(), mergePos[mergeCount], null, 1 / Mathf.Ceil(Mathf.Sqrt(mergeCount)));

                        componentManager.spawnComponent(pos, Spawn);
                    } else 
                        checks.Add(c);
                }
            break;

            case "necromant" :
                specialBehaviour zombie = Resources.Load<specialBehaviour>("components/component pack/zombie");
                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) {
                    c.template.deathBehaviours.Add(zombie);
                    if (c.template.description.Contains("When destroyed"))
                        c.template.description = c.template.description.Replace("When destroyed", "When destroyed, this component loses this action, and a copy of this component is spawned in an adjacent empty space, ");
                    else
                        c.template.description += "\n- When destroyed, this component loses this action, and a copy of this component is spawned in an adjacent empty space.";

                    c.template.name = "zombie " + c.template.name;
                }
            break;

            case "zombie" :
                componenttemplate z = This.template;

                z.deathBehaviours.Remove(Resources.Load<specialBehaviour>("components/component pack/zombie"));

                z.description = z.description.Replace("When destroyed, this component loses this action, and a copy of this component is spawned in an adjacent empty space", "When destroyed");
                z.name = z.name.Remove(0, 7);

                componentManager.spawnComponent(pos, z);
            break;

            case "is" :
                if (pos.x == 0 || pos.x == componentManager.size.x - 1)
                    break;

                component left = componentManager.getComponent(pos + (0, -1).v()), right = componentManager.getComponent(pos + (0, 1).v());
                var2 = left.template;

                if (left.template == null || right.template == null)
                    break;

                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template == left.template)
                        c.template.behaviours.AddAll(right.template.behaviours);
            break;

            case "is2" :
                foreach (component c in componentManager.GetAll().removeNulls())
                    if (c.template.tags.ContainsRange(var2.tags))
                        c.template.behaviours = var2.behaviours;
            break;

            case "bulldozer" :
                Vector2Int forward = This.template.getBehaviours<moverBehaviour>()[0].direction;

                componentManager.remove(pos + forward, (0, -1).v());
            break;

            case "upgrader" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    c.permaGainCoins(1);
            break;

            case "back and forth buffer" :
                if (pos.x == 0 || pos.x == componentManager.size.x-1) 
                    break;

                component g = componentManager.getComponent(pos + This.template.getBehaviours<moverBehaviour>()[0].direction);

                g.multiplyCoins(2);
                buff = true;
            break;

            case "spawn buffer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (componentManager.getNewComponents().Contains(c)) {
                        buff = true;
                        break;
                    }

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    c.multiplyCoins(2);
            break;

            case "seller" :
                if (pos.x == 0)
                    break;

                component target = componentManager.getComponent(pos - (1, 0).v());

                if (target.template == null)
                    break;

                componentManager.remove(pos - (1, 0).v(), pos);
                eat = true;
                for (int j = 0; j < target.template.coin; j++)
                    componentManager.spawnComponent(pos, componentManager.allComponents.Find(c => c.tags.Contains("coin component")));
            break;

            case "zombificator" :
                specialBehaviour Zombie = Resources.Load<specialBehaviour>("components/component pack/zombie");

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (componentManager.getNewComponents().Contains(c)) {
                        c.template.deathBehaviours.Add(Zombie);
                        if (c.template.description.Contains("When destroyed"))
                            c.template.description = c.template.description.Replace("When destroyed", "When destroyed, this component loses this action, and a copy of this component is spawned in an adjacent empty space, ");
                        else
                            c.template.description += "\n- When destroyed, this component loses this action, and a copy of this component is spawned in an adjacent empty space.";

                        c.template.name = "zombie " + c.template.name;
                    }
            break;

            case "strengthener" :
                cyclicBehaviour cyclic = This.template.getBehaviours<cyclicBehaviour>()[0];
                if (cyclic.isActive = (cyclic.behavioursToActivate[0] as spawnerBehaviour).spawn.Count > 0)
                    break;

                if (pos.x == 0)
                    break;

                (var2 = ScriptableObject.CreateInstance<componenttemplate>()).setTo(componentManager.getComponent(pos - (0, 1).v()).template);

                if (var2 == null)
                    break;

                componentManager.remove(pos - (0, 1).v(), (0, -1).v());
                var2.behaviours[baseObject.time.turn].Add(Resources.Load<cyclicBehaviour>("components/component pack/activate"));
                if (var2.description.Contains("When activated"))
                    var2.description = var2.description.Replace("When activated", "- When activated, its cycle advances of 1.\n(1 / 2) : this component is activated again, ");
                else
                    var2.description += "\n- When activated, its cycle advances of 1.\n(1 / 2) : this component is activated again.";

                if (!var2.name.EndsWith(" ") && !var2.name.EndsWith("I"))
                    var2.name += " ";
                var2.name += "I";
            
                (cyclic.behavioursToActivate[0] as spawnerBehaviour).spawn.Add(var2);
            break;

            case "strengthener complete" :
                (This.template.getBehaviours<cyclicBehaviour>()[0].behavioursToActivate[0] as spawnerBehaviour).spawn.Clear();
            break;

            case "hermit" :
                This.selfGainCoins(Mathf.Clamp(componentManager.GetAll().removeNulls().Count - componentManager.getAdjacents(pos).removeNulls().Count, 0, 25));
            break;

            case "activate" :
                if (var1 == componentManager.turns)
                    break;
                    
                This.Activate(pos);

                var1 = componentManager.turns;
            break;

            case "mechanical overflow" :
                This.selfGainCoins(3 * var.cranks);
                This.gainCranks(-var.cranks);
            break;

            case "crank market" :
                int money = 0;

                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    money += c.coin;

                This.gainCranks(Mathf.FloorToInt(money / 5f));
            break;

            case "powered buffer" :
                foreach (component c in componentManager.getAdjacents(pos).removeNulls()) 
                    if (c.isPowered) {
                        buff = true;
                        c.multiplyCoins(2);
                    }
            break;

            case "electrolyse" :
                if (This.isPowered)
                    break;

                This.selfGainCoins(componentManager.getAdjacents(pos).removeNulls().FindAll(c => c.template.conductive).Count * 3);
            break;

            case "tesla coils" :
                if (This.isPowered)
                    break;

                This.selfGainCoins(componentManager.GetAll(c => c.isPowered).Count * 2);
            break;

            case "alternative current 0" :
                if (This.isPowered) {
                    coroutiner.start(metaData.sounds["powered"].play());
                    This.permaGainCoins(1);
                }
            break;

            case "alternative current 1" :
                if (This.isPowered) {
                    coroutiner.start(metaData.sounds["powered"].play());
                    This.permaGainCoins(-1);
                }
            break;

            case "moving wire" : 
                if (!componentManager.validPos(pos + Directions[var1]) || componentManager.getComponent(pos + Directions[var1]).template != null) {
                    componentAnimation Anim = metaData.animations["rotating"];
                    Transform t = This.transform;

                    var1 += 2;
                    var1 %= 4;
                    Anim.rotationCoef = 180;
                    
                    coroutiner.start(Anim.play(t, componentManager.AnimTime));
                } else {
                    This.activated = true;
                    componentManager.moveComponent(pos, pos + Directions[var1], false);
                    This.activated = false;
                }
            break;

            case "random unique start" :
                var2 = This.template;
                componenttemplate New = componentManager.allComponents[0];
                while (New.rarity < 5)
                    New = componentManager.allComponents[UnityEngine.Random.Range(0, componentManager.allComponents.Count)];

                New.symbol = spriteMerger.merge(new Sprite[] {This.template.symbol, var2.symbol}, new Vector2Int[] {(0, 0).v(), (256, 256).v()}, new Vector2[] {(1f, 1f).v(), (.5f, .5f).v()});

                New.behaviours[baseObject.time.postTurn].Add(Resources.Load<specialBehaviour>("components/component pack/random unique end"));

                componentManager.removeDiscrete(pos);
                componentManager.addComponentDiscrete(pos, New);

                componentManager.getComponent(pos).template.PreTurn(pos);
            break;

            case "random unique end" :
                componentManager.removeDiscrete(pos);
                componentManager.addComponentDiscrete(pos, var2);
            break;

            case "nuclear recycler" :
                int wount = 0;
                foreach (component c in componentManager.getAdjacents(pos).removeNulls())
                    if (c.template.tags.Contains("detritus") || c == componentManager.polymorph) {
                        wount ++;
                        componentManager.remove((0, -1).v(), (0, -1).v(), c);
                        eat = true;
                    }

                while (wount >= 2) {
                    wount -= 2;
                    componentManager.spawnComponent(pos, var2);
                }
            break;

            case "scrabble champion" :
                This.gainCoins(This.template.name.Length);
            break;

            case "animal0" :
                This.template.getBehaviours<specialBehaviour>(baseObject.time.postTurn).Find(spe => spe.name == "animal post").var1 = Convert.ToInt32(pos.x.ToString() + pos.y.ToString());
            break;

            case "animal1" :
                Vector2Int dist = pos - (Convert.ToInt32(var1.ToString()[0].ToString()), Convert.ToInt32(var1.ToString()[1].ToString())).v();
                This.gainCoins(This.coin * (Mathf.Abs(dist.x) + Mathf.Abs(dist.y) - 1));
            break;
        }

        anim = (buff || eat) && triggerAnim;

        if (triggerAnim) { 
            if (buff)
                Anim(This, typeof(bufferBehaviour).ToString());
            if (eat)
                Anim(This, typeof(eaterBehaviour).ToString());
        }

        return anim;
    }

    public override bool click(Vector2Int pos) {
        componentAnimation anim = metaData.animations["rotating"];
        component This = componentManager.getComponent(pos);
        Transform t = This.transform;

        switch (Name) {
            case "arrow":
                var1 = UnityEngine.Random.Range(0, 8);

                anim.rotationCoef = t.localEulerAngles.z - var1 * 45;
            
                t.GetComponent<component>().StartCoroutine(anim.play(t, componentManager.AnimTime));
            break;

            case "pointer" :
                This.template.clicked = false;

                var1 ++;
                var1 %= 4;
                anim.rotationCoef = -90;
                
                coroutiner.start(anim.play(t, componentManager.AnimTime));
            break;

            case "moving wire" :
                This.template.clicked = false;

                var1 ++;
                var1 %= 4;
                anim.rotationCoef = -90;
                
                coroutiner.start(anim.play(t, componentManager.AnimTime));
            break;

            case "money pile" :
                componentManager.getComponent(pos).gainCoins(5 * var1);
                componentManager.remove(pos, (0, -1).v());
            break;

            case "destroy" :
                componentManager.remove(pos, (0, -1).v());
            break;

            case "quick money" :
                componentManager.spawnComponent(pos, componentManager.allComponents.Find(c => c.tags.Contains("credit card")));
            break;
        }

        return false;
    }

    public override componentBehaviour copy() {
        specialBehaviour c = ScriptableObject.CreateInstance<specialBehaviour>();

        c.name = name;
        c.isActive = isActive;
        c.triggerAnim = triggerAnim;
        c.isTerrain = isTerrain;

        c.Name = Name;
        c.adjacencyRequirement = adjacencyRequirement;
        c.var1 = var1;
        c.var2 = var2;

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

        s += $"{Indent}Name:{Name};\n";
        s += $"{Indent}adjacencyRequirement:{adjacencyRequirement};\n";
        s += $"{Indent}var1:{var1};\n";
        s += $"{Indent}var2:" + (var2 == null ? "null;\n" : ("{\n" + var2.encode(indent + 1) + Indent + "};\n"));

        return s;
    }

    public override void decode(gameSave.element e) {
        name = e["name"];
        isActive = Convert.ToBoolean(e["isActive"]);
        triggerAnim = Convert.ToBoolean(e["triggerAnim"]);
        isTerrain = Convert.ToBoolean(e["isTerrain"]);

        Name = e["Name"];
        adjacencyRequirement = Convert.ToBoolean(e["adjacencyRequirement"]);
        var1 = Convert.ToInt32(e["var1"]);
        if (e["var2"] != "null") {
            var2 = ScriptableObject.CreateInstance<componenttemplate>();
            var2.decode(e["var2"]);
        }
    }
}