using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DAT.Core.DesignPatterns;

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("Tutorial Objects")]
    [SerializeField] List<GameObject> tutorialObjects;
    [Header("Tutorial Settings")]
    [SerializeField] bool isTutorialEnabled = false;
    [SerializeField] bool isHighlightObjects = false;
    [SerializeField] public List<MeshRenderer> highlightObjects;
    [SerializeField] Material highlightMaterial;
    [Header("Tutorial variables")]
    public bool isTutorialActive = false;
    public float highlightIntensityMin = 0f;
    public float highlightIntensityMax = 1f;
    public int currentHighlightIndex = 0;
    bool isValueUpHighlightIntensity = false;
    bool isValueDownHighlightIntensity = false;

    void Start(){
        if(isTutorialEnabled){
            ShowTutorial();
        }
        if(isHighlightObjects && highlightObjects.Count > 0){
            HighlightObject(currentHighlightIndex);
            RunHighlightIntensity();
        }
    }
    #region Tutorial Functions
    public void ShowTutorial()
    {
        foreach(var obj in tutorialObjects){
            obj.SetActive(true);
        }
        isTutorialActive = true;
    }

    public void HideTutorial()
    {
        foreach(var obj in tutorialObjects){
            obj.SetActive(false);
        }
        isTutorialActive = false;
    }
    #endregion

    #region Highlight Functions
    public void HighlightObject(int index){
        Material[] currentMaterials = highlightObjects[index].materials;
        Material[] newMaterials = new Material[currentMaterials.Length + 1];
        for(int i = 0; i < currentMaterials.Length; i++){
            newMaterials[i] = currentMaterials[i];
        }
        newMaterials[currentMaterials.Length] = highlightMaterial;
        highlightObjects[index].materials = newMaterials;
    }
    public void UnhighlightObject(int index){
        Material[] currentMaterials = highlightObjects[index].materials;
        Material[] newMaterials = new Material[currentMaterials.Length - 1];
        for(int i = 0; i < currentMaterials.Length - 1; i++){
            newMaterials[i] = currentMaterials[i];
        }
        highlightObjects[index].materials = newMaterials;
    }
    public void HighlightNextObject(){
        if(currentHighlightIndex >= highlightObjects.Count){
            return;
        }
        UnhighlightObject(currentHighlightIndex);
        currentHighlightIndex++;
        HighlightObject(currentHighlightIndex);
    }
    public void HighlightPreviousObject(){
        if(currentHighlightIndex <= 0){
            return;
        }
        UnhighlightObject(currentHighlightIndex);
        currentHighlightIndex--;
        HighlightObject(currentHighlightIndex);
    }
    #endregion
    #region Highlight Intensity Functions
    public void RunHighlightIntensity(){
        highlightMaterial.SetFloat("_Intensity", highlightIntensityMin);
        isValueUpHighlightIntensity = true;
        StartCoroutine(HighlightIntensityCoroutine());

    }
    public IEnumerator HighlightIntensityCoroutine(){
        if(isValueUpHighlightIntensity){
            highlightMaterial.SetFloat("_HighlightIntensity", highlightMaterial.GetFloat("_HighlightIntensity") + 0.01f);
            yield return new WaitForSeconds(0.01f);
            if(highlightMaterial.GetFloat("_HighlightIntensity") >= highlightIntensityMax){
                isValueUpHighlightIntensity = false;
                isValueDownHighlightIntensity = true;
            }
        }
        else if(isValueDownHighlightIntensity){
            highlightMaterial.SetFloat("_HighlightIntensity", highlightMaterial.GetFloat("_HighlightIntensity") - 0.01f);
            yield return new WaitForSeconds(0.01f);
            if(highlightMaterial.GetFloat("_HighlightIntensity") <= highlightIntensityMin){
                isValueDownHighlightIntensity = false;
                isValueUpHighlightIntensity = true;
            }
        }
        StartCoroutine(HighlightIntensityCoroutine());
    }
    #endregion
}
