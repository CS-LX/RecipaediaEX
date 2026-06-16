using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>二级配方弹窗内 Crafter 横向 Tab（仅 Overlay）。</summary>
    public class RecipaediaOverlayCrafterTabBar : CanvasWidget {
        ScrollPanelWidget m_scrollPanel;
        StackPanelWidget m_tabRow;
        readonly List<RecipaediaOverlayCrafterTabButton> m_buttons = [];
        Action<int>? m_selectionChanged;
        int m_selectedIndex = -1;

        public int SelectedIndex => m_selectedIndex;

        public RecipaediaOverlayCrafterTabBar() {
            m_scrollPanel = new ScrollPanelWidget {
                Direction = LayoutDirection.Horizontal,
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                ClampToBounds = true,
            };
            m_tabRow = new StackPanelWidget { Direction = LayoutDirection.Horizontal };
            m_scrollPanel.Children.Add(m_tabRow);
            Children.Add(m_scrollPanel);
        }

        public void SetGroups(IReadOnlyList<RecipaediaCrafterRecipeGroup> groups, int selectedIndex, Action<int> onSelectionChanged) {
            ClearTabs();
            m_selectionChanged = onSelectionChanged;
            m_selectedIndex = selectedIndex;
            IsVisible = groups.Count > 0;
            for (int i = 0; i < groups.Count; i++) {
                var button = new RecipaediaOverlayCrafterTabButton(groups[i].RepresentativeBlockValue);
                m_buttons.Add(button);
                m_tabRow.Children.Add(button);
            }
            UpdateSelectionVisuals();
        }

        public void ClearTabs() {
            foreach (RecipaediaOverlayCrafterTabButton button in m_buttons) m_tabRow.Children.Remove(button);
            m_buttons.Clear();
            m_selectedIndex = -1;
            m_selectionChanged = null;
            IsVisible = false;
        }

        public override void Update() {
            base.Update();
            for (int i = 0; i < m_buttons.Count; i++) {
                if (!m_buttons[i].IsClicked) continue;
                if (m_selectedIndex == i) continue;
                m_selectedIndex = i;
                UpdateSelectionVisuals();
                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                m_selectionChanged?.Invoke(i);
                break;
            }
        }

        public void SelectTab(int index) {
            if (index < 0 || index >= m_buttons.Count || m_selectedIndex == index) return;
            m_selectedIndex = index;
            UpdateSelectionVisuals();
        }

        void UpdateSelectionVisuals() {
            for (int i = 0; i < m_buttons.Count; i++) m_buttons[i].IsSelected = i == m_selectedIndex;
        }
    }

    sealed class RecipaediaOverlayCrafterTabButton : CanvasWidget {
        readonly RectangleWidget m_background;
        readonly BlockIconWidget m_icon;
        readonly ClickableWidget m_clickable;

        bool m_isSelected;

        public bool IsSelected {
            get => m_isSelected;
            set {
                m_isSelected = value;
                m_background.FillColor = value
                    ? new Color(40, 120, 40, 180)
                    : new Color(0, 0, 0, 96);
            }
        }

        public bool IsClicked => m_clickable.IsClicked;

        public RecipaediaOverlayCrafterTabButton(int blockValue) {
            Size = new Vector2(48, 48);
            Margin = new Vector2(2, 0);
            m_background = new RectangleWidget {
                OutlineColor = Color.Transparent,
                FillColor = new Color(0, 0, 0, 96),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                Size = new Vector2(48, 48),
            };
            m_icon = new BlockIconWidget {
                Value = blockValue,
                Light = 15,
                Size = new Vector2(36, 36),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
            };
            m_clickable = new ClickableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
            };
            Children.Add(m_background);
            Children.Add(m_icon);
            Children.Add(m_clickable);
        }
    }
}
