using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UIDrawerController : MonoBehaviour
{
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void OpenCloseDrawer()
    {
        anim.SetBool("Open", !anim.GetBool("Open"));
        EventSystem.current.SetSelectedGameObject(null);
    }
}
