using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class metaData : ScriptableObject {
    public static metaData This;

    public List<string> AnimationsName = new List<string>(); 
    public List<componentAnimation> Animations = new List<componentAnimation>();
    public static Dictionary<string, componentAnimation> animations;

    public List<string> SoundsName = new List<string>(); 
    public List<Sound> Sounds = new List<Sound>();
    public static Dictionary<string, Sound> sounds;

    public List<string> ColorsName = new List<string>(); 
    public List<Color> Colors = new List<Color>();
    public static Dictionary<string, Color> colors;

    public static List<string> allPacks;
    public List<string> AllPacks;

    public static Color rerollColor, skipColor;
    public Color RerollColor, SkipColor;

    public string[] rarityNames;
    public Color[] rarityColors;
    public static string[] RarityNames;
    public static Color[] RarityColors;

    public static int maxRarity;
    [SerializeField] private int MaxRarity = 6;

    public static void Awake() {
        This = Resources.Load<metaData>("data");
        This.awake();

        saveFile.instance = Resources.Load<saveFile>("save file/save");

        saveFile.Awake();
    }

    public void awake() {
        colors = new Dictionary<string, Color>();
        for (int i = 0; i < Colors.Count; i ++)
            colors.Add(ColorsName[i], Colors[i]);

        animations = new Dictionary<string, componentAnimation>();
        for (int i = 0; i < Animations.Count; i ++)
            animations.Add(AnimationsName[i], Animations[i]);

        sounds = new Dictionary<string, Sound>();
        for (int i = 0; i < Sounds.Count; i ++)
            sounds.Add(SoundsName[i], Sounds[i]);

        allPacks = AllPacks;

        RarityColors = rarityColors;
        RarityNames = rarityNames;

        maxRarity = MaxRarity;
    }
}