using Component;
using Unity.Entities;
using UnityEngine;

namespace Authoring.Weapon {
    public class ProjectileWeaponAuthoring : WeaponBaseAuthoring {
        [SerializeField] private WeaponProjectile weaponProjectile;
        [SerializeField] private GameObject projectilePrefab;

        private class Baker : Baker<ProjectileWeaponAuthoring> {
            public override void Bake(ProjectileWeaponAuthoring authoring) {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                var projectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic);
                authoring.weaponProjectile.ProjectilePrefab = projectilePrefab;
                AddComponent(entity, authoring.weaponComponent);
                AddComponent(entity, authoring.weaponProjectile);
                  AddComponent<WeaponMounted>(entity);
                AddBuffer<ProjectileShootingEvent>(entity);
            }
        }
    }
}