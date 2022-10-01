using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UnboundLib;
using System.Reflection.Emit;
using System.Reflection;

namespace CardsPlusPlugin.Utils
{
    public class CustomHealthBar : MonoBehaviour
    {
        private HealthBar healthBar;

        /// <summary>
        /// Current health value, clamped to the range <c>[0, <see cref="MaxHealth"/>]</c>
        /// </summary>
        public float CurrentHealth { get => _currentHealth; set => SetCurrentHealth(value); }
        private float _currentHealth = 100;

        /// <summary>
        /// Max health value, clamped to the range <c>[0, <see cref="float.PositiveInfinity"/>]</c>
        /// </summary>
        public float MaxHealth { get => _maxHealth; set => SetMaxHealth(value); }
        private float _maxHealth = 100;

        /// <summary>
        /// Override the values of <c>CurrentHealth</c> and <c>MaxHealth</c>
        /// </summary>
        /// <param name="currentHealth"></param>
        /// <param name="maxHealth"></param>
        public void SetValues(float currentHealth, float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        private void Awake()
        {
            healthBar = Instantiate(Assets.BaseHealthBar.gameObject, transform).GetComponent<HealthBar>();
        }

        private void Start()
        {
            healthBar.transform.Find("Canvas/PlayerName").gameObject.SetActive(false);
        }

        private void SetCurrentHealth(float value)
        {
            _currentHealth = Math.Max(0, Math.Min(MaxHealth, value));
            UpdateHealthBar();
        }

        private void SetMaxHealth(float value)
        {
            _maxHealth = Math.Max(0, value);
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            healthBar.TakeDamage(Vector2.zero, false);
        }

        /// <summary>
        /// Returns the underlying <c>HealthBar</c> managed by this instance
        /// </summary>
        public HealthBar GetBaseHealthBar()
        {
            return healthBar;
        }
    }

    [HarmonyPatch(typeof(HealthBar))]
    public static class HealthBarPatches
    {
        public static bool CustomHealthBarExists(GameObject healthBar)
        {
            var customHealthBar = healthBar.GetComponentInParent<CustomHealthBar>();
            return customHealthBar != null;
        }

        public static float HealthBarCalculatePercentageOverride(GameObject healthBar)
        {
            var baseHealthBar = healthBar.GetComponent<HealthBar>();
            if (baseHealthBar == null) return -1;

            var customHealthBar = healthBar.GetComponentInParent<CustomHealthBar>();
            if (customHealthBar == null)
            {
                var data = (CharacterData)baseHealthBar.GetFieldValue("data");
                return data.health / data.maxHealth;
            }

            return customHealthBar.CurrentHealth / customHealthBar.MaxHealth;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("Update")]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            FieldInfo hpTargField = AccessTools.Field(typeof(HealthBar), "hpTarg");
            FieldInfo characterDataField = AccessTools.Field(typeof(HealthBar), "data");

            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < code.Count; i++)
            {
                var currentInstruction = code[i];

                // /* 0x0002444A */ IL_0002: ldfld     class CharacterData HealthBar::data	// Finds the value of a field in the object
                if (startIndex < 0 && currentInstruction.opcode == OpCodes.Ldfld && currentInstruction.LoadsField(characterDataField))
                    startIndex = i;

                // /* 0x00024460 */ IL_0018: stfld     float32 HealthBar::hpTarg	// Replaces the value stored in the field of an object
                if (endIndex < 0 && currentInstruction.opcode == OpCodes.Stfld && currentInstruction.StoresField(hpTargField))
                    endIndex = i;
            }

            if (startIndex < 0 || endIndex < 0)
            {
                CardsPlus.GetLogger().LogError($"[HealthBar] Update transpiler unable to find code block to replace");
                return code;
            }

            code.RemoveRange(startIndex, (endIndex - startIndex) + 1);
            code.InsertRange(startIndex, new List<CodeInstruction>
            {
                // this.hpTarg = HealthBarPatches.HealthBarCalculatePercentageOverride(base.gameObject);
                CodeInstruction.Call(typeof(UnityEngine.Component), "get_gameObject"),
                CodeInstruction.Call(typeof(HealthBarPatches), nameof(HealthBarPatches.HealthBarCalculatePercentageOverride), parameters: new []{ typeof(GameObject) }),
                CodeInstruction.StoreField(typeof(HealthBar), "hpTarg")
            });

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("Start")]
        static IEnumerable<CodeInstruction> StartTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var code = new List<CodeInstruction>(instructions);

            Label continueLabel = generator.DefineLabel();
            MethodInfo getCharacterStatModifiersInParentMethod = AccessTools.Method(typeof(UnityEngine.Component), "GetComponentInParent", generics: new[] { typeof(CharacterStatModifiers) });

            int insertIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                // /* 0x0000000D */ IL_000D: call      instance !!0 [UnityEngine.CoreModule]UnityEngine.Component::GetComponentInParent<class CharacterStatModifiers>()	// Calls the method indicated by the passed method descriptor.
                if (insertIndex < 0 && code[i].opcode == OpCodes.Ldarg_0 && code[i + 1].Calls(getCharacterStatModifiersInParentMethod))
                {
                    insertIndex = i;
                    code[i].WithLabels(continueLabel);
                    break;
                }
            }

            if (insertIndex == -1)
            {
                CardsPlus.GetLogger().LogError($"[HealthBar] Start transpiler unable to find call to 'GetComponentInParent<CharacterStatModifiers>'");
                return code;
            }

            code.InsertRange(insertIndex, new List<CodeInstruction>
            {
                // call `HealthBarPatches.CustomHealthBarExists(base.gameObject)
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(UnityEngine.Component), "get_gameObject"),
                CodeInstruction.Call(typeof(HealthBarPatches), nameof(HealthBarPatches.CustomHealthBarExists), parameters: new[] { typeof(GameObject) }),

                // continue if returned false, otherwise exit method
                new CodeInstruction(OpCodes.Brfalse_S, continueLabel),
                new CodeInstruction(OpCodes.Ret)
            });

            return code;
        }
    }
}
