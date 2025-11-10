using TMPro;
using UnityEditor;
using UnityEngine;

public class NoteReader : MonoBehaviour
{
    [Header("Настройка UI")]
    public CanvasGroup hintCanvasGroup;
    public TMP_Text hintText;     // UI-элемент "Нажмите [E]"
    public float fadeSpeed = 4f;

    public GameObject noteReadPanel;
    public TMP_Text noteTitleField;
    public TMP_Text noteTextField;
    public NoteContent noteContent;

    [Header("Настройка Взаимодействия")]
    public float interactDistance = 2.5f; // Максимальная дистанция, с которой можно открыть
    private KeyCode interactKey = KeyCode.F; // Клавиша взаимодействия

    bool lookingAtNote;

    void ReadNote()
    {
        lookingAtNote = false;

        // создать луч из позиции объекта, на котором висит данный скрипт, направление луча - вперед
        Ray ray = new Ray(transform.position, transform.forward);

        // отобразить Ray в сцене, ray.direction умножается на 10, поскольку 
        // он нормализован (то есть его величина, длина, равна 1).
        Debug.DrawRay(ray.origin, ray.direction * 10);

        // объявляем RaycastHit - структуру данных, хранящей в себе информацию о 
        // любых столкновениях луча с объектами в сцене 
        RaycastHit hit;

        // функция RayCast позволяет отследить любое столкновение луча с объектами в сцене 
        // и записать информацию о столкновении в переменной RaycastHit (в данном случае hit)
        // ключевое слово out позволяет не просто передать переменную в качестве параметра, 
        // но и перезаписать ее исходное значение

        // Physics.Raycast(ray, out hit); используем ее прямо в условии

        // также можем получить тэг объекта, с которым была коллизия 
        // string tag = hit.collider.tag;

        // можно также получить ссылку на объект, с которым была коллизия 
        // GameObject hitObject = hit.transform.gameObject;

        // получим, например, расстояние от начала луча до объекта, с которым есть коллизия
        // float hitDistance = hit.distance;

        if(Physics.Raycast(ray, out hit, interactDistance))
        {
            if(hit.collider.tag == "Note")
            {
                lookingAtNote = true;
                if (Input.GetKeyDown(interactKey))
                {
                    noteReadPanel.SetActive(true);
                    noteTitleField.text = noteContent.noteTitle;
                    noteTextField.text = noteContent.noteText;
                } else if (Input.GetKeyUp(interactKey)){
                    noteReadPanel.SetActive(false);
                }
            }
        }

        // плавное появление текста подсказки
        float targetAlpha = lookingAtNote ? 1f : 0f;
        if (hintCanvasGroup != null)
            hintCanvasGroup.alpha = Mathf.MoveTowards(hintCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Обновление текста и взаимодействие
        if (lookingAtNote && hintText != null)
            hintText.text = $"Удерживайте <F> чтобы прочитать записку";

        if(!lookingAtNote)
            noteReadPanel.SetActive(false);
    }

    private void Start()
    {
        if (hintCanvasGroup != null)
            hintCanvasGroup.alpha = 0f;

        noteReadPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ReadNote();
    }
}
