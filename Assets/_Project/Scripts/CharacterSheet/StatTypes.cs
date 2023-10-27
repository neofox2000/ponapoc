namespace RPGData
{
    public enum StatTypes
    {
        //Basic types
        hP = 10,
        hP_Max = 12,
        hP_Regen = 14,
        mP = 20,
        mP_Max = 22,
        mP_Regen = 24,
        sP = 30,
        sP_Max = 32,
        sP_Regen = 34,
        aP = 40,
        aP_Max = 42,
        aP_Regen = 44,
        cP = 50,
        cP_Max = 52,
        cP_Regen = 54,

        //Item types
        itemStatAffect = 100,
        itemDamage = 101,
        itemCritRate = 102,
        itemProtection = 103,

        itemCraftQuantity = 110,
        itemCraftMaterialCost = 111,
        itemDeconstructMaterialReturn = 112,

        //Combat types (defense)
        unarmedDamagePhysical = 150,
        unarmedDamageChemical = 151,
        unarmedDamageEnergy = 153,
        unarmedDamageBiological = 154,
        itemDamagePhysical = 155,
        itemDamageChemical = 156,
        itemDamageEnergy = 157,
        itemDamageBiological = 158,

        physicalProtection = 160,
        chemicalProtection = 161,
        energyProtection = 162,
        biologicalProtection = 163,
        itemPhysicalProtection = 164,
        itemChemicalProtection = 165,
        itemEnergyProtection = 166,
        itemBiologicalProtection = 167,

        staggerChance = 190,

        //Movement types
        speed = 200,
        //runSpeed = 210,
        //sneakSpeed = 220,
        dashCost = 230,
        detectionRange = 240,
        stealthRange = 250,

        //Misc types
        carryWeight = 300,
        haggleAttempts = 310,
        haggleSuccessRate = 311,
        lockpickSuccessRate = 320,
        hackingSuccessRate = 330,
        skillPointsPerLevel,
    }
}