using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    public UnityEvent Click_Event;

    protected void OnMouseDown()
    {
        Click_Event.Invoke();
    }
}
