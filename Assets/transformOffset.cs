using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

public class transformOffset : MonoBehaviour
{
    public Dictionary<componentAnimation, Vector2> pos;
    public Vector2 globalPos;
    public Vector2 basePos;

    public Dictionary<componentAnimation, Vector2> size;
    public Vector2 globalSize;
    public Vector2 baseSize;

    public Dictionary<componentAnimation, float> rotation;
    public float globalRotation;
    public float baseRotation;

    public Dictionary<componentAnimation, Color> LineColor,
                                                NameColor, 
                                                CoinsColor, 
                                                CranksColor, 
                                                ButtonColor, 
                                                TerrainColor, 
                                                PackBackgroundColor, 
                                                TerrainBackgroundColor, 
                                                ImageColor, 
                                                ClockColor;

    public Color baseLineColor,
                baseNameColor, 
                baseCoinsColor, 
                baseCranksColor, 
                baseButtonColor, 
                baseTerrainColor, 
                basePackBackgroundColor, 
                baseTerrainBackgroundColor, 
                baseImageColor, 
                baseClockColor;

    
    public void Add(componentAnimation anim) {


        updateGlobals();
    }

    public void Remove(componentAnimation anim) {
        

        updateGlobals();
    }

    void Update() {
        if (componentAnimation.currentTargets.ContainsValue(transform)) 
            Apply();
    }

    public void Apply() {
        
    }

    public void updateGlobals() {

    }
}
