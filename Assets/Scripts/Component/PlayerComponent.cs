using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Component {
    [Serializable]
    public struct ExperienceGrowthFormula {
        public float baseExperience; // A
        public float growthRate; // b

        private static double CalculateExperience(int level, ExperienceGrowthFormula formula) {
            return formula.baseExperience * Math.Pow(formula.growthRate, level);
        }
    }


    [Serializable]
    public struct PlayerAttributes : IComponentData {
        public float speed;
        public uint maxWeaponCount;
    }


    [GhostComponent]
    public struct PlayerComponent : IComponentData {
        public float Experience;
        public uint Level;
        public ulong NextLevelExperience;
        public PlayerAttributes InGameAttributes;
        public PlayerAttributes BaseAttributes;
    }


    [GhostComponent]
    public struct PlayerInput : IInputComponentData {
        [GhostField] public float Horizontal;
        [GhostField] public float Vertical;
    }

    public class InputActions : IComponentData {
        public InputAction MoveAction;
    }

    public struct CameraFollowed : IComponentData { }

    public class PlayerVisualizationComponent : IComponentData {
        public Animator Animator;
        public GameObject PlayerVisualizationPrefab;
        public GameObject VisualizationInstance;

        /// <summary>
        ///     有点纠结这个函数的位置，感觉应该放在PlayerManagedComponent里但是这个类是个组件
        ///     《重构》里的典型坏代码
        ///     还是放在这了
        /// </summary>
        public void CreatePlayerVisualizationInstance() {
            //实例化可视化对象
            VisualizationInstance = Object.Instantiate(PlayerVisualizationPrefab);
            Animator = VisualizationInstance.GetComponent<Animator>();
        }
    }
}