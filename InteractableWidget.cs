using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace RecipaediaEX
{
    public class InteractableWidget : ClickableWidget
    {
        /// <summary>
        /// 鼠标悬停
        /// </summary>
        public bool IsMouseHover { get; set; }

        public override void Update()
        {
            base.Update();
            if (HitTestGlobal(Input.MousePosition ?? new Engine.Vector2(0, 0)) == this || HitTestGlobal(Input.PadCursorPosition) == this)
            {
                IsMouseHover = true;
            }
            else IsMouseHover = false;
        }
    }
}
