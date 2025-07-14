using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;
using ZLinq;

namespace RecipaediaEX {
    public class RecipaediaEXDescriptionScreen : Screen
	{
		public CanvasWidget m_iconDisplayerWidget;

		public LabelWidget m_nameWidget;

		public ButtonWidget m_leftButtonWidget;

		public ButtonWidget m_rightButtonWidget;

		public LabelWidget m_descriptionWidget;

		public LabelWidget m_propertyNames1Widget;

		public LabelWidget m_propertyValues1Widget;

		public LabelWidget m_propertyNames2Widget;

		public LabelWidget m_propertyValues2Widget;

		public int m_index;

		public IList<IRecipaediaDescriptionItem> m_valuesList;

		public static string fName = "RecipaediaEXDescriptionScreen";

		public RecipaediaEXDescriptionScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/RecipaediaEXDescriptionScreen");
			LoadContents(this, node);
			m_iconDisplayerWidget = Children.Find<CanvasWidget>("IconDisplayer");
			m_nameWidget = Children.Find<LabelWidget>("Name");
			m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
			m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
			m_descriptionWidget = Children.Find<LabelWidget>("Description");
			m_propertyNames1Widget = Children.Find<LabelWidget>("PropertyNames1");
			m_propertyValues1Widget = Children.Find<LabelWidget>("PropertyValues1");
			m_propertyNames2Widget = Children.Find<LabelWidget>("PropertyNames2");
			m_propertyValues2Widget = Children.Find<LabelWidget>("PropertyValues2");
		}

		public override void Enter(object[] parameters)
		{
			IRecipaediaItem item = (IRecipaediaItem)parameters[0];
            var valueList = ((IList<IRecipaediaItem>)parameters[1]).AsValueEnumerable();
            if (valueList.Any(x => x is not IRecipaediaDescriptionItem)) {
                throw new ArgumentException("All element in the list must be IRecipaediaDescriptionItem");
            }
            m_valuesList = valueList.OfType<IRecipaediaDescriptionItem>().ToList();
			m_index = valueList.Select((value, i) => new { Index = i, Value = value }).First(x => x.Value == item).Index;
			UpdateBlockProperties();
		}

        public override void Update()
		{
			m_leftButtonWidget.IsEnabled = m_index > 0;
			m_rightButtonWidget.IsEnabled = m_index < m_valuesList.Count - 1;
			if (m_leftButtonWidget.IsClicked || Input.Left)
			{
				m_index = MathUtils.Max(m_index - 1, 0);
				UpdateBlockProperties();
			}
			if (m_rightButtonWidget.IsClicked || Input.Right)
			{
				m_index = MathUtils.Min(m_index + 1, m_valuesList.Count - 1);
				UpdateBlockProperties();
			}
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}

		public virtual void UpdateBlockProperties()
		{
			if (m_index >= 0 && m_index < m_valuesList.Count)
			{
				IRecipaediaDescriptionItem value = m_valuesList[m_index];
                m_iconDisplayerWidget.ClearChildren();
                m_iconDisplayerWidget.AddChildren(value.Icon);
                m_nameWidget.Text = value.Name;
                m_descriptionWidget.Text = value.Description;
				m_propertyNames1Widget.Text = string.Empty;
				m_propertyValues1Widget.Text = string.Empty;
				m_propertyNames2Widget.Text = string.Empty;
				m_propertyValues2Widget.Text = string.Empty;
				Dictionary<string, string> blockProperties = value.GetProperties();
				int num2 = 0;
				foreach (KeyValuePair<string, string> item in blockProperties)
				{
					if (num2 < blockProperties.Count - (blockProperties.Count / 2))
					{
						LabelWidget propertyNames1Widget = m_propertyNames1Widget;
                        propertyNames1Widget.Text = propertyNames1Widget.Text + item.Key + ":\n";
						LabelWidget propertyValues1Widget = m_propertyValues1Widget;
						propertyValues1Widget.Text = propertyValues1Widget.Text + item.Value + "\n";
					}
					else
					{
						LabelWidget propertyNames2Widget = m_propertyNames2Widget;
						propertyNames2Widget.Text = propertyNames2Widget.Text + item.Key + ":\n";
						LabelWidget propertyValues2Widget = m_propertyValues2Widget;
						propertyValues2Widget.Text = propertyValues2Widget.Text + item.Value + "\n";
					}
					num2++;
				}
			}
		}
	}
}
