using UnityEngine;

public class PickupItem : MonoBehaviour
{
    //  СКРИПТ ВЕШАЕТСЯ НА ПРЕДМЕТ, КОТОРЫЙ ХОТИМ ПОДНЯТЬ  

    public string itemName = "";
    public Inventory inventory;

    public UnityEngine.UI.Image inImages;
    public Sprite spr;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inventory.AddItem(itemName);
            Destroy(gameObject);

            inImages.sprite = spr;
            inImages.color = new Color(0, 0, 0, 0.8f);
        }
    }
}
