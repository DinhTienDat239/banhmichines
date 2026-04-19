using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAndFXExample : MonoBehaviour
{
    [SerializeField]
    GameObject section1;
    [SerializeField]
    GameObject section2;

    private int _currentSection = 0;

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    public void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeSection(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeSection(2);
        }
    }
    public void ChangeSection(int section)
    {
        _currentSection = section;
        switch(section)
        {
            case 1:
                section1.SetActive(true);
                section2.SetActive(false); break;
            case 2:
                section1.SetActive(false);
                section2.SetActive(true); break;

        }
    }
}
