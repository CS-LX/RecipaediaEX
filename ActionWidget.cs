using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace RecipaediaEX
{ 
    /// <summary>
    /// 用于实现点击配方就将格子置换为智能格子
    /// </summary>
    public class ActionWidget : ClickableWidget
    {
        public RecipaediaRecipesScreen belongScreen;
        private Action onStart;
        private Action onClicked;
        private bool RemoveAfterClicked;
        private bool haveUpdate = false;
        public ActionWidget(RecipaediaRecipesScreen belongScreen, Action onStart, Action onClicked, bool removeAfterClicked = false)
        {
            this.onStart = onStart;
            this.onClicked = onClicked;
            this.belongScreen = belongScreen;
            this.RemoveAfterClicked = removeAfterClicked;
            SoundName = "Audio/UI/ButtonClick";
        }

        public override void Update()
        {
            base.Update();
            if (IsClicked)
            {
                onClicked?.Invoke(); 
                if(RemoveAfterClicked)
                {
                    ParentWidget.Children.Remove(this);
                }
            }
            if (haveUpdate)
            {
                return;
            }
            onStart?.Invoke();
            haveUpdate = true;
        }
    }
}
