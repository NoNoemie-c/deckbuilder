using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class menuManager : MonoBehaviour
{
    public Button continueGameButton, newGameButton, storyButton, tutorialsButton, generateButton;
    public List<Button> tutorialButtons, storyButtons;
    private static new RectTransform transform;
    private RectMask2D mask;
    public List<Toggle> packs;
    public Dictionary<string, Slider> sliders;
    public Transform options;
    public Image transition;
    public Vector2 p;

    void Start() {
        saveFile.Awake();

        transform = GetComponent<RectTransform>();

        mask = transform.parent.GetComponentInChildren<RectMask2D>();

        clicker.transition = transition;

        options = transform.parent.Find("game");
        p = options.position;

        Button[] buttons = transform.parent.GetComponentsInChildren<Button>(true);

        tutorialButtons = new List<Button>();
        for (int i = 1; i < 4; i ++)
            tutorialButtons.Add(Array.Find(buttons, b => b.name == $"tutorial{i}"));

        storyButtons = new List<Button>();
        for (int i = 4; i < saveFile.instance.rents.Count + 1; i ++)
            storyButtons.Add(Array.Find(buttons, b => b.name == $"story{i}"));

        continueGameButton = Array.Find(buttons, b => b.name == "continue");
        newGameButton = Array.Find(buttons, b => b.name == "new game");
        tutorialsButton = Array.Find(buttons, b => b.name == "tutorials");
        storyButton = Array.Find(buttons, b => b.name == "story");
        generateButton = Array.Find(buttons, b => b.name == "play");

        Slider[] Sliders = transform.parent.GetComponentsInChildren<Slider>(true);
        sliders = new Dictionary<string, Slider>();
        foreach (Slider slider in Sliders)
            sliders.Add(slider.gameObject.name, slider);

        Toggle[] toggles = transform.parent.GetComponentsInChildren<Toggle>(true);
        packs = new List<Toggle>(toggles);

        newGameButton.interactable = false;

        saveFile.save(saveFile.Progress);

        switch (saveFile.Progress) {
            case 0 :
                clicker.transition.transform.localPosition = (0f, 0f).v();
                clicker.transition.gameObject.SetActive(true);

                SceneManager.LoadScene("tutorial 1");
            break;

            case 1 :
                storyButton.interactable = false;
                continueGameButton.interactable = false;

                tutorialButtons[3].interactable = false;
            break;

            case 2 :
                storyButton.interactable = false;
                continueGameButton.interactable = false;
            break;

            default :
                newGameButton.interactable = true;
                for (int i = saveFile.Progress + 2; i < saveFile.instance.rents.Count + 1; i ++)
                    storyButtons[i - 3 -1].interactable = false;
                for (int i = saveFile.Progress -1; i < saveFile.instance.rents.Count; i++)
                    packs[i].interactable = false;
 
                if (saveFile.Progress < saveFile.instance.terrainThreshold)
                    sliders["terrainAmount"].interactable = false;
            break;
        }

        Cursor.SetCursor(Resources.Load<Texture2D>("images/cursor"), (0, 0).v(), CursorMode.Auto);

        if (Resources.Load<gameSave>("save file/game").data == "")
            continueGameButton.interactable = false;
    }

    void Update() {
        Camera.main.orthographicSize = 0.281f * Screen.width;   
    }

    public void hovered(string name) {
        string s = "";

        switch (name) {
            case "new" : s = "play a custom game"; break;

            case "continue" : s = "continue the last game"; break;

            case "story" : s = "story mode (but there's no story lmao)\nto unlock new packs"; break;

            case "tutorials" : s = "tutorials"; break;
            case "tutorial1" : s = "tutorial 1 - basics"; break;
            case "tutorial2" : s = "tutorial 2 - shop and components"; break;
            case "tutorial3" : s = "tutorial 3 - electric and mechanic packs"; break;

            case "story4" : s = "null pack"; break;
            case "story5" : s = "stone pack"; break;
            case "story6" : s = "math pack"; break;
            case "story7" : s = "casino pack"; break;
            case "story8" : s = "fruits pack"; break;
            case "story9" : s = "animal pack"; break;
            case "story10" : s = "industrial pack"; break;
            case "story11" : s = "time pack"; break;
            case "story12" : s = "component pack"; break;
        }

        GetComponentInChildren<TextMeshProUGUI>().text = s;
    }
    
    public void game() {
        for (int i = 0; i < tutorialButtons.Count; i ++)
            StartCoroutine(disappear(tutorialButtons[i].transform));

        for (int i = 0; i < storyButtons.Count; i ++)
            StartCoroutine(disappear(storyButtons[i].transform));

        StartCoroutine(appearAt(options, newGameButton.transform.position));
        StartCoroutine(tweenTo(options, p));
    }
    public void story () {
        mask.padding = Vector4.zero;

        for (int i = 0; i < tutorialButtons.Count; i ++)
            StartCoroutine(disappear(tutorialButtons[i].transform));

        StartCoroutine(disappear(options));

        for (int i = 0; i < storyButtons.Count; i ++) {
            StartCoroutine(appearAt(storyButtons[i].transform, storyButton.transform.position));
            tweenTo(storyButtons[i].transform, (1 + i % 4, 3 - Mathf.FloorToInt(i / 4f)).v());
        }
    }

    public void tutorials() {
        mask.padding = Vector4.zero;

        StartCoroutine(disappear(options));

        for (int i = 0; i < storyButtons.Count; i ++)
            StartCoroutine(disappear(storyButtons[i].transform));

        for (int i = 0; i < tutorialButtons.Count; i ++) {
            StartCoroutine(appearAt(tutorialButtons[i].transform, tutorialsButton.transform.position));
            tweenTo(tutorialButtons[i].transform, (1 + i, 2).v());
        }
    }

    public void continueGame() =>
        StartCoroutine(ContinueGame());
    public IEnumerator ContinueGame() {
        yield return coroutiner.start(clicker.transitionOut());
        SceneManager.LoadScene("game");
    }

    public void newGame() =>
        StartCoroutine(NewGame());
    public IEnumerator NewGame() {
        gameSave save;
        if ((save = Resources.Load<gameSave>("save file/game")) != null)
            save.reset();

        List<string> Packs = new List<string>();
        for (int i = 0; i < packs.Count; i++)
            if (packs[i].isOn)
                Packs.Add(packs[i].name.Replace("Pack", ""));

        int rentLengthMin = Convert.ToInt32(sliders["rentLengthMin"].value), rentLengthMax;
        if (rentLengthMin > Convert.ToInt32(sliders["rentLengthMax"].value)) {
            rentLengthMin = Convert.ToInt32(sliders["rentLengthMax"].value);
            rentLengthMax = Convert.ToInt32(sliders["rentLengthMin"].value);
        } else 
            rentLengthMax = Convert.ToInt32(sliders["rentLengthMax"].value);

        clicker.d = saveFile.generate(saveFile.instance.rents.Count, 
            Convert.ToInt32(sliders["difficulty"].value),
            Convert.ToInt32(sliders["length"].value),
            rentLengthMin,
            rentLengthMax,
            (float) Convert.ToDouble(sliders["destroyQuantity"].value),
            Convert.ToInt32(sliders["destroysAmount"].value),
            Convert.ToInt32(sliders["terrainAmount"].value),
            Packs);

        clicker.currentProgress = 12;

        yield return coroutiner.start(clicker.transitionOut());
        SceneManager.LoadScene("game");
    }

    public void Tutorial(int tutorialId) => 
        StartCoroutine(tutorial(tutorialId));
    public IEnumerator tutorial(int tutorialId) {
        yield return coroutiner.start(clicker.transitionOut());

        if (tutorialId == 2 && saveFile.tuto2ex)
            SceneManager.LoadScene($"tutorial 2ex");

        SceneManager.LoadScene($"tutorial {tutorialId}");
    }

    public void StoryStart(int storyId) =>
        StartCoroutine(storyStart(storyId));
    public IEnumerator storyStart(int storyId) {
        gameSave save;
        if ((save = Resources.Load<gameSave>("save file/game")) != null)
            save.reset();

        clicker.d = new saveFile.data();
            
        clicker.currentProgress = storyId - 1;

        yield return coroutiner.start(clicker.transitionOut());

        SceneManager.LoadScene("game");
    }

    public static IEnumerator tweenTo(Transform t, Vector2 pos, float duration = .5f) {
        Vector2 start = t.position;

        for (int i = 0; i < 50; i ++) {
            t.position = Vector2.Lerp(start, pos, i / (duration * 50));
            yield return new WaitForSeconds(duration / 50);
        }
    }
    public static void tweenTo(Transform t, Vector2Int pos, float duration = .5f) =>
        coroutiner.start(tweenTo(t, GridToWorld(pos), duration));

    public static IEnumerator appearAt(Transform t, Vector2 pos, float duration = .5f) {
        if (t.gameObject.activeSelf)
            yield break;

        t.position = pos;
        float size = t.localScale.x;
        t.gameObject.SetActive(true);

        for (int i = 0; i < 50; i ++) {
            t.localScale = Vector2.Lerp((0f, 0f).v(), (size, size).v(), i / (duration * 50));
            yield return new WaitForSeconds(duration / 50);
        }

        t.localScale = (size, size).v();
    }
    public static void appearAt(Transform t, Vector2Int pos, float duration = .5f) =>
        coroutiner.start(appearAt(t, GridToWorld(pos), duration));

    public static IEnumerator disappear(Transform t, float duration = .5f) {
        if (!t.gameObject.activeSelf)
            yield break;

        float size = t.localScale.x;
        
        for (int i = 0; i < 50; i ++) {
            t.localScale = Vector2.Lerp((size, size).v(), (0f, 0f).v(), i / (duration * 50));
            yield return new WaitForSeconds(duration / 50);
        }
        
        t.localScale = (size, size).v();
        t.gameObject.SetActive(false);
    }

    public static Vector2 GridToWorld(Vector2Int v) {
        Vector2 vec = v;
        vec = vec * 133.33f - new Vector2((transform.rect.size / 2).x, (transform.rect.size / 2).y) + (133.33f, 133.33f).v() / 2;
        return transform.TransformPoint(vec);
    }
}
