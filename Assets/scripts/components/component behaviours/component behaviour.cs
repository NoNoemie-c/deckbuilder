using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class componentBehaviour : ScriptableObject {
    public bool triggerAnim = true;
    public bool isActive = true;
    public bool isTerrain = false;

    public abstract bool activate(Vector2Int pos);
    public abstract bool click(Vector2Int pos);
    public abstract componentBehaviour copy();

    public abstract string encode(int indent);
    public abstract void decode(gameSave.element e);

    public virtual void Anim(component c, string animName) {
        if (triggerAnim)
            c.StartCoroutine(metaData.animations[animName].play(c.transform, componentManager.AnimTime));
    }
}