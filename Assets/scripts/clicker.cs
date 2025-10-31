using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/* stuff to add
    - CONCEPTS & mechanics: 
        - make the game organised in phases. the shop phase where you can add if you want 1 component to your deck but it costs all of your money 
          and a normal phase with a rent cycle and where you can acquire components of your deck in a random order. This would force the perma discard of the in-game
          shop. Maybe make synergies only appear after a certain amount of cycles, so that your deck must be evoluting constently if you want to make money.
        - // lifespan to certain cards + perma-discard on buy (never gonna get the same twice) to encourage switching of main synergy, but either limit the amount 
          of times this is required, or make LOTS of different synergies. (to consider, but not mandatory for every component)
        - badges to create another incentive of thinking 2 billion steps ahead and thinking about placing and all that,  except idk what they would give in exchange.
          maybe the component activates twice if the condition is met, but it doesn't at all if it isn't met. plus, idk how you'd get them
        - synergy packs for roguelite progression
        - // techs become normal components, and every X turns, you are getting one that makes the map bigger depending on where it's placed
        - BOSSES (one that destroys your best comp every 3 turns, one that is some sort of clone, it duplicates the stuff you acquire and places it at a bad spot (after youve placed yours))
        - comps without description, to increase exploration and experimentation
        - roguelike aspect = a comps grid where comps are upgrades and fughts and you lmove on it, but when you move in a direction, the game scrolls 
          and the stuff in the opposite direction is lost for gud (and it's randomized what appears). also, maybe the games are random encounters
        
    - P L A Y T E S T

    -/ badges
    -/ techs

    - file organizer script ?
    - resources.UnloadUnusedAssets (very dangewous, but very gud for performance)

    - taming gaming database for accessibility

    options : BUMPSCOCITY


    - component ideas : (L be a L steam page and devlog for more ideas)
        - terrain pack
        - chemistry pack ?
        - magic/inscryption cards that give coins when killed, have an attack, a defense and advance/ attack forward automatically -> strong synergy with animals (AI ??)

        - one that makes it so that only diagonal / orthogonal adjacencies work, but doubles them
        -/ make rarer components appear
        - every x truns you do 1 more turn
        - maybe let non buyables be offered ?
        - evolving ?
        - maybe polymorph is unbuyable and spawnbehaviours have a 1% chance of spawning it instead of normal spawn

        - toggle a pack
        - grid expansion

        - techs that you make yourself (it = a component of your choice) :
          every | start of turn    | it activates
                | end of turn      | it is permanently worth +1
                | times you remove | it is removed
                | times you spawn  | it is worth 2x
                | times a component is <effect of a pack>

    - badges ideas :


    - techs baby !!


    - challenge ideas :
        - midas's touch : when you buy, the comp turns adj to gold -> they don't activate anymore, but perma give x5 or smth
        - scarcity : only 2 proposal
        - no items : 1 rerolls, 2 movers, 3 removes, 4 all + when you would have gotten one, you get 10 instead
        - no text 
*/

/* category theory ??? -> coding (smh)
    /loop hero
    lone fungus
    antonblast
    shovel knight dig
    talos 2
    blasphemous 2
*/ 

public class clicker : MonoBehaviour
{
    public static gameSave save;

    public static bool debug = true;
    public static int tuto = 0;

    public component select, c;

    private string str = "";
    private static TextMeshPro text;

    private InGame controls;
    public static Vector2 mousePos;
    public Texture2D[] cursors;

    public static int state;
    public const int REMOVING = 1, MOVING = 2, DIDNTSHOP = 0;
    public Color[] buttonsColors;
    private static Button removeButton, skipButton, rerollButton, moveButton;

    public static List<GameObject> DontAffect;

    public static new Transform transform;

    public static saveFile.data d;

    public static Image transition;

    public static Dictionary<Sound.SoundType, Transform> audioSource;
    public static List<Sound> sounds;

    public static int currentProgress;

    public static AudioClip[] music;
    public static new AudioSource audio;

    void Awake() {
        transform = GetComponent<Transform>();
    }

    void Start() {
        controls = new InGame();
        debug = true;

        if (debug)
            Keyboard.current.onTextInput += character => type(character);

        cursors = new Texture2D[4]{Resources.Load<Texture2D>("images/cursor"), 
                                   Resources.Load<Texture2D>("images/removing cursor"), 
                                   Resources.Load<Texture2D>("images/moving cursor"), 
                                   Resources.Load<Texture2D>("images/buying cursor")};

        if (GetComponent<componentManager>() == null) {
            removeButton = GameObject.Find("remove button").GetComponent<Button>();
            rerollButton = GameObject.Find("reroll button").GetComponent<Button>();
            moveButton = GameObject.Find("move button").GetComponent<Button>();
            skipButton = GameObject.Find("skip button").GetComponent<Button>();
            
            transition = transform.Find("transition").GetComponent<Image>();
        }

        audio = transform.Find("music").GetComponent<AudioSource>();
        music = Resources.LoadAll<AudioClip>("music");

        text = GetComponent<TextMeshPro>();

        DontAffect = new List<GameObject>();

        tutorialModule.validation = false;

        //endGame = false;

        audioSource = new Dictionary<Sound.SoundType, Transform>{
            {Sound.SoundType.instant, transform.Find("sounds/instant")},
            {Sound.SoundType.startAnim, transform.Find("sounds/startAnim")},
            {Sound.SoundType.anim, transform.Find("sounds/anim")}
        };

        sounds = new List<Sound>();

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame() {
        if (music.Length > 0) 
            StartCoroutine(playMusic());

        save = Resources.Load<gameSave>("save file/game");
            
        tuto = 0;
        tutorialModule tutorial = GetComponent<tutorialModule>();
        if (gameObject.scene.name.Contains("tutorial")) {
            tuto = Convert.ToInt32(gameObject.scene.name[9].ToString());

            save.reset();
            save.Current = new List<string>(){"coin"};
            if (tuto == 2)
                save.Current.Add("mechanic, electric");
            componentManager.start();

            tutorialModule.This = tutorial;
            if (gameObject.scene.name.EndsWith("ex"))
                switch (tuto) {
                    case 2 :
                        tuto = -2;
                        StartCoroutine(tutorial.trigger2ex());
                    break;
                }
            else
                switch (tuto) {
                    case 1 :
                        StartCoroutine(tutorial.trigger1());
                    break;

                    case 2 :
                        StartCoroutine(tutorial.trigger2());
                    break;

                    case 3 :
                        StartCoroutine(tutorial.trigger3());
                    break;
                }

            yield break;
        }

        state = DIDNTSHOP;

        if (GetComponent<componentManager>() != null) {
            componentManager.start();
            yield break;
        }

        if (save.data == "") {
            save.reset();

            if (d.Equals(new saveFile.data()))
                d = saveFile.generate(currentProgress);

            componentManager.Rent = d.rents.ToArray();
            var.Rent = componentManager.Rent[0].x;
            for (int x = 0; x < componentManager.size.x; x++)
                for (int y = 0; y < componentManager.size.y; y++) {
                    if (d.starters[x][y] != null)
                        componentManager.addComponent((x, y).v(), d.starters[x][y]);

                    componentManager.getComponent((x, y).v()).under = d.terrains[x][y];
                }
            save.Current = d.packs;
            componentManager.destroys = d.destroys.ToArray();

            componentManager.start();
            
            save.current = componentManager.allPacks.GetAll(d.packs);

            yield return coroutiner.start(transitionIn());

            componentManager.canGrid = false;

            yield return StartCoroutine(shop.roll(3));

            componentManager.canGrid = true;
            
            save.Save();
        } else {
            componentManager.start();

            save.Load();

            yield return coroutiner.start(transitionIn());
        }
    }

    public IEnumerator playMusic() {
        while (true) {
            audio.clip = music[UnityEngine.Random.Range(0, music.Length)];
            audio.Play();

            for (int i = 0; i < 50; i++) {
                audio.volume += .02f;
                yield return new WaitForSeconds(.2f);
            }

            yield return new WaitUntil(() => audio.time >= audio.clip.length - 1);

            for (int i = 0; i < 50; i++) {
                audio.volume -= .02f;
                yield return new WaitForSeconds(.2f);
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(60, 300 +1));
        }
    }

    void Update() {
        mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //Camera.main.orthographicSize = (663.2728f / 1660) * Screen.width;

        if (GetComponent<componentManager>() == null)
            updateState(state);
    }

    void updateState(int s) {
        state = s;

        foreach (component comp in shop.proposal)
            if (comp != null)
                activate(comp.gameObject, !shop.rolling && !tutorialModule.validation);

        activate(skipButton.gameObject, !componentManager.updating && !shop.rolling && !tutorialModule.validation);
        activate(rerollButton.gameObject, var.rerolls > 0 && !shop.rolling && !tutorialModule.validation);
        activate(removeButton.gameObject, var.removes > 0 && componentManager.canGrid && !tutorialModule.validation && !componentManager.aboutToDestroy);
        activate(moveButton.gameObject, var.movers > 0 && componentManager.canGrid && !tutorialModule.validation && !componentManager.aboutToDestroy);

        Cursor.SetCursor(cursors[state], Vector2.zero, CursorMode.Auto);

        if (componentManager.canDestroy) {
            Cursor.SetCursor(cursors[REMOVING], Vector2.zero, CursorMode.Auto);
            changeColors(true);
            return;
        }

        switch (state) {
            case DIDNTSHOP :
                removeButton.changeBaseColor(new Color(1, 1, 1, 0));
                moveButton.changeBaseColor(new Color(1, 1, 1, 0));
                if (select != null) {
                    if (componentManager.canGrid)
                        changeColors(false);
                    Cursor.SetCursor(cursors[3], Vector2.zero, CursorMode.Auto);
                } else
                    changeColors(true);
            break;

            case REMOVING :
                moveButton.changeBaseColor(new Color(1, 1, 1, 0));
                removeButton.changeBaseColor(removeButton.colors.selectedColor);
                changeColors(true);
            break;

            case MOVING :
                removeButton.changeBaseColor(new Color(1, 1, 1, 0));
                moveButton.changeBaseColor(moveButton.colors.selectedColor);
                if (select != null)
                    changeColors(false);
                else
                    changeColors(true);
            break;
        }
    }

    void changeColors(bool nul) {
        foreach (component c in componentManager.transform.GetComponentsInChildren<component>()) {
            if (c == select)
                continue;

            Button b = c.GetComponentInChildren<Button>(true);

            if (nul) {
                if (c.template == null)
                    b.targetGraphic.color = Color.clear;
                else if (!componentAnimation.currentTargets.ContainsValue(c.transform)) {
                    if (componentManager.canDestroy)
                        b.targetGraphic.color = buttonsColors[REMOVING];
                    else
                        b.targetGraphic.color = buttonsColors[state];
                }
            } else {
                if (componentManager.canPlaceOn(componentManager.GetPosition(c))) 
                    b.targetGraphic.color = buttonsColors[state];
                else if (!componentAnimation.currentTargets.ContainsValue(c.transform))
                    b.targetGraphic.color = Color.clear;
            }
        }
    }

    public static void activate(GameObject g, bool b) {
        if (!g.activeSelf || DontAffect.Contains(g))
            return;

        Button button = g.GetComponentInChildren<Button>(true);
        Image image = g.GetComponentInChildren<Image>(true);
        
        button.interactable = b;
        //button.targetGraphic.enabled = b;

        if (g.TryGetComponent<component>(out component c)) 
            c.setActive(b, .5f);
        else {
            if (b)
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            else
                image.color = new Color(image.color.r, image.color.g, image.color.b, .5f);
        }
    }

    private void type(char c) {
        if (Keyboard.current.backspaceKey.isPressed || Keyboard.current.deleteKey.isPressed) { // has backspace/delete been pressed?
            if (str.Length != 0)
                str = str.Substring(0, str.Length - 1);
        } else
            str += c;

        text.text = str;
    }

    public void OnLeftClickGrid() {
        if (!componentManager.canGrid || tutorialModule.validation)
            return;

        c = componentManager.getComponent(componentManager.WorldToGrid(mousePos));

        if (componentManager.canDestroy) {
            if (c.template != null)
                StartCoroutine(remove(c));

            return;
        } else if (componentManager.aboutToDestroy)
            return;

        componenttemplate txt = shop.getComponent(str);
        if (select == null) {
            if (state == REMOVING) {
                if (c.template != null || c.under != null) 
                    StartCoroutine(remove(c));
            } else if (c.template != null) {
                if (state == MOVING) {
                    select = c;
                    StartCoroutine(metaData.animations["selected"].play(select.transform.transform, componentManager.AnimTime / 5));
                } else if (!c.template.clicked) {
                    coroutiner.start(metaData.sounds["click"].play());
                    c.click(componentManager.WorldToGrid(mousePos));
                } else
                    error();
            } else if (txt != null) {
                if (componentManager.canPlaceOn(componentManager.WorldToGrid(mousePos))) {
                    str = "";
                    StartCoroutine(acquire(mousePos, componentManager.addComponent(componentManager.WorldToGrid(mousePos), txt, true)));
                } else 
                    error();
            } else
                updateState(DIDNTSHOP);
        } else {
            if (c.template == null) {
                if (state == MOVING) {
                    if (componentManager.canPlaceOn(componentManager.WorldToGrid(mousePos))) {
                        var.movers --;
                        updateState(DIDNTSHOP);
                        StartCoroutine(clicker.ScreenShake(5, componentManager.moveComponent(componentManager.GetPosition(select), componentManager.WorldToGrid(mousePos), false, true).transform));
                        select = null;
                    }
                } else if (componentManager.canPlaceOn(componentManager.WorldToGrid(mousePos)))
                    StartCoroutine(acquire(mousePos, componentManager.addComponent(componentManager.WorldToGrid(mousePos), select.template, true)));
            } else
                error();
        }
    }

    /*public void OnPressGrid() {
        if (!componentManager.canGrid || tutorialModule.validation)
            return;

        print("press grid");

        if (state == MOVING)
            select = componentManager.getComponent(componentManager.WorldToGrid(mousePos));

        if (select?.template == null)
            select = null;
    }

    public void OnReleaseGrid() {
        if (select == null || tutorialModule.validation || !componentManager.canGrid)
            return;

        print("release grid");

        if (componentManager.canPlaceOn(componentManager.WorldToGrid(mousePos))) {
            if (state == MOVING) {
                componentManager.removeDiscrete(c: select);
                componentManager.addComponentDiscrete(componentManager.WorldToGrid(mousePos), select.template);
            } else 
                StartCoroutine(acquire(mousePos, componentManager.addComponent(componentManager.WorldToGrid(mousePos), select.template)));
        }

        select = null;
    }

    public void OnPressShop() {
        if (shop.rolling || tutorialModule.validation)
            return;

        print("shop press");

        c = shop.pick(mousePos);

        updateState(DIDNTSHOP);
        if (!tutorialModule.validation)
            select = c;

        // arrow = true;
    }

    public void OnReleaseNowhere() {
        if (select == null)
            return;

        print("releaseNowhere");

        if (state == MOVING) {
            componentAnimation anim = metaData.animations["movingTool"];
            Vector2 delta = componentManager.transform.InverseTransformPoint(componentManager.GridToWorld(componentManager.GetPosition(select))).V2() - componentManager.transform.InverseTransformPoint(select.transform.position).V2();
            anim.yPositionCoef = delta.y;
            anim.xPositionCoef = delta.x;

            StartCoroutine(anim.play(select.transform, .2f));

            updateState(DIDNTSHOP);
        }

        select = null;
    }*/

    public void OnLeftClickShop() {
        if (shop.rolling || tutorialModule.validation)
            return;

        c = shop.pick(mousePos);

        updateState(DIDNTSHOP);
        if (c == select) {
            StartCoroutine(metaData.animations["unselected"].play(select.transform, componentManager.AnimTime / 5));
            select = null;
        } else {
            select?.StartCoroutine(metaData.animations["unselected"].play(select.transform, componentManager.AnimTime / 5));

            select = c;
            StartCoroutine(metaData.animations["selected"].play(select.transform, componentManager.AnimTime / 5));
        }
    }

    private IEnumerator acquire(Vector2 pos, component c) {
        Vector2Int Pos = componentManager.WorldToGrid(pos);
        component bought = componentManager.getComponent(Pos);
        bought.progress = 0;
        bought.StopAllCoroutines();
        foreach (componentAnimation anim in componentAnimation.currentTargets.keysOf(bought.transform))
            componentAnimation.currentTargets.Remove(anim);

        componentManager.canGrid = false;

        if (select != null) {
            StartCoroutine(metaData.animations["unselected"].play(select.transform, componentManager.AnimTime / 5));
        
            componentAnimation anim = metaData.animations["movingTool"];
            Vector2 delta = componentManager.transform.InverseTransformPoint(componentManager.GridToWorld(Pos)).V2() - componentManager.transform.InverseTransformPoint(select.transform.position).V2();
            anim.yPositionCoef = delta.y;
            anim.xPositionCoef = delta.x;

            bought.trail = true;
            bought.trailStart = select.transform.position.V2();
            bought.trailEnd = componentManager.GridToWorld(Pos);

            if (tuto != 1) {
                foreach (Graphic item in select.GetComponentsInChildren<Graphic>())
                    item.enabled = false;
                
                select.gameObject.SetActive(false);
            }
            select = null;

            yield return StartCoroutine(anim.play(bought.transform, componentManager.AnimTime));
        } else
            yield return new WaitForSeconds(componentManager.AnimTime);

        coroutiner.start(metaData.sounds["place"].play());
        Instantiate(meta.buyDustPrefab, bought.transform.position, Quaternion.identity, transform);
        StartCoroutine(ScreenShake(5));

        activate(skipButton.gameObject, false);
        DontAffect.Add(skipButton.gameObject);
        foreach (component comp in shop.proposal) {
            activate(comp.gameObject, false);
            DontAffect.Add(comp.gameObject);
        }

        yield return new WaitUntil(delegate() {
            componentManager.canGrid = false;
            return !componentManager.updating;
        });

        DontAffect.Remove(skipButton.gameObject);
        activate(skipButton.gameObject, true);
        foreach (component comp in shop.proposal) {
            DontAffect.Remove(comp.gameObject);
            activate(comp.gameObject, true);
        }

        StartCoroutine(componentManager.transform.GetComponent<componentManager>().update());
    }

    private IEnumerator remove(component c) {
        updateState(DIDNTSHOP);

        if (!componentManager.canDestroy)
            var.removes --;

        c.GetComponentInChildren<Button>().interactable = false;
        c.GetComponentInChildren<Button>().targetGraphic.enabled = false;
        c.StopAllCoroutines();

        Instantiate(meta.deathPrefab, c.transform.position, Quaternion.identity, transform);
        StartCoroutine(ScreenShake(5));

        StartCoroutine(metaData.sounds["remove"].play());

        yield return StartCoroutine(metaData.animations["killerBehaviour"].play(c.transform, componentManager.AnimTime));

        if (c.template != null)
            componentManager.removeDiscrete(c: c);
        else
            componentManager.removeTerrain(componentManager.GetPosition(c));
    }

    public void OnSkip() {
        if (componentManager.updating || tutorialModule.validation)
            return;

        select = null;

        updateState(DIDNTSHOP);
        StartCoroutine(componentManager.transform.GetComponent<componentManager>().update(true));
    }

    public void OnReroll() {
        if (shop.rolling || tutorialModule.validation)
            return;

        select = null;
        updateState(DIDNTSHOP);

        if (var.rerolls > 0) {
            StartCoroutine(shop.roll(3, 1));
            var.rerolls --;
        } else
            error();
    }

    public void OnMove() {
        if (!componentManager.canGrid || tutorialModule.validation)
            return;

        if (state != MOVING) {
            if (var.movers > 0) {
                select?.StartCoroutine(metaData.animations["unselected"].play(select.transform.transform, componentManager.AnimTime / 5));
                select = null;

                updateState(MOVING);
            } else
                error();
        } else {
            select?.StartCoroutine(metaData.animations["unselected"].play(select.transform.transform, componentManager.AnimTime / 5));
            select = null;
            updateState(DIDNTSHOP);
        }
    }

    public void OnRemove() {
        if (!componentManager.canGrid || tutorialModule.validation)
            return;

        if (state != REMOVING) {
            if (var.removes > 0) {
                select?.StartCoroutine(metaData.animations["unselected"].play(select.transform.transform, componentManager.AnimTime / 5));
                select = null;
                updateState(REMOVING);
            } else
                error();
        } else
            updateState(DIDNTSHOP);
    }

    public static void error() {
        coroutiner.start(metaData.sounds["error"].play());
        //Camera.main.backgroundColor = new Color(.6f, .4f, .4f);

        text.StartCoroutine(posScreenShake(7/*, null, .1f*/));
    }

    public static IEnumerator ScreenShake(float strength, Transform waitForAnim = null, float timeBeforeStart = 0) {
        if (waitForAnim != null)
            yield return new WaitUntil(() => !componentAnimation.currentTargets.ContainsValue(waitForAnim));
        yield return new WaitForSeconds(timeBeforeStart);

        transform.Rotate(Vector3.forward * strength, Space.Self);

        while (transform.localEulerAngles.z > .001f) {
            transform.Rotate(Vector3.forward * -transform.localEulerAngles.z / 5, Space.Self);
            yield return new WaitForSeconds(.02f);
        }

        transform.localEulerAngles = Vector3.forward * 0;
    }

    public static IEnumerator posScreenShake(float strength, Transform waitForAnim = null, float timeBeforeStart = 0) {
        if (waitForAnim != null)
            yield return new WaitUntil(() => !componentAnimation.currentTargets.ContainsValue(waitForAnim));
        yield return new WaitForSeconds(timeBeforeStart);

        Camera.main.backgroundColor = new Color(.4f, .4f, .4f);

        transform.position += (strength * UnityEngine.Random.Range(-1, 2), strength * UnityEngine.Random.Range(-1, 2), 0f).v();

        while (transform.localPosition.magnitude > .001f) {
            transform.position += -transform.localPosition / 5;
            yield return new WaitForSeconds(.02f);
        }

        transform.localPosition = (0f, 0f).v();
    }

    public static IEnumerator gameOver() {
        componentManager.updating = true;
        
        Camera.main.backgroundColor = new Color(.6f, .4f, .4f);
        yield return new WaitForSeconds(1);
        foreach (component c in componentManager.GetAll().removeNulls()) {
            yield return new WaitForSeconds(.5f);

            coroutiner.start(clicker.ScreenShake(5));
            componentManager.remove((1, 0).v(), (0, -1).v(), c);
        }

        yield return new WaitForSeconds(1);
        Camera.main.backgroundColor = new Color(.4f, .4f, .4f);
        
        componentManager.updating = false;
    }

    public static IEnumerator winScreen() {
        componentManager.updating = true;
        shop.rolling = true;
        componentManager.canGrid = false;

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display();
        informationWindow.Override("well done, you've won a game !");
        tutorialModule.validation = true;

        yield return new WaitForSeconds(1);

        Image fade = transform.Find("fade").GetComponent<Image>();
        fade.gameObject.SetActive(true);
        for (float f = 0; f < .5f; f += .01f) {
            fade.color = new Color(0, 0, 0, f);
            yield return new WaitForSeconds(.02f);
        }

        yield return coroutiner.start(tutorialModule.WaitForValidate(1));

        graph();

        TextMeshProUGUI text = transform.Find("endScreen/winLose").GetComponent<TextMeshProUGUI>();
        text.text = "gg";
        
        transform.Find("endScreen").GetComponent<appear>().Appear(.5f);

        if (saveFile.New.Count < 1)
            transform.Find("endScreen/newPacks").gameObject.SetActive(false);

        yield return new WaitForSeconds(1);

        for (int i = 0; i < saveFile.New.Count; i++)
            Instantiate(meta.ghostPackPrefab, (120f + i * 200, 100f).v(), Quaternion.identity, transform).GetComponent<ghostComponent>().pack = componentManager.allPacks[saveFile.New[i]];

        if (saveFile.New.Count > 0) {
            yield return new WaitUntil(() => metaData.allPacks.Contains(tutorialModule.lastDisplayed));
            yield return coroutiner.start(tutorialModule.WaitForValidate(1));
        }

        transform.Find("endButtons/menuButton").GetComponent<appear>().Appear(.5f); 

        save.reset();
    }

    public static IEnumerator loseScreen() {
        componentManager.updating = true;
        shop.rolling = true;
        componentManager.canGrid = false;

        transform.Find("vars/rent").GetComponentInChildren<RightClick>().Display();
        informationWindow.Override("you lost, but I'm sure you'll do better next time !");

        yield return new WaitForSeconds(1);

        Image fade = transform.Find("fade").GetComponent<Image>();
        fade.gameObject.SetActive(true);
        for (float f = 0; f < .3f; f += .01f) {
            fade.color = new Color(0, 0, 0, f);
            yield return new WaitForSeconds(.02f);
        }

        yield return coroutiner.start(tutorialModule.WaitForValidate(1));

        graph();

        TextMeshProUGUI text = transform.Find("endScreen/winLose").GetComponent<TextMeshProUGUI>();
        text.text = "welp";
        
        transform.Find("endScreen").GetComponent<appear>().Appear(.5f);

        yield return new WaitForSeconds(1);

        transform.Find("endButtons/menuButton").GetComponent<appear>().Appear(.5f);
        transform.Find("endButtons/retryButton").GetComponent<appear>().Appear(.5f);

        save.reset();
    }

    public void BackToMenu() => 
        coroutiner.start(backToMenu());
    public static IEnumerator backToMenu() {
        Image fade = transform.Find("fade").GetComponent<Image>();
        transform.Find("endScreen").GetComponent<appear>().Disappear(.5f);
        for (float f = .3f; f > 0; f -= .01f) {
            fade.color = new Color(0, 0, 0, f);
            yield return new WaitForSeconds(.02f);
        }
        fade.gameObject.SetActive(false);
        
        yield return coroutiner.start(gameOver());

        yield return coroutiner.start(transitionOut());
        SceneManager.LoadScene("menu");
    }

    public void Retry() => 
        coroutiner.start(retry());
    public static IEnumerator retry() {
        Image fade = transform.Find("fade").GetComponent<Image>();
        transform.Find("endScreen").GetComponent<appear>().Disappear(.5f);
        for (float f = .3f; f > 0; f -= .01f) {
            fade.color = new Color(0, 0, 0, f);
            yield return new WaitForSeconds(.02f);
        }
        fade.gameObject.SetActive(false);
        
        yield return coroutiner.start(gameOver());

        yield return coroutiner.start(transitionOut());
        SceneManager.LoadScene(transform.gameObject.scene.name + ((saveFile.tuto2ex && tuto == 2)? "ex" : ""));
    }

    public static IEnumerator transitionIn() {
        transition.gameObject.SetActive(true);
        float width = (transition.canvas.transform as RectTransform).rect.width;

        for (int i = 0; i < 51; i ++)  {
            transition.transform.localPosition = Vector2.Lerp((0f, 0f).v(), (width, 0f).v(), i / 50f);
            yield return new WaitForSeconds(.02f);
        }

        transition.gameObject.SetActive(false);
    }

    public static IEnumerator transitionOut() {
        transition.gameObject.SetActive(true);
        float width = (transition.canvas.transform as RectTransform).rect.width;

        for (int i = 0; i < 51; i ++)  {
            transition.transform.localPosition = Vector2.Lerp((0f - width, 0f).v(), (0f, 0f).v(), i / 50f);
            yield return new WaitForSeconds(.02f);
        }
    }

    private static void graph() {
        RectTransform graph = transform.Find("endScreen/historicCoin") as RectTransform;
        lineGraphic coinLine = graph.Find("coinLine").GetComponent<lineGraphic>();
        lineGraphic rentLine = graph.Find("rentLine").GetComponent<lineGraphic>();

        coinLine.color = metaData.colors["permanentCoins"];
        rentLine.color = var.coinsColor;

        Vector2Int[] c = var.historicCoin.ToArray();
        int maxX =  c.Length;
        int maxY = 0;
        foreach (Vector2Int v in c)
            maxY = Mathf.Max(maxY, v.x, v.y);
        float unitX = graph.rect.width / maxX, unitY = graph.rect.height / maxY;

        for (int i = 0; i < c.Length; i++) {
            coinLine.points.Add((i * unitX, c[i].x * unitY).v());
            rentLine.points.Add((i * unitX, c[i].y * unitY).v());

            if (i % 5 == 0) {
                RectTransform t = Instantiate(meta.graphTextPrefab, graph.Find("graph")).transform as RectTransform;
                t.localPosition = (i * unitX, -15f).v();
                TextMeshProUGUI T = t.GetComponent<TextMeshProUGUI>();
                T.text = $"{i + 1}";

                t = Instantiate(meta.graphTextPrefab, graph.Find("graph")).transform as RectTransform;
                t.localPosition = (-20f, c[i].y * unitY).v();
                T = t.GetComponent<TextMeshProUGUI>();
                T.text = $"{c[i].y}";
                T.color = rentLine.color;

                t = Instantiate(meta.graphTextPrefab, graph.Find("graph")).transform as RectTransform;
                t.localPosition = (-20f, c[i].x * unitY).v();
                T = t.GetComponent<TextMeshProUGUI>();
                T.text = $"{c[i].x}";
                T.color = coinLine.color;
            }
        }
    }
}