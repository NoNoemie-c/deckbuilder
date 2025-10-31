using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class tutorialModule : MonoBehaviour
{
    public enum fail : int {
        wait,
        lose,
        loseEx
    }
    public static int tuto = 0;
    public static tutorialModule This;
    public static string lastDisplayed;
    public static bool validate, validation;
    public static bool EndEarly = false, showCoins = true;
    public static fail punishFail = fail.lose;
    public AnimationCurve flashCurve;
    public List<string> len;

    public void Update() {
        if (len.Count == 0)
            transform.Find("len").position = (-2000f, 0f).v();
    }

    public IEnumerator trigger1() {
        punishFail = fail.wait;
        appear.flashCurve = flashCurve;
        validate = false;
        lastDisplayed = "";
        transform.Find("len").gameObject.SetActive(true);
        transform.Find("prompts").gameObject.SetActive(true);
        len = new List<string>();

        StartCoroutine(introduce("shop/skip button", "skip button", () => componentManager.GetAll().removeNulls().Count == componentManager.size.x * componentManager.size.y, "Left click to go directly to the next turn without placing a new component.", true));
        StartCoroutine(introduce("vars/move button", "move button", () => componentManager.GetAll().removeNulls().Count == componentManager.size.x * componentManager.size.y - 10, "Left click a component on the grid to select it, and left click an empty space to send it there. Costs 1 moving consumable.", true, delegate() {var.movers = 2; return true;}));
        StartCoroutine(introduce("vars/remove button", "remove button", () => componentManager.GetAll().removeNulls().Count == componentManager.size.x * componentManager.size.y - 5, "Left click a <b>component</b> while having selected this button to destroy it. Costs 1 removal consumable. ('when destroyed' effects are not triggered by this)", true, delegate() {var.removes = 2; return true;}));
        StartCoroutine(introduce("vars/destroyTimer", "destruction timer", () => componentManager.currentTurn() == 11, "after the written amount of turns, you will have to destroy some of your components. The amount is at the right of this.", true));

        shop.addComponent(componentManager.allComponents.Find(c => c.name == "credit card"), 0).gameObject.AddComponent<appear>();
        shop.addComponent(componentManager.allComponents.Find(c => c.name == "coin component"), 1).gameObject.AddComponent<appear>();
        shop.addComponent(componentManager.allComponents.Find(c => c.name == "bank"), 2).gameObject.AddComponent<appear>();

        transform.BroadcastMessage("InstantDisappear", SendMessageOptions.DontRequireReceiver);

        yield return coroutiner.start(clicker.transitionIn());

        yield return new WaitForSeconds(clicker.debug ? 0 : 3);

        StartCoroutine(appFlash("vars/rent"));
        app("vars/coins");

        StartCoroutine(mousePrompt("prompts/mousePrompt", 5, () => lastDisplayed == "rent"));
        yield return new WaitUntil(() => lastDisplayed == "rent");
        informationWindow.Override("Your goal is to collect as many coins as written here.");
        len.Remove("vars/rent");
        StartCoroutine(appFlash("information window/first comp/name", false));
        StartCoroutine(appFlash("information window/first comp/desc mask", false, false));
        StartCoroutine(appFlash("information window/first comp/image", false, false));
        StartCoroutine(appFlash("information window/first comp/coins", false, false));
        StartCoroutine(appFlash("information window/first comp/background", false, false));
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash("vars/timer"));

        yield return new WaitUntil(() => lastDisplayed == "timer");
        informationWindow.Override("This is your timer, it displays the number of turns left before the rent is taken from you. After, and only after this amount of turns will the rent be taken, will you continue to the next challenge. If you have more, stockpile them for the next rent !");
        len.Remove("vars/timer");
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash("vars/coinsTxt")); 
        
        yield return new WaitUntil(() => lastDisplayed == "coin counter");
        informationWindow.Override("This is your coin counter. Gain as many of these as possible to survive the rents !\nTo do so, place <b>components</b> on the <b>grid</b>.");
        len.Remove("vars/coinsTxt");
        yield return StartCoroutine(WaitForValidate(2));

        app("shop");
        StartCoroutine(appFlash(shop.proposal[1].gameObject));

        yield return new WaitUntil(() => lastDisplayed == "coin component");
        informationWindow.Override("Left click to select this <b>component</b>, and then left click on the <b>grid</b> to place it.\nAs you can see in the top right, this component gives 1 coin.");
        len.Remove(shop.proposal[1].gameObject.name);
        StartCoroutine(mousePrompt("prompts/mouseLeftPrompt", 5, () => GetComponent<clicker>().select?.template.name == "coin component"));

        yield return new WaitUntil(() => GetComponent<clicker>().select?.template.name == "coin component");
        
        app("grid");

        yield return new WaitUntil(() => componentManager.GetAll().removeNulls().Count > 2);

        clicker.activate(shop.proposal[1].gameObject, false);
        clicker.DontAffect.Add(shop.proposal[1].gameObject);

        yield return new WaitUntil(() => componentManager.actual > 0);

        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("well done ! But can you get 18 coins in only 3 turns ?");
        yield return StartCoroutine(WaitForValidate(1));

        clicker.DontAffect.Remove(shop.proposal[1].gameObject);
        clicker.activate(shop.proposal[1].gameObject, true);
        
        yield return new WaitUntil(() => componentManager.fail);
        componentManager.actual --;
        var.Rent = componentManager.Rent[componentManager.actual].x;
        componentManager.fail = false;
        yield return StartCoroutine(clicker.gameOver());
        var.coins = 0; var.movers = var.removes = 2;
        componentManager.updating = false;

        yield return new WaitForSeconds(clicker.debug ? 0 : 3);
        
        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("uh you lost.\nBut I kinda made this impossible, so it doesn't count. Here, take <b>these</b>, they might help");
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash(shop.proposal[0].gameObject));

        yield return new WaitUntil(() => lastDisplayed == "credit card");
        informationWindow.Override("gives 2 coins");
        len.Remove(shop.proposal[0].gameObject.name);

        yield return new WaitUntil(delegate() {
            if (componentManager.fail) {
                componentManager.actual --;
                var.Rent = componentManager.Rent[componentManager.actual].x;
                componentManager.fail = false;
                StartCoroutine(clicker.gameOver());
                var.coins = 0; var.movers = var.removes = 2;

                return false;
            }

            if (componentManager.GetAll().removeNulls().Count > 2) {
                
            }

            return componentManager.actual > 1;
        });

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Ok, now we can make money for real, with <b>buffer</b> components like the bank.");
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash(shop.proposal[2].gameObject));

        clicker.activate(shop.proposal[0].gameObject, false);
        clicker.DontAffect.Add(shop.proposal[0].gameObject);

        yield return new WaitUntil(() => lastDisplayed == "bank");
        informationWindow.Override("- When activated, all adjacent coin components (and only coin components) give +2. (stackable)");
        len.Remove(shop.proposal[2].gameObject.name);

        yield return new WaitUntil(delegate() {
            if (componentManager.fail) {
                componentManager.actual --;
                var.Rent = componentManager.Rent[componentManager.actual].x;
                componentManager.fail = false;
                StartCoroutine(clicker.gameOver());
                var.coins = 0; var.movers = var.removes = 2;

                return false;
            }

            return componentManager.actual > 2;
        });

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Awesome ! The tutorial is done for now. To continue, get to the 5th rent cycle.");
        app("information window/animTime");
        app("information window/coinTime");
        var.movers = var.removes = 2;
        yield return StartCoroutine(WaitForValidate(1));

        yield return new WaitUntil(delegate() {
            if (componentManager.fail) {
                componentManager.actual = 2;
                var.Rent = componentManager.Rent[componentManager.actual].x;
                componentManager.fail = false;
                StartCoroutine(clicker.gameOver());
                var.coins = 0; var.movers = var.removes = 2;

                return false;
            }

            return componentManager.actual > 5;
        });

        saveFile.save(1);
        yield return StartCoroutine(clicker.gameOver());
        yield return StartCoroutine(clicker.transitionOut());
        saveFile.next();
    }

    public IEnumerator trigger2() {
        var.movers = var.removes = 0;
        lastDisplayed = "";transform.Find("len").gameObject.SetActive(true);
        appear.flashCurve = flashCurve;
        validate = false;
        punishFail = fail.lose;

        instaDis("vars/reroll button");
        transform.Find("information window").BroadcastMessage("InstantDisappear", SendMessageOptions.DontRequireReceiver);
        component wallet = componentManager.getComponent((2, 2).v()), investor = componentManager.getComponent((1, 0).v());
        wallet.gameObject.AddComponent<appear>().InstantDisappear();
        investor.gameObject.AddComponent<appear>().InstantDisappear();
        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);
        shop.addComponent(componentManager.allComponents.Find(c => c.name == "credit card"), 0).gameObject.AddComponent<appear>();
        shop.addComponent(componentManager.allComponents.Find(c => c.name == "coin component"), 0).gameObject.AddComponent<appear>();
        shop.addComponent(componentManager.allComponents.Find(c => c.name == "bank"), 0).gameObject.AddComponent<appear>();
        foreach (component c in shop.proposal) {
            clicker.activate(c.gameObject, false);
            clicker.DontAffect.Add(c.gameObject);
        }

        app("information window/first comp/name");
        app("information window/first comp/desc mask");
        app("information window/first comp/image");
        app("information window/first comp/coins");
        app("information window/first comp/background");
        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Hope you had fun playing with banks, because components are only going to get crazier. Here, look at these !");

        yield return coroutiner.start(clicker.transitionIn());

        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash(investor.gameObject));

        yield return new WaitUntil(() => lastDisplayed == "investor");
        len.Remove(investor.gameObject.name);
        app("information window/first comp/rarity");
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash(wallet.gameObject));

        yield return new WaitUntil(() => lastDisplayed == "wallet");
        len.Remove(investor.gameObject.name);
        yield return StartCoroutine(WaitForValidate(2));

        yield return new WaitForSeconds(1);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Yeah, that's kinda complicated. Maybe you should skip, and see for yourself what they do.");
        yield return StartCoroutine(WaitForValidate(2));

        clicker.DontAffect.Remove(transform.Find("shop/skip button").gameObject);
        clicker.activate(transform.Find("shop/skip button").gameObject, true);

        yield return new WaitUntil(() => componentManager.actual > 0);

        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);
        
        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Show that you understand how these work, and pick a component. Choose wisely !");
        yield return StartCoroutine(WaitForValidate(2));
        
        yield return StartCoroutine(shop.roll(3));

        informationWindow.tipOverride("bank", componentManager.allComponents.Find(c => c.name == "bank").description + "\n\nThis component is a buffer, it makes other components give more. Most startegies rely on this type of components to make lots of money.");
        informationWindow.tipOverride("credit card", componentManager.allComponents.Find(c => c.name == "credit card").description + "3 coins is pretty good early on, but beware ! Soon enough, you'll realise that it's taking precious space. Only take this in the early game if you are struggling, or if you have a good use for this, maybe with a compoent whose name starts with 's' ?");
        informationWindow.tipOverride("coin component", componentManager.allComponents.Find(c => c.name == "coin component").description + "1 coin per turn might seem very small, but with a clever arrangement of banks, it will be worth more than a lot of components.");
        informationWindow.tipOverride("temporary money", componentManager.allComponents.Find(c => c.name == "temporary money").description + "\n\nIt only gives 2 coins, but being able to destroy it later is quite useful long-term. You need the coins in the early game, but once you find something better replacing this component gives more space. A crucial resource in this game.");
        informationWindow.tipOverride("loan", componentManager.allComponents.Find(c => c.name == "loan").description + "\n\nThis component will give lots of money and be useful in the early game, but it will quickly become detrimental (after 8 turns), and later in the game, it will no longer be worth the space it takes, and by then you'll have something better to do with this precious space.");

        yield return new WaitUntil(() => GetComponent<clicker>().select != null);

        informationWindow.Override(GetComponent<clicker>().select?.template.name == "bank" ? "You made the right choice ! Indeed, this component is going to give you the most money : 2 for the component itself + 2 for the wallet + 2 for the coin component spawned by the investor (every 3 turns) = 4/turn + 2/3turns" 
        : "nope you should have picked the bank, Indeed, this component is going to give you the most money : 2 for the component itself + 2 for the investor + 2 for the coin component spawned by the investor (every 3 turns) = 4/turn + 2/3turns. Better than just 2 or 3.");
        yield return StartCoroutine(WaitForValidate(2));

        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);
        foreach (component c in shop.proposal) {
            clicker.activate(c.gameObject, false);
            clicker.DontAffect.Add(c.gameObject);
        }

        yield return new WaitUntil(() => componentManager.updating);
        yield return new WaitUntil(() => !componentManager.updating);

        informationWindow.overrides.Remove("bank");

        clicker.DontAffect.Remove(transform.Find("shop/skip button").gameObject);
        clicker.activate(transform.Find("shop/skip button").gameObject, true);
        foreach (component c in shop.proposal) {
            clicker.DontAffect.Remove(c.gameObject);
            clicker.activate(c.gameObject, true);
        }

        informationWindow.tipOverride("investor", componentManager.allComponents.Find(c => c.name == "investor").description + "\n\nThis compoent is a spawner : by spawning components, the investor allows to increase your 'acceleration' of coins per turn (normally you gain more coins by placing new components, but this allows you to place more compoents at once, only every 3 turns, sadly). Just remember that these coin compoents will take some room !");
        informationWindow.tipOverride("wallet", componentManager.allComponents.Find(c => c.name == "wallet").description + "\n\nThis compoent is an eater : it destroys components in exchange for something. They are useful because they clear space for better components. This one in particular can be buffed by banks, and gains the coin production of what it eats. Therefore, it can be sumed up as a compactor of coin components, it combines them into one space.");

        yield return new WaitUntil(() => componentManager.actual > 1);
        
        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("Good job ! But the game is going to get harder soon, and we need to think on the long-term. The best component for this is the wallet as it saves tons of space by 'compacting' the coin compoents in one space.\n"
        + "The thing is that it's <color=#E74235>legendary</color> as you might have seen, and as the name suggests its hard to find. Instead of wasting turns looking for it, let's reroll the shop.");
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash("vars/reroll button"));
        var.rerolls = 2;

        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);
        foreach (component c in shop.proposal) {
            clicker.activate(c.gameObject, false);
            clicker.DontAffect.Add(c.gameObject);
        }

        yield return new WaitUntil(() => lastDisplayed == "reroll button");
        len.Remove("vars/reroll button"); 

        informationWindow.Override("The components on the shop aren't good enough ? Press this button to get other, rarer ones. In fact, you are guaranted at least as rare components as the current rarer you are proposed. Costs 1 reroll consumable.");

        yield return new WaitUntil(() => shop.rolling);
        yield return new WaitUntil(() => !shop.rolling);
        yield return new WaitUntil(() => shop.rolling);
        yield return new WaitUntil(() => !shop.rolling);

        clicker.DontAffect.Remove(transform.Find("shop/skip button").gameObject);
        clicker.activate(transform.Find("shop/skip button").gameObject, true);
        foreach (component c in shop.proposal) {
            clicker.DontAffect.Remove(c.gameObject);
            clicker.activate(c.gameObject, true);
        }

        yield return new WaitUntil(() => lastDisplayed == "wallet" || lastDisplayed == "credit card" || lastDisplayed == "temporary money");

        len.Remove(shop.proposal[1].gameObject.name);

        yield return new WaitUntil(() => componentManager.actual > 2);

        punishFail = fail.loseEx;
        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("I just added a bunch more compoents for you to play with. But to progress, you'll have to make it to the 6th rent, of 500 coins.");
        yield return StartCoroutine(WaitForValidate(2));

        var.removes = var.rerolls = var.movers = 2;

        // add other tips ?

        yield return new WaitUntil(() => componentManager.actual > 5);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("ggs");
        yield return StartCoroutine(WaitForValidate(2));

        saveFile.tuto2ex = false;

        saveFile.save(2);
        yield return StartCoroutine(clicker.gameOver());
        yield return StartCoroutine(clicker.transitionOut());
        saveFile.next();
    }

    public IEnumerator trigger2ex() {
        punishFail = fail.lose;
        var.movers = var.removes = var.rerolls = 2;
        lastDisplayed = "";
        transform.Find("len").gameObject.gameObject.SetActive(true);
        appear.flashCurve = flashCurve;
        validate = false;

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("welp. Maybe you'll get it next time, good luck ! (oh, and btw, I won't annoy you with more tutorial until you beat this)");
        yield return coroutiner.start(clicker.transitionIn());
        yield return StartCoroutine(WaitForValidate(2));

        informationWindow.tipOverride("investor", componentManager.allComponents.Find(c => c.name == "investor").description + "\n\nThis compoent is a spawner : by spawning components, the investor allows to increase your 'acceleration' of coins per turn (normally you gain more coins by placing new components, but this allows you to place more compoents at once, only every 3 turns, sadly). Just remember that these coin compoents will take some room !");
        informationWindow.tipOverride("wallet", componentManager.allComponents.Find(c => c.name == "wallet").description + "\n\nThis compoent is an eater : it destroys components in exchange for something. They are useful because they clear space for better components. This one in particular can be buffed by banks, and gains the coin production of what it eats. Therefore, it can be sumed up as a compactor of coin components, it combines them into one space.");

        informationWindow.tipOverride("bank", componentManager.allComponents.Find(c => c.name == "bank").description + "\n\nThis component is a buffer, it makes other components give more. Most startegies rely on this type of components to make lots of money.");
        informationWindow.tipOverride("credit card", componentManager.allComponents.Find(c => c.name == "credit card").description + "\n\n3 coins is pretty good early on, but beware ! Soon enough, you'll realise that it's taking precious space. Only take this in the early game if you are struggling, or if you have a good use for this, maybe with a compoent whose name starts with 's' ?");
        informationWindow.tipOverride("coin component", componentManager.allComponents.Find(c => c.name == "coin component").description + "\n\n1 coin per turn might seem very small, but with a clever arrangement of banks, it will be worth more than a lot of components.");
        informationWindow.tipOverride("temporary money", componentManager.allComponents.Find(c => c.name == "temporary money").description + "\n\nIt only gives 2 coins, but being able to destroy it later is quite useful long-term. You need the coins in the early game, but once you find something better replacing this component gives more space. A crucial resource in this game.");
        informationWindow.tipOverride("loan", componentManager.allComponents.Find(c => c.name == "loan").description + "\n\nThis component will give lots of money and be useful in the early game, but it will quickly become detrimental (after 8 turns), and later in the game, it will no longer be worth the space it takes, and by then you'll have something better to do with this precious space.");

        yield return new WaitUntil(() => componentManager.actual > 5);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("ggs");
        yield return StartCoroutine(WaitForValidate(2));

        saveFile.tuto2ex = false;

        saveFile.save(2);
        yield return StartCoroutine(clicker.gameOver());
        yield return StartCoroutine(clicker.transitionOut());
        saveFile.next();
    }

    public IEnumerator trigger3() {
        punishFail = fail.lose;
        var.movers = var.removes = var.rerolls = 0;
        lastDisplayed = "";
        transform.Find("len").gameObject.gameObject.SetActive(true);
        appear.flashCurve = flashCurve;
        validate = false;

        component powerPlant = componentManager.getComponent((1, 2).v()), 
                  plug = componentManager.getComponent((4, 2).v()), 
                  wire = componentManager.getComponent((2, 2).v());
        plug.gameObject.AddComponent<appear>().InstantDisappear();
        wire.gameObject.AddComponent<appear>().InstantDisappear();
        powerPlant.gameObject.AddComponent<appear>().InstantDisappear();

        clicker.activate(transform.Find("shop/skip button").gameObject, false);
        clicker.DontAffect.Add(transform.Find("shop/skip button").gameObject);
        componenttemplate w = ScriptableObject.CreateInstance<componenttemplate>();
        w.setTo(componentManager.allComponents.Find(c => c.name == "wire"));
        shop.addComponent(w, 0).gameObject.AddComponent<appear>();
        shop.addComponent(w, 1).gameObject.AddComponent<appear>();
        shop.addComponent(w, 2).gameObject.AddComponent<appear>();
        foreach (component c in shop.proposal) {
            clicker.activate(c.gameObject, false);
            clicker.DontAffect.Add(c.gameObject);
        }

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("This is the last tutorial, after that, you'll be free to play as you want. Up until now, you've only been playing with the coin pack, but there are (as of the last update) 12 in the game ! So let's check out the electric pack.");
        yield return coroutiner.start(clicker.transitionIn());
        yield return StartCoroutine(WaitForValidate(2));

        StartCoroutine(appFlash(plug.gameObject));
        informationWindow.tipOverride("plug", plug.template.description + "This component will give money when powered.");
        yield return new WaitUntil(() => lastDisplayed == "plug");
        len.Remove(plug.gameObject.name);
        yield return new WaitUntil(() => validate);

        informationWindow.tipOverride("power plant", powerPlant.template.description + "As you can see from the bottom left icon, and the effect on this component, it produces electricity, at the cost of 5 coins. It is therefore always powered.");
        StartCoroutine(appFlash(powerPlant.gameObject));
        yield return new WaitUntil(() => lastDisplayed == "power plant");
        len.Remove(powerPlant.gameObject.name);
        yield return new WaitUntil(() => validate);

        informationWindow.tipOverride("wire", "This component is conductive, as you can see in the bottom left of this text, and by the glimmer effect it has. Powered components transmit their electrcity to adjacent conductive components, making them powered, so that they transmit electricity and et caetera. But the plug is too far away to be powered, you'll need to add another wire.");
        StartCoroutine(appFlash(wire.gameObject));
        yield return new WaitUntil(() => lastDisplayed == "wire");
        len.Remove(wire.gameObject.name);
        yield return new WaitUntil(() => validate);

        foreach (component c in shop.proposal) {
            clicker.DontAffect.Remove(c.gameObject);
            clicker.activate(c.gameObject, true);
        }
        yield return new WaitUntil(() => GetComponent<clicker>().select?.name == "wire");

        informationWindow.Override("Place this between the wire and the plug to complete the circuit and bring power to the plug.");
        yield return StartCoroutine(WaitForValidate(2));
        informationWindow.overrides.Remove("wire");

        var.movers = var.rerolls = var.removes = 2;
        clicker.DontAffect.Remove(transform.Find("shop/skip button").gameObject);
        clicker.activate(transform.Find("shop/skip button").gameObject, true);
        
        yield return new WaitUntil(() => componentManager.actual > 4);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("It is usually better to pick comps of the same pack (same background color), as they will often have synergy, but the mechanic pack goes pretty well with the electrci pack you already have, so here are some of that pack.");
        yield return StartCoroutine(WaitForValidate(2));

        componentManager.updating = true;
        componentManager.canGrid = false;
        yield return StartCoroutine(shop.roll(3));
        componentManager.updating = false;
        componentManager.canGrid = true;

        string crankConsumeTip = "\n\nAt the bottom left of this text, you can see the cranks consumption of components if it is negative and production if it is positive. If there are not enough cranks in the system when it's this component's turn to activate, it will not activate, and neither will the next one do";
        informationWindow.tipOverride("spinner", componentManager.allComponents.Find(c => c.name == "spinner").description + crankConsumeTip + "\n\n4 coins is a pretty good deal for only 1 crank, but it might not be enough to stay relevant later.");
        informationWindow.tipOverride("windmill", componentManager.allComponents.Find(c => c.name == "windmill").description + crankConsumeTip + "\n\n10 coins is really good, but the crank consumption is quite high, so you won't be able to use it until a few turns.");
        informationWindow.tipOverride("mechanical overflow", componentManager.allComponents.Find(c => c.name == "mechanical overflow").description + crankConsumeTip + "\n\nYou'll often find better money for cranks, but this allows you to make use of extra cranks at the end of the turn, or if you actually WANT a component to not be activated.");

        string crankProduceTip = "\n\nSome components like this one produce cranks, like you can see in the bottom left and on counters, they add up until the end of the turn, you can see them in the bottom left of the screen. They are mainly used to activate some components that require them to activate and give money in exchange, those have negative crank production.";
        informationWindow.tipOverride("cog", crankProduceTip + "\n\ncranks are going to be very important, and although 1 is not a lot, it's still something.");
        informationWindow.tipOverride("crank market", componentManager.allComponents.Find(c => c.name == "crank market").description + crankProduceTip + "\n\nThis component can give you a lot of cranks, but remember that the cranks are produced when it activates, and so components that will get more coins later won't do so before this activates.");
        informationWindow.tipOverride("electrical converter", componentManager.allComponents.Find(c => c.name == "electrcial converter").description + crankProduceTip + "\n\nA very good source of cranks if you can power it.");
        
        yield return new WaitUntil(() => componentManager.actual > 8);

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display(); coroutiner.start(metaData.sounds["infowindowSpe"].play());
        informationWindow.Override("That's all the tutorials, I trust you with figuring out new mechanics when you see them. Play with harder and harder difficulties to gain new packs every time.");
        yield return StartCoroutine(WaitForValidate(2));

        saveFile.save(3);
        yield return StartCoroutine(clicker.gameOver());
        yield return StartCoroutine(clicker.transitionOut());
        saveFile.next();
    }

    private IEnumerator mousePrompt(string name, float time, Func<bool> stopCondition = null) {
        if (stopCondition == null)
            yield return new WaitForSeconds(clicker.debug ? 0 : time);
        else
            for (float i = 0; i < time; i += .02f) {
                if (stopCondition.Invoke())
                    yield break;
                yield return new WaitForSeconds(clicker.debug ? 0 : .02f);
            }

        app(name);

        if (stopCondition != null) {
            yield return new WaitUntil(stopCondition);

            dis(name);
        }
    }

    public static void OnDisplay(component c, baseObject t) {
        if (c != null && c.template != null)
            lastDisplayed = c.template.name;
        else if (t != null)
            lastDisplayed = t.name;
    }

    public static void OnDisplayStr(string Name) =>
        lastDisplayed = Name;

    public void OnDValidate() {
        validate = true;
        validation = false;
    }

    private static IEnumerator unValidate(float time) {
        yield return new WaitForSeconds(clicker.debug ? 0 : time);

        validate = false;
    }

    private static void app(string path) =>
        coroutiner.This.transform.Find(path).GetComponent<appear>().Appear(.5f);

    private IEnumerator appFlash(string path, bool Len = true, bool sound = true) {
        if (Len)
            len.Add(path);

        RectTransform t = transform.Find(path) as RectTransform;
        t.GetComponent<appear>().AppearFlash(.5f);

        if (sound)
            coroutiner.start(metaData.sounds["appFlash"].play());

        if (!Len)
            yield break;

        yield return new WaitForSeconds(10);

        if (!len.Contains(path))
            yield break;

        validation = true;
        
        RectTransform lenMask = transform.Find("len") as RectTransform;
        InvertedMaskImage I = lenMask.GetComponentInChildren<InvertedMaskImage>();
        lenMask.position = t.position;
        float size = (t.rect.width * t.lossyScale.x, t.rect.height * t.lossyScale.y).v().magnitude;
        lenMask.sizeDelta = (1f, 1f, 0f).v() * size;
        I.transform.position = new Vector3(801, 450.5f, 1);

        for (int i = 0; i < 50; i ++) {
            yield return new WaitForSeconds(.02f);
            I.color = new Color(0, 0, 0, i / 100f);
        }

        yield return new WaitUntil(() => !len.Contains(path));

        StartCoroutine(hideLen());
    }
    private IEnumerator appFlash(GameObject obj, bool Len = true, bool sound = true) {
        if (Len)
            len.Add(obj.name);

        RectTransform t = obj.transform as RectTransform;
        t.GetComponent<appear>().AppearFlash(.5f);

        if (sound)
            coroutiner.start(metaData.sounds["appFlash"].play());

        if (!Len)
            yield break;

        yield return new WaitForSeconds(10);

        if (!len.Contains(obj.name))
            yield break;

        validation = true;
        
        RectTransform lenMask = transform.Find("len") as RectTransform;
        InvertedMaskImage I = lenMask.GetComponentInChildren<InvertedMaskImage>();
        lenMask.position = t.position;
        float size = (t.rect.width * t.lossyScale.x, t.rect.height * t.lossyScale.y).v().magnitude;
        lenMask.sizeDelta = (1f, 1f, 0f).v() * size;
        I.transform.position = new Vector3(801, 450.5f, 1);

        for (int i = 1; i <= 50; i ++) {
            I.color = new Color(0, 0, 0, i / 100f);
            yield return new WaitForSeconds(.02f);
        }

        yield return new WaitUntil(() => !len.Contains(obj.name));

        StartCoroutine(hideLen());
    }

    private IEnumerator hideLen() {
        RectTransform lenMask = transform.Find("len") as RectTransform;
        InvertedMaskImage I = lenMask.GetComponentInChildren<InvertedMaskImage>();

        if (!informationWindow.transform.Find("first comp/validate").gameObject.activeSelf)
            validation = false;

        if (I.color.a == 0)
            yield break;

        for (int i = 50 - 1; i >= 0; i --) {
            I.color = new Color(0, 0, 0, i / 100f);
            yield return new WaitForSeconds(clicker.debug ? 0 : .02f);
        }
    }

    private static void dis(string path) =>
        This.transform.Find(path).GetComponent<appear>().Disappear(.5f);
    
    private static void instaDis(string path) =>
        This.transform.Find(path).GetComponent<appear>().InstantDisappear();

    public static IEnumerator WaitForValidate(float time) {
        validation = true;

        yield return new WaitForSeconds(clicker.debug ? 0 : time);
        
        app("information window/first comp/validate");
        yield return new WaitUntil(() => validate);
        coroutiner.start(unValidate(.05f));
        coroutiner.This.transform.Find("information window/first comp/validate").GetComponent<appear>().Disappear(.2f);
    }

    private IEnumerator introduce(string path, string name, Func<bool> when, string overrideText = "", bool waitForValidate = false, Func<bool> effect = null) {
        yield return new WaitUntil(() => when.Invoke());
        
        StartCoroutine(appFlash(path));

        if (overrideText == "")
            yield break;

        yield return new WaitUntil(() => lastDisplayed == name);

        len.Remove(path);

        informationWindow.Override(overrideText);

        if (waitForValidate)
            StartCoroutine(WaitForValidate(2));

        effect?.Invoke();
    }
}
