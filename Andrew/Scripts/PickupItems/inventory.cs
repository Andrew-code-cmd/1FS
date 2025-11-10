using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<string> items = new List<string>();
    public int totalCountToCollect = 10;
    public AudioSource audioSource;
    public AudioClip collectItemSound;
    public TMP_Text countOfJijaLabel;
    public GameObject tasksPanel;

    bool tasksOpened;

    void Start()
    {
        tasksPanel.SetActive(false);
    }

    void OpenTasks()
    {
        if (!tasksOpened && Input.GetKey(KeyCode.Tab))
        {
            tasksPanel.SetActive(true);
        }
        else
        {
            tasksPanel.SetActive(false);
        }
    }

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        audioSource.PlayOneShot(collectItemSound);
        countOfJijaLabel.text = items.Count.ToString();
    }

    void Update()
    {
        OpenTasks();
    }
}
