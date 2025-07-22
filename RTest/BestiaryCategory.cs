using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;
using RecipaediaEX.UI;
using TemplatesDatabase;

namespace RTest {
    public class BestiaryCategory : IAdvancedCategory {
        public List<BestiaryItem> m_animals;
        public string Id => "Animals";
        public string DisplayName => "动物";
        public IEnumerable<IRecipaediaItem> GetItems() => m_animals;
        public Func<IRecipaediaItem, Widget> ItemWidgetFactory => CreateItemWidget;
        public float ListItemSize => 120;
        public LayoutDirection ListDirection => LayoutDirection.Vertical;

        public BestiaryCategory() {
            m_animals = new List<BestiaryItem>();
            var list = new List<BestiaryCreatureInfo>();
			foreach (ValuesDictionary entitiesValuesDictionary in DatabaseManager.EntitiesValuesDictionaries)
			{
				ValuesDictionary valuesDictionary = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentCreature));
				if (valuesDictionary != null)
				{
					string value = valuesDictionary.GetValue<string>("DisplayName");
					if (value.StartsWith("[") && value.EndsWith("]"))
					{
						string[] lp = value.Substring(1, value.Length - 2).Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
						value = LanguageControl.GetDatabase("DisplayName", lp[1]);
					}
					if (!string.IsNullOrEmpty(value))
					{
						int order = -1;
						ValuesDictionary value2 = entitiesValuesDictionary.GetValue<ValuesDictionary>("CreatureEggData", null);
						ValuesDictionary value3 = entitiesValuesDictionary.GetValue<ValuesDictionary>("Player", null);
						if (value2 != null || value3 != null)
						{
							if (value2 != null)
							{
								int value4 = value2.GetValue<int>("EggTypeIndex");
								if (value4 < 0)
								{
									continue;
								}
								order = value4;
							}
							ValuesDictionary valuesDictionary2 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentCreatureModel));
							ValuesDictionary valuesDictionary3 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentBody));
							ValuesDictionary valuesDictionary4 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentHealth));
							ValuesDictionary valuesDictionary5 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentMiner));
							ValuesDictionary valuesDictionary6 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentLocomotion));
							ValuesDictionary valuesDictionary7 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentHerdBehavior));
							ValuesDictionary valuesDictionary8 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentMount));
							ValuesDictionary valuesDictionary9 = DatabaseManager.FindValuesDictionaryForComponent(entitiesValuesDictionary, typeof(ComponentLoot));
							string dy = valuesDictionary.GetValue<string>("Description");
							if (dy.StartsWith("[") && dy.EndsWith("]"))
							{
								string[] lp = dy.Substring(1, dy.Length - 2).Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
								dy = LanguageControl.GetDatabase("Description", lp[1]);
							}
							var bestiaryCreatureInfo = new BestiaryCreatureInfo
							{
								EntityValuesDictionary = entitiesValuesDictionary,
								Order = order,
								DisplayName = value,
								Description = dy,
								ModelName = valuesDictionary2.GetValue<string>("ModelName"),
								TextureOverride = valuesDictionary2.GetValue<string>("TextureOverride"),
								Mass = valuesDictionary3.GetValue<float>("Mass"),
								AttackResilience = valuesDictionary4.GetValue<float>("AttackResilience"),
								AttackPower = valuesDictionary5?.GetValue<float>("AttackPower") ?? 0f,
								MovementSpeed = MathUtils.Max(valuesDictionary6.GetValue<float>("WalkSpeed"), valuesDictionary6.GetValue<float>("FlySpeed"), valuesDictionary6.GetValue<float>("SwimSpeed")),
								JumpHeight = MathUtils.Sqr(valuesDictionary6.GetValue<float>("JumpSpeed")) / 20f,
								IsHerding = valuesDictionary7 != null,
								CanBeRidden = valuesDictionary8 != null,
								HasSpawnerEgg = value2?.GetValue<bool>("ShowEgg") ?? false,
								Loot = (valuesDictionary9 != null) ? ComponentLoot.ParseLootList(valuesDictionary9.GetValue<ValuesDictionary>("Loot")) : []
							};
							if (value3 != null && entitiesValuesDictionary.DatabaseObject.Name.ToLower().Contains("female"))
							{
								bestiaryCreatureInfo.AttackPower *= 0.8f;
								bestiaryCreatureInfo.AttackResilience *= 0.8f;
								bestiaryCreatureInfo.MovementSpeed *= 1.03f;
								bestiaryCreatureInfo.JumpHeight *= MathUtils.Sqr(1.03f);
							}
							list.Add(bestiaryCreatureInfo);
						}
					}
				}
			}
            m_animals = list.Select(x => new BestiaryItem(x)).ToList();
        }

        public Widget CreateItemWidget(IRecipaediaItem item) {
            if (item is not BestiaryItem bestiaryItem) return null;
            var bestiaryCreatureInfo2 = bestiaryItem.m_value;
            XElement node2 = ContentManager.Get<XElement>("Widgets/BestiaryItem");
            var obj = (ContainerWidget)Widget.LoadWidget(this, node2, null);
            ModelWidget modelWidget = obj.Children.Find<ModelWidget>("BestiaryItem.Model");
            BestiaryScreen.SetupBestiaryModelWidget(bestiaryCreatureInfo2, modelWidget, new Vector3(-1f, 0f, -1f), autoRotate: false, autoAspect: false);
            obj.Children.Find<LabelWidget>("BestiaryItem.Text").Text = bestiaryCreatureInfo2.DisplayName;
            obj.Children.Find<LabelWidget>("BestiaryItem.Details").Text = bestiaryCreatureInfo2.Description;
            return obj;
        }
    }
}