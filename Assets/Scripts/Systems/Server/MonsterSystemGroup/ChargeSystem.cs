using Aspect;
using Component;
using Systems.Server.RoundSystemGroup;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Server.MonsterSystemGroup {
    /// <summary>
    /// 怪物的冲锋行为 依赖ChargeComponent
    /// </summary>
    [UpdateInGroup(typeof(MonsterBehaviorGroup))]
    [UpdateAfter(typeof(SearchingTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ChargeSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            var monsterQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, ChargeComponent, MonsterComponent>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build();
            state.RequireForUpdate(monsterQuery);
            state.RequireForUpdate<RoundData>();
        }


        /// <summary>
        ///     暂时是否使用异步只是基于可能激活此系统的怪物数量
        /// </summary>
        /// <param name="state"></param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (monsterAspect, chargeComponent, entity)
                     in SystemAPI.Query<MonsterAspectWithHealthRW, RefRW<ChargeComponent>>()
                         .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
                         .WithEntityAccess()) {
                var roundData = SystemAPI.GetSingleton<RoundData>();
                if (roundData.Phase != RoundPhase.Combat) return; //不在战斗阶段

                if (monsterAspect.HealthComponent.ValueRO.IsDead) continue; //怪物死亡


                ref var chargeDataRW = ref chargeComponent.ValueRW;
                ref var cd = ref chargeDataRW.coolDownData;

                var isCharging = state.EntityManager.IsComponentEnabled<ChargeComponent>(entity);
                if (!isCharging) {
                    if (!cd.IsCoolDownReadyWithBaseCd(SystemAPI.Time.ElapsedTime)) continue; //冷却时间未到
                    var rangeSq = chargeDataRW.chargeRange * chargeDataRW.chargeRange;
                    if (monsterAspect.Monster.ValueRO.targetDistanceSq > rangeSq) continue; //超出范围

                    //如果没有激活冲锋则初始化冲锋数据
                    cd.TriggerCoolDown(SystemAPI.Time.ElapsedTime); //触发冷却
                    chargeDataRW.direction = monsterAspect.Monster.ValueRO.targetPlayerDirNormalized;
                    EnableCharge(ref state, entity, true);
                    monsterAspect.LocalTransform.ValueRW.Rotation =
                        quaternion.LookRotationSafe(math.forward(), chargeDataRW.direction);
                }

                //冲锋状态移动
                monsterAspect.LocalTransform.ValueRW.Position +=
                    chargeDataRW.direction * chargeDataRW.speed * SystemAPI.Time.DeltaTime;

                //如果未超过冲锋时间则不进行处理
                if (SystemAPI.Time.ElapsedTime - cd.TriggerTime < chargeDataRW.chargeTotalTime) continue;
                //超过冲锋时间则停止冲锋
                EnableCharge(ref state, entity, false);
            }
        }


        private void EnableCharge(ref SystemState state, Entity entity, bool enable) {
            state.EntityManager.SetComponentEnabled<ChargeComponent>(entity, enable);
            state.EntityManager.SetComponentEnabled<ChaseComponent>(entity, !enable);
        }
    }
}