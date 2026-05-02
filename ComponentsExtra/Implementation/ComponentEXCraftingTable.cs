using System;
using System.Globalization;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Events;
using RecipaediaEX.Implementation;
using TemplatesDatabase;

namespace RecipaediaEX.ComponentsExtra.Implementation {
    public class ComponentEXCraftingTable : ComponentCraftingTable, IUpdateable {
        public new int m_craftingGridSize;

        public new string[] m_matchedIngredients = new string[36];

        public new OriginalCraftingRecipe m_matchedRecipe;
        public new int RemainsSlotIndex => SlotsCount - 1;

        public new UpdateOrder UpdateOrder => UpdateOrder.Default;

        public new bool m_recipeUpdateNeeded;

        public new bool m_recipeRefindNeeded;
        public new int ResultSlotIndex => SlotsCount - 2;

        public new bool m_resetWhenSlotItemsRemoved = false;

        public override void Update(float dt) {
            if (m_recipeUpdateNeeded) {
                UpdateCraftingResult(m_recipeRefindNeeded);
            }
            m_recipeUpdateNeeded = false;
            m_recipeRefindNeeded = false;
        }

        public override int GetSlotCapacity(int slotIndex, int value) {
            return slotIndex < SlotsCount - 2 ? base.GetSlotCapacity(slotIndex, value) : 0;
        }

        public override void AddSlotItems(int slotIndex, int value, int count) {
            int oldCount = GetSlotCount(slotIndex);
            Base_AddSlotItems(slotIndex, value, count);
            if (oldCount == 0) m_recipeRefindNeeded = true;
            m_recipeUpdateNeeded = true;
            m_slots[ResultSlotIndex].Count = 0;
        }

        public override int RemoveSlotItems(int slotIndex, int count) {
            int num = 0;
            int[] originalCount = new int[SlotsCount - 2];
            for (int i = 0; i < originalCount.Length; i++) {
                originalCount[i] = GetSlotCount(i);
            }
            if (slotIndex == ResultSlotIndex) {
                if (m_matchedRecipe != null) {
                    if (m_matchedRecipe.RemainsValue != 0
                        && m_matchedRecipe.RemainsCount > 0) {
                        if (m_slots[RemainsSlotIndex].Count == 0
                            || m_slots[RemainsSlotIndex].Value == m_matchedRecipe.RemainsValue) {
                            int num2 = BlocksManager.Blocks[Terrain.ExtractContents(m_matchedRecipe.RemainsValue)].GetMaxStacking(m_matchedRecipe.RemainsValue) - m_slots[RemainsSlotIndex].Count;
                            count = MathUtils.Min(count, num2 / m_matchedRecipe.RemainsCount * m_matchedRecipe.ResultCount);
                        }
                        else {
                            count = 0;
                        }
                    }
                    count = count / m_matchedRecipe.ResultCount * m_matchedRecipe.ResultCount;
                    int outputBlockValue = GetSlotValue(ResultSlotIndex);
                    num = Base_RemoveSlotItems(slotIndex, count);
                    if (num > 0) {
                        for (int i = 0; i < 36; i++) {
                            if (!string.IsNullOrEmpty(m_matchedIngredients[i])) {
                                int index = (i % 6) + (m_craftingGridSize * (i / 6));
                                m_slots[index].Count = MathUtils.Max(m_slots[index].Count - (num / m_matchedRecipe.ResultCount), 0);
                            }
                        }
                        if (m_matchedRecipe.RemainsValue != 0
                            && m_matchedRecipe.RemainsCount > 0) {
                            m_slots[RemainsSlotIndex].Value = m_matchedRecipe.RemainsValue;
                            m_slots[RemainsSlotIndex].Count += num / m_matchedRecipe.ResultCount * m_matchedRecipe.RemainsCount;
                        }
                        ComponentPlayer componentPlayer = FindInteractingPlayer();
                        if (componentPlayer is { PlayerStats: not null }) {
                            componentPlayer.PlayerStats.ItemsCrafted += num;
                        }
                        if (outputBlockValue != 0) {
                            RecipaediaEventBus.CrafterOutputRemoved.Publish(new CrafterOutputRemovedEvent(
                                Project,
                                componentPlayer,
                                outputBlockValue,
                                num,
                                CrafterInventorySurfaceKind.CraftingTable));
                        }
                    }
                }
            }
            else {
                num = Base_RemoveSlotItems(slotIndex, count);
            }
            m_recipeUpdateNeeded = true;
            if (m_resetWhenSlotItemsRemoved) m_slots[ResultSlotIndex].Count = 0;
            for (int i = 0; i < originalCount.Length; i++) {
                if (originalCount[i] > 0
                    && GetSlotCount(i) == 0)
                    m_recipeRefindNeeded = true;
            }
            return num;
        }

        public override void DropAllItems(Vector3 position) {
            for (int i = 0; i < SlotsCount; i++) {
                if (i != ResultSlotIndex) DropSlotItems(this, i, position, m_random.Float(5f, 10f) * Vector3.Normalize(new Vector3(m_random.Float(-1f, 1f), m_random.Float(1f, 2f), m_random.Float(-1f, 1f))));
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            base.Load(valuesDictionary, idToEntityMap);
            m_craftingGridSize = (int)MathF.Sqrt(SlotsCount - 2);
            UpdateCraftingResult(true);
        }

        public override void UpdateCraftingResult(bool recipeRefindNeeded) {
            int num = int.MaxValue;
            int?[] actualIngredients = new int?[36];
            for (int i = 0; i < m_craftingGridSize; i++) {
                for (int j = 0; j < m_craftingGridSize; j++) {
                    int num2 = i + (j * 6);
                    int slotIndex = i + (j * m_craftingGridSize);
                    int slotValue = GetSlotValue(slotIndex);
                    int slotCount = GetSlotCount(slotIndex);
                    if (slotCount > 0) {
                        m_matchedIngredients[num2] = ToCraftingID(slotValue);
                        num = MathUtils.Min(num, slotCount);
                        actualIngredients[num2] = slotValue;
                    }
                    else {
                        m_matchedIngredients[num2] = null;
                        actualIngredients[num2] = null;
                    }
                }
            }
            ComponentPlayer componentPlayer = FindInteractingPlayer();
            float playerLevel = componentPlayer?.PlayerData.Level ?? 1f;
            OriginalCraftingRecipe craftingRecipe;
            if (recipeRefindNeeded) {
                OriginalCraftingRecipe actualCraftingRecipe = new() { Ingredients = m_matchedIngredients, RequiredHeatLevel = 0f, RequiredPlayerLevel = playerLevel };
                actualCraftingRecipe.SetExtraValue("Project", Project);
                actualCraftingRecipe.SetExtraValue<IInventory>("Inventory", this);
                actualCraftingRecipe.SetExtraValue("ActualIngredients", actualIngredients);
                craftingRecipe = OriginalComponentsExtensions.FindCraftingRecipe(Project.FindSubsystem<SubsystemTerrain>(), actualCraftingRecipe);
            }
            else {
                craftingRecipe = m_matchedRecipe;
            }
            if (craftingRecipe != null
                && craftingRecipe.ResultValue != 0) {
                m_matchedRecipe = craftingRecipe;
                m_slots[ResultSlotIndex].Value = craftingRecipe.ResultValue;
                m_slots[ResultSlotIndex].Count = craftingRecipe.ResultCount * num;
            }
            else {
                m_matchedRecipe = null;
                m_slots[ResultSlotIndex].Value = 0;
                m_slots[ResultSlotIndex].Count = 0;
            }
            if (craftingRecipe != null
                && !string.IsNullOrEmpty(craftingRecipe.Message)) {
                string message = craftingRecipe.Message;
                if (message.StartsWith("[")
                    && message.EndsWith("]")) {
                    message = LanguageControl.Get("CRMessage", message.Substring(1, message.Length - 2));
                }
                componentPlayer?.ComponentGui.DisplaySmallMessage(message, Color.White, blinking: true, playNotificationSound: true);
            }
        }

        public virtual int Base_RemoveSlotItems(int slotIndex, int count) {
            if (slotIndex >= 0
                && slotIndex < m_slots.Count) {
                Slot slot = m_slots[slotIndex];
                count = MathUtils.Min(count, GetSlotCount(slotIndex));
                slot.Count -= count;
                return count;
            }
            return 0;
        }


        public virtual void Base_AddSlotItems(int slotIndex, int value, int count) {
            if (count > 0
                && slotIndex >= 0
                && slotIndex < m_slots.Count) {
                Slot slot = m_slots[slotIndex];
                int slotValue = GetSlotValue(slotIndex);
                int slotCount = GetSlotCount(slotIndex);
                int slotCapacity = GetSlotCapacity(slotIndex, value);
                if (slotCount != 0
                    && slotValue != value) {
                    throw new InvalidOperationException(
                        string.Format(
                            "Cannot add slot items because items are different. Slot {0} Contains BlockValue {1} with count {2}. The value to add is {3} with count {4}. Slot capacity is {5}",
                            slotIndex,
                            slotValue,
                            slotCount,
                            value,
                            count,
                            slotCapacity
                        )
                    );
                }
                if (GetSlotCount(slotIndex) + count > slotCapacity) {
                    throw new InvalidOperationException(
                        string.Format(
                            "Cannot add slot items because it exceeded capacity. Slot {0} Contains BlockValue {1} with count {2}. The value to add is {3} with count {4}. Slot capacity is {5}",
                            slotIndex,
                            slotValue,
                            slotCount,
                            value,
                            count,
                            slotCapacity
                        )
                    );
                }
                slot.Value = value;
                slot.Count += count;
            }
        }

        public virtual string ToCraftingID(int slotValue) {
            int content = Terrain.ExtractContents(slotValue);
            int data = Terrain.ExtractData(slotValue);
            Block block = BlocksManager.Blocks[content];
            return block.GetCraftingId(slotValue) + ":" + data.ToString(CultureInfo.InvariantCulture);
        }
    }
}