using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public GameObject prefab;

    List<GameObject> pool;

    int counter = 0;

    // Use this for initialization
    void Awake()
    {
        pool = new List<GameObject>();
    }

    public GameObject GetNextObject()
    {
        int count = pool.Count;
        for (int i = 0; i < count; i++)
        {
            counter++;
            counter = counter % count;
            if (pool[counter].activeSelf == false)
            {
                pool[counter].SetActive(true);
                return pool[counter];
            }
        }
        GameObject newGO = Instantiate(prefab);
        newGO.name = prefab.name + transform.childCount.ToString("00");
        newGO.transform.SetParent(transform);
        pool.Add(newGO);
        return newGO;
    }

    public void DeactivateAllObjects()
    {
        pool.ForEach(go => go.SetActive(false));
    }
}
