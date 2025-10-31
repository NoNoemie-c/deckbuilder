using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

[CreateAssetMenu(menuName = "component animation", fileName = "new component animation")]
public class componentAnimation : ScriptableObject
{
    public enum ClockType : int {
        None = 0,
        Normal = 1,
        Complete = 2
    } 
    [Flags] public enum TargetImages : int {
        Nothing = 0,
        Line = 1,
        Button = 2,
        CoinText = 4,
        CrankText = 8,
        StrengthText = 16,
        NameText = 32,
        TerrainImage = 64,
        Image = 128,
        PackBackground = 256,
        TerrainBackground = 512,
        Clock = 1024,
        Texts = CoinText | CrankText | NameText | StrengthText,
        Visible = Texts | Line | Image | TerrainBackground | TerrainImage | PackBackground
    }

    public Sound sound;

    [Space(5)] public AnimationCurve xSizeDistorsion; 
    public float xSizeCoef = 0;
    [Space(5)] public AnimationCurve ySizeDistorsion;
    public float ySizeCoef = 0;
    [Space(5)] public AnimationCurve xPositionDistorsion; 
    public float xPositionCoef = 0;
    [Space(5)] public AnimationCurve yPositionDistorsion; 
    public float yPositionCoef = 0;
    [Space(5)] public AnimationCurve rotationDistorsion; 
    public float rotationCoef = 0;
    [Space(5)] public AnimationCurve RColorDistorsion = AnimationCurve.Constant(0, 1, 0); 
    public float RColorCoef = 1;
    public float GColorCoef = 1;
    public float BColorCoef = 1;
    [Space(5)] public AnimationCurve AColorDistorsion = AnimationCurve.Constant(0, 1, 1);
    public float AColorCoef = 1;

    [Space(10)] public bool reverse;
    public TargetImages targetImages;

    [Space(10)] public ClockType clock = ClockType.None;
    public float currentTime = 1, previousTime = 0;

    public static Dictionary<componentAnimation, Transform> currentTargets = new Dictionary<componentAnimation, Transform>();

    public IEnumerator play(Transform target, float duration) {
        componentAnimation anim = ScriptableObject.CreateInstance<componentAnimation>();

        currentTargets.Add(anim, target);

        anim.xSizeDistorsion = xSizeDistorsion; 
        anim.xSizeCoef = xSizeCoef;
        anim.ySizeDistorsion = ySizeDistorsion;
        anim.ySizeCoef = ySizeCoef;
        anim.xPositionDistorsion = xPositionDistorsion; 
        anim.xPositionCoef = xPositionCoef;
        anim.yPositionDistorsion = yPositionDistorsion; 
        anim.yPositionCoef = yPositionCoef;
        anim.rotationDistorsion = rotationDistorsion; 
        anim.rotationCoef = rotationCoef;
        anim.RColorDistorsion = RColorDistorsion;
        anim.RColorCoef = RColorCoef;
        anim.GColorCoef = GColorCoef;
        anim.BColorCoef = BColorCoef;
        anim.AColorDistorsion = AColorDistorsion;
        anim.AColorCoef = AColorCoef;
        anim.clock = clock;
        anim.currentTime = currentTime;
        anim.previousTime = previousTime;
        anim.reverse = reverse;
        anim.targetImages = targetImages;
        anim.sound = sound;

        yield return coroutiner.start(anim.instance(target, duration));

        if (target != null)
            target.GetComponent<component>().trail = false;

        currentTargets.Remove(anim);
    }

    public IEnumerator instance(Transform target, float duration) {
        component component = target.GetComponent<component>();

        if (component.template == null)
            yield break;

        ClockType clockType = this.clock;

        yield return new WaitForEndOfFrame();

        if (target == null)
            yield break;

        if (sound != null)
            coroutiner.start(sound.play());

        SpriteShapeController shape = null;    
        if (clockType != ClockType.None)
            shape = target.GetComponentInChildren<SpriteShapeController>();

        lineGraphic line = target.GetComponent<lineGraphic>();
        TextMeshProUGUI name, coins, cranks, strength;
        SpriteShapeRenderer clock = null;
        Image button, terrain = null, packBackground, terrainBackground = null, image, coinsImg, cranksImg, strengthImg;
        Color baseLineColor = Color.white,
              baseNameColor = Color.black, 
              baseCoinsColor = Color.black, 
              baseCranksColor = Color.black, 
              baseStrengthColor = Color.black,
              baseButtonColor = Color.white, 
              baseTerrainColor = Color.white, 
              basePackBackgroundColor = Color.white, 
              baseTerrainBackgroundColor = Color.white, 
              baseImageColor = Color.white, 
              baseClockColor = Color.white;
        TextMeshProUGUI[] texts = target.GetComponentsInChildren<TextMeshProUGUI>(true);
        baseNameColor = (name = Array.Find(texts, t => t.name == "name")).color;
        baseCoinsColor = (coins = Array.Find(texts, t => t.name == "coins")).color;
        baseCranksColor = (cranks = Array.Find(texts, t => t.name == "crank")).color;
        baseStrengthColor = (strength = Array.Find(texts, t => t.name == "strength")).color;
        Image[] imgs = target.GetComponentsInChildren<Image>(true);
        basePackBackgroundColor = (packBackground = Array.Find(imgs, t => t.name == "pack")).color;
        baseImageColor = (image = Array.Find(imgs, t => t.name == "description")).color;
        baseButtonColor = (button = Array.Find(imgs, t => t.name == "square")).color;
        coinsImg = Array.Find(imgs, t => t.name == "coinImg");
        cranksImg = Array.Find(imgs, t => t.name == "crankImg");
        strengthImg = Array.Find(imgs, t => t.name == "strengthImg");
        if (line != null) {
            baseClockColor = (clock = target.GetComponentInChildren<SpriteShapeRenderer>(true)).color;
            baseTerrainColor = (terrain = Array.Find(imgs, t => t.name == "terrain")).color;
            baseTerrainBackgroundColor = (terrainBackground = Array.Find(imgs, t => t.name == "terrainBackground")).color;
            if (targetImages.HasFlag(TargetImages.Button))
                button.GetComponent<Button>().enabled = false;
            baseLineColor = line.color;
        }

        Vector3 pos = target.localPosition;
        Quaternion rotation = target.localRotation;
        Vector3 size = target.localScale;
        RectMask2D mask = target.GetComponent<RectMask2D>();

        for (float i = 0; i < 50 + 1; i ++) {
            if (target == null) yield break;

            float f = i / 50;

            target.localPosition += new Vector3(xPositionDistorsion.growth(f) * xPositionCoef, yPositionDistorsion.growth(f) * yPositionCoef, 0);
            target.localScale += new Vector3(xSizeDistorsion.growth(f) * xSizeCoef, ySizeDistorsion.growth(f) * ySizeCoef, 0);
            target.Rotate(0, 0, rotationDistorsion.growth(f) * rotationCoef, Space.Self);
            if (mask != null && rotationCoef != 0)
                mask.padding = Vector4.one * -200;

            if (clockType != ClockType.None)
                shape.squareClock(Mathf.Lerp(previousTime, currentTime, f));

            Color c = new Color(RColorCoef, GColorCoef, BColorCoef, AColorDistorsion.Evaluate(f) * AColorCoef);

            if (line != null) {
                if (targetImages.HasFlag(TargetImages.Line))
                    line.color = Color.Lerp(line.color, c, RColorDistorsion.Evaluate(f)); 

                if (targetImages.HasFlag(TargetImages.Clock))
                    clock.color = Color.Lerp(clock.color, c, RColorDistorsion.Evaluate(f));

                if (targetImages.HasFlag(TargetImages.TerrainImage))
                    terrain.color = Color.Lerp(terrain.color, c, RColorDistorsion.Evaluate(f));
                if (targetImages.HasFlag(TargetImages.TerrainBackground))
                    terrainBackground.color = Color.Lerp(terrainBackground.color, c, RColorDistorsion.Evaluate(f));
            }
            
            if (targetImages.HasFlag(TargetImages.NameText))
                name.color = Color.Lerp(name.color, c, RColorDistorsion.Evaluate(f));
            if (targetImages.HasFlag(TargetImages.CoinText)) {
                coins.color = Color.Lerp(coins.color, c, RColorDistorsion.Evaluate(f));
                coinsImg.color = Color.Lerp(coinsImg.color, c, RColorDistorsion.Evaluate(f));
            } if (targetImages.HasFlag(TargetImages.CrankText)) {
                cranks.color = Color.Lerp(cranks.color, c, RColorDistorsion.Evaluate(f));
                cranksImg.color = Color.Lerp(cranksImg.color, c, RColorDistorsion.Evaluate(f));
            } if (targetImages.HasFlag(TargetImages.StrengthText)) {
                strength.color = Color.Lerp(strength.color, c, RColorDistorsion.Evaluate(f));
                strengthImg.color = Color.Lerp(strengthImg.color, c, RColorDistorsion.Evaluate(f));
            }

            if (targetImages.HasFlag(TargetImages.PackBackground))
                packBackground.color = Color.Lerp(packBackground.color, c, RColorDistorsion.Evaluate(f));
            if (targetImages.HasFlag(TargetImages.Image))
                image.color = Color.Lerp(image.color, c, RColorDistorsion.Evaluate(f));
            if (targetImages.HasFlag(TargetImages.Button))
                button.color = Color.Lerp(button.color, c, RColorDistorsion.Evaluate(f));

            yield return new WaitForSeconds(duration / 50);
        }

        if (reverse) {
            for (float i = 50; i > -1; i --) {
                if (target == null) yield break;

                float f = i / 50;

                target.localPosition += new Vector3(xPositionDistorsion.growth(f, -.02f) * xPositionCoef, yPositionDistorsion.growth(f, -.02f) * yPositionCoef, 0);
                target.localScale += new Vector3(xSizeDistorsion.growth(f, -.02f) * xSizeCoef, ySizeDistorsion.growth(f, -.02f) * ySizeCoef, 0);
                target.Rotate(0, 0, rotationDistorsion.growth(f, -.02f) * rotationCoef, Space.Self);
                if (mask != null && rotationCoef != 0)
                    mask.padding = Vector4.one * -200;

                if (clockType == ClockType.Complete)
                    shape.squareClock(1 - Mathf.Lerp(1, 0, f));

                Color c = new Color(RColorCoef, GColorCoef, BColorCoef, AColorDistorsion.Evaluate(f) * AColorCoef);

                if (line != null) {
                    if (targetImages.HasFlag(TargetImages.Line))
                        line.color = Color.Lerp(component.baseLineColor, c, RColorDistorsion.Evaluate(f)); 

                    if (targetImages.HasFlag(TargetImages.Clock))
                        clock.color = Color.Lerp(component.baseClockColor, c, RColorDistorsion.Evaluate(f));

                    if (targetImages.HasFlag(TargetImages.TerrainImage))
                        terrain.color = Color.Lerp(component.baseTerrainColor, c, RColorDistorsion.Evaluate(f));
                    if (targetImages.HasFlag(TargetImages.TerrainBackground))
                        terrainBackground.color = Color.Lerp(component.baseTerrainBackgroundColor, c, RColorDistorsion.Evaluate(f));
                }
                
                if (targetImages.HasFlag(TargetImages.NameText))
                    name.color = Color.Lerp(component.baseNameColor, c, RColorDistorsion.Evaluate(f));
                if (targetImages.HasFlag(TargetImages.CoinText)) {
                    coins.color = Color.Lerp(component.baseCoinsColor, c, RColorDistorsion.Evaluate(f));
                    coinsImg.color = Color.Lerp(component.baseCoinImgColor, c, RColorDistorsion.Evaluate(f));
                } if (targetImages.HasFlag(TargetImages.CrankText)) {
                    cranks.color = Color.Lerp(component.baseCranksColor, c, RColorDistorsion.Evaluate(f));
                    cranksImg.color = Color.Lerp(component.baseCrankImgColor, c, RColorDistorsion.Evaluate(f));
                } if (targetImages.HasFlag(TargetImages.StrengthText)) {
                    strength.color = Color.Lerp(component.baseStrengthColor, c, RColorDistorsion.Evaluate(f));
                    strengthImg.color = Color.Lerp(component.baseStrengthImgColor, c, RColorDistorsion.Evaluate(f));
                }

                if (targetImages.HasFlag(TargetImages.PackBackground))
                    packBackground.color = Color.Lerp(component.basePackBackgroundColor, c, RColorDistorsion.Evaluate(f));
                if (targetImages.HasFlag(TargetImages.Image))
                    image.color = Color.Lerp(component.baseImageColor, c, RColorDistorsion.Evaluate(f));
                if (targetImages.HasFlag(TargetImages.Button))
                    button.color = Color.Lerp(Color.clear, c, RColorDistorsion.Evaluate(f));

                yield return new WaitForSeconds(duration / 50);
            }

            if (mask != null && rotationCoef != 0)
                mask.padding = Vector4.zero;
        }

        if (targetImages.HasFlag(TargetImages.Button)) {
            button.GetComponent<Button>().enabled = true;
            button.color = component.baseButtonColor;
        }
    }   

    private void draw(float i) {

    }
}
