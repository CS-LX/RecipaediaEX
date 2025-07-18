﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace RecipaediaEX
{
    public class RecipaediaEXLoader : ModLoader
    {
        public override void __ModInitialize()
        {
            base.__ModInitialize();
            ModsManager.RegisterHook("OnLoadingFinished", this);
        }

        public override void OnLoadingFinished(List<Action> actions)
        {
            base.OnLoadingFinished(actions);
            RecipesLoadManager.Initialize();
            RecipaediaEXManager.Initialize();
            ScreensManager.m_screens["Recipaedia"] = new RecipaediaEXScreen();
            ScreensManager.m_screens["RecipaediaDescription"] = new RecipaediaEXDescriptionScreen();
        }
    }
}
