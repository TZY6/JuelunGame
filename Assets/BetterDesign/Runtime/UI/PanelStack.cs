using System.Collections.Generic;
using UnityEngine;

namespace BetterDesign
{
    // Modal stack owns pause, keeping overlays and gameplay input in sync.
    public sealed class PanelStack
    {
        readonly Stack<GameObject> panels = new Stack<GameObject>(4);
        public bool IsOpen { get { return panels.Count > 0; } }
        public void Push(GameObject panel)
        {
            if (panels.Count > 0 && panels.Peek() == panel) return;
            if (panels.Count > 0) panels.Peek().SetActive(false);
            panels.Push(panel); panel.SetActive(true);
        }
        public void Back()
        {
            if (panels.Count == 0) return;
            panels.Pop().SetActive(false);
            if (panels.Count > 0) panels.Peek().SetActive(true);
        }
        public void Clear() { while (panels.Count > 0) Back(); }
    }
}
