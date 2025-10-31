using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class spriteMerger
{
    public static Sprite merge(Sprite[] spritesToMerge, Vector2Int[] positions, Vector2[] scales, float globalScale = 0) {
        Sprite output;
        Texture2D newTex = new Texture2D(512, 512);

        if (scales == null) {
            scales = new Vector2[spritesToMerge.Length];
            for (int i = 0; i < spritesToMerge.Length; i++)
                scales[i] = (globalScale, globalScale).v();
        }

        for (int x = 0; x < newTex.width; x++)
            for (int y = 0; y < newTex.height; y++)
                newTex.SetPixel(x, y, Color.clear);
        
        for (int i = 0; i < spritesToMerge.Length; i++)
            for (int x = 0; x < newTex.width; x++)
                for (int y = 0; y < newTex.height; y++)
                    if (spritesToMerge[i].texture.GetPixel(x, y).a != 0)
                        newTex.SetPixel(Mathf.RoundToInt(Mathf.Clamp(x * scales[i].x + positions[i].x, 0, newTex.width)), 
                                        Mathf.RoundToInt(Mathf.Clamp(y * scales[i].y + positions[i].y, 0, newTex.height)), 
                                        spritesToMerge[i].texture.GetPixel(x, y));

        newTex.Apply();
        output = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(.5f, .5f));
        output.name = "new Sprite";

        return output;
    }
}
