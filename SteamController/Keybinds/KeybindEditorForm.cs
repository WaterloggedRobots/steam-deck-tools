using System.Diagnostics;

namespace SteamController.Keybinds
{
    /// Window to edit keybind profiles on an outline of the Steam Deck
    internal class KeybindEditorForm : Form
    {
        private readonly Context context;
        private KeybindConfig config;
        private KeybindProfile profile;
        private KeybindLayer layer = KeybindLayer.Pressed;
        private ControllerMode mode = ControllerMode.X360;
        private bool dirty;
        private bool updating;

        private readonly ComboBox profileCombo = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly TextBox gamesText = new TextBox() { Width = 260, PlaceholderText = "game.exe, other.exe" };
        private readonly CheckBox defaultCheck = new CheckBox() { Text = "Use when no game matches", AutoSize = true, AutoCheck = false };
        private readonly ComboBox modeCombo = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly DeckOutlineControl outline = new DeckOutlineControl() { Dock = DockStyle.Fill };
        private readonly GroupBox selectedGroup = new GroupBox() { Dock = DockStyle.Fill, Padding = new Padding(8) };
        private readonly KeybindActionEditor actionEditor = new KeybindActionEditor();
        private readonly Label defaultLabel = new Label() { AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(3, 8, 3, 3) };
        private readonly Label statusLabel = new Label() { AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
        private readonly Button saveButton = new Button() { Text = "&Save", AutoSize = true };
        private readonly List<RadioButton> layerButtons = new List<RadioButton>();
        private readonly System.Windows.Forms.Timer pressedTimer = new System.Windows.Forms.Timer() { Interval = 50 };

        public KeybindEditorForm(Context context)
        {
            this.context = context;
            this.config = KeybindStore.GetConfig();
            this.profile = config.Get(KeybindStore.Active.Name) ?? config.Profiles.First();

            SuspendLayout();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = Controller.TitleWithVersion + " Keybinds";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 760);
            MinimumSize = new Size(800, 560);
            Icon = Resources.microsoft_xbox_controller;

            Controls.Add(outline);
            Controls.Add(BuildBottomPanel());
            Controls.Add(BuildLayerRow());
            Controls.Add(BuildProfileRow());

            outline.Describe = DescribeButton;
            outline.CanBind = (button) => KeybindRunner.CanBind(layer, button);
            outline.ButtonSelected += (button) => UpdateSelected();
            actionEditor.Changed += ActionEditor_Changed;

            pressedTimer.Tick += PressedTimer_Tick;
            pressedTimer.Start();

            KeybindStore.Changed += KeybindStore_Changed;

            ResumeLayout(false);
            PerformLayout();

            RefreshProfiles();
            UpdateStatus();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var workingArea = Screen.FromControl(this).WorkingArea;
            if (Width > workingArea.Width || Height > workingArea.Height)
                WindowState = FormWindowState.Maximized;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.Cancel || !dirty)
                return;

            var result = MessageBox.Show(this, "Save changes to keybinds?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                e.Cancel = true;
            else if (result == DialogResult.Yes)
                Save();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            KeybindStore.Changed -= KeybindStore_Changed;
            pressedTimer.Stop();
            pressedTimer.Dispose();
            base.OnFormClosed(e);
        }

        private Control BuildProfileRow()
        {
            var row = NewRow();
            row.Dock = DockStyle.Top;
            row.Padding = new Padding(6, 6, 6, 0);

            var newButton = NewButton("&New", NewProfile_Click);
            var copyButton = NewButton("&Copy", CopyProfile_Click);
            var renameButton = NewButton("&Rename", RenameProfile_Click);
            var deleteButton = NewButton("&Delete", DeleteProfile_Click);
            var pickButton = NewButton("Add running &game...", PickGame_Click);

            profileCombo.SelectedIndexChanged += delegate
            {
                if (updating)
                    return;
                var selected = config.Get(profileCombo.SelectedItem as String ?? "");
                if (selected is not null)
                {
                    profile = selected;
                    UpdateProfile();
                }
            };

            gamesText.TextChanged += delegate
            {
                if (updating)
                    return;
                profile.Games = gamesText.Text.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((game) => game.Trim()).Where((game) => game != "").ToList();
                MarkDirty();
            };

            defaultCheck.Click += delegate
            {
                config.DefaultProfile = profile.Name;
                defaultCheck.Checked = true;
                MarkDirty();
            };

            row.Controls.AddRange(new Control[] {
                NewLabel("Profile:"), profileCombo, newButton, copyButton, renameButton, deleteButton,
                NewLabel("   Auto-use for games:"), gamesText, pickButton, defaultCheck
            });
            return row;
        }

        private Control BuildLayerRow()
        {
            var row = NewRow();
            row.Dock = DockStyle.Top;
            row.Padding = new Padding(6, 0, 6, 0);
            row.Controls.Add(NewLabel("Edit:"));

            foreach (var value in Enum.GetValues<KeybindLayer>())
            {
                var layerValue = value;
                var radio = new RadioButton()
                {
                    Text = KeybindNames.Layer(layerValue),
                    AutoSize = true,
                    Checked = layerValue == layer,
                    Margin = new Padding(3, 7, 9, 3)
                };
                radio.CheckedChanged += delegate
                {
                    if (!radio.Checked)
                        return;
                    layer = layerValue;
                    UpdateProfile();
                };
                layerButtons.Add(radio);
                row.Controls.Add(radio);
            }

            foreach (var value in Enum.GetValues<ControllerMode>())
                modeCombo.Items.Add(KeybindNames.Mode(value));
            modeCombo.SelectedIndex = (int)mode;
            modeCombo.SelectedIndexChanged += delegate
            {
                mode = (ControllerMode)modeCombo.SelectedIndex;
                UpdateSelected();
            };

            row.Controls.Add(NewLabel("   Show built-in mapping of:"));
            row.Controls.Add(modeCombo);
            return row;
        }

        private Control BuildBottomPanel()
        {
            var panel = new TableLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(6)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var editorRow = NewRow();
            editorRow.Controls.Add(actionEditor);
            editorRow.Controls.Add(NewButton("Reset to &default", delegate
            {
                actionEditor.Value = new KeybindAction();
                ActionEditor_Changed();
            }));

            var groupContent = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            groupContent.Controls.Add(editorRow);
            groupContent.Controls.Add(defaultLabel);
            selectedGroup.AutoSize = true;
            selectedGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            selectedGroup.Controls.Add(groupContent);

            saveButton.Click += delegate { Save(); };
            var closeButton = NewButton("Close", delegate { Close(); });

            var buttons = new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var buttonRow = NewRow();
            buttonRow.Controls.Add(saveButton);
            buttonRow.Controls.Add(closeButton);
            buttons.Controls.Add(buttonRow);
            buttons.Controls.Add(statusLabel);

            panel.Controls.Add(selectedGroup, 0, 0);
            panel.Controls.Add(buttons, 1, 0);
            return panel;
        }

        private (String text, bool custom) DescribeButton(DeckButton button)
        {
            var action = profile.Find(layer, button);
            if (action is not null)
                return (KeybindNames.Action(action), true);
            return (KeybindDefaults.Describe(mode, layer, button), false);
        }

        private void RefreshProfiles()
        {
            updating = true;
            try
            {
                profileCombo.Items.Clear();
                foreach (var item in config.Profiles)
                    profileCombo.Items.Add(item.Name);
                profileCombo.SelectedItem = profile.Name;
            }
            finally
            {
                updating = false;
            }

            UpdateProfile();
        }

        private void UpdateProfile()
        {
            updating = true;
            try
            {
                gamesText.Text = String.Join(", ", profile.Games);
                defaultCheck.Checked = config.DefaultProfile == profile.Name;
            }
            finally
            {
                updating = false;
            }

            outline.Caption = profile.Name + "\n" + KeybindNames.Layer(layer);
            UpdateSelected();
        }

        private void UpdateSelected()
        {
            var button = outline.SelectedButton;
            bool canBind = KeybindRunner.CanBind(layer, button);

            String title = KeybindNames.Button(button);
            if (layer == KeybindLayer.Steam)
                title = "STEAM + " + title;
            else if (layer == KeybindLayer.QuickAccess)
                title = "3 dots + " + title;
            selectedGroup.Text = title;

            actionEditor.Value = profile.Find(layer, button) ?? new KeybindAction();
            actionEditor.Enabled = canBind;

            if (!canBind)
                defaultLabel.Text = "This button is used for shortcuts and switching profiles, so it cannot be changed here.";
            else
                defaultLabel.Text = "Built-in (" + KeybindNames.Mode(mode) + "): " + KeybindDefaults.Describe(mode, layer, button);

            outline.Invalidate();
        }

        private void ActionEditor_Changed()
        {
            profile.Set(layer, outline.SelectedButton, actionEditor.Value);
            MarkDirty();
            outline.Invalidate();
        }

        private void MarkDirty()
        {
            dirty = true;
            UpdateStatus();
        }

        private void Save()
        {
            if (KeybindStore.Save(config))
            {
                dirty = false;
                UpdateStatus();
            }
            else
            {
                MessageBox.Show(this, "Failed to save keybinds to:\n" + KeybindStore.ConfigPath, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus()
        {
            var status = "Active: " + KeybindStore.Active.Name;
            if (KeybindStore.ActiveGame is not null)
                status += " (" + KeybindStore.ActiveGame + ")";
            if (dirty)
                status += "\nUnsaved changes";
            statusLabel.Text = status;
            saveButton.Enabled = dirty;
        }

        private void KeybindStore_Changed()
        {
            if (IsDisposed || !IsHandleCreated)
                return;
            BeginInvoke(new Action(UpdateStatus));
        }

        private void PressedTimer_Tick(object? sender, EventArgs e)
        {
            var steam = context.Steam;
            outline.PressedButtons = KeybindRunner.AllButtons.Where(
                (button) => KeybindRunner.GetButton(steam, button).Value).ToList();
        }

        private String? AskName(String title, String name)
        {
            using (var dialog = new Form())
            {
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.AutoSize = true;
                dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialog.AutoScaleDimensions = new SizeF(96F, 96F);
                dialog.AutoScaleMode = AutoScaleMode.Dpi;

                var textBox = new TextBox() { Text = name, Width = 260 };
                var okButton = new Button() { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
                var cancelButton = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };

                var row = NewRow();
                row.Padding = new Padding(8);
                row.Controls.AddRange(new Control[] { NewLabel("Name:"), textBox, okButton, cancelButton });
                dialog.Controls.Add(row);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;

                var newName = textBox.Text.Trim();
                if (newName == "")
                    return null;

                if (newName != name && config.Get(newName) is not null)
                {
                    MessageBox.Show(this, "A profile named '" + newName + "' already exists.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                return newName;
            }
        }

        private String UniqueName(String name)
        {
            var newName = name;
            for (int i = 2; config.Get(newName) is not null; i++)
                newName = name + " " + i;
            return newName;
        }

        private void NewProfile_Click(object? sender, EventArgs e)
        {
            var name = AskName("New keybind profile", UniqueName("Game"));
            if (name is null)
                return;

            profile = new KeybindProfile() { Name = name };
            config.Profiles.Add(profile);
            MarkDirty();
            RefreshProfiles();
        }

        private void CopyProfile_Click(object? sender, EventArgs e)
        {
            var name = AskName("Copy keybind profile", UniqueName(profile.Name + " copy"));
            if (name is null)
                return;

            profile = profile.Clone();
            profile.Name = name;
            profile.Games.Clear();
            config.Profiles.Add(profile);
            MarkDirty();
            RefreshProfiles();
        }

        private void RenameProfile_Click(object? sender, EventArgs e)
        {
            var name = AskName("Rename keybind profile", profile.Name);
            if (name is null || name == profile.Name)
                return;

            if (config.DefaultProfile == profile.Name)
                config.DefaultProfile = name;
            profile.Name = name;
            MarkDirty();
            RefreshProfiles();
        }

        private void DeleteProfile_Click(object? sender, EventArgs e)
        {
            if (config.Profiles.Count <= 1)
            {
                MessageBox.Show(this, "The last profile cannot be deleted.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Delete profile '" + profile.Name + "'?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            config.Profiles.Remove(profile);
            config.Normalize();
            profile = config.Get(config.DefaultProfile) ?? config.Profiles.First();
            MarkDirty();
            RefreshProfiles();
        }

        private void PickGame_Click(object? sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            var ownProcess = Process.GetCurrentProcess().ProcessName;
            var names = new SortedSet<String>(StringComparer.OrdinalIgnoreCase);

            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero && process.ProcessName != ownProcess && process.ProcessName != "explorer")
                            names.Add(process.ProcessName);
                    }
                    catch (Exception) { }
                }
            }

            foreach (var name in names)
            {
                var gameName = name;
                menu.Items.Add(gameName, null, delegate
                {
                    if (!profile.MatchesGame(gameName))
                    {
                        profile.Games.Add(gameName);
                        UpdateProfile();
                        MarkDirty();
                    }
                });
            }

            if (names.Count == 0)
                menu.Items.Add("No windows found").Enabled = false;

            menu.Closed += delegate { BeginInvoke(new Action(menu.Dispose)); };
            var source = sender as Control ?? this;
            menu.Show(source, new Point(0, source.Height));
        }

        private static FlowLayoutPanel NewRow()
        {
            return new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true
            };
        }

        private static Label NewLabel(String text)
        {
            return new Label() { Text = text, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
        }

        private static Button NewButton(String text, EventHandler onClick)
        {
            var button = new Button() { Text = text, AutoSize = true };
            button.Click += onClick;
            return button;
        }
    }
}
