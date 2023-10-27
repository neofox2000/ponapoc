using UnityEngine;
//using Fungus;

public class FlowchartTrigger : MonoTrigger
{
    [Tooltip("Flowchart to use")]
    public Fungus.Flowchart flowchart;
    [Tooltip("Block to run in selected flowchart")]
    public string block;

    protected override void fire()
    {
        base.fire();

        Fungus.Flowchart flowchartToUse = flowchart;
        if (!flowchartToUse)
            flowchartToUse = GetComponentInParent<Fungus.Flowchart>();

        if (flowchartToUse)
            flowchartToUse.ExecuteBlock(block);
        else
            Debug.LogWarning("FlowchartTrigger could not find a valid Flowchart!");
    }
}