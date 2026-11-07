using UnityEngine;

namespace REPOssessed.Cheats.Core
{
    public abstract class ToggleCheat : Cheat
    {
        private new bool enabled = false;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                if (enabled) OnEnable();
                else OnDisable();
            }
        }

        public ToggleCheat() { }
        public ToggleCheat(KeyCode defaultKeybind) : base(defaultKeybind) { }

        public virtual void Toggle() => Enabled = !Enabled;
        public virtual void OnGui() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}
