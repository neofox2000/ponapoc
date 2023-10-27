using UnityEngine;

namespace Fungus
{
    [CommandInfo("Custom",
                 "Spawn Enemies",
                 "Spawns one or more enemies and optionally executes a block once they have been defeated")]
    [AddComponentMenu("")]
    public class SpawnEnemies : Call
    {
        [Tooltip("Character that is speaking")]
        [SerializeField] protected EnemyGroup enemyGroup;

        public override void OnEnter()
        {
            if (enemyGroup)
                enemyGroup.SpawnEnemy(delegate { base.OnEnter(); });
            else
                Debug.LogWarning(name + " flowchart block has no enemy spawner - skipping");

            Continue();
        }

        public override string GetSummary()
        {
            string summary = "";

            if(enemyGroup != null)
            {
                summary = "spawn " + enemyGroup.name;
            }

            if (targetBlock != null)
            {
                summary += " then call " + targetBlock.BlockName + " when they die(?)";
            }

            return summary;
        }
    }
}