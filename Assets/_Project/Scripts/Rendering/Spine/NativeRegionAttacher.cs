using UnityEngine;
using Spine.Unity;
using Spine;

public class NativeRegionAttacher : MonoBehaviour
{
    //Atlas atlas;
    //AtlasAttachmentLoader loader;

    SkeletonRenderer skeletonRenderer = null;

    public void Awake()
    {
        skeletonRenderer = GetComponentInChildren<SkeletonRenderer>();
    }

    public void Apply(string slotName, string region)
    {
        Slot slot = skeletonRenderer.skeleton.FindSlot(slotName);

        if (slot != null)
        {
            //Load a native attachment
            try
            {
                if (region != "")
                    skeletonRenderer.skeleton.SetAttachment(slotName, region);
                else
                    skeletonRenderer.skeleton.SetAttachment(slotName, null);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error while loading native attachment: " + e.Message);
            }
        }
        else
            Debug.LogError("Spine Slot missing!");
    }
    public string getRegion(string slotName)
    {
        Slot slot = skeletonRenderer.skeleton.FindSlot(slotName);
        if (slot != null)
            return slot.Data.AttachmentName;
        
        return string.Empty;
    }

    /*
    public void Apply(SkeletonRenderer skeletonRenderer)
    {
        Slot slot = skeletonRenderer.skeleton.FindSlot(this.slot);

        if (slot != null)
        {
            if (atlasID < 0)
            {
                //Load a native attachment
                try
                {
                    if (region != "")
                        skeletonRenderer.skeleton.SetAttachment(this.slot, region);
                    else
                        skeletonRenderer.skeleton.SetAttachment(this.slot, null);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error while loading native attachment: " + e.Message);
                }
            }
            else
            {
                //Load an attachment from a different atlas
                atlas = SpineAtlasBank.instance.spineAtlasBank[atlasID].GetAtlas();
                loader = new AtlasAttachmentLoader(atlas);

                float scaleMultiplier = skeletonRenderer.skeletonDataAsset.scale;

                if (region != "")
                {
                    try
                    {
                        RegionAttachment regionAttachment = loader.NewRegionAttachment(null, region, region);
                        regionAttachment.Width = regionAttachment.RegionOriginalWidth * scaleMultiplier;
                        regionAttachment.Height = regionAttachment.RegionOriginalHeight * scaleMultiplier;

                        //regionAttachment.SetColor(new Color(1, 1, 1, 1));
                        regionAttachment.UpdateOffset();

                        slot.Attachment = regionAttachment;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Error while loading atlas attachment: " + e.Message);
                    }
                }
                else
                    slot.Attachment = null;
            }
        }
        else
            Debug.LogError("Spine Slot missing!");
    }
    */
}
