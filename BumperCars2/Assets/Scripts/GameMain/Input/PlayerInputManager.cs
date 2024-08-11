using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerInputManager : GameModule
    {
        //输入轴设置
        public float motorDeltaP1 { get; private set; }
        public float motorDeltaP2 { get; private set; }
        public float steeringDeltaP1 { get; private set; }
        public float steeringDeltaP2 { get; private set; }
        public bool handbrakeP1 { get; private set; }
        public bool handbrakeP2 { get; private set; }
        public bool boostP1 { get; private set; }
        public bool boostP2 { get; private set; }
        
        public bool switchP1 { get; private set; }
        public bool switchP2 { get; private set; }
        public bool skill1P1 { get; private set; }
        public bool skill1P2 { get; private set; }
        public bool skill2P1 { get; private set; }
        public bool skill2P2 { get; private set; }
        public bool skill3P1 { get; private set; }
        public bool skill3P2 { get; private set; }

        public enum InputType
        {
            GetAxisRaw,
            GetButtonDown,
        }
        
        private struct InputSetting
        {
            public string Name;
            public bool Invert;
            public InputType inputType;
        }

        public bool enableInput = true;

        //P1输入配置
        private static InputSetting forwardInputP1;
        private static InputSetting forwardInputP2;
        private static InputSetting reverseInputP1;
        private static InputSetting reverseInputP2;
        private static InputSetting steerRightInputP1;
        private static InputSetting steerRightInputP2;
        private static InputSetting steerLeftInputP1;
        private static InputSetting steerLeftInputP2;
        private static InputSetting fire1InputP1;
        private static InputSetting fire1InputP2;
        private static InputSetting fire2InputP1;
        private static InputSetting fire2InputP2;
        private static InputSetting fire3InputP1;
        private static InputSetting fire3InputP2;
        private static InputSetting handbrakeInputP1;
        private static InputSetting handbrakeInputP2;
        private static InputSetting boostInputP1;
        private static InputSetting boostInputP2;
        private static InputSetting switchInputP1;
        private static InputSetting switchInputP2;

        public void Initialize()
        {
            forwardInputP1 = new InputSetting { Name = "Vertical_P1", Invert = false, inputType = InputType.GetAxisRaw};
            forwardInputP2 = new InputSetting { Name = "Vertical_P2", Invert = false, inputType = InputType.GetAxisRaw};
            reverseInputP1 = new InputSetting { Name = "Vertical_P1", Invert = true , inputType = InputType.GetAxisRaw};
            reverseInputP2 = new InputSetting { Name = "Vertical_P2", Invert = true , inputType = InputType.GetAxisRaw};
            steerRightInputP1 = new InputSetting { Name = "Horizontal_P1",Invert = false, inputType = InputType.GetAxisRaw};
            steerRightInputP2 = new InputSetting { Name = "Horizontal_P2",Invert = false, inputType = InputType.GetAxisRaw};
            steerLeftInputP1 = new InputSetting { Name = "Horizontal_P1",Invert = true, inputType = InputType.GetAxisRaw };
            steerLeftInputP2 = new InputSetting { Name = "Horizontal_P2",Invert = true, inputType = InputType.GetAxisRaw };
            fire1InputP1 = new InputSetting { Name = "Fire1_P1", Invert = false , inputType = InputType.GetButtonDown};
            fire1InputP2 = new InputSetting { Name = "Fire1_P2", Invert = false , inputType = InputType.GetButtonDown};
            fire2InputP1 = new InputSetting { Name = "Fire2_P1", Invert = false , inputType = InputType.GetButtonDown};
            fire2InputP2 = new InputSetting { Name = "Fire2_P2", Invert = false , inputType = InputType.GetButtonDown};
            fire3InputP1 = new InputSetting { Name = "Fire3_P1", Invert = false , inputType = InputType.GetButtonDown};
            fire3InputP2 = new InputSetting { Name = "Fire3_P2", Invert = false , inputType = InputType.GetButtonDown};
            handbrakeInputP1 = new InputSetting { Name = "Handbrake_P1", Invert = false , inputType = InputType.GetAxisRaw};
            handbrakeInputP2 = new InputSetting { Name = "Handbrake_P2", Invert = false , inputType = InputType.GetAxisRaw};
            boostInputP1 = new InputSetting { Name = "Boost_P1", Invert = false , inputType = InputType.GetButtonDown};
            boostInputP2 = new InputSetting { Name = "Boost_P2", Invert = false , inputType = InputType.GetButtonDown};
            switchInputP1 = new InputSetting { Name = "Switch_P1", Invert = false , inputType = InputType.GetButtonDown};
            switchInputP2 = new InputSetting { Name = "Switch_P2", Invert = false , inputType = InputType.GetButtonDown};
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (enableInput)
            {
                motorDeltaP1 = GetInput(forwardInputP1) - GetInput(reverseInputP1);
                steeringDeltaP1 = GetInput(steerRightInputP1) - GetInput(steerLeftInputP1);
                handbrakeP1 = GetInput(handbrakeInputP1) >= 0.5f;
                boostP1 = GetInput(boostInputP1) >= 0.5f;
                switchP1 = GetInput(switchInputP1)>=0.5f;
                skill1P1 = GetInput(fire1InputP1) >= 0.5f;
                skill2P1 = GetInput(fire2InputP1) >= 0.5f;
                skill3P1 = GetInput(fire3InputP1) >= 0.5f;
                
                motorDeltaP2 = GetInput(forwardInputP2) - GetInput(reverseInputP2);
                steeringDeltaP2 = GetInput(steerRightInputP2) - GetInput(steerLeftInputP2);
                handbrakeP2 = GetInput(handbrakeInputP2) >= 0.5f;
                boostP2 = GetInput(boostInputP2) >= 0.5f;
                switchP2 = GetInput(switchInputP2)>=0.5f;
                skill1P2 = GetInput(fire1InputP2) >= 0.5f;
                skill2P2 = GetInput(fire2InputP2) >= 0.5f;
                skill3P2 = GetInput(fire3InputP2) >= 0.5f;
            }
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        private float GetInput(InputSetting v)
        {
            if(v.inputType==InputType.GetAxisRaw)
            {
                float value = 0;
                value = Input.GetAxisRaw(v.Name);
                //Debug.Log($"{v.Name}:{value}");
                if (v.Invert) value *= -1;
                return Mathf.Clamp01(value);
            }
            else if(v.inputType == InputType.GetButtonDown)
            {
                var boolValue = Input.GetButtonDown(v.Name);
                return boolValue?1:0;
            }
            
            return 0;
        }

        internal override void Shutdown()
        {
        }
    }
}