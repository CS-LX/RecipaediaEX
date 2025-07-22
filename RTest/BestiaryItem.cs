using System.Globalization;
using Engine;
using Game;
using RecipaediaEX.UI;

namespace RTest {
    public class BestiaryItem : IRecipaediaItem, IRecipaediaDescriptionItem {
        public BestiaryCreatureInfo m_value;

        public object Value => m_value;
        public string DetailScreenName => "RecipaediaDescription";
        public string RecipeScreenName => "RecipaediaDescription";

        public BestiaryItem(BestiaryCreatureInfo value) {
            m_value = value;
        }

        public string Name => m_value.DisplayName;

        public Widget Icon {
            get {
                ModelWidget modelWidget = new ModelWidget {AutoRotationVector = new Vector3(0f, 1f, 0f)};
                BestiaryScreen.SetupBestiaryModelWidget(m_value, modelWidget, new Vector3(-1f, 0f, -1f), autoRotate: true, autoAspect: true);
                return modelWidget;
            }
        }
        public string Description => m_value.Description;

        public Dictionary<string, string> GetProperties()
        {
            var props = new Dictionary<string, string>();
            string typeName = nameof(BestiaryDescriptionScreen);

            props.Add(LanguageControl.Get(typeName, "resilience"), m_value.AttackResilience.ToString(CultureInfo.InvariantCulture));

            props.Add(LanguageControl.Get(typeName, "attack"),
                (m_value.AttackPower > 0f) ? m_value.AttackPower.ToString("0.0") : LanguageControl.None);

            props.Add(LanguageControl.Get(typeName, "herding"),
                m_value.IsHerding ? LanguageControl.Yes : LanguageControl.No);

            props.Add(LanguageControl.Get(typeName, 1),
                m_value.CanBeRidden ? LanguageControl.Yes : LanguageControl.No);

            props.Add(LanguageControl.Get(typeName, "speed"),
                (m_value.MovementSpeed * 3.6).ToString("0") + LanguageControl.Get(typeName, "speed unit"));

            props.Add(LanguageControl.Get(typeName, "jump height"),
                m_value.JumpHeight.ToString("0.0") + LanguageControl.Get(typeName, "length unit"));

            props.Add(LanguageControl.Get(typeName, "weight"),
                m_value.Mass.ToString(CultureInfo.InvariantCulture) + LanguageControl.Get(typeName, "weight unit"));

            props.Add(LanguageControl.Get("BlocksManager", "Spawner Eggs") + ":",
                m_value.HasSpawnerEgg ? LanguageControl.Exists : LanguageControl.None);

            return props;
        }
    }
}