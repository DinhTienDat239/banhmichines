using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeBook : MonoBehaviour
{
    [SerializeField]
    List<string> recipeNameList;
    [SerializeField]
    List<string> recipeDescriptionList;
    [SerializeField]
    TextMeshProUGUI recipeNameTxt;
    [SerializeField]
    TextMeshProUGUI recipeDescriptionTxt;
    int currentPage = 0;
    void Awake(){
        Init();
    }
    public void Init(){
        currentPage = 0;
        recipeNameTxt.text = recipeNameList[currentPage];
        recipeDescriptionTxt.text = recipeDescriptionList[currentPage];
    }
    public void NextPage(){
        if (recipeNameList == null || recipeDescriptionList == null || recipeNameList.Count == 0)
        {
            return;
        }

        int maxPage = recipeNameList.Count - 1;
        currentPage = currentPage < maxPage ? currentPage + 1 : 0;
        InitPageContent();
    }
    public void PreviousPage(){
        if (recipeNameList == null || recipeDescriptionList == null || recipeNameList.Count == 0)
        {
            return;
        }

        int maxPage = recipeNameList.Count - 1;
        currentPage = currentPage > 0 ? currentPage - 1 : maxPage;
        InitPageContent();
    }

    private void InitPageContent()
    {
        int safeIndex = Mathf.Clamp(currentPage, 0, recipeNameList.Count - 1);
        currentPage = safeIndex;

        if (recipeNameTxt != null)
        {
            recipeNameTxt.text = recipeNameList[safeIndex];
        }

        if (recipeDescriptionTxt != null && recipeDescriptionList != null && recipeDescriptionList.Count > safeIndex)
        {
            recipeDescriptionTxt.text = recipeDescriptionList[safeIndex];
        }
    }
}
