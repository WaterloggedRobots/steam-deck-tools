using SteamController.ProfilesSettings;

namespace SteamController.Keybinds
{
    /// Lets the user pick what a button does: keyboard key with modifiers,
    /// mouse button, X360/DS4 button or a Windows action
    internal class KeybindActionEditor : UserControl
    {
        private class Item<T>
        {
            public T Value;
            public String Text;

            public Item(T value, String text)
            {
                Value = value;
                Text = text;
            }

            public override String ToString()
            {
                return Text;
            }
        }

        private readonly ComboBox typeCombo = NewCombo(230);
        private readonly FlowLayoutPanel keyboardPanel = NewRow();
        private readonly CheckBox ctrlCheck = NewCheck("Ctrl");
        private readonly CheckBox shiftCheck = NewCheck("Shift");
        private readonly CheckBox altCheck = NewCheck("Alt");
        private readonly CheckBox winCheck = NewCheck("Win");
        private readonly ComboBox keyCombo = NewCombo(160);
        private readonly ComboBox mouseCombo = NewCombo(200);
        private readonly ComboBox x360Combo = NewCombo(200);
        private readonly ComboBox ds4Combo = NewCombo(200);
        private readonly ComboBox specialCombo = NewCombo(240);

        private bool updating;

        public event Action? Changed;

        public KeybindActionEditor()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            foreach (var type in Enum.GetValues<KeybindActionType>())
                typeCombo.Items.Add(new Item<KeybindActionType>(type, KeybindNames.ActionType(type)));
            foreach (var key in Enum.GetValues<VirtualKeyCode>())
                keyCombo.Items.Add(new Item<VirtualKeyCode>(key, KeybindNames.Key(key)));
            foreach (var button in Enum.GetValues<Devices.MouseController.Button>())
                mouseCombo.Items.Add(new Item<Devices.MouseController.Button>(button, KeybindNames.Mouse(button)));
            foreach (var code in Enum.GetValues<VirtualX360Code>())
                x360Combo.Items.Add(new Item<VirtualX360Code>(code, KeybindNames.X360(code)));
            foreach (var code in Enum.GetValues<VirtualDS4Code>())
                ds4Combo.Items.Add(new Item<VirtualDS4Code>(code, KeybindNames.DS4(code)));
            foreach (var special in Enum.GetValues<SpecialAction>())
                specialCombo.Items.Add(new Item<SpecialAction>(special, KeybindNames.Special(special)));

            keyboardPanel.Controls.AddRange(new Control[] { ctrlCheck, shiftCheck, altCheck, winCheck, keyCombo });

            var layout = NewRow();
            layout.Controls.AddRange(new Control[] { typeCombo, keyboardPanel, mouseCombo, x360Combo, ds4Combo, specialCombo });
            Controls.Add(layout);

            foreach (var combo in new ComboBox[] { typeCombo, keyCombo, mouseCombo, x360Combo, ds4Combo, specialCombo })
                combo.SelectedIndexChanged += delegate { OnValueChanged(); };
            foreach (var check in new CheckBox[] { ctrlCheck, shiftCheck, altCheck, winCheck })
                check.CheckedChanged += delegate { OnValueChanged(); };

            Value = new KeybindAction();
        }

        public KeybindAction Value
        {
            get
            {
                return new KeybindAction()
                {
                    Type = Selected<KeybindActionType>(typeCombo),
                    Key = Selected<VirtualKeyCode>(keyCombo),
                    Ctrl = ctrlCheck.Checked,
                    Shift = shiftCheck.Checked,
                    Alt = altCheck.Checked,
                    Win = winCheck.Checked,
                    Mouse = Selected<Devices.MouseController.Button>(mouseCombo),
                    X360 = Selected<VirtualX360Code>(x360Combo),
                    DS4 = Selected<VirtualDS4Code>(ds4Combo),
                    Special = Selected<SpecialAction>(specialCombo)
                };
            }
            set
            {
                updating = true;
                try
                {
                    SelectValue(typeCombo, value.Type);
                    SelectValue(keyCombo, value.Key);
                    ctrlCheck.Checked = value.Ctrl;
                    shiftCheck.Checked = value.Shift;
                    altCheck.Checked = value.Alt;
                    winCheck.Checked = value.Win;
                    SelectValue(mouseCombo, value.Mouse);
                    SelectValue(x360Combo, value.X360);
                    SelectValue(ds4Combo, value.DS4);
                    SelectValue(specialCombo, value.Special);
                    UpdateVisibility();
                }
                finally
                {
                    updating = false;
                }
            }
        }

        private void OnValueChanged()
        {
            if (updating)
                return;

            UpdateVisibility();
            Changed?.Invoke();
        }

        private void UpdateVisibility()
        {
            var type = Selected<KeybindActionType>(typeCombo);
            keyboardPanel.Visible = type == KeybindActionType.Keyboard;
            mouseCombo.Visible = type == KeybindActionType.Mouse;
            x360Combo.Visible = type == KeybindActionType.Xbox360;
            ds4Combo.Visible = type == KeybindActionType.DS4;
            specialCombo.Visible = type == KeybindActionType.Special;
        }

        private static T Selected<T>(ComboBox combo) where T : struct
        {
            if (combo.SelectedItem is Item<T> item)
                return item.Value;
            return default(T);
        }

        private static void SelectValue<T>(ComboBox combo, T value) where T : struct
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is Item<T> item && item.Value.Equals(value))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
        }

        private static ComboBox NewCombo(int width)
        {
            return new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = width,
                MaxDropDownItems = 20,
                Margin = new Padding(3, 3, 6, 3)
            };
        }

        private static CheckBox NewCheck(String text)
        {
            return new CheckBox()
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(3, 6, 3, 3)
            };
        }

        private static FlowLayoutPanel NewRow()
        {
            return new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0)
            };
        }
    }
}
