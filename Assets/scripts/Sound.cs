using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName="new sound", menuName="Sound")]
public class Sound : ScriptableObject
{
    public float startAt, volume = 1;
    public AudioClip clip;
    public enum SoundType : int {
        anim, instant, startAnim
    }
    public SoundType type;

    public enum playMode : int {
        normal, Override, both
    }
    public playMode PlayMode;

    [NonSerialized] public AudioSource source;

    public IEnumerator play() {
        if (type != SoundType.instant && PlayMode == playMode.normal && clicker.sounds.FindAll(x => x.type == type).Count != 0)
            yield break;

        List<Sound> toDestroy = new List<Sound>();
        if (PlayMode == playMode.Override)
            foreach (Sound s in clicker.sounds)
                if (s.type == type) {
                    s.stop();
                    toDestroy.Add(s);
                }

        foreach (Sound s in toDestroy)
            clicker.sounds.Remove(s);

        clicker.sounds.Add(this);

        yield return new WaitForSeconds(startAt);

        float time = 1;
        if (type != SoundType.instant)
           time = componentManager.AnimTime;

        source = Instantiate(meta.soundPrefab, clicker.audioSource[type]).GetComponent<AudioSource>();

        //clip.length = clip.length * time;

        source.clip = clip;
        source.volume = volume;
        source.Play();

        yield return new WaitUntil(() => source.isPlaying);
        yield return new WaitUntil(() => source == null || !source.isPlaying);

        stop();
        
        clicker.sounds.RemoveAll(s => s == this);
    }

    public void stop() {
        coroutiner.end(play());

        if (source != null)
            Destroy(source.gameObject);
    }
}
