using UnityEngine;
using UnityEngine.EventSystems;

public class ClickMarker : MonoBehaviour,IPointerClickHandler
{
    public int id;
    public void OnPointerClick(PointerEventData eventData)
    {
        Click.instance.Execute(id,transform.GetComponent<furniController>().furnidata);
    }
}
