using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace RecipaediaEX
{
    public class Demo : ModLoader
    {
        public override void __ModInitialize()
        {
            base.__ModInitialize();
            ModsManager.RegisterHook("OnLoadingFinished", this);
            ModsManager.RegisterHook("SubsystemUpdate", this);
        }

        public override void OnLoadingFinished(List<Action> actions)
        {
            base.OnLoadingFinished(actions);
            RecipaediaEXManager.Initialize();
        }

        public override void SubsystemUpdate(float dt)
        {
            base.SubsystemUpdate(dt);
        }
    }
}
