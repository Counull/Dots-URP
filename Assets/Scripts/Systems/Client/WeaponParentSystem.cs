using Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Systems.Client {
    /// <summary>
    /// 用于在客户端为武器添加父级
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct WeaponParentSystem : ISystem {
        EntityQuery _weaponQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _weaponQuery = SystemAPI.QueryBuilder().WithAll<WeaponComponent>().WithNone<Parent>().Build();
            state.RequireForUpdate(_weaponQuery);
            state.RequireForUpdate<WeaponMounted>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            using var weaponEntities = _weaponQuery.ToEntityArray(Allocator.Temp);
            foreach (var weaponEntity in weaponEntities) {
                var targetEntity = state.EntityManager.GetComponentData<WeaponMounted>(weaponEntity).PlayerEntity;
                state.EntityManager.AddComponentData(weaponEntity, new Parent {Value = targetEntity});
            }

         
        }
    }
}