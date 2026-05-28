using UnityEngine;
using UnityEngine.EventSystems;

public class ClickMarker : MonoBehaviour,IPointerClickHandler
{
    public int id;
    private object cacheContext;
    public void Init(int markID,object context)
    {
        this.id=markID;
        this.cacheContext = context;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Click.instance.Execute(id,cacheContext);
    }
}
