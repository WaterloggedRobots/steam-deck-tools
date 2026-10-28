# Steam Controller - Shortcuts

| Button                     | Desktop                | X360 (with Rumble)     | DS4 (with Rumble)      | Steam                  | Steam With Shortcuts   |
|----------------------------|------------------------|------------------------|------------------------|------------------------|------------------------|
| X360 Controller            | Not connected          | Connected              | Not connected          | Not connected          | Not connected          |
| DS4 Controller             | Not connected          | Not connected          | Connected              | Not connected          | Not connected          |
| Gyroscope / Acceloremeter  | Not supported          | Not supported          | Emulated via DS4       | Not supported          | Not supported          |
| Options (hold for 1s)      | Switch to next profile | Switch to next profile | Switch to next profile | Switch to next profile | Switch to next profile |
| Options (hold for 2s)      | Toggle desktop mode    | Toggle desktop mode    | Toggle desktop mode    |                        |                        |
| STEAM + 3 dots             | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  |
| STEAM + Options            | WIN + TAB              | WIN + TAB              | WIN + TAB              | WIN + TAB              | WIN + TAB              |
| STEAM + Menu               | F11                    | F11                    | F11                    | F11                    | F11                    |
| STEAM + Right Stick Press  | WIN + D (Desktop)      | WIN + D (Desktop)      | WIN + D (Desktop)      |                        | WIN + D (Desktop)      |
| STEAM + A                  | RETURN                 | RETURN                 | RETURN                 |                        | RETURN                 |
| STEAM + B (hold for 1s)    | ALT + F4               | ALT + F4               | ALT + F4               |                        | ALT + F4               |
| STEAM + B (hold for 3s)    | Kill active process    | Kill active process    | Kill active process    |                        | Kill active process    |
| STEAM + X                  | Toggle Keyboard        | Toggle Keyboard        | Toggle Keyboard        |                        | Toggle Keyboard        |
| STEAM + L1                 | Toggle Magnify         | Toggle Magnify         | Toggle Magnify         |                        | Toggle Magnify         |
| STEAM + R1                 | Screenshot             | Screenshot             | Screenshot             |                        | Screenshot             |
| STEAM + Left Joystick Up   | Increase Brightness    | Increase Brightness    | Increase Brightness    |                        | Increase Brightness    |
| STEAM + Left Joystick Down | Decrease Brightness    | Decrease Brightness    | Decrease Brightness    |                        | Decrease Brightness    |
| STEAM + DPad Right         | RETURN                 | RETURN                 | RETURN                 |                        | RETURN                 |
| STEAM + DPad Down          | TAB                    | TAB                    | TAB                    |                        | TAB                    |
| STEAM + DPad Left          | ESCAPE                 | ESCAPE                 | ESCAPE                 |                        | ESCAPE                 |
| STEAM + DPad Up            | CTRL + ALT + U         | CTRL + ALT + U         | CTRL + ALT + U         |                        | CTRL + ALT + U         |
| STEAM + Left Pad           | Mouse Scroll           | Mouse Scroll           | Mouse Scroll           |                        | Mouse Scroll           |
| STEAM + Left Joystick      | Mouse Scroll           | Mouse Scroll           | Mouse Scroll           |                        | Mouse Scroll           |
| STEAM + Right Joystick     | Mouse Trackpad         | Mouse Trackpad         | Mouse Trackpad         |                        | Mouse Trackpad         |
| STEAM + Right Pad          | Mouse Move             | Mouse Move             | Mouse Move             |                        | Mouse Move             |
| STEAM + L2                 | Mouse Right Click      | Mouse Right Click      | Mouse Right Click      |                        | Mouse Right Click      |
| STEAM + R2                 | Mouse Left Click       | Mouse Left Click       | Mouse Left Click       |                        | Mouse Left Click       |
| STEAM + Left Pad Press     | Mouse Right Click      | Mouse Right Click      | Mouse Right Click      |                        | Mouse Right Click      |
| STEAM + Right Pad Press    | Mouse Left Click       | Mouse Left Click       | Mouse Left Click       |                        | Mouse Left Click       |
| Left Pad                   | Mouse Scroll           | Mouse Scroll           | DS4 Trackpad           |                        |                        |
| Left Joystick              | Mouse Scroll           |                        | DS4 Trackpad           |                        |                        |
| Right Joystick             | Mouse Trackpad         |                        |                        |                        |                        |
| Right Pad                  | Mouse Move             | Mouse Move             |                        |                        |                        |
| DPad Arrows                | Keyboard Arrows        |                        |                        |                        |                        |
| A                          | RETURN                 |                        |                        |                        |                        |
| B                          | BACKSPACE              |                        |                        |                        |                        |
| Hold L+R Pad Press (0.6.x) |                        | Toggle Mouse Trackpad  |                        |                        |                        |

## Custom keybinds

Right click the Steam Controller tray icon and pick **Keybinds...** to change the mappings above.

- Click a button on the Steam Deck outline (or its label) and pick what it should do:
  a keyboard key with Ctrl/Shift/Alt/Win, a mouse button, an X360 or DS4 button,
  or a Windows action (on-screen keyboard, screenshot, Task Manager, brightness, and more).
- **Edit** switches between the button alone, **STEAM + button** and **3 dots + button**.
  A combo you set replaces the built-in one for that button (e.g. STEAM + X), and
  **Do nothing** disables a built-in combo.
- **Default (built-in)** keeps the mapping from the table above.
  **Show built-in mapping of** picks which mode (Desktop, X360, DS4) the outline describes.
- STEAM, 3 dots and Options cannot be remapped alone, as they drive shortcuts and profile switching.
- L2/R2 bindings fire on a full trigger pull, and the analog trigger is held back while they do.
- Create one profile per game, and list the game's process names in **Auto-use for games**
  (or use **Add running game...**). That profile is used whenever the game is in the foreground;
  otherwise the profile marked **Use when no game matches** is used. The tray menu
  **Keybind Profile** switches that default profile.
- Profiles are saved to `SteamController.keybinds.json` next to `SteamController.exe`.
