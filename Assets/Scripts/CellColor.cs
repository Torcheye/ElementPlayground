using Unity.Entities;
using UnityEngine;

public class CellColor : MonoBehaviour
{
    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        //_renderer.color = Type == 1 ? Color.yellow : Color.black;
        //World.DefaultGameObjectInjectionWorld.EntityManager.
    }
}