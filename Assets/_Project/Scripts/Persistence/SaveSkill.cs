using RPGData;

namespace RPGData
{
    [System.Serializable]
    public class SaveSkill
    {
        public int ID;
        public float value;

        public SaveSkill()
        {
            ID = 0;
            value = 0;
        }
        public SaveSkill(Attribute saveMe)
        {
            ID = saveMe.template.saveID;
            value = saveMe.valueBase;
        }
        public void unpack(Attribute unpackToMe)
        {
            unpackToMe.valueBase = value;
        }
    }
}