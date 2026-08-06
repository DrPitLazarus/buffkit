using BuffKit.UI;
using HarmonyLib;
using UnityEngine;

namespace BuffKit.LoadoutDragAndDrop;

[HarmonyPatch]
internal class LoadoutDragAndDrop : MonoBehaviour
{
    private static GameObject[] _skillContainers;

    [HarmonyPatch(typeof(UIManager.UINewCharacterState), nameof(UIManager.UINewCharacterState.Enter))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        // Original code by trgk in feature/loadout-dnd. Pit tested and fixed slot 5 bug.
        _skillContainers ??=
            [
                UIPageFrame.Instance.characterCustomizer.pilotSkillContainer,
                UIPageFrame.Instance.characterCustomizer.engineerSkillContainer,
                UIPageFrame.Instance.characterCustomizer.gunnerSkillContainer,
                UIPageFrame.Instance.characterCustomizer.specialSkillContainer
            ];

        foreach (var skillGroup in _skillContainers)
        {
            var handler = skillGroup.GetComponent<DropHandler>();
            if (handler != null) continue;

            handler = skillGroup.AddComponent<DropHandler>();

            handler.OnDropped += (previous, current) =>
            {
                // Get the lowest slot number of this container and use it as first slot number.
                var slotOffset = int.MaxValue;
                for (int i = 0; i < current.Count; i++)
                {
                    var slot = current.Values[i].GetComponent<UICharacterSkillSlot>();
                    if (slot.Skill.Slot < slotOffset) slotOffset = slot.Skill.Slot;
                }
                //MuseLog.Info($"SlotOffset is {slotOffset}");
                // Set the new slot numbers and mark them as changed.
                for (int i = 0; i < current.Count; i++)
                {
                    var newSlotNumber = slotOffset + i;
                    // Skip slot 5, it is the PvE ability slot and not gunner 2nd engineer slot.
                    // Pilot: Piloting 0 1 2, Gunnery 3, Engineering 4
                    // Gunner: Gunnery 0 1 2, Piloting 3, Engineering 4 6 <-- SIX!
                    // Engineer: Engineering 0 1 2, Piloting 3, Gunnery 4
                    if (i > 0 && newSlotNumber >= 5) newSlotNumber++;
                    var slot = current.Values[i].GetComponent<UICharacterSkillSlot>();
                    slot.Skill.Slot = newSlotNumber;
                    slot.Skill.Changed = true;
                    //MuseLog.Info($"Index {i} is {slot.Skill.SimpleString}");
                }
            };
        }
    }
}
