using System.Collections;
using DAT.Core.DesignPatterns;
using UnityEngine;

public class VFXManager : Singleton<VFXManager>
{
    [SerializeField]
    float timeTillDestroy = 3f;
    [SerializeField]
    public GameObject upgradeFX;
    public void SpawnVFX(GameObject vfx, Vector3 spawnPos){
        GameObject obj = Instantiate(vfx);
        obj.transform.position = spawnPos;
        obj.SetActive(true);
    }
}
