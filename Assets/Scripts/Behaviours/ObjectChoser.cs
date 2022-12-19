using UnityEngine;

// Класс, который активирует один случайный объект из списка при вызове OnEnable
public class ObjectChoser : MonoBehaviour
{
    private enum RotationType
    {
        NoRotate, RotateBy90Degree, FreeRotate
    }

    [SerializeField] private GameObject[] gameObjects;
    [SerializeField] private RotationType rotationType;
    private int lastObjectIndex;

    private void Start()
    {
        foreach (var gameObject in gameObjects)
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        ActiveRandomObject();
    }

    private void OnEnable()
    {
        ActiveRandomObject();
    }

    public void ActiveRandomObject()
    {
        if (gameObjects.Length == 0)
            return;

        if (gameObjects[lastObjectIndex] != null)
            gameObjects[lastObjectIndex].SetActive(false);

        lastObjectIndex = Random.Range(0, gameObjects.Length);

        var gameObject = gameObjects[lastObjectIndex];
        if (gameObject == null)
            return;

        gameObject.SetActive(true);
        var angle = rotationType switch
        {
            RotationType.RotateBy90Degree => Random.Range(0, 4) * 90f,
            RotationType.FreeRotate => Random.Range(0f, 360f),
            _ => gameObject.transform.rotation.eulerAngles.y,
        };
        gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
}
