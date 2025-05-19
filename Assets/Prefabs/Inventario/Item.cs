using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Dados do item")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite sprite;
    [TextArea]       [SerializeField] private string description;

}
