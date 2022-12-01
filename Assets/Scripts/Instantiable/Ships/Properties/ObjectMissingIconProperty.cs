using UnityEngine;

public class ObjectMissingIconProperty : MonoBehaviour
{
    [SerializeField] EntityCreator.ObjectMissingIcons icon;

    private GameObject instantiatedIcon;
    private void Start()
    {
        InstantiateIcon();
    }
    private void InstantiateIcon()
    {
        instantiatedIcon = Instantiate(EntityFactory.GetPrefab(icon), transform.position, Quaternion.identity);
        ObjectMissingIcon objectMissingIcon = instantiatedIcon.GetComponent<ObjectMissingIcon>();
        objectMissingIcon.SetObjectToFollow(gameObject);
    }
}
