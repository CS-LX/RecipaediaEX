using System.Globalization;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Implementation;
using TemplatesDatabase;
using FurnaceBlock = Game.FurnaceBlock;

namespace RecipaediaEX.ComponentsExtra.Implementation {
    public class ComponentEXFurnace : ComponentFurnace, IUpdateable {
        public new SubsystemTerrain m_subsystemTerrain;

        public new SubsystemExplosions m_subsystemExplosions;

        public new ComponentBlockEntity m_componentBlockEntity;

        #region 1.8.1.1变量

        public new SubsystemGameInfo m_subsystemGameInfo;

        public new SubsystemTime m_subsystemTime;

        public new FireParticleSystem m_fireParticleSystem;

        public new SubsystemParticles m_subsystemParticles;

        public new bool StopFuelWhenNoRecipeIsActive = true;

        public new float SmeltSpeed = 0.15f;

        /// <summary>
        /// 没有燃料时，冶炼进度倒退速率
        /// </summary>
        public new float SmeltProgressReductionSpeed = float.PositiveInfinity;

        /// <summary>
        /// 使用燃料时，燃料实际补充的时间倍数
        /// </summary>
        public new float FuelTimeEfficiency = 1f;

        /// <summary>
        /// 燃料耗尽时间
        /// 开发时，注意这个不能改成{get;set;}形式，否则会出现mod兼容问题
        /// </summary>
        public new float m_fuelEndTime;

        public override float FireTimeRemaining => m_fireTimeRemaining;

        #endregion

        public new int m_furnaceSize;

        public new string[] m_matchedIngredients = new string[36];

        public override float m_fireTimeRemaining { get; set; }

        public new float m_heatLevel;

        public new bool m_updateSmeltingRecipe;

        public new OriginalSmeltingRecipe m_smeltingRecipe;

        public new float m_smeltingProgress;

        public override int RemainsSlotIndex => SlotsCount - 1;

        public override int ResultSlotIndex => SlotsCount - 2;

        public override int FuelSlotIndex => SlotsCount - 3;

        public override float HeatLevel => m_heatLevel;

        public override float SmeltingProgress => m_smeltingProgress;

        public new UpdateOrder UpdateOrder => UpdateOrder.Default;

        public override void OnEntityRemoved() {
            m_subsystemParticles.RemoveParticleSystem(m_fireParticleSystem);
        }

        public override void AddSlotItems(int slotIndex, int value, int count) {
            m_updateSmeltingRecipe = true;
            base.AddSlotItems(slotIndex, value, count);
        }

        public override int RemoveSlotItems(int slotIndex, int count) {
            m_updateSmeltingRecipe = true;
            return base.RemoveSlotItems(slotIndex, count);
        }

        public new void Update(float dt) {
            m_fuelEndTime = (float)(m_subsystemGameInfo.TotalElapsedGameTime + m_fireTimeRemaining);
            if (m_heatLevel > 0f) {
                m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
                if (m_fireTimeRemaining == 0f) {
                    m_heatLevel = 0f;
                }
            }
            if (m_updateSmeltingRecipe) {
                UpdateSmeltingRecipe();
            }
            if (m_smeltingRecipe == null) //没有配方，处理空烧
            {
                if (StopFuelWhenNoRecipeIsActive) {
                    StopSmelting(true);
                }
            }
            if (m_smeltingRecipe != null
                && m_fireTimeRemaining <= 0f) {
                UseFuel();
            }
            if (m_fireTimeRemaining <= 0f) {
                m_smeltingRecipe = null;
                m_smeltingProgress = MathUtils.Max(0, m_smeltingProgress - dt * SmeltProgressReductionSpeed);
            }
            if (m_smeltingRecipe != null) {
                m_smeltingProgress = MathUtils.Min(m_smeltingProgress + (SmeltSpeed * dt), 1f);
                if (m_smeltingProgress >= 1f) {
                    for (int i = 0; i < m_furnaceSize; i++) {
                        if (m_slots[i].Count > 0) {
                            m_slots[i].Count--;
                        }
                    }
                    m_slots[ResultSlotIndex].Value = m_smeltingRecipe.ResultValue;
                    m_slots[ResultSlotIndex].Count += m_smeltingRecipe.ResultCount;
                    if (m_smeltingRecipe.RemainsValue != 0
                        && m_smeltingRecipe.RemainsCount > 0) {
                        m_slots[RemainsSlotIndex].Value = m_smeltingRecipe.RemainsValue;
                        m_slots[RemainsSlotIndex].Count += m_smeltingRecipe.RemainsCount;
                    }
                    m_smeltingRecipe = null;
                    m_smeltingProgress = 0f;
                    m_updateSmeltingRecipe = true;
                }
            }
            //根据熔炉燃烧状态调整方块值
            int cellValue = m_componentBlockEntity.BlockValue;
            if (m_heatLevel > 0f) {
                m_fireParticleSystem.m_position = m_componentBlockEntity.Position + new Vector3(0.5f, 0.2f, 0.5f);
                if (Terrain.ExtractContents(cellValue) == FurnaceBlock.Index) m_subsystemParticles.AddParticleSystem(m_fireParticleSystem);
                m_componentBlockEntity.BlockValue = Terrain.ReplaceContents(cellValue, LitFurnaceBlock.Index);
            }
            else {
                if (Terrain.ExtractContents(cellValue) == LitFurnaceBlock.Index) m_subsystemParticles.RemoveParticleSystem(m_fireParticleSystem);
                m_componentBlockEntity.BlockValue = Terrain.ReplaceContents(cellValue, FurnaceBlock.Index);
            }
        }

        /// <summary>
        /// 更新配方逻辑
        /// </summary>
        public override void UpdateSmeltingRecipe() {
            m_updateSmeltingRecipe = false;
            float heatLevel = 0f;
            if (m_heatLevel > 0f) {
                heatLevel = m_heatLevel;
            }
            else {
                Slot slot = m_slots[FuelSlotIndex];
                if (slot.Count > 0) {
                    int num = Terrain.ExtractContents(slot.Value);
                    heatLevel = BlocksManager.Blocks[num].GetFuelHeatLevel(slot.Value);
                }
            }
            OriginalSmeltingRecipe craftingRecipe = FindSmeltingRecipe(heatLevel);
            if (craftingRecipe != m_smeltingRecipe) {
                m_smeltingRecipe = (craftingRecipe != null && craftingRecipe.ResultValue != 0) ? craftingRecipe : null;
                m_smeltingProgress = 0f;
            }
        }

        /// <summary>
        /// 使用燃料逻辑，目前返回值在API熔炉中无作用
        /// </summary>
        /// <returns>是否成功消耗燃料</returns>
        public override bool UseFuel() {
            Point3 coordinates = m_componentBlockEntity.Coordinates;
            Slot slot2 = m_slots[FuelSlotIndex];
            if (slot2.Count > 0) {
                int num2 = Terrain.ExtractContents(slot2.Value);
                Block block = BlocksManager.Blocks[num2];
                if (block.GetExplosionPressure(slot2.Value) > 0f) {
                    slot2.Count = 0;
                    m_subsystemExplosions.TryExplodeBlock(coordinates.X, coordinates.Y, coordinates.Z, slot2.Value);
                }
                else if (block.GetFuelHeatLevel(slot2.Value) > 0f) {
                    slot2.Count--;
                    m_fireTimeRemaining = block.GetFuelFireDuration(slot2.Value) * FuelTimeEfficiency;
                    m_heatLevel = block.GetFuelHeatLevel(slot2.Value);
                    return true;
                }
            }
            return false;
        }

        public override void StopSmelting(bool resetProgress) {
            m_heatLevel = 0f;
            m_fireTimeRemaining = 0f;
            if (resetProgress) m_smeltingProgress = 0f;
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            base.Load(valuesDictionary, idToEntityMap);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
            m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
            m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_furnaceSize = SlotsCount - 3;
            m_fireTimeRemaining = valuesDictionary.GetValue<float>("FireTimeRemaining");
            m_heatLevel = valuesDictionary.GetValue<float>("HeatLevel");
            m_updateSmeltingRecipe = true;
            m_fireParticleSystem = new FireParticleSystem(m_componentBlockEntity.Position + new Vector3(0.5f, 0.2f, 0.5f), 0.15f, 16f);
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap) {
            base.Save(valuesDictionary, entityToIdMap);
            valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
            valuesDictionary.SetValue("HeatLevel", m_heatLevel);
        }

        public new virtual OriginalSmeltingRecipe FindSmeltingRecipe(float heatLevel) {
            if (heatLevel > 0f) {
                for (int i = 0; i < m_furnaceSize; i++) {
                    int slotValue = GetSlotValue(i);
                    int num = Terrain.ExtractContents(slotValue);
                    int num2 = Terrain.ExtractData(slotValue);
                    if (GetSlotCount(i) > 0) {
                        Block block = BlocksManager.Blocks[num];
                        m_matchedIngredients[i] = block.GetCraftingId(slotValue) + ":" + num2.ToString(CultureInfo.InvariantCulture);
                    }
                    else {
                        m_matchedIngredients[i] = null;
                    }
                }
                ComponentPlayer componentPlayer = FindInteractingPlayer();
                float playerLevel = componentPlayer?.PlayerData.Level ?? 1f;
                OriginalSmeltingRecipe actualSmeltingRecipe = new OriginalSmeltingRecipe { Ingredients = m_matchedIngredients, RequiredHeatLevel = heatLevel, RequiredPlayerLevel = playerLevel };
                actualSmeltingRecipe.SetExtraValue("Project", Project);
                actualSmeltingRecipe.SetExtraValue<IInventory>("Inventory", this);
                OriginalSmeltingRecipe craftingRecipe = OriginalComponentsExtensions.FindCraftingRecipe(m_subsystemTerrain, actualSmeltingRecipe);
                if (craftingRecipe != null
                    && craftingRecipe.ResultValue != 0) {
                    if (craftingRecipe.RequiredHeatLevel <= 0f) {
                        craftingRecipe = null;
                    }
                    if (craftingRecipe != null) {
                        Slot slot = m_slots[ResultSlotIndex];
                        int num3 = Terrain.ExtractContents(craftingRecipe.ResultValue);
                        if (slot.Count != 0
                            && (craftingRecipe.ResultValue != slot.Value || craftingRecipe.ResultCount + slot.Count > BlocksManager.Blocks[num3].GetMaxStacking(craftingRecipe.ResultValue))) {
                            craftingRecipe = null;
                        }
                    }
                    if (craftingRecipe != null
                        && craftingRecipe.RemainsValue != 0
                        && craftingRecipe.RemainsCount > 0) {
                        if (m_slots[RemainsSlotIndex].Count == 0
                            || m_slots[RemainsSlotIndex].Value == craftingRecipe.RemainsValue) {
                            if (BlocksManager.Blocks[Terrain.ExtractContents(craftingRecipe.RemainsValue)].GetMaxStacking(craftingRecipe.RemainsValue) - m_slots[RemainsSlotIndex].Count < craftingRecipe.RemainsCount) {
                                craftingRecipe = null;
                            }
                        }
                        else {
                            craftingRecipe = null;
                        }
                    }
                }
                if (craftingRecipe != null
                    && !string.IsNullOrEmpty(craftingRecipe.Message)) {
                    componentPlayer?.ComponentGui.DisplaySmallMessage(craftingRecipe.Message, Color.White, blinking: true, playNotificationSound: true);
                }
                return craftingRecipe;
            }
            return null;
        }
    }
}