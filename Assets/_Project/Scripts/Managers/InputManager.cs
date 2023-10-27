using UnityEngine;

public class InputManager
{
    #region Structures
    public const float defaultAxisDeadZone = 0.1f;
    public enum AxisMultiPref { Sum, Largest }
    public enum ButtonState { None = 0, Down, Held, Up }

    [System.Serializable]
    public class Axis
    {
        public AxisCode axis;
        public float deadzone;
        [SerializeField] bool negative;

        ButtonState state = ButtonState.None;
        float lastValue = 0;
        int lastFrame = 0;

        public Axis(AxisCode axis, float deadzone, bool negative)
        {
            this.axis = axis;
            this.deadzone = negative ? Mathf.Abs(deadzone) * -1 : Mathf.Abs(deadzone);
            this.negative = negative;
        }

        void updateState(float currentValue)
        {
            bool isOverDeadzone = negative ?
                currentValue < deadzone :
                currentValue > deadzone;

            switch (state)
            {
                case ButtonState.None:
                    if (isOverDeadzone)
                        state = ButtonState.Down;
                    break;
                case ButtonState.Down:
                    if (isOverDeadzone)
                        state = ButtonState.Held;
                    else
                        state = ButtonState.Up;
                    break;
                case ButtonState.Held:
                    if (!isOverDeadzone)
                        state = ButtonState.Up;
                    break;
                case ButtonState.Up:
                    if (isOverDeadzone)
                        state = ButtonState.Down;
                    else
                        state = ButtonState.None;
                    break;
            }
        }
        /// <summary>
        /// Checks to see if the value has changed
        /// </summary>
        /// <returns>True if lastValue has changed</returns>
        void readValue()
        {
            float currentValue = InputX.Axis(axis);

            //If this is a different frame, check for state changes
            if (lastFrame != Time.frameCount)
                updateState(currentValue);

            lastValue = currentValue;
            lastFrame = Time.frameCount;
        }
        public float getValue()
        {
            readValue();
            return lastValue;
        }
        public bool up()
        {
            readValue();
            return state == ButtonState.Up;
        }
        public bool down()
        {
            readValue();
            return state == ButtonState.Down;
        }
        public bool held()
        {
            readValue();
            return state == ButtonState.Held;
        }
    }
    [System.Serializable]
    public class InputSet
    {
        public string name;
        public InputCode[] buttons;
        public Axis[] axises;

        public InputSet(string name, InputCode[] buttons, Axis[] axises = null)
        {
            //Setup name
            this.name = name;

            //Setup Buttons
            if (buttons == null)
                this.buttons = new InputCode[0] { };
            else
                this.buttons = buttons;

            //Setup Axises
            if (axises == null)
                this.axises = new Axis[0] { };
            else
                this.axises = axises;
        }

        public bool down()
        {
            //Check all inputcodes - if even 1 of them is down, return true
            foreach (InputCode ic in buttons)
            {
                if (InputX.Down(ic))
                    return true;
            }

            //Check all axises - if even 1 of them is down, return true
            foreach (Axis ax in axises)
            {
                if (ax.down())
                    return true;
            }

            //No buttons or axises are down this frame
            return false;
        }
        public bool up()
        {
            //Check all inputcodes - if even 1 of them is down, return true
            foreach (InputCode ic in buttons)
            {
                if (InputX.Up(ic))
                    return true;
            }

            //Check all axises - if even 1 of them is down, return true
            foreach (Axis ax in axises)
            {
                if (ax.up())
                    return true;
            }

            //No buttons or axises are up this frame
            return false;
        }
        public bool held()
        {
            //Check all inputcodes
            foreach (InputCode ic in buttons)
            {
                if (InputX.Pressed(ic))
                    return true;
            }

            //Check all axises
            foreach (Axis ax in axises)
            {
                if (ax.held())
                    return true;
            }

            return false;
        }
        public float axisValue(AxisMultiPref pref = AxisMultiPref.Sum)
        {
            //Default return value
            float ret = 0;

            //Check all axises
            foreach (Axis ax in axises)
            {
                //Fetch the axis value
                float axisReading = ax.getValue();

                //Decide what to do with it
                if (Mathf.Abs(axisReading) > Mathf.Abs(ax.deadzone))
                {
                    if (pref == AxisMultiPref.Sum)
                    {
                        ret += axisReading;
                    }
                    else
                    {
                        if (Mathf.Abs(axisReading) > Mathf.Abs(ret))
                            ret = axisReading;
                    }
                }
            }

            return ret;
        }
    }
    #endregion
    #region Key Definitions
    //Movement
    public static InputSet moveUp;
    public static InputSet moveDown;
    public static InputSet moveLeft;
    public static InputSet moveRight;
    public static InputSet sneak;
    public static InputSet dash;

    //Actions
    public static InputSet firePrimary;
    public static InputSet useThrowingWeapon;
    public static InputSet nextWeapon;
    public static InputSet reload;
    public static InputSet interact;
    public static InputSet activateAbility;
    public static InputSet useConsumable;

    //Quickslots
    public static InputSet weaponQuickslot;
    public static InputSet throwableQuickslot;
    public static InputSet consumableQuickslot;
    public static InputSet abilityQuickslot;

    //Other
    public static InputSet showLog;
    #endregion


    //Save/Load Methods
    public static void ResetToDefault()
    {
        //Movement
        moveUp = new InputSet("Move Up",
            null,
            new Axis[2] {
                new Axis(AxisCode.KeyboardWS, 0, true),
                new Axis(AxisCode.JoystickY, 0.1f, true)});
        moveDown = new InputSet("Move Down",
            null,
            new Axis[2] {
                new Axis(AxisCode.KeyboardWS, 0, false),
                new Axis(AxisCode.JoystickY, 0.1f, false)});
        moveLeft = new InputSet("Move Left",
            null,
            new Axis[2] {
                new Axis(AxisCode.KeyboardAD, 0, true),
                new Axis(AxisCode.JoystickX, 0.1f, true)});
        moveRight = new InputSet("Move Right",
            null,
            new Axis[2] {
                new Axis(AxisCode.KeyboardAD, 0, false),
                new Axis(AxisCode.JoystickX, 0.1f, false)});
        sneak = new InputSet("Sneak",
            new InputCode[2] { InputCode.LeftControl, InputCode.XboxLeftBumper });


        //Actions
        firePrimary = new InputSet("Primary Fire",
            new InputCode[1] { InputCode.MouseClickLeft },
            new Axis[1] { new Axis(AxisCode.Joystick3, 0, true) });  //Right Trigger
        useThrowingWeapon = new InputSet("Use Throwing Weapon",
            new InputCode[2] { InputCode.G, InputCode.XboxButtonX });
        dash = new InputSet("Dash",
            new InputCode[2] { InputCode.LeftShift, InputCode.XboxRightBumper });
        activateAbility = new InputSet("Use Ability",
            new InputCode[1] { InputCode.MouseClickRight },
            new Axis[1] { new Axis(AxisCode.Joystick3, 0, false) });  //Left Trigger
        reload = new InputSet("Reload",
            new InputCode[2] { InputCode.R, InputCode.XboxButtonB });
        interact = new InputSet( "Interact",
            new InputCode[2] { InputCode.E, InputCode.XboxButtonA });
        useConsumable = new InputSet("Use Consumable",
            new InputCode[2] { InputCode.F, InputCode.XboxButtonY });

        //Quickslots
        weaponQuickslot = new InputSet("Change Weapon",
            new InputCode[1] { InputCode.Alpha1 },
            new Axis[1] { new Axis(AxisCode.Joystick6, 0, false) });
        consumableQuickslot = new InputSet("Change Consumable",
            new InputCode[1] { InputCode.Alpha2 },
            new Axis[1] { new Axis(AxisCode.Joystick7, 0, true) });
        throwableQuickslot = new InputSet("Change Throwable",
            new InputCode[1] { InputCode.Alpha3 },
            new Axis[1] { new Axis(AxisCode.Joystick6, 0, true) });
        abilityQuickslot = new InputSet("Change Ability",
            new InputCode[1] { InputCode.Alpha4 },
            new Axis[1] { new Axis(AxisCode.Joystick7, 0, false) });

        //Other
        showLog = new InputSet("Show Log",
            new InputCode[2] { InputCode.CapsLock, InputCode.XboxBackButton });
    }
    public static void SaveConfig()
    {
        //TODO: Save to playerPrefs
    }
    public static void LoadConfig()
    {
        //TODO: Try to load input from playerPrefs, or setup defaults if that fails
        ResetToDefault();
    }
}