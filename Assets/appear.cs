using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class appear : MonoBehaviour
{
    public bool affectChildren = true;
    public static AnimationCurve flashCurve;

    public void Appear(float time) =>
        coroutiner.start(_change(time * 50, true));

    public void AppearFlash(float time) =>
        coroutiner.start(_changeF(time * 50, true));

    public void Disappear(float time) =>
        coroutiner.start(_change(time * 50, false));

    private IEnumerator _change(float frames, bool appear) {
        if (appear) {
            if (affectChildren)
                gameObject.SetActive(true);
            else
                gameObject.SetActiveSelf(true);  
        }

        for (int i = 0; i <= frames; i ++) {
            yield return new WaitForSeconds(.02f);
            
            float a = appear ? Mathf.Lerp(0, 1, i / frames) : Mathf.Lerp(1, 0, i / frames);
            if (affectChildren) {
                changeColors(GetComponentsInChildren<Graphic>(), a);
                foreach (LineRenderer l in GetComponentsInChildren<LineRenderer>())
                    l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, a);
            } else {
                changeColors(GetComponent<Graphic>(), a);
                LineRenderer l = GetComponent<LineRenderer>();
                if (l != null)
                    l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, a);
            }
        }

        if (!appear) {
            if (affectChildren)
                gameObject.SetActive(false);
            else
                gameObject.SetActiveSelf(false);  
        }
    }

    private IEnumerator _changeF(float frames, bool appear) {
        if (appear) {
            if (affectChildren)
                gameObject.SetActive(true);
            else
                gameObject.SetActiveSelf(true);  
        }

        for (int i = 0; i <= frames; i ++) {
            yield return new WaitForSeconds(.02f);
            
            float a = appear ? flashCurve.Evaluate(i / frames) : flashCurve.Evaluate(1 / i / frames);
            if (affectChildren) {
                changeColors(GetComponentsInChildren<Graphic>(), a);
                foreach (LineRenderer l in GetComponentsInChildren<LineRenderer>())
                    l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, a);
            } else {
                changeColors(GetComponent<Graphic>(), a);
                LineRenderer l = GetComponent<LineRenderer>();
                if (l != null)
                    l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, a);
            }
        }

        if (!appear) {
            if (affectChildren)
                gameObject.SetActive(false);
            else
                gameObject.SetActiveSelf(false);  
        }
    }

    private void changeColors(Graphic[] l, float a) {
        foreach (Graphic g in l) {
            float f = 1;
            switch (g.gameObject.name) {
                case "image" :
                    f = .298f;
                break;
                    
                case "checker" :
                    f = .3f;
                break;
            }
            g.color = new Color(g.color.r, g.color.g, g.color.b, f * a);
        }
    }
    private void changeColors(Graphic g, float a) {
        if (g == null)
            return;

        float f = 1;
        switch (g.gameObject.name) {
            case "image" :
                f = .298f;
            break;
                
            case "checker" :
                f = .4f;
            break;
        }
        g.color = new Color(g.color.r, g.color.g, g.color.b, f * a);
    }

    public void InstantAppear() {
        if (affectChildren)
            gameObject.SetActive(true);
        else
            gameObject.SetActiveSelf(true);  

        if (affectChildren) {
            changeColors(GetComponentsInChildren<Graphic>(), 1);
            foreach (LineRenderer l in GetComponentsInChildren<LineRenderer>())
                l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, 1);
        } else {
            changeColors(GetComponent<Graphic>(), 1);
            LineRenderer l = GetComponent<LineRenderer>();
            if (l != null)
                l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, 1);
        }
    }

    public void InstantDisappear() {
        if (affectChildren) {
            changeColors(GetComponentsInChildren<Graphic>(), 0);
            foreach (LineRenderer l in GetComponentsInChildren<LineRenderer>())
                l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, 0);
        } else {
            changeColors(GetComponents<Graphic>(), 0);
            LineRenderer l = GetComponent<LineRenderer>();
            if (l != null)
                l.startColor = l.endColor = new Color(l.startColor.r, l.startColor.g, l.startColor.b, 0);
        }

        if (affectChildren)
            gameObject.SetActive(false);
        else
            gameObject.SetActiveSelf(false);  
    }
}
