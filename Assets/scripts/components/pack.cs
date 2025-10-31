using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "new pack", menuName = "pack")]
public class pack : ScriptableObject {
    public Color color;
    public Sprite image;
    [TextArea] public string description;

    public componenttemplate[] getComponents() {
        List<componenttemplate> removes = new List<componenttemplate>(), components = new List<componenttemplate>(Resources.LoadAll<componenttemplate>($"components/{name} pack"));

        foreach (componenttemplate component in components) {
            component.pack.RemoveAll(s => s == "");
            if (component.pack.Count == 0 || component.pack[0] != "all")
                component.pack.AddSingle(new List<string>{name});
            component.Pack();

            if (!randCompSpawner.active && !clicker.save.Current.ContainsRange(component.pack))
                removes.Add(component);
        }

        foreach (componenttemplate component in removes)
            components.Remove(component);

        return components.ToArray();
    }

    public terrain[] getTerrains() {
        List<terrain> removes = new List<terrain>(), components = new List<terrain>(Resources.LoadAll<terrain>($"components/{name} pack"));

        foreach (terrain component in components) {
            component.pack.RemoveAll(s => s == "");
            if (component.pack.Count == 0 || component.pack[0] != "all")
                component.pack.AddSingle(new List<string>{name});
            component.Pack();

            if (!randCompSpawner.active && !clicker.save.Current.ContainsRange(component.pack))
                removes.Add(component);
        }

        foreach (terrain component in removes)
            components.Remove(component);

        return components.ToArray();
    }
}
