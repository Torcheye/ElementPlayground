using Unity.Entities;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public float brushSize;
    private EntityManager _entityManager;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void SetBrushType(int type)
    {
        var query = _entityManager.CreateEntityQuery(typeof(Brush));
        if (query.IsEmpty) return;
        
        _entityManager.SetComponentData(query.GetSingletonEntity(), new Brush()
        {
            BrushSize = brushSize * 0.01f,
            Type = (CellType)type
        });
    }
}