using System.Globalization;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Events;
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

        public new virtual bool StopFuelWhenNoRecipeIsActive => true;

        public new virtual float SmeltSpeed => 0.15f;

        /// <summary>
        /// 没有燃料时，冶炼进度倒退速率
        /// </summary>
        public new virtual float SmeltProgressReductionSpeed => float.PositiveInfinity;

        /// <summary>
        /// 使用燃料时，燃料实际补充的时间倍数
        /// </summary>
        public new virtual float FuelTimeEfficiency => 1f;

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
            int outputBlockValue = 0;
            if (slotIndex == ResultSlotIndex) {
                outputBlockValue = GetSlotValue(ResultSlotIndex);
                if (count > 0
                    && outputBlockValue != 0
                    && !RecipaediaInterceptBus.TryProceed(new CrafterOutputRemovingContext(
                        Project,
                        this,
                        FindInteractingPlayer(),
                        m_smeltingRecipe,
                        outputBlockValue,
                        count,
                        CrafterKind.Furnace))) {
                    return 0;
                }
            }
            int removed = base.RemoveSlotItems(slotIndex, count);
            if (removed > 0 && slotIndex == ResultSlotIndex && outputBlockValue != 0) {
                RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(
                    Project,
                    FindInteractingPlayer(),
                    outputBlockValue,
                    removed,
                    CrafterKind.Furnace));
            }
            return removed;
        }

        public new void Update(float dt) {
            OnBeginUpdate(dt);
            m_fuelEndTime = (float)(m_subsystemGameInfo.TotalElapsedGameTime + m_fireTimeRemaining);
            UpdateFireState(dt);
            if (m_updateSmeltingRecipe) {
                UpdateSmeltingRecipe();
            }
            HandleNoActiveRecipe();
            TryAcquireFuelForRecipe();
            HandleNoFireState(dt);
            ProcessSmelting(dt);
            OnBeforeReplaceFurnace(dt);
            int cellValue = m_componentBlockEntity.BlockValue;
            ReplaceFurnace(cellValue);
            OnEndUpdate(dt);
        }

        protected virtual void OnBeginUpdate(float dt) {
        }

        protected virtual void UpdateFireState(float dt) {
            if (m_heatLevel > 0f) {
                m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
                if (m_fireTimeRemaining == 0f) {
                    m_heatLevel = 0f;
                }
            }
        }

        protected virtual void HandleNoActiveRecipe() {
            if (m_smeltingRecipe == null && StopFuelWhenNoRecipeIsActive) {
                StopSmelting(true);
            }
        }

        protected virtual void TryAcquireFuelForRecipe() {
            if (m_smeltingRecipe != null && m_fireTimeRemaining <= 0f) {
                TryAcquireFuel();
            }
        }

        protected virtual bool TryAcquireFuel() {
            return UseFuel();
        }

        protected virtual void HandleNoFireState(float dt) {
            if (m_fireTimeRemaining <= 0f) {
                m_smeltingRecipe = null;
                m_smeltingProgress = MathUtils.Max(0f, m_smeltingProgress - dt * SmeltProgressReductionSpeed);
            }
        }

        protected virtual void ProcessSmelting(float dt) {
            if (m_smeltingRecipe == null) {
                return;
            }
            m_smeltingProgress = MathUtils.Min(m_smeltingProgress + (SmeltSpeed * dt), 1f);
            if (m_smeltingProgress >= 1f) {
                ConsumeIngredientsAndCreateResult();
            }
        }

        protected virtual void ConsumeIngredientsAndCreateResult() {
            OriginalSmeltingRecipe recipe = m_smeltingRecipe;
            int outputBlockValue = recipe.ResultValue;
            int producedCount = recipe.ResultCount;
            if (outputBlockValue != 0
                && producedCount > 0
                && !RecipaediaInterceptBus.TryProceed(new CrafterOutputProducingContext(
                    Project,
                    this,
                    FindInteractingPlayer(),
                    recipe,
                    outputBlockValue,
                    producedCount,
                    CrafterKind.Furnace))) {
                return;
            }
            for (int i = 0; i < m_furnaceSize; i++) {
                if (m_slots[i].Count > 0) {
                    m_slots[i].Count--;
                }
            }
            m_slots[ResultSlotIndex].Value = recipe.ResultValue;
            m_slots[ResultSlotIndex].Count += recipe.ResultCount;
            if (recipe.RemainsValue != 0 && recipe.RemainsCount > 0) {
                m_slots[RemainsSlotIndex].Value = recipe.RemainsValue;
                m_slots[RemainsSlotIndex].Count += recipe.RemainsCount;
            }
            m_smeltingRecipe = null;
            m_smeltingProgress = 0f;
            m_updateSmeltingRecipe = true;
            if (outputBlockValue != 0 && producedCount > 0) {
                RecipaediaEventBus.GetPublisher<CrafterOutputProducedEvent>().Publish(
                    new CrafterOutputProducedEvent(
                        Project,
                        this,
                        FindInteractingPlayer(),
                        recipe,
                        outputBlockValue,
                        producedCount,
                        CrafterKind.Furnace));
            }
        }

        protected virtual void OnBeforeReplaceFurnace(float dt) {
        }

        protected virtual void OnEndUpdate(float dt) {
        }

        /// <summary>
        /// 更新配方逻辑
        /// </summary>
        public override void UpdateSmeltingRecipe() {
            m_updateSmeltingRecipe = false;
            float heatLevel = GetHeatLevelForRecipeSearch();
            OriginalSmeltingRecipe craftingRecipe = FindSmeltingRecipe(heatLevel);
            if (craftingRecipe != m_smeltingRecipe) {
                OriginalSmeltingRecipe previousRecipe = m_smeltingRecipe;
                m_smeltingRecipe = (craftingRecipe != null && craftingRecipe.ResultValue != 0) ? craftingRecipe : null;
                m_smeltingProgress = 0f;
                RecipaediaEventBus.GetPublisher<SmeltingRecipeChangedEvent>().Publish(
                    new SmeltingRecipeChangedEvent(Project, this, FindInteractingPlayer(), previousRecipe, m_smeltingRecipe));
            }
        }

        protected virtual float GetHeatLevelForRecipeSearch() {
            if (m_heatLevel > 0f) {
                return m_heatLevel;
            }
            Slot slot = m_slots[FuelSlotIndex];
            if (slot.Count > 0) {
                int num = Terrain.ExtractContents(slot.Value);
                return BlocksManager.Blocks[num].GetFuelHeatLevel(slot.Value);
            }
            return 0f;
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
                    int fuelBlockValue = slot2.Value;
                    float fireDuration = block.GetFuelFireDuration(fuelBlockValue) * FuelTimeEfficiency;
                    float heatLevel = block.GetFuelHeatLevel(fuelBlockValue);
                    if (!RecipaediaInterceptBus.TryProceed(new FurnaceFuelConsumingContext(
                            Project,
                            this,
                            fuelBlockValue,
                            heatLevel,
                            fireDuration))) {
                        return false;
                    }
                    slot2.Count--;
                    m_fireTimeRemaining = fireDuration;
                    m_heatLevel = heatLevel;
                    RecipaediaEventBus.GetPublisher<FurnaceFuelUsedEvent>().Publish(
                        new FurnaceFuelUsedEvent(Project, this, fuelBlockValue, m_heatLevel, fireDuration));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 每帧同步熔炉外观：先更新火焰粒子，再写入方块值（子类通常只覆写 <see cref="ReplaceFurnaceBlock"/>）。
        /// </summary>
        public virtual void ReplaceFurnace(int cellValue) {
            UpdateFurnaceFireParticles(cellValue);
            ReplaceFurnaceBlock(cellValue);
        }

        /// <summary>
        /// 根据当前燃烧状态更新原版熔炉火焰粒子（与方块内容 ID 无关的工业炉可在子类中覆写为空或自定义）。
        /// </summary>
        protected virtual void UpdateFurnaceFireParticles(int cellValue) {
            if (m_heatLevel > 0f) {
                m_fireParticleSystem.m_position = m_componentBlockEntity.Position + new Vector3(0.5f, 0.2f, 0.5f);
                if (Terrain.ExtractContents(cellValue) == FurnaceBlock.Index) {
                    m_subsystemParticles.AddParticleSystem(m_fireParticleSystem);
                }
            }
            else if (Terrain.ExtractContents(cellValue) == LitFurnaceBlock.Index) {
                m_subsystemParticles.RemoveParticleSystem(m_fireParticleSystem);
            }
        }

        /// <summary>
        /// 将方块值替换为点燃/未点燃的原版熔炉方块。
        /// </summary>
        protected virtual void ReplaceFurnaceBlock(int cellValue) {
            m_componentBlockEntity.BlockValue = Terrain.ReplaceContents(cellValue, m_heatLevel > 0f ? LitFurnaceBlock.Index : FurnaceBlock.Index);
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
            int?[] actualIngredients = new int?[36];
            if (heatLevel > 0f) {
                for (int i = 0; i < m_furnaceSize; i++) {
                    int slotValue = GetSlotValue(i);
                    int num = Terrain.ExtractContents(slotValue);
                    int num2 = Terrain.ExtractData(slotValue);
                    if (GetSlotCount(i) > 0) {
                        Block block = BlocksManager.Blocks[num];
                        m_matchedIngredients[i] = block.GetCraftingId(slotValue) + ":" + num2.ToString(CultureInfo.InvariantCulture);
                        actualIngredients[i] = slotValue;
                    }
                    else {
                        m_matchedIngredients[i] = null;
                        actualIngredients[i] = null;
                    }
                }
                ComponentPlayer componentPlayer = FindInteractingPlayer();
                float playerLevel = componentPlayer?.PlayerData.Level ?? 1f;
                OriginalSmeltingRecipe actualSmeltingRecipe = new() { Ingredients = m_matchedIngredients, RequiredHeatLevel = heatLevel, RequiredPlayerLevel = playerLevel };
                actualSmeltingRecipe.SetExtraValue(RecipeExtraKeys.Project, Project);
                actualSmeltingRecipe.SetExtraValue<IInventory>(RecipeExtraKeys.Inventory, this);
                actualSmeltingRecipe.SetExtraValue(RecipeExtraKeys.ActualIngredients, actualIngredients);
                RecipeMatchResult matchResult = RecipeMatchPipeline.Resolve(actualSmeltingRecipe, CrafterMatchMode.Furnace, Project);
                OriginalSmeltingRecipe? craftingRecipe = !matchResult.IsHint && matchResult.Recipe is OriginalSmeltingRecipe productiveRecipe
                    ? productiveRecipe
                    : null;
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
                if (matchResult.IsHint || !string.IsNullOrEmpty(matchResult.Recipe?.Message)) {
                    CrafterHints.TryShow(componentPlayer, matchResult.Recipe);
                }
                return craftingRecipe;
            }
            return null;
        }
    }
}