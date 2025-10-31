using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textEffect : MonoBehaviour
{
    private TMP_Text text;
    public float wobbleCoeff;

    void Start() {
        text = GetComponent<TMP_Text>();
    }

    void Update() {
        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;
        
        // effects
        for (int i = 0; i < textInfo.characterCount; i ++) {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for (int j = 0; j < 4; j++) {
                vertices[charInfo.vertexIndex + j] += new Vector2(0, Mathf.Sin(Time.time * 4 + vertices[charInfo.vertexIndex + j].x * .01f) * wobbleCoeff).V3();
            }
        }

        // update the mesh
        for (int i = 0; i < textInfo.meshInfo.Length; i ++) {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
